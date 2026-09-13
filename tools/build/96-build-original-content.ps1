<#
.SYNOPSIS
    Compiles the game's assets from Refactored Games' original source, with no Visual Studio and
    no installed MonoGame SDK.

.DESCRIPTION
    The studio's content project is Content\Content.mgcb - 431 items: textures, sound effects,
    songs, effects, fonts, a video, and XML data - built by MGCB, the MonoGame Content Builder,
    with three custom processors that live in the solution's own pipeline projects.

    Nothing about that is installed on a machine that only has the .NET SDK, so this script:

      1. builds the solution (via 95-build-original-source.ps1), which produces the custom
         pipeline assemblies Content.mgcb references;
      2. builds MGCB.exe from the MonoGame fork next door;
      3. runs MGCB over Content.mgcb and reports what compiled and what did not.

    DEBUG, NOT RELEASE, is the default configuration: Content.mgcb references the pipeline
    assemblies by path, and those paths say bin\Debug. That is the studio's own wiring, left alone.

    The build writes .xnb files to -OutputDir, which defaults to a folder under $env:TEMP rather
    than into the source tree.

.PARAMETER SourceRoot
    Folder holding the UnclaimedWorld and MonoGame checkouts side by side. Defaults to
    original_src beside this repository.

.PARAMETER Configuration
    Configuration for the solution and MGCB build. Debug (default) matches Content.mgcb.

.PARAMETER OutputDir
    Where the compiled .xnb files go. Defaults to <WorkDir>\content-out.

.PARAMETER IntermediateDir
    MGCB's intermediate folder. Defaults to <WorkDir>\content-obj. Keeping it lets a second run
    rebuild only what changed.

.PARAMETER WorkDir
    Scratch folder, shared with 95-build-original-source.ps1. Defaults to $env:TEMP\uw-original-build.

.PARAMETER SkipSolutionBuild
    Assume the pipeline assemblies and MGCB.exe are already built.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File build\96-build-original-content.ps1

.EXAMPLE
    powershell -File build\96-build-original-content.ps1 -OutputDir D:\uw-content -SkipSolutionBuild
#>
[CmdletBinding()]
param(
    [string] $SourceRoot,
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',
    [string] $OutputDir,
    [string] $IntermediateDir,
    [string] $WorkDir,
    [switch] $SkipSolutionBuild,
    # Everything learned about this tree is ON by default and switched OFF by name. Switches, not
    # [bool] parameters: powershell.exe -File passes every argument as a STRING, so -Something:$false
    # arrives as the text "$false" and fails to bind - which silently aborts the run.
    [switch] $SkipPipelineFix,
    [switch] $SkipReconstruct,
    [switch] $SkipModels,
    # Where the game should find its assets. The exe loads "Content" beside itself, so building
    # into a scratch folder is fine for checking the pipeline and useless for running the game.
    # Defaults to the game's own output folder; -DeployTo '' turns deployment off.
    # Target platform. Each .mgcb says /platform:Windows; passing this overrides it on the command
    # line, which is how the same manifests produce a DesktopGL asset set. Outputs and MGCB caches
    # are kept apart per platform - one cache for two platforms makes every run rebuild everything.
    [ValidateSet('Windows', 'DesktopGL')]
    [string] $Platform = 'Windows',
    [string] $DeployTo,
    # -DeployTo '' cannot be expressed through powershell.exe -File (an empty argument is dropped),
    # so turning deployment off gets its own switch.
    [switch] $NoDeploy,
    # A retail installation to fill the gaps from. Assets this build cannot produce - the 47 meshes,
    # while their sources are FBX 6.1 - are copied from here so the game can actually start.
    [string] $FillFrom
)

$ApplyPipelineFix   = -not $SkipPipelineFix
$ReconstructMissing = -not $SkipReconstruct
$IncludeModels      = -not $SkipModels

$ErrorActionPreference = 'Stop'

function Write-Step { param([string] $Text) Write-Host "==> $Text" -ForegroundColor Cyan }
function Write-Ok   { param([string] $Text) Write-Host "    ok    $Text" -ForegroundColor Green }
function Write-Fail { param([string] $Text) Write-Host "    FAIL  $Text" -ForegroundColor Red }
function Write-Note { param([string] $Text) Write-Host "    $Text" -ForegroundColor DarkGray }

