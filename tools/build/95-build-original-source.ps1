<#
.SYNOPSIS
    Builds Refactored Games' original Unclaimed World source with nothing but the .NET SDK.

.DESCRIPTION
    The studio's solution is a Visual Studio 2017 / .NET Framework 4.8 build: thirteen old-style
    csproj projects plus the MonoGame fork next to it. Building it the documented way needs Visual
    Studio and the .NET Framework 4.8 targeting pack installed.

    This script builds it on a machine that has NEITHER - only the .NET SDK (8.x) - by supplying
    the two things that installation would otherwise provide:

      1. REFERENCE ASSEMBLIES. Restored from the Microsoft.NETFramework.ReferenceAssemblies.net48
         NuGet package and handed to MSBuild as FrameworkPathOverride.

      2. THE DESIGN-TIME FACADES. A real targeting pack makes MSBuild expand these automatically;
         the NuGet package does not, so SharpDX's type-forwards into System.Runtime, System.IO and
         friends cannot be resolved and MonoGame fails with 60+ CS0012. This script writes a
         .targets file referencing every facade in the pack and injects it with
         CustomAfterMicrosoftCommonTargets, so the original tree is never modified.

    It also works around one project-file quirk, described at the Steam DLL block below.

    NOTHING IN original_src IS EDITED. The injected targets file lives in the work directory; the
    only thing the build leaves behind in the source tree is bin\ and obj\, which the studio's own
    .gitignore already covers. Pass -Clean to remove them afterwards.

.PARAMETER SourceRoot
    Folder holding BOTH checkouts side by side, as the studio's README requires:
        <SourceRoot>\UnclaimedWorld\Unclaimed World Mono.sln
        <SourceRoot>\MonoGame\
    Defaults to original_src beside this repository.

.PARAMETER Configuration
    Release (default) or Debug.

.PARAMETER WorkDir
    Where the reference-assembly restore project, the injected targets and the build log go.
    Defaults to a folder under $env:TEMP.

.PARAMETER Clean
    Delete bin\ and obj\ under both checkouts before building.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File build\95-build-original-source.ps1

.EXAMPLE
    powershell -File build\95-build-original-source.ps1 -SourceRoot D:\src -Configuration Debug -Clean
#>
[CmdletBinding()]
param(
    [string] $SourceRoot,
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [string] $WorkDir,
    [switch] $Clean
)

$ErrorActionPreference = 'Stop'

# The NuGet package that carries the .NET Framework 4.8 reference assemblies. Pinned: this is a
# build input, and an unpinned one turns "does it compile" into "did it compile today".
$RefPackageId      = 'Microsoft.NETFramework.ReferenceAssemblies.net48'
$RefPackageVersion = '1.0.3'
$TargetFramework   = 'v4.8'

function Write-Step  { param([string] $Text) Write-Host "==> $Text" -ForegroundColor Cyan }
function Write-Ok    { param([string] $Text) Write-Host "    ok    $Text" -ForegroundColor Green }
function Write-Fail  { param([string] $Text) Write-Host "    FAIL  $Text" -ForegroundColor Red }
function Write-Note  { param([string] $Text) Write-Host "    $Text" -ForegroundColor DarkGray }

# ----------------------------------------------------------------------------- locate the inputs

if (-not $SourceRoot) {
    $SourceRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'original_src'
}
$SourceRoot = (Resolve-Path -LiteralPath $SourceRoot).Path

$solution = Join-Path $SourceRoot 'UnclaimedWorld\Unclaimed World Mono.sln'
$monoGame = Join-Path $SourceRoot 'MonoGame\MonoGame.Framework\MonoGame.Framework.Windows.csproj'

Write-Step "source: $SourceRoot"
if (-not (Test-Path -LiteralPath $solution)) {
    throw "Solution not found: $solution"
}
if (-not (Test-Path -LiteralPath $monoGame)) {
    throw @"
MonoGame's Windows project is missing: $monoGame

The studio's README has the recipe - MonoGame must sit NEXT TO the Unclaimed World solution and
its projects must be generated first:

    cd <SourceRoot>\MonoGame
    git submodule update --init --recursive
    .\Protobuild.exe --generate Windows
"@
}
Write-Ok "solution and MonoGame\MonoGame.Framework.Windows.csproj are both present"

# The projects the solution actually lists, resolved to full paths. Used for the -Clean sweep and
# for the closing report, so both describe the studio's own project set rather than a guess.
$solutionDir  = Split-Path -Parent $solution
$projectFiles = @()
foreach ($line in (Get-Content -LiteralPath $solution)) {
    if ($line -match '^Project\("\{[^}]+\}"\)\s*=\s*"[^"]+",\s*"([^"]+)"') {
        $candidate = Join-Path $solutionDir $Matches[1]
        if (Test-Path -LiteralPath $candidate) {
            $projectFiles += (Resolve-Path -LiteralPath $candidate).Path
        }
    }
}
Write-Ok "$($projectFiles.Count) projects in the solution"

