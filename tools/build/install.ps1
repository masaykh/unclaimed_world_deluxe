# Installs the Unclaimed World .NET 8 / MonoGame 3.8.5.1 port over an existing game install.
#
#   powershell -ExecutionPolicy Bypass -File install.ps1
#   powershell -ExecutionPolicy Bypass -File install.ps1 -Target "D:\Games\Unclaimed World"
#   powershell -ExecutionPolicy Bypass -File install.ps1 -WhatIf
#
# Safe to run more than once, and safe to run from inside the game folder (which is what
# happens if you extract the archive there).
#
# What it does, in order:
#   1. works out which installation to target - the folder it is running from, if that is one
#   2. checks the payload is complete BEFORE changing anything
#   3. backs up every file it will replace or remove, into _original-backup\
#   4. installs the port's binaries
#   5. removes files the port supersedes
#   6. confirms the converted effects landed in port-content (Content is never modified)
#   7. verifies the result
#
# Step 6 is not optional: the shipped effects are MonoGame 3.6's MGFX v8 and MonoGame 3.8
# refuses them. Only the container framing is rewritten - the compiled shader bytecode is
# copied through untouched, so the shaders stay bit-identical to what shipped.
#
# To undo: restore from _original-backup\, or use Steam's
# "Verify integrity of game files", which re-downloads anything changed.
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Target,
    # Check an existing installation and change nothing. Use this when reporting a problem:
    # a mixed install (some files from the port, some still original) is the most common cause
    # of odd content-loading errors, and this is what detects it.
    [switch]$VerifyOnly,
    # Install even when the game's version does not match the one this archive was built for.
    # The mismatch is reported either way; this only stops it being fatal.
    [switch]$SkipVersionCheck
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
    $vdf = "${env:ProgramFiles(x86)}\Steam\steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        foreach ($m in [regex]::Matches((Get-Content $vdf -Raw), '"path"\s*"([^"]+)"')) {
            $guesses += (Join-Path ($m.Groups[1].Value -replace '\\\\', '\') 'steamapps\common\Unclaimed World')
        }
    }
    foreach ($g in $guesses) { if (Test-GameDir $g) { return (Resolve-Path $g).Path } }
    return $null
}

# ---- 1. target selection ------------------------------------------------------------------
# Running from inside a game folder means THAT is the installation to patch. Searching Steam
# in that situation is how an earlier version of this script patched a completely different
# copy of the game than the one the user was standing in.
$inPlace = $false
if ($Target) {
    $Target = (Resolve-Path $Target).Path
    if ((Test-GameDir $here) -and ($here -ne $Target)) {
        Write-Host "REFUSING: this script is sitting inside a game installation" -ForegroundColor Red
        Write-Host "  here:   $here"
        Write-Host "  target: $Target"
        Write-Host 'Those are different installations. Move the archive out of the game folder,'
        Write-Host 'or drop -Target to patch the one you are in.'
        exit 2
    }
} elseif (Test-GameDir $here) {
    $Target = $here
    $inPlace = $true
    Write-Host '==> running from inside the game folder; patching this installation'
} else {
    $Target = Find-GameDir
}

if (-not $Target) {
    Write-Host 'Could not find the game. Pass it explicitly:' -ForegroundColor Yellow
    Write-Host '  install.ps1 -Target "C:\Path\To\Unclaimed World"'
    exit 2
}
if (-not (Test-Path (Join-Path $Target 'UnclaimedWorld.exe'))) {
    throw "$Target does not look like an Unclaimed World installation (no UnclaimedWorld.exe)."
}
if (-not (Test-GameDir $Target)) {
    throw "$Target has no Content and data folders - is this a complete installation?"
}
if ($here -eq $Target) { $inPlace = $true }

Write-Host "==> target: $Target"

# ---- 1b. game version --------------------------------------------------------------------
# The port is compiled from ONE game version's code and its content is version-specific. Run it
# against a different build and it fails at content load with a bare cast exception - a real
# report was "Unable to cast object of type 'SpriteSheet' to type 'LightSourceSpriteSheet'",
# which came from 1.0.4.8 having deleted that class. Nothing in the message says "wrong game
# version", so it is checked here instead, before anything is modified.
#
# The check is skipped when installing in place over an already-installed port, because by then
# UnclaimedWorld.exe is OURS and reports the version we built - not the game's.
$expectedVersionFile = Join-Path $here 'game-version.txt'
if ((Test-Path $expectedVersionFile) -and -not $inPlace) {
    $expected = (Get-Content $expectedVersionFile | Where-Object { $_ -and -not $_.StartsWith('#') } | Select-Object -First 1).Trim()
    $actual = (Get-Item (Join-Path $Target 'UnclaimedWorld.exe')).VersionInfo.FileVersion
    if ($actual -and $expected -and $actual -ne $expected) {
        Write-Host ''
        Write-Host "GAME VERSION MISMATCH" -ForegroundColor Red
        Write-Host "    this archive is built for game version : $expected"
        Write-Host "    the installation in $Target is          : $actual"
        Write-Host ''
        Write-Host 'Installing anyway will very likely fail at content load. Either switch the'
        Write-Host 'game to the matching version in Steam (right-click the game > Properties >'
        Write-Host 'Betas), or get the port archive built for your version.'
        Write-Host ''
        Write-Host 'To override anyway, re-run with -SkipVersionCheck.'
        if (-not $SkipVersionCheck) { exit 1 }
        Write-Host 'Continuing because -SkipVersionCheck was given.' -ForegroundColor Yellow
    }
    elseif ($actual) {
        Write-Host "==> game version: $actual (matches this archive)"
    }
}

# ---- 2. verify the payload before touching anything ---------------------------------------
# An explicit manifest, rather than "every file next to the script" - otherwise running from
# inside the game folder treats the game's own files as payload and copies them onto
# themselves.
$manifestPath = Join-Path $here 'payload.txt'
if (-not (Test-Path $manifestPath)) { throw "missing payload.txt - incomplete archive?" }
$payload = Get-Content $manifestPath | Where-Object { $_ -and -not $_.StartsWith('#') }

$missing = @($payload | Where-Object { -not (Test-Path (Join-Path $here $_)) })
if ($missing.Count) {
    Write-Host 'REFUSING: the archive is incomplete. Missing:' -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "    $_" }
    exit 2
}