if (-not $SourceRoot) {
    $SourceRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'original_src'
}
$SourceRoot = (Resolve-Path -LiteralPath $SourceRoot).Path
if (-not $WorkDir)         { $WorkDir         = Join-Path $env:TEMP 'uw-original-build' }
$platformSuffix = ''
if ($Platform -ne 'Windows') { $platformSuffix = "-$Platform" }
if (-not $OutputDir)       { $OutputDir       = Join-Path $WorkDir "content-out$platformSuffix" }
if (-not $IntermediateDir) { $IntermediateDir = Join-Path $WorkDir "content-obj$platformSuffix" }

# BOTH content projects. The game's assets are not all in the game's project: the UI library
# carries its own, and that is where every font the interface uses comes from - Fonts\CRTBasic and
# friends. Building only UnclaimedWorld\Content leaves the output with Arial and SpriteFont1 and
# nothing else, which looks like "the fonts are missing" and is really "the second project was
# never built".
$contentProjects = @(
    [pscustomobject]@{
        Name         = 'UnclaimedWorld'
        Dir          = Join-Path $SourceRoot 'UnclaimedWorld\UnclaimedWorld\Content'
        Intermediate = $IntermediateDir
    },
    [pscustomobject]@{
        Name         = 'WindowSystem'
        Dir          = Join-Path $SourceRoot 'UnclaimedWorld\WindowSystem\Content'
        # Its own intermediate folder: MGCB's up-to-date cache is per project, and pointing two
        # projects at one cache makes each run think the other's outputs are stale.
        Intermediate = "$IntermediateDir-windowsystem"
    }
)

$mgcbProject = Join-Path $SourceRoot 'MonoGame\Tools\MGCB\MGCB.Windows.csproj'
$itemCount = 0

# ------------------------------------------------------------------- the models, from the backup
#
# The published Content.mgcb builds no models at all: its Models\ entries are textures. The 47
# meshes live in the manifest beside it - "Content - with models.mgcb.bak" - which is the state of
# the project before they were dropped, and in the older XNA content project (.contentproj), which
# lists the same 47 as FbxImporter items.
#
# So the model manifest is generated here rather than maintained: take every item the backup has
# and the current project does not, keep the ones whose source file actually exists, and give them
# the pipeline references they need. The two model processors - AnimatedModelProcessor and
# AnimatedVehicleProcessor - live in AnimationComponentPipeline and VehiclePipeline, which the
# backup does not reference at all (its two /reference lines cover only the sprite pipelines, one
# of them via an x64 path that this solution does not produce).
if ($IncludeModels) {
    $gameContentDir = $contentProjects[0].Dir
    $backup = Join-Path $gameContentDir 'Content - with models.mgcb.bak'
    $current = Join-Path $gameContentDir 'Content.mgcb'

    if (Test-Path -LiteralPath $backup) {
        $currentItems = @{}
        foreach ($line in (Get-Content -LiteralPath $current)) {
            if ($line -match '^#begin (.+)$') { $currentItems[$Matches[1].Trim()] = $true }
        }

        $extra = New-Object System.Text.StringBuilder
        [void]$extra.AppendLine('# Generated by build/96-build-original-content.ps1 - do not edit.')
        [void]$extra.AppendLine('/platform:Windows')
        [void]$extra.AppendLine('/profile:HiDef')
        [void]$extra.AppendLine('/compress:True')
        foreach ($dll in @('SpriteSheetPipeline', 'SpriteEffectsPipeline',
                           'AnimationComponentPipeline', 'VehiclePipeline')) {
            [void]$extra.AppendLine("/reference:..\..\$dll\bin\$Configuration\$dll.dll")
        }

        $block = $null
        $keep = $false
        $added = 0
        $skipped = 0
        foreach ($line in (Get-Content -LiteralPath $backup)) {
            if ($line -match '^#begin (.+)$') {
                $asset = $Matches[1].Trim()
                $sourcePath = Join-Path $gameContentDir $asset.Replace('/', '\')
                $keep = (-not $currentItems.ContainsKey($asset)) -and (Test-Path -LiteralPath $sourcePath)
                if ($keep) {
                    $added++
                    [void]$extra.AppendLine('')
                    [void]$extra.AppendLine($line)
                } elseif (-not $currentItems.ContainsKey($asset)) {
                    $skipped++
                }
                continue
            }
            if ($keep -and $line.Trim() -ne '') { [void]$extra.AppendLine($line) }
        }

        if ($added -gt 0) {
            $modelsManifest = Join-Path $WorkDir 'models.mgcb'
            $extra.ToString() | Set-Content -LiteralPath $modelsManifest -Encoding utf8
            $contentProjects += [pscustomobject]@{
                Name         = 'Models'
                Dir          = $gameContentDir
                Intermediate = "$IntermediateDir-models"
                File         = $modelsManifest
                Items        = $added
            }
            Write-Step "models: $added item(s) recovered from the backup manifest"
            if ($skipped -gt 0) { Write-Note "$skipped item(s) skipped - source file not in the tree" }
        }
    }
}

foreach ($project in $contentProjects) {
    # The generated models manifest arrives with File and Items already set; the two real projects
    # get theirs here.
    if (-not ($project.PSObject.Properties.Name -contains 'File')) {
        $project | Add-Member -NotePropertyName File -NotePropertyValue (Join-Path $project.Dir 'Content.mgcb')
        if (-not (Test-Path -LiteralPath $project.File)) { throw "Content.mgcb not found at $($project.File)" }
        $count = @(Select-String -LiteralPath $project.File -Pattern '^#begin ').Count
        $project | Add-Member -NotePropertyName Items -NotePropertyValue $count
    }
    $itemCount += $project.Items
    Write-Step "content project: $($project.File)"
    Write-Ok "$($project.Items) content items"
}

# --------------------------------------------------------- 1. the solution (custom processors)

if (-not $SkipSolutionBuild) {
    Write-Step "building the solution ($Configuration) for the custom pipeline assemblies"
    $solutionScript = Join-Path $PSScriptRoot '95-build-original-source.ps1'
    & $solutionScript -SourceRoot $SourceRoot -Configuration $Configuration -WorkDir $WorkDir | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "95-build-original-source.ps1 failed - fix the code build first." }
    Write-Ok 'solution built'
}