$dotnet = $null
foreach ($candidate in @($env:DOTNET_HOST_PATH,
                         (Get-Command dotnet.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source),
                         "$env:ProgramFiles\dotnet\dotnet.exe")) {
    if ($candidate -and (Test-Path -LiteralPath $candidate)) { $dotnet = $candidate; break }
}
if (-not $dotnet) { throw 'dotnet.exe not found. Install the .NET SDK (8.0 or later).' }
Write-Ok "dotnet: $dotnet ($(& $dotnet --version))"

if (-not $WorkDir) {
    $WorkDir = Join-Path $env:TEMP 'uw-original-build'
}
if (-not (Test-Path -LiteralPath $WorkDir)) {
    New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
}
Write-Note "work directory: $WorkDir"

# ------------------------------------------------------------------- 1. get reference assemblies

Write-Step "reference assemblies ($RefPackageId $RefPackageVersion)"

$globalPackages = $null
foreach ($line in (& $dotnet nuget locals global-packages --list)) {
    if ($line -match 'global-packages:\s*(.+)$') { $globalPackages = $Matches[1].Trim() }
}
if (-not $globalPackages) { throw 'Could not determine the NuGet global-packages folder.' }

$refRoot = Join-Path $globalPackages "$($RefPackageId.ToLowerInvariant())\$RefPackageVersion\build\.NETFramework\$TargetFramework"

if (-not (Test-Path -LiteralPath $refRoot)) {
    # Restore it through a throwaway project: no NuGet.exe needed, and it lands in the same cache
    # any later build would use.
    $restoreProject = Join-Path $WorkDir 'refs.csproj'
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="$RefPackageId" Version="$RefPackageVersion" />
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $restoreProject -Encoding utf8

    Write-Note 'restoring (needs network access the first time only)'
    & $dotnet restore $restoreProject --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore of $RefPackageId failed." }
}

if (-not (Test-Path -LiteralPath $refRoot)) {
    throw "Reference assemblies still not at $refRoot"
}
$facadeDir = Join-Path $refRoot 'Facades'
$facades = @(Get-ChildItem -LiteralPath $facadeDir -Filter *.dll -ErrorAction SilentlyContinue)
Write-Ok "$($facades.Count) facade assemblies under $facadeDir"

# ------------------------------------------------------------------- 2. write the injected targets

Write-Step 'writing the injected targets (the original tree is not modified)'

$injected = Join-Path $WorkDir 'uw-original.targets'
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('<Project>')
[void]$sb.AppendLine('  <!-- Generated by build/95-build-original-source.ps1. Injected with')
[void]$sb.AppendLine('       CustomAfterMicrosoftCommonTargets so that the studio source stays untouched. -->')
[void]$sb.AppendLine('  <ItemGroup Condition="''$(TargetFrameworkIdentifier)'' == ''.NETFramework''">')
foreach ($facade in $facades) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($facade.Name)
    $path = $facade.FullName.Replace('\', '/')
    [void]$sb.AppendLine("    <Reference Include=""$name""><HintPath>$path</HintPath><Private>false</Private></Reference>")
}
[void]$sb.AppendLine('  </ItemGroup>')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('  <!-- The game project ships the native Steam DLLs (CSteamworks.dll, steam_api.dll and their')
[void]$sb.AppendLine('       64-bit twins) beside its sources and lists them as <Content> so they land next to the')
[void]$sb.AppendLine('       .exe. This MSBuild hands them to the C# compiler as /reference: as well, and a native')
[void]$sb.AppendLine('       PE has no managed metadata:')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('           CSC : error CS0009: Metadata file ...\steam_api.dll could not be opened')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('       They are not in ReferencePath when ResolveAssemblyReferences finishes - something after')
[void]$sb.AppendLine('       it adds them - so the fix is applied as late as possible: drop them from the compiler''s')
[void]$sb.AppendLine('       own reference lists, immediately before it runs. Nothing else changes; they are still')
[void]$sb.AppendLine('       copied to the output folder, which is what the <Content> entries are for.')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('       Only this project is touched, and only these four file names. -->')
[void]$sb.AppendLine('  <Target Name="UwDropNativeReferences" BeforeTargets="CoreCompile"')
[void]$sb.AppendLine('          Condition="''$(MSBuildProjectName)'' == ''UnclaimedWorld''">')
[void]$sb.AppendLine('    <ItemGroup>')
[void]$sb.AppendLine('      <ReferencePath Remove="$(MSBuildProjectDirectory)\CSteamworks.dll" />')
[void]$sb.AppendLine('      <ReferencePath Remove="$(MSBuildProjectDirectory)\CSteamworks64.dll" />')
[void]$sb.AppendLine('      <ReferencePath Remove="$(MSBuildProjectDirectory)\steam_api.dll" />')
[void]$sb.AppendLine('      <ReferencePath Remove="$(MSBuildProjectDirectory)\steam_api64.dll" />')
[void]$sb.AppendLine('      <ReferencePathWithRefAssemblies Remove="$(MSBuildProjectDirectory)\CSteamworks.dll" />')
[void]$sb.AppendLine('      <ReferencePathWithRefAssemblies Remove="$(MSBuildProjectDirectory)\CSteamworks64.dll" />')
[void]$sb.AppendLine('      <ReferencePathWithRefAssemblies Remove="$(MSBuildProjectDirectory)\steam_api.dll" />')
[void]$sb.AppendLine('      <ReferencePathWithRefAssemblies Remove="$(MSBuildProjectDirectory)\steam_api64.dll" />')
[void]$sb.AppendLine('    </ItemGroup>')
[void]$sb.AppendLine('    <Message Importance="high" Text="UW: compiler now has @(ReferencePathWithRefAssemblies->Count()) reference(s)." />')
[void]$sb.AppendLine('  </Target>')
[void]$sb.AppendLine('</Project>')
$sb.ToString() | Set-Content -LiteralPath $injected -Encoding utf8
Write-Ok "$injected"