# Compares each payload file in the target against this archive's copy, by hash. A MIXED
# install - some files replaced by the port, others still the originals - is the most common
# cause of confusing content-loading errors, because the original support assemblies were
# built against the studio's MonoGame 3.6 fork and cannot interoperate with 3.8.5.1. It looks
# like a content bug and is not one.
function Compare-Payload {
    $result = [ordered]@{ Same = @(); Different = @(); Absent = @() }
    foreach ($rel in $payload) {
        $src = Join-Path $here $rel
        $dest = Join-Path $Target $rel
        if (-not (Test-Path $dest)) { $result.Absent += $rel; continue }
        if (Test-Path $dest -PathType Container) { $result.Same += $rel; continue }
        $a = (Get-FileHash $src -Algorithm SHA256).Hash
        $b = (Get-FileHash $dest -Algorithm SHA256).Hash
        if ($a -eq $b) { $result.Same += $rel } else { $result.Different += $rel }
    }
    return $result
}

# Still shipped, and still used - but only to INSPECT now, never to rewrite the game's Content\.
# See PORT DEVIATION 18: the converted effects arrive in port-content\ as part of the payload.
$tool = Join-Path $here 'tools\mgfxtranscode.exe'
if (-not (Test-Path $tool)) { throw "missing tools\mgfxtranscode.exe - incomplete archive?" }

function Show-Effects {
    # Reports on port-content\ when it is there, because that is what the game actually loads.
    # Content\ is expected to stay at v8 forever now, so reporting on it would look like a fault.
    $dir = Join-Path $Target 'port-content'
    if (-not (Test-Path $dir)) { $dir = Join-Path $Target 'Content' }
    $out = & $tool scan $dir | ForEach-Object { "$_" }
    $out | Where-Object { $_ -match 'effect\(s\) found' } | ForEach-Object { "    $_" }
}