# The assemblies each project names, so a missing one is reported here rather than as 40 identical
# "processor not found" errors later.
foreach ($project in $contentProjects) {
    foreach ($reference in (Select-String -LiteralPath $project.File -Pattern '^/reference:(.+)$' |
                            ForEach-Object { $_.Matches[0].Groups[1].Value.Trim() })) {
        $resolved = Join-Path $project.Dir $reference
        if (Test-Path -LiteralPath $resolved) {
            Write-Ok "reference: $reference"
        } else {
            Write-Fail "reference missing: $reference"
            Write-Note 'Content.mgcb names its pipeline assemblies by path, and those paths say bin\Debug.'
            Write-Note 'Run this script without -Configuration Release, or rebuild that project in Debug.'
        }
    }
}

# ---------------------------------------------------------------------------- 2. build MGCB.exe

$mgcbExe   = Join-Path $SourceRoot "MonoGame\Tools\MGCB\bin\Windows\AnyCPU\$Configuration\MGCB.exe"
$monoGame  = Join-Path $SourceRoot 'MonoGame'
$fixPatch  = Join-Path (Split-Path -Parent $PSScriptRoot) 'patches\original-src\01-curvekeycollection-expanded-form.patch'
$fixActive = $false

if ($ApplyPipelineFix) {
    # TimeOfDay_Alpha.xml is authored in XNA's expanded curve-key form
    # (<CurveKey><Position>..</Position>..</CurveKey>), which this MonoGame fork's
    # CurveKeyCollectionSerializer cannot read: it stops at the first end tag it meets and then
    # rejects the file for having one value instead of a multiple of five. The patch teaches the
    # serializer to walk the element by depth, which reads both forms.
    #
    # Applied only long enough to compile MGCB, then reverted - the studio's checkout is left as
    # it was found, and the fix lives on inside the tool binary this script then uses.
    Write-Step 'applying the curve-serializer fix to the MonoGame fork (temporarily)'
    if (-not (Test-Path -LiteralPath $fixPatch)) { throw "Patch not found: $fixPatch" }
    if (-not (Get-Command git.exe -ErrorAction SilentlyContinue)) { throw 'git is needed to apply the fix.' }

    # Tracked changes only: bin\ and obj\ from an earlier build sit under this path and are not
    # a reason to refuse.
    $dirty = @(& git -c core.autocrlf=true -C $monoGame status --porcelain --untracked-files=no -- MonoGame.Framework.Content.Pipeline)
    if ($dirty.Count -gt 0) { throw "The MonoGame pipeline tree has local changes; refusing to patch it." }

    & git -c core.autocrlf=true -C $monoGame apply --check $fixPatch
    if ($LASTEXITCODE -ne 0) { throw "The fix does not apply cleanly to $monoGame." }
    & git -c core.autocrlf=true -C $monoGame apply $fixPatch
    if ($LASTEXITCODE -ne 0) { throw 'Applying the fix failed.' }
    $fixActive = $true

    # Force MGCB to be rebuilt, so the patched serializer is the one that runs.
    if (Test-Path -LiteralPath $mgcbExe) { Remove-Item -LiteralPath $mgcbExe -Force }
    Write-Ok 'patch applied'
}