# ------------------------------------------------------------------------------------ 3. build

if ($Clean) {
    # ONLY <project folder>\bin and <project folder>\obj, for the projects the solution lists.
    #
    # Deliberately not a recursive sweep for directories named bin or obj: ThirdParty carries
    # prebuilt dependencies in folders of exactly those names, and the working trees under it are
    # not all registered submodules, so anything deleted there cannot be restored with git. An
    # earlier version of this script used
    #     Get-ChildItem -LiteralPath $root -Recurse -Directory -Include bin,obj
    # where -Include silently matches nothing useful against -LiteralPath and the pipeline
    # delivered every directory instead. It emptied both checkouts. Enumerate, then delete.
    Write-Step 'removing bin\ and obj\ for the solution''s own projects'
    $removed = 0
    foreach ($projectDir in ($projectFiles | ForEach-Object { Split-Path -Parent $_ } | Sort-Object -Unique)) {
        foreach ($leaf in @('bin', 'obj')) {
            $target = Join-Path $projectDir $leaf
            if ((Test-Path -LiteralPath $target) -and (Split-Path -Leaf $target) -in @('bin', 'obj')) {
                Remove-Item -LiteralPath $target -Recurse -Force -ErrorAction SilentlyContinue
                $removed++
            }
        }
    }
    Write-Ok "$removed folder(s) removed"
}

Write-Step "building $Configuration"
$log = Join-Path $WorkDir 'build.log'

# -m:1 because the projects write into shared output folders and a parallel build races itself.
& $dotnet msbuild $solution `
    "-p:Configuration=$Configuration" `
    "-p:FrameworkPathOverride=$refRoot" `
    "-p:CustomAfterMicrosoftCommonTargets=$injected" `
    -v:m -nologo -m:1 2>&1 | Tee-Object -FilePath $log | Out-Null
$buildExit = $LASTEXITCODE

# ----------------------------------------------------------------------------------- 4. report

$lines    = Get-Content -LiteralPath $log
$errors   = @($lines | Where-Object { $_ -match ': error ' })
$outputs  = @($lines | Where-Object { $_ -match '^\s*[A-Za-z0-9._]+ -> .+\.(dll|exe)$' } |
                       ForEach-Object { $_.Trim() } | Sort-Object -Unique)

Write-Host ''
Write-Step 'assemblies produced'
foreach ($o in $outputs) {
    $name = ($o -split ' -> ')[0]
    Write-Ok $name
}

if ($errors.Count -gt 0) {
    Write-Host ''
    Write-Step "$($errors.Count) error line(s) - first 20, deduplicated"
    foreach ($e in ($errors | ForEach-Object { $_ -replace '\s*\[[A-Z]:\\.*$', '' } | Sort-Object -Unique | Select-Object -First 20)) {
        Write-Fail $e.Trim()
    }
}

Write-Host ''
Write-Note "full log: $log"

if ($buildExit -eq 0) {
    Write-Host "ORIGINAL SOURCE BUILDS - $($outputs.Count) assemblies, $Configuration." -ForegroundColor Green
    exit 0
}

Write-Host "BUILD FAILED - $($errors.Count) error line(s), $($outputs.Count) assemblies produced." -ForegroundColor Red
exit 1