if ($VerifyOnly) {
    Write-Host '==> verify only; nothing will be changed'
    $cmp = Compare-Payload
    Write-Host "    $($cmp.Same.Count) file(s) match this archive"
    if ($cmp.Different.Count) {
        Write-Host "    $($cmp.Different.Count) file(s) DIFFER - this is a mixed install:" -ForegroundColor Red
        $cmp.Different | ForEach-Object { Write-Host "        $_" }
    }
    if ($cmp.Absent.Count) {
        Write-Host "    $($cmp.Absent.Count) file(s) MISSING from the installation:" -ForegroundColor Red
        $cmp.Absent | ForEach-Object { Write-Host "        $_" }
    }
    Write-Host '==> effects'
    Show-Effects
    Write-Host ''
    if ($cmp.Different.Count -or $cmp.Absent.Count) {
        Write-Host 'This installation is INCOMPLETE. Re-run install.ps1 without -VerifyOnly.' -ForegroundColor Yellow
        exit 1
    }
    Write-Host 'Binaries match this archive.' -ForegroundColor Green
    Write-Host 'If the effects line above shows v8 in Content, that is correct and expected:'
    Write-Host 'the converted copies live in port-content and the game prefers them.'
    exit 0
}

# .NET 8 Desktop Runtime check.
#
# `dotnet --list-runtimes` is the authoritative answer, but `dotnet` is not reliably on PATH -
# it is not on the machine this port was built on, and asking there produced a false
# "not detected" warning on a system that had 8.0.26 installed. So fall back to looking for
# the shared framework directory directly before saying anything.
function Test-Net8Desktop {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        $runtimes = & $dotnet.Source --list-runtimes 2>$null
        if ($runtimes | Select-String -Quiet 'Microsoft.WindowsDesktop.App 8\.') { return $true }
    }
    $roots = @($env:DOTNET_ROOT, "$env:ProgramFiles\dotnet", "$env:ProgramW6432\dotnet") |
        Where-Object { $_ } | Select-Object -Unique
    foreach ($r in $roots) {
        $fw = Join-Path $r 'shared\Microsoft.WindowsDesktop.App'
        if (-not (Test-Path $fw)) { continue }
        $v8 = Get-ChildItem $fw -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like '8.*' }
        if ($v8) { return $true }
    }
    return $false
}
if (-not (Test-Net8Desktop)) {
    Write-Host 'WARNING: .NET 8 Desktop Runtime (x64) not detected. The game needs it:' -ForegroundColor Yellow
    Write-Host '         https://dotnet.microsoft.com/download/dotnet/8.0'
}

if (-not $PSCmdlet.ShouldProcess($Target, 'install the port')) {
    Write-Host "(-WhatIf) would install $($payload.Count) payload entries, remove the superseded"
    Write-Host '          files, including the converted effects in port-content. Nothing'
    Write-Host '          was changed, and Content is never modified.'
    exit 0
}

$backup = Join-Path $Target '_original-backup'
New-Item -ItemType Directory -Force -Path $backup | Out-Null

# ---- 3/4. back up and install --------------------------------------------------------------
Write-Host "==> installing $($payload.Count) payload entr$(if ($payload.Count -eq 1) {'y'} else {'ies'})"
$copied = 0; $skipped = 0
foreach ($rel in $payload) {
    $src = Join-Path $here $rel
    $dest = Join-Path $Target $rel

    # In-place: source and destination are the same file. Nothing to do, and Copy-Item would
    # fail with "Cannot overwrite the item with itself".
    if ((Test-Path $dest) -and ((Resolve-Path $src).Path -eq (Resolve-Path $dest).Path)) {
        $skipped++
        continue
    }
    if (Test-Path $dest) {
        $b = Join-Path $backup $rel
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $b) | Out-Null
        if (-not (Test-Path $b)) { Copy-Item $dest $b -Recurse }
    }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Copy-Item $src $dest -Force -Recurse
    $copied++
}
Write-Host "    $copied copied, $skipped already in place"

# ---- 5. superseded files -------------------------------------------------------------------
# vshost*: Visual Studio debug-host leftovers the studio shipped by accident.
# CSteamworks*: retired - current Steamworks.NET calls steam_api64 directly.
# *.exe.config: .NET 8 uses UnclaimedWorld.runtimeconfig.json instead.
# InputEventSystem/RoundLines: merged into WindowSystem.dll.
# steam_api.dll: 32-bit, and this is an x64 build.
# *.xml: 17 MB of IntelliSense docs shipped by accident; dead weight at runtime.
$supersede = @(
    'UnclaimedWorld.vshost.exe', 'UnclaimedWorld.vshost.exe.config',
    'UnclaimedWorld.vshost.exe.manifest', 'UnclaimedWorld.exe.config',
    'UnclaimedWorld.dll.config', 'CSteamworks.dll', 'CSteamworks64.dll',
    'InputEventSystem.dll', 'RoundLines.dll', 'steam_api.dll',
    'MonoGame.Framework.xml'
) + @(Get-ChildItem $Target -Filter 'SharpDX*.xml' -ErrorAction SilentlyContinue | ForEach-Object Name)