try {

if (-not (Test-Path -LiteralPath $mgcbExe)) {
    Write-Step 'building MGCB.exe from the MonoGame fork'
    if (-not (Test-Path -LiteralPath $mgcbProject)) {
        throw "MGCB project not found: $mgcbProject (run Protobuild.exe --generate Windows in the MonoGame folder)"
    }

    # Same two crutches the code build needs: net48 reference assemblies and the design-time
    # facades. 95-build-original-source.ps1 leaves both ready in the work directory.
    $injected = Join-Path $WorkDir 'uw-original.targets'
    if (-not (Test-Path -LiteralPath $injected)) {
        throw "$injected not found - run 95-build-original-source.ps1 first (or drop -SkipSolutionBuild)."
    }
    $refRoot = (Select-String -LiteralPath $injected -Pattern 'build/\.NETFramework/v4\.8' |
                Select-Object -First 1).Line -replace '.*<HintPath>([^<]+)/Facades/.*', '$1'
    $refRoot = $refRoot.Replace('/', '\')

    $dotnet = $null
    foreach ($candidate in @($env:DOTNET_HOST_PATH,
                             (Get-Command dotnet.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source),
                             "$env:ProgramFiles\dotnet\dotnet.exe")) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) { $dotnet = $candidate; break }
    }
    if (-not $dotnet) { throw 'dotnet.exe not found.' }

    & $dotnet msbuild $mgcbProject `
        "-p:Configuration=$Configuration" `
        "-p:FrameworkPathOverride=$refRoot" `
        "-p:CustomAfterMicrosoftCommonTargets=$injected" `
        -v:m -nologo -m:1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'MGCB failed to build.' }
}
Write-Ok "MGCB: $mgcbExe"

} finally {
    if ($fixActive) {
        & git -c core.autocrlf=true -C $monoGame apply -R $fixPatch
        if ($LASTEXITCODE -eq 0) {
            Write-Ok 'curve-serializer fix reverted - the studio checkout is unchanged'
        } else {
            Write-Fail "Could not revert $fixPatch - check 'git -C $monoGame status'."
        }
    }
}

# -------------------------------------------------------------------------- 3. build the content

foreach ($dir in @($OutputDir, $IntermediateDir)) {
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
}

