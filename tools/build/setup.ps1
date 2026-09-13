# Sets up a SELF-CONTAINED Unclaimed World port installation, without modifying your game.
#
#   powershell -ExecutionPolicy Bypass -File setup.ps1
#   powershell -ExecutionPolicy Bypass -File setup.ps1 -Game "D:\Games\Unclaimed World"
#   powershell -ExecutionPolicy Bypass -File setup.ps1 -LinkContent    # junction, no 317 MB copy
#   powershell -ExecutionPolicy Bypass -File setup.ps1 -WhatIf
#
# It finds your installed game, copies Content\ and data\ out of it into THIS folder, and stops.
# Your game folder is only ever READ. Nothing in it is written, replaced, deleted or backed up,
# so Steam's "Verify integrity of game files" stays clean and the retail game keeps working
# exactly as before - you choose which one to run by choosing which exe to launch.
#
# This is the alternative to install.ps1, which patches the port over your existing installation
# (replacing UnclaimedWorld.exe, MonoGame.Framework.dll and four other assemblies, and deleting
# the files the port supersedes). Use install.ps1 only if you want the port to BE the game Steam
# launches; use this if you would rather leave the original alone.
#
# WHY data\ IS COPIED BUT Content\ NEED NOT BE
#   Content\ (317 MB) is read-only to the port. Since the converted shaders live in
#   port-content\, nothing writes to Content\ any more, so -LinkContent can point at the
#   original with a directory junction and save the space.
#   data\ (102 MB) IS written: --export-data dumps the game's tables into data\BaseData\. A
#   junction there would write into your real game folder, which is the thing this script exists
#   to avoid, so data\ is always a real copy.
#
# Saves and replays are NOT affected either way: those live in
# Documents\Unclaimed World\, shared with the retail game.
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    # Where the original game is. Found automatically if omitted.
    [string]$Game,
    # Link Content\ with a directory junction instead of copying it. Saves 317 MB and is
    # instant. See the warning printed when it is used.
    [switch]$LinkContent,
    # Set up even when the game's version is not the one this archive was built for.
    [switch]$SkipVersionCheck,
    # Replace Content\ / data\ here if they already exist.
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$here = (Resolve-Path (Split-Path -Parent $MyInvocation.MyCommand.Path)).Path

function Test-GameDir([string]$p) {
    if (-not $p) { return $false }
    return (Test-Path (Join-Path $p 'Content')) -and (Test-Path (Join-Path $p 'data'))
}

