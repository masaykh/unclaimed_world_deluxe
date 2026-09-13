# Launches a build, optionally clicks a point in its window, and captures the window to a PNG.
#
# This is the only validation that has produced ground truth about the DesktopGL port. Loading
# an effect proves nothing about whether it draws, and rendering effects in isolation missed
# that the Save/Load screen renders on WindowsDX and not on DesktopGL at all. Running both
# builds, clicking the same point in each and comparing the images found that in one pass.
#
# Usage:
#   powershell -File build/35-screenshot.ps1 -Dir game -Out dx-menu.png
#   powershell -File build/35-screenshot.ps1 -Dir game -Out dx-load.png -FracX 0.461 -FracY 0.923
#
# Click position is a FRACTION of the window so the same coordinates work for either build at
# any resolution - which is what makes the two captures comparable.
#
# PrintWindow with PW_RENDERFULLCONTENT is used rather than CopyFromScreen so the capture does
# not depend on the window being unobscured, with CopyFromScreen as a fallback.
param(
    [Parameter(Mandatory = $true)][string]$Dir,
    [Parameter(Mandatory = $true)][string]$Out,
    [int]$WaitSeconds = 16,
    [int]$AfterClickSeconds = 8,
    # Click position as a FRACTION of the window, so it works at any resolution.
    [double]$FracX = -1,
    [double]$FracY = -1
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type @'
using System;
using System.Runtime.InteropServices;
public class Shot2 {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, IntPtr e);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
  public const uint LEFTDOWN = 0x0002, LEFTUP = 0x0004;
}
'@

$exe = Join-Path $Dir 'UnclaimedWorld.exe'
Remove-Item (Join-Path $Dir 'Errors.txt') -ErrorAction SilentlyContinue
$so = "$env:TEMP\shot2.out"

$p = Start-Process $exe -WorkingDirectory $Dir -RedirectStandardOutput $so -RedirectStandardError "$env:TEMP\shot2.err" -PassThru
$null = $p.Handle
Start-Sleep -Seconds $WaitSeconds
$p.Refresh()
if ($p.HasExited) { Write-Host "exited early, code $($p.ExitCode)"; Get-Content $so | Select-Object -First 20; exit 1 }

$h = $p.MainWindowHandle
[void][Shot2]::SetForegroundWindow($h)
Start-Sleep -Milliseconds 1500

$r = New-Object Shot2+RECT
[void][Shot2]::GetWindowRect($h, [ref]$r)
$w = $r.Right - $r.Left; $ht = $r.Bottom - $r.Top

if ($FracX -ge 0 -and $FracY -ge 0) {
    $cx = [int]($r.Left + $w * $FracX)
    $cy = [int]($r.Top + $ht * $FracY)
    Write-Host "clicking ($cx,$cy) - fraction ($FracX,$FracY) of $w x $ht"
    [void][Shot2]::SetCursorPos($cx, $cy)
    Start-Sleep -Milliseconds 400
    [Shot2]::mouse_event([Shot2]::LEFTDOWN, 0, 0, 0, [IntPtr]::Zero)
    Start-Sleep -Milliseconds 90
    [Shot2]::mouse_event([Shot2]::LEFTUP, 0, 0, 0, [IntPtr]::Zero)
    Start-Sleep -Seconds $AfterClickSeconds
    $p.Refresh()
    if ($p.HasExited) {
        Write-Host "process exited AFTER the click, code $($p.ExitCode)"
        Get-Content $so | Select-Object -First 20
        $e = Join-Path $Dir 'Errors.txt'
        if (Test-Path $e) { Write-Host '--- Errors.txt ---'; Get-Content $e | Select-Object -First 30 }
        exit 1
    }
}

$bmp = New-Object System.Drawing.Bitmap($w, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
$ok = [Shot2]::PrintWindow($h, $hdc, 0x2)
$g.ReleaseHdc($hdc)
if (-not $ok) { $g.CopyFromScreen($r.Left, $r.Top, 0, 0, $bmp.Size) }
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Host "saved $Out"

$p.Kill()
Write-Host '--- stdout (last 12) ---'
if (Test-Path $so) { Get-Content $so | Select-Object -Last 12 }
Write-Host '--- Errors.txt ---'
$e = Join-Path $Dir 'Errors.txt'
if (Test-Path $e) { Get-Content $e | Select-Object -First 30 } else { Write-Host '(none)' }
