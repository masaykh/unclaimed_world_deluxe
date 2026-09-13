# Launch the deployed game and report what happened.
#
# Why this is not just "run the exe": the game's own crash handler
# (UnclaimedWorld.HandleExceptionInReleaseMode) shows a MODAL MessageBox and only writes
# Errors.txt after that box is dismissed. So an unattended run looks like a silent hang with
# no log at all. This script reads the dialog's text straight out of the live window, which
# turns every startup failure into a readable stack trace without anyone clicking OK.
#
# Usage:  powershell -ExecutionPolicy Bypass -File build\50-run.ps1 [seconds]
#         (default 30; the process is killed at the end unless -Keep is passed)
param(
    [int]$Seconds = 30,
    [switch]$Keep,
    # Kill an already-running UnclaimedWorld before launching. OFF by default: this script is
    # used while someone may be playing, and silently force-killing their session (losing
    # unsaved progress) is not an acceptable side effect of a diagnostic run.
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$game = Join-Path $PSScriptRoot '..\game' | Resolve-Path
$exe = Join-Path $game 'UnclaimedWorld.exe'
$errorsTxt = Join-Path $game 'Errors.txt'
$scratch = [System.IO.Path]::GetTempPath()
$stdout = Join-Path $scratch 'uw-run.out'
$stderr = Join-Path $scratch 'uw-run.err'

if (-not (Test-Path $exe)) { throw "Not deployed: $exe missing. Run build/40-deploy.sh first." }

Add-Type @'
using System; using System.Text; using System.Collections.Generic; using System.Runtime.InteropServices;
public class UwWin {
  public delegate bool EnumProc(IntPtr h, IntPtr p);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr p);
  [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr h, EnumProc cb, IntPtr p);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowTextW(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetClassNameW(IntPtr h, StringBuilder s, int n);

  // Returns the text of any dialog (#32770) owned by the process - i.e. the game's fatal-error box.
  public static List<string> Dialogs(uint target) {
    var found = new List<string>();
    EnumWindows((h, p) => {
      uint pid; GetWindowThreadProcessId(h, out pid);
      if (pid != target) return true;
      var cls = new StringBuilder(256); GetClassNameW(h, cls, 256);
      if (cls.ToString() != "#32770") return true;
      var title = new StringBuilder(1024); GetWindowTextW(h, title, 1024);
      var sb = new StringBuilder();
      sb.AppendLine("TITLE: " + title);
      EnumChildWindows(h, (c, q) => {
        var ccls = new StringBuilder(256); GetClassNameW(c, ccls, 256);
        if (ccls.ToString() != "Static") return true;
        var ctxt = new StringBuilder(16384); GetWindowTextW(c, ctxt, 16384);
        if (ctxt.Length > 0) sb.AppendLine(ctxt.ToString());
        return true;
      }, IntPtr.Zero);
      found.Add(sb.ToString());
      return true;
    }, IntPtr.Zero);
    return found;
  }
}
'@

$existing = @(Get-Process UnclaimedWorld -ErrorAction SilentlyContinue)
if ($existing.Count) {
    if (-not $Force) {
        Write-Host "UnclaimedWorld is already running (pid $($existing.Id -join ', '))." -ForegroundColor Yellow
        Write-Host 'Refusing to kill it - someone may be playing, with unsaved progress.'
        Write-Host 'Close the game first, or re-run with -Force to terminate it.'
        exit 2
    }
    Write-Host "==> -Force: terminating existing instance (pid $($existing.Id -join ', '))"
    $existing | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}
Remove-Item $errorsTxt -ErrorAction SilentlyContinue

Write-Host "==> launching $exe"
$p = Start-Process $exe -WorkingDirectory $game -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru

$dialogs = @()
$elapsed = 0
while ($elapsed -lt $Seconds) {
    Start-Sleep -Seconds 3
    $elapsed += 3
    $p.Refresh()
    if ($p.HasExited) {
        Write-Host ("t={0,-4}s EXITED code={1} (0x{2:X8})" -f $elapsed, $p.ExitCode, [uint32]$p.ExitCode)
        break
    }
    $dialogs = [UwWin]::Dialogs([uint32]$p.Id)
    $state = if ($dialogs.Count) { 'FATAL DIALOG OPEN' } else { "window='$($p.MainWindowTitle)'" }
    Write-Host ("t={0,-4}s ws={1,6:N0} MB threads={2,-3} {3}" -f $elapsed, ($p.WorkingSet64 / 1MB), $p.Threads.Count, $state)
    if ($dialogs.Count) { break }
}

if ($dialogs.Count) {
    Write-Host ''
    Write-Host '=== FATAL ERROR DIALOG ===' -ForegroundColor Red
    $dialogs | ForEach-Object { $_ }
}

foreach ($pair in @(@{n = 'stdout'; f = $stdout }, @{n = 'stderr'; f = $stderr })) {
    if ((Test-Path $pair.f) -and (Get-Item $pair.f).Length -gt 0) {
        Write-Host ''
        Write-Host "=== $($pair.n) ==="
        Get-Content $pair.f | Select-Object -First 40
    }
}

Write-Host ''
Write-Host '=== Errors.txt ==='
if (Test-Path $errorsTxt) { Get-Content $errorsTxt } else { Write-Host '(none written)' }

# Whether the game was still alive at the end. Captured BEFORE we kill it, because Kill() sets
# an exit code of its own and an earlier version of this script then reported a perfectly
# healthy timed-out run as a failure.
$survived = -not $p.HasExited
$exitCode = if ($survived) { 0 } else { $p.ExitCode }

if ($survived) {
    if ($Keep) {
        Write-Host ''
        Write-Host "==> still running as pid $($p.Id) (-Keep). Stop it with: Stop-Process -Id $($p.Id)"
    }
    else {
        $p.Kill()
        Write-Host ''
        Write-Host "==> still running after ${Seconds}s (which is the good outcome); killed"
    }
}

# Non-zero if the run produced a fatal dialog, or if the game exited on its own with a bad
# code. Reaching the timeout still running is success: that is what "it launched and stayed up"
# looks like.
if ($dialogs.Count) { exit 1 }
if ($null -ne $exitCode -and $exitCode -ne 0) { exit 1 }
exit 0