foreach ($name in ($supersede | Select-Object -Unique)) {
    if ($payload -contains $name) { continue }   # never delete something we just installed
    $p = Join-Path $Target $name
    if (Test-Path $p) {
        $b = Join-Path $backup $name
        if (-not (Test-Path $b)) { Copy-Item $p $b }
        Remove-Item $p -Force
        Write-Host "    - $name"
    }
}

# ---- 6. effects ----------------------------------------------------------------------------
#
# PORT DEVIATION 18. This step used to rewrite the 19 effects from MGFX v8 to v10 IN PLACE inside
# the player's Content\, backing the originals up first. It no longer touches Content\ at all:
# the converted effects ship in this archive as port-content\, which the game prefers at load
# time, and step 4 has already copied it across as part of the payload.
#
# Why that is better: Content\ stays exactly as the studio shipped it (so Steam's file
# verification has nothing to repair and uninstalling is deleting files, not restoring a backup),
# and there is no conversion step left that can be skipped - which was a real failure, because
# the archive contains an UnclaimedWorld.exe and running it directly bypassed this installer and
# died with MonoGame's bare "This MGFX effect is for an older release of MonoGame".
Write-Host '==> effects'
$overrideDir = Join-Path $Target 'port-content'
if (Test-Path $overrideDir) {
    $n = @(Get-ChildItem $overrideDir -Recurse -Filter *.xnb).Count
    Write-Host "    $n converted effect(s) in port-content\ - Content\ left untouched"
} else {
    Write-Host 'MISSING port-content\ - the game will not load its shaders.' -ForegroundColor Red
    Write-Host 'This archive is incomplete; re-download it.'
    exit 1
}

# ---- 7. verify -----------------------------------------------------------------------------
Write-Host '==> verifying'
$cmp = Compare-Payload
if ($cmp.Different.Count -or $cmp.Absent.Count) {
    Write-Host 'INSTALL INCOMPLETE - these payload files do not match this archive:' -ForegroundColor Red
    ($cmp.Different + $cmp.Absent) | ForEach-Object { Write-Host "    $_" }
    Write-Host "Restore $backup, or use Steam > Verify integrity of game files, then retry."
    exit 1
}
Write-Host "    $($cmp.Same.Count) binaries match this archive"
# Validate the OVERRIDE folder, which is what the game loads - not Content\, which is expected
# to remain the shipped v8 and is deliberately never rewritten (PORT DEVIATION 18).
#
# skinFX_0.xnb is expected to fail on older game builds: MGFX v7, already unloadable by the
# retail build, referenced by no model or code. Filter it so a good install does not look broken.
$verify = & $tool validate (Join-Path $Target 'port-content') | ForEach-Object { "$_" }
$stillV8 = @($verify | Where-Object { $_ -match 'Expected MGFX v10' -and $_ -notmatch 'skinFX_0' })
$verify | Where-Object { $_ -match 'valid MGFX v10' } | ForEach-Object { "    $_" }
if ($stillV8.Count) {
    Write-Host 'Some effects in port-content\ are not v10:' -ForegroundColor Red
    $stillV8 | ForEach-Object { Write-Host "    $_" }
    exit 1
}

Write-Host ''
Write-Host 'Done.' -ForegroundColor Green
Write-Host "Originals backed up to: $backup"
Write-Host 'To revert: restore that folder, or use Steam > Verify integrity of game files.'
Write-Host ''
# Only say this when the file is actually there. Game 1.0.4.8 deleted skinFX_0.xnb - the studio
# dropped the same orphan we identified - so on that version the note would send the reader
# looking for a file that does not exist.
if (Test-Path (Join-Path $Target 'Content\skinFX_0.xnb')) {
    Write-Host 'Note: skinFX_0.xnb is reported invalid and that is correct - it is MGFX v7, was'
    Write-Host 'already unloadable by the retail build, and no model or code references it.'
}