$staged = @()
if ($ReconstructMissing) {
    # Three items the content project builds are not in the published tree. None of them needs art
    # invented, and none is copied out of the shipped game:
    #
    #   landscapeSquare_0.png, landscapeSquare_1.png - the shipped game's landscapeSquare.xnb,
    #       landscapeSquare_0.xnb and landscapeSquare_1.xnb are BYTE-IDENTICAL to each other, so the
    #       three sources were the same image. Staging copies of the one that is present rebuilds
    #       both to files byte-identical to retail.
    #
    #   spriteEffects\ - FlatSprites.xml lists it among the folders its processor scans, and the
    #       processor calls Directory.GetFiles on it. This sheet takes ground sprites only
    #       (ProcessOrdinarySprites=False), so the folder contributes nothing to it: built with the
    #       folder empty, the sheet carries the same 426 sprites as retail. It only has to exist.
    #
    # Staged before the build and removed after, so the studio's checkout is left as it was found.
    Write-Step 'staging the three sources the release is missing'
    $gameContent = $contentProjects[0].Dir
    $base = Join-Path $gameContent 'landscapeSquare.png'

    foreach ($name in @('landscapeSquare_0.png', 'landscapeSquare_1.png')) {
        $target = Join-Path $gameContent $name
        if ((Test-Path -LiteralPath $base) -and -not (Test-Path -LiteralPath $target)) {
            Copy-Item -LiteralPath $base -Destination $target
            $staged += $target
            Write-Ok "$name (copy of landscapeSquare.png)"
        }
    }

    $spriteEffects = Join-Path $gameContent 'spriteEffects'
    if (-not (Test-Path -LiteralPath $spriteEffects)) {
        New-Item -ItemType Directory -Path $spriteEffects -Force | Out-Null
        $staged += $spriteEffects
        Write-Ok 'spriteEffects\ (empty - the processor only needs the path to exist)'
    }

    # Two of the UI fonts name a typeface that is not in the tree and not on a stock Windows:
    #
    #   LCDandHUDBody.spritefont, LCDandHUDSubHeading.spritefont -> <FontName>Electrolize</FontName>
    #
    # FontDescriptionProcessor looks the name up in the installed-fonts registry first, and when
    # that misses it searches a list of directories - including the .spritefont's OWN folder - for
    # <FontName>.ttf. So the file only has to sit beside the descriptors for the length of the
    # build; nothing is installed on the machine. Electrolize is a Google font under the SIL Open
    # Font License, cached in the work directory after the first fetch.
    $fontsDir = Join-Path $SourceRoot 'UnclaimedWorld\WindowSystem\Content\Fonts'
    $fontTarget = Join-Path $fontsDir 'Electrolize.ttf'
    if ((Test-Path -LiteralPath $fontsDir) -and -not (Test-Path -LiteralPath $fontTarget)) {
        $fontCache = Join-Path $WorkDir 'fonts\Electrolize.ttf'
        if (-not (Test-Path -LiteralPath $fontCache)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $fontCache) -Force | Out-Null
            $url = 'https://github.com/google/fonts/raw/main/ofl/electrolize/Electrolize-Regular.ttf'
            try {
                Write-Note 'fetching Electrolize (SIL Open Font License) - first run only'
                [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                Invoke-WebRequest -Uri $url -OutFile $fontCache -UseBasicParsing
            } catch {
                Write-Fail "could not fetch Electrolize: $($_.Exception.Message)"
                Write-Note 'LCDandHUDBody and LCDandHUDSubHeading will fail. Install the typeface, or'
                Write-Note "place Electrolize.ttf at $fontCache and run again."
            }
        }
        if (Test-Path -LiteralPath $fontCache) {
            Copy-Item -LiteralPath $fontCache -Destination $fontTarget
            $staged += $fontTarget
            Write-Ok 'Electrolize.ttf staged beside the .spritefont files'
        }
    }
}

try {

Write-Step "compiling $itemCount items from $($contentProjects.Count) content project(s)"
Write-Note "output:       $OutputDir"

$contentLog = Join-Path $WorkDir 'content.log'
if (Test-Path -LiteralPath $contentLog) { Remove-Item -LiteralPath $contentLog -Force }
$mgcbExit = 0

foreach ($project in $contentProjects) {
    if (-not (Test-Path -LiteralPath $project.Intermediate)) {
        New-Item -ItemType Directory -Path $project.Intermediate -Force | Out-Null
    }
    Write-Note "  $($project.Name): $($project.Items) items -> $($project.Intermediate)"

    $projectLog = Join-Path $WorkDir "content-$($project.Name).log"
    Push-Location $project.Dir
    try {
        # The paths inside Content.mgcb are relative to its own folder, so the working directory is
        # part of the contract. /outputDir and /intermediateDir come AFTER the response file so they
        # override the ones it sets - which point back into the source tree. Both projects write
        # into ONE output folder, which is how the game expects to find its assets at runtime.
        & $mgcbExe "/@:$($project.File)" "/platform:$Platform" "/outputDir:$OutputDir" "/intermediateDir:$($project.Intermediate)" 2>&1 |
            Tee-Object -FilePath $projectLog | Out-Null
        if ($LASTEXITCODE -ne 0) { $mgcbExit = $LASTEXITCODE }
    } finally {
        Pop-Location
    }
    Get-Content -LiteralPath $projectLog | Add-Content -LiteralPath $contentLog
}

# ----------------------------------------------------------------------------------- 4. report

$log      = Get-Content -LiteralPath $contentLog
# One summary line per content project, so add them up rather than reporting the last one.
$succeeded = 0
$failedCount = 0
foreach ($line in ($log | Where-Object { $_ -match '^Build (\d+) succeeded, (\d+) failed' })) {
    if ($line -match '^Build (\d+) succeeded, (\d+) failed') {
        $succeeded += [int]$Matches[1]
        $failedCount += [int]$Matches[2]
    }
}
$summary = "Build $succeeded succeeded, $failedCount failed (across $($contentProjects.Count) content projects)"
# -cmatch, case-sensitively, and on MGCB's own "<asset>: error: <what>" shape. PowerShell's -match
# ignores case, which also caught each exception's "...: Error importing file:" detail line and
# double-counted every failure.
$failures = @($log | Where-Object { $_ -cmatch ': error: ' } |
                     ForEach-Object { ($_ -replace '.*[\\/]Content[\\/]', '').Trim() } |
                     Sort-Object -Unique)
$xnb      = @(Get-ChildItem -LiteralPath $OutputDir -Recurse -Filter *.xnb -ErrorAction SilentlyContinue)
$bytes    = ($xnb | Measure-Object -Property Length -Sum).Sum

Write-Host ''
Write-Step 'result'
if ($summary) { Write-Note $summary }
Write-Ok ("{0} .xnb files, {1:N1} MB" -f $xnb.Count, ($bytes / 1MB))

if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Step "$($failures.Count) item(s) did not build"
    foreach ($f in $failures) { Write-Fail $f }
    Write-Host ''
    # Three causes, all of them about inputs rather than about this build:
    Write-Note 'Known causes on this tree, none of them a toolchain fault:'
    Write-Note '  .fbx           the meshes are FBX 6.1 (FBXVersion: 6100, Autodesk 1997-2010).'
    Write-Note '                 XNA''s own importer read that format; MonoGame uses Assimp, which'
    Write-Note '                 supports FBX 2011-2013 only. Convert the meshes to FBX 2013 and'
    Write-Note '                 they import - the files themselves are complete.'
    Write-Note '  .spritefont    LCDandHUDBody and LCDandHUDSubHeading ask for the Electrolize'
    Write-Note '                 typeface, which is not in the tree. Install it and both build.'
    Write-Note '  anything else  a source file the release does not carry; -ReconstructMissing'
    Write-Note '                 already covers the three that can be rebuilt without new art.'
}

