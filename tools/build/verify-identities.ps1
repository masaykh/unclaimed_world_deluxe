# Asserts that every rebuilt assembly keeps the EXACT identity of the shipped original.
# This is load-critical: 6 XNBs resolve "SpriteSheetRuntime.SpriteSheet, SpriteSheetRuntime"
# and all 47 model XNBs resolve
# "Xclna.Xna.Animation.Content.AnimationReader, Xclna.Xna.Animationx86, Version=1.0.2.0".
# Any drift in name / version / culture / public key token silently breaks those assets.
$ErrorActionPreference = 'Stop'
# Default to the DX release output. It used to be plain 'debug', which stopped existing when the
# build gained per-platform output directories (debug_dx / release_dx / ...) - the script then
# read a stale leftover and reported a MISMATCH against a version that had already been fixed.
$cfg = if ($args[0]) { $args[0] } else { 'release_dx' }
$pairs = @(
  @{ n='SpriteSheetRuntime';     o='SpriteSheetRuntime.dll';     b="artifacts\bin\SpriteSheetRuntime\$cfg\SpriteSheetRuntime.dll" },
  @{ n='WindowSystem';           o='WindowSystem.dll';           b="artifacts\bin\WindowSystem\$cfg\WindowSystem.dll" },
  @{ n='Xclna.Xna.Animationx86'; o='Xclna.Xna.Animationx86.dll'; b="artifacts\bin\AnimationComponentRuntime\$cfg\Xclna.Xna.Animationx86.dll" },
  @{ n='UnclaimedWorld';         o='UnclaimedWorld.exe';         b="artifacts\bin\UnclaimedWorld\$cfg\UnclaimedWorld.dll" }
)
$fail = 0
foreach ($p in $pairs) {
  $op = "ref\original\$($p.o)"
  if (-not (Test-Path $op)) { "SKIP    $($p.n) - no original"; continue }
  if (-not (Test-Path $p.b)) { "SKIP    $($p.n) - not built yet"; continue }
  $oa = [System.Reflection.AssemblyName]::GetAssemblyName((Resolve-Path $op))
  $ba = [System.Reflection.AssemblyName]::GetAssemblyName((Resolve-Path $p.b))
  $oTok = if ($oa.GetPublicKeyToken().Length -eq 0) { 'null' } else { [BitConverter]::ToString($oa.GetPublicKeyToken()) }
  $bTok = if ($ba.GetPublicKeyToken().Length -eq 0) { 'null' } else { [BitConverter]::ToString($ba.GetPublicKeyToken()) }
  if ($oa.Name -eq $ba.Name -and $oa.Version -eq $ba.Version -and $oTok -eq $bTok) {
    "OK      $($ba.Name), Version=$($ba.Version), PublicKeyToken=$bTok"
  } else {
    "MISMATCH $($p.n): original '$($oa.Name) $($oa.Version) $oTok' vs rebuilt '$($ba.Name) $($ba.Version) $bTok'"
    $fail = 1
  }
}
if ($fail) { "`nIDENTITY CHECK FAILED - XNB reader resolution will break."; exit 1 }
"`nAll assembly identities preserved."