function Find-GameDir {
    $guesses = @(
        "${env:ProgramFiles(x86)}\Steam\steamapps\common\Unclaimed World",
        "$env:ProgramFiles\Steam\steamapps\common\Unclaimed World"
    )
    # Extra Steam library folders, which is where most people's games actually are.
    $vdf = "${env:ProgramFiles(x86)}\Steam\steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        foreach ($m in [regex]::Matches((Get-Content $vdf -Raw), '"path"\s*"([^"]+)"')) {
            $guesses += (Join-Path ($m.Groups[1].Value -replace '\\\\', '\') 'steamapps\common\Unclaimed World')
        }
    }
    foreach ($g in $guesses) { if (Test-GameDir $g) { return (Resolve-Path $g).Path } }
    return $null
}

# ---- 1. locate the original ---------------------------------------------------------------
if ($Game) {
    if (-not (Test-Path $Game)) { throw "No such folder: $Game" }
    $Game = (Resolve-Path $Game).Path
} else {
    $Game = Find-GameDir
}
if (-not $Game) {
    Write-Host 'Could not find your Unclaimed World installation. Pass it explicitly:' -ForegroundColor Yellow
    Write-Host '  setup.ps1 -Game "C:\Path\To\Unclaimed World"'
    exit 2
}
if (-not (Test-GameDir $Game)) {
    throw "$Game has no Content and data folders - is this a complete installation?"
}
if ($Game -eq $here) {
    Write-Host 'REFUSING: this folder IS the game installation.' -ForegroundColor Red
    Write-Host 'setup.ps1 builds a SEPARATE self-contained copy and must be run from its own'
    Write-Host 'folder. If you meant to patch the game in place, use install.ps1 instead.'
    exit 2
}

Write-Host "==> original game: $Game"
Write-Host "==> setting up in: $here"

# ---- 2. version check ---------------------------------------------------------------------
# The port is compiled from ONE game version's code and its content is version-specific. A
# mismatch surfaces as a bare cast exception at content load - a real report was "Unable to cast
# object of type 'SpriteSheet' to type 'LightSourceSpriteSheet'", which came from game 1.0.4.8
# having deleted that class. Nothing in that message says "wrong game version".
$expectedFile = Join-Path $here 'game-version.txt'
if (Test-Path $expectedFile) {
    $expected = (Get-Content $expectedFile | Where-Object { $_ -and -not $_.StartsWith('#') } | Select-Object -First 1).Trim()
    $gameExe = Join-Path $Game 'UnclaimedWorld.exe'
    $actual = if (Test-Path $gameExe) { (Get-Item $gameExe).VersionInfo.FileVersion } else { $null }
    if ($actual -and $expected -and $actual -ne $expected) {
        Write-Host ''
        Write-Host 'GAME VERSION MISMATCH' -ForegroundColor Red
        Write-Host "    this port is built for game version : $expected"
        Write-Host "    your installation is                : $actual"
        Write-Host ''
        Write-Host 'The content will very likely fail to load. Either switch the game to the'
        Write-Host 'matching version in Steam (right-click > Properties > Betas), or get the port'
        Write-Host 'built for your version. To proceed anyway, re-run with -SkipVersionCheck.'
        if (-not $SkipVersionCheck) { exit 1 }
        Write-Host 'Continuing because -SkipVersionCheck was given.' -ForegroundColor Yellow
    } elseif ($actual) {
        Write-Host "==> game version: $actual (matches this port)"
    }
}

# ---- 3. sanity-check this folder ----------------------------------------------------------
foreach ($required in @('UnclaimedWorld.exe', 'port-content')) {
    if (-not (Test-Path (Join-Path $here $required))) {
        Write-Host "REFUSING: $required is missing from this folder." -ForegroundColor Red
        Write-Host 'Run setup.ps1 from the extracted port archive, not from an empty directory.'
        exit 2
    }
}

$destContent = Join-Path $here 'Content'
$destData    = Join-Path $here 'data'
foreach ($d in @($destContent, $destData)) {
    if ((Test-Path $d) -and -not $Force) {
        Write-Host "$([System.IO.Path]::GetFileName($d))\ already exists here." -ForegroundColor Yellow
        Write-Host 'Re-run with -Force to replace it, or delete it first.'
        exit 1
    }
}

if (-not $PSCmdlet.ShouldProcess($here, 'copy Content and data from the game')) {
    $mode = if ($LinkContent) { 'junction to' } else { 'copy of' }
    Write-Host "(-WhatIf) would create a $mode $Game\Content and a copy of $Game\data here."
    Write-Host '          Your game folder would not be modified. Nothing was changed.'
    exit 0
}

# ---- 4. Content -----------------------------------------------------------------------------
if (Test-Path $destContent) { Remove-Item $destContent -Recurse -Force }

if ($LinkContent) {
    Write-Host '==> Content\  (directory junction - no copy)'
    # /J is a junction: no administrator rights needed, unlike /D symlinks.
    & cmd /c mklink /J "$destContent" "$(Join-Path $Game 'Content')" | Out-Null
    if (-not (Test-Path $destContent)) {
        Write-Host 'Junction failed; falling back to a copy.' -ForegroundColor Yellow
        Copy-Item (Join-Path $Game 'Content') $destContent -Recurse
    } else {
        Write-Host ''
        Write-Host 'WARNING about -LinkContent:' -ForegroundColor Yellow
        Write-Host "  Content\ here now POINTS AT $Game\Content."
        Write-Host '  Deleting this folder with a tool that follows junctions can therefore delete'
        Write-Host '  your real game content. To remove it safely, run first:'
        Write-Host "      cmd /c rmdir `"$destContent`""
        Write-Host '  (rmdir removes the junction without touching the target.) If that worries'
        Write-Host '  you at all, re-run setup.ps1 without -LinkContent and take the 317 MB copy.'
        Write-Host ''
    }
} else {
    Write-Host '==> Content\  (copying 317 MB, this takes a minute)'
    Copy-Item (Join-Path $Game 'Content') $destContent -Recurse
}

# ---- 5. data --------------------------------------------------------------------------------
# Always a real copy: --export-data writes into data\BaseData\, and a junction would put that in
# the original game folder.
Write-Host '==> data\  (copying 102 MB - always a copy, it is written to)'
if (Test-Path $destData) { Remove-Item $destData -Recurse -Force }
Copy-Item (Join-Path $Game 'data') $destData -Recurse

# ---- 6. steam_appid.txt ---------------------------------------------------------------------
# Needed for Steamworks to attach; achievements are unavailable without it (the game degrades
# gracefully - see PORT DEVIATION 4 - but there is no reason to lose them).
$appid = Join-Path $Game 'steam_appid.txt'
if (Test-Path $appid) {
    Copy-Item $appid (Join-Path $here 'steam_appid.txt') -Force
    Write-Host '==> steam_appid.txt'
}

# ---- 7. verify ------------------------------------------------------------------------------
Write-Host '==> verifying'
$problems = @()
foreach ($required in @('Content', 'data', 'UnclaimedWorld.exe', 'port-content')) {
    if (-not (Test-Path (Join-Path $here $required))) { $problems += "missing $required" }
}
# The shaders the game will actually load live in port-content\; Content\ keeps its original v8
# copies and that is correct (see PORT DEVIATION 18).
$fx = @(Get-ChildItem (Join-Path $here 'port-content') -Recurse -Filter *.xnb -ErrorAction SilentlyContinue)
if ($fx.Count -lt 1) { $problems += 'port-content\ has no effects in it' }

if ($problems.Count) {
    Write-Host 'SETUP INCOMPLETE:' -ForegroundColor Red
    $problems | ForEach-Object { Write-Host "    $_" }
    exit 1
}

Write-Host "    Content\ and data\ present, $($fx.Count) converted effect(s) in port-content\"

# Load representative assets through the game's own content pipeline, effects included via the
# override. This catches more than a version-string comparison does - a version mismatch (the
# cast exception described above), a truncated copy, and content belonging to the wrong build -
# all as one pass/fail, using the machine's real graphics device.
$probe = Join-Path $here 'tools\contentprobe.exe'
if (Test-Path $probe) {
    Write-Host '==> checking the content actually loads'
    $out = & $probe $destContent '--override' (Join-Path $here 'port-content') 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host ''
        Write-Host 'CONTENT CHECK FAILED - the game is unlikely to start. Details:' -ForegroundColor Red
        $out | ForEach-Object { Write-Host "    $_" }
        Write-Host ''
        Write-Host "The most common cause is a game version other than $expected."
        exit 1
    }
    $out | Where-Object { $_ -match 'loaded,' } | ForEach-Object { Write-Host "    $_" }
}
Write-Host ''
Write-Host 'Done. Your game folder was not modified.' -ForegroundColor Green
Write-Host "Run the port with:  $(Join-Path $here 'UnclaimedWorld.exe')"
Write-Host ''
Write-Host 'To remove it: delete this folder.'
if ($LinkContent) {
    Write-Host "             (run  cmd /c rmdir `"$destContent`"  FIRST - see the warning above)" -ForegroundColor Yellow
}