# ---------------------------------------------------------------------- 5. deploy, and fill gaps

if ($NoDeploy) {
    $DeployTo = ''
} elseif (-not $PSBoundParameters.ContainsKey('DeployTo')) {
    $DeployTo = Join-Path $SourceRoot "UnclaimedWorld\UnclaimedWorld\bin\Windows\$Configuration\Content"
}

if ($DeployTo) {
    Write-Host ''
    Write-Step "deploying to $DeployTo"
    New-Item -ItemType Directory -Path $DeployTo -Force | Out-Null
    Copy-Item -Path (Join-Path $OutputDir '*') -Destination $DeployTo -Recurse -Force
    $deployed = @(Get-ChildItem -LiteralPath $DeployTo -Recurse -Filter *.xnb -ErrorAction SilentlyContinue)
    Write-Ok "$($deployed.Count) asset(s) in place beside the game"

    if ($FillFrom) {
        # Anything this build cannot produce yet - today that is the 47 meshes, whose sources are
        # FBX 6.1 and whose importer starts at FBX 2011 - taken from a retail installation so the
        # game can start. Only names that are MISSING are copied: nothing built here is overwritten,
        # so a freshly built asset always wins over the shipped one.
        if (-not (Test-Path -LiteralPath $FillFrom)) { throw "-FillFrom not found: $FillFrom" }
        Write-Step "filling gaps from $FillFrom"
        $copied = 0
        foreach ($source in Get-ChildItem -LiteralPath $FillFrom -Recurse -Filter *.xnb) {
            $relative = $source.FullName.Substring($FillFrom.TrimEnd('\').Length + 1)
            $target = Join-Path $DeployTo $relative
            if (-not (Test-Path -LiteralPath $target)) {
                New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
                Copy-Item -LiteralPath $source.FullName -Destination $target
                $copied++
            }
        }
        Write-Ok "$copied asset(s) copied from the retail install"
        if ($copied -gt 0) {
            Write-Note 'Those are the assets still to be rebuilt from source - see the causes above.'
        }
    }
}

Write-Host ''
Write-Note "full log: $contentLog"

} finally {
    # Whatever was staged comes back out, success or failure, so the studio's checkout is exactly
    # as it was found. Only paths this run created are touched.
    foreach ($path in $staged) {
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    if ($staged.Count -gt 0) {
        Write-Ok "$($staged.Count) staged item(s) removed - the source tree is unchanged"
    }
}

if ($mgcbExit -eq 0 -and $failures.Count -eq 0) {
    Write-Host "CONTENT BUILDS - $($xnb.Count) assets." -ForegroundColor Green
    exit 0
}
Write-Host "CONTENT BUILT WITH $($failures.Count) FAILURE(S) - $($xnb.Count) assets produced." -ForegroundColor Yellow
exit 2
