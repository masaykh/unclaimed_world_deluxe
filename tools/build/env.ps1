# Shell environment for this repo (PowerShell).  . .\build\env.ps1
$env:UW_REPO  = 'C:\tools\unclaimedworld_fxna'
$env:UW_STEAM = 'C:\Program Files (x86)\Steam\steamapps\common\Unclaimed World'
$env:UW_GAME  = 'C:\tools\unclaimedworld_fxna\game'
$env:PATH     = "C:\Program Files\dotnet;C:\Program Files\7-Zip;$env:PATH"
# Never write into $env:UW_STEAM. It is the golden reference; game\ is the run target.
