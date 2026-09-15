@echo off
setlocal enabledelayedexpansion
rem  pushd, not `cd /d "%~dp0"`. %~dp0 ends in a backslash, so the quoted form becomes
rem  cd /d "C:\...\GLProbe\" - and cmd reads that trailing \" as an escaped quote, leaves the
rem  working directory where it was, and then cannot find contentprobe.exe. Every call below is
rem  also anchored to %~dp0 for the same reason.
pushd "%~dp0"

rem  Double-click this. The window stays open at the end, and the same output is written to
rem  contentprobe.log beside it either way.
rem
rem  A bare contentprobe.exe launched by double-click opens a console, prints into it, and closes
rem  it again faster than anyone can read - which is indistinguishable from "produces no output",
rem  and is the first thing to rule out when someone reports that.

set "CONTENT=%~1"

if "%CONTENT%"=="" (
  for %%P in (
    "C:\Program Files (x86)\Steam\steamapps\common\Unclaimed World\Content"
    "C:\Program Files\Steam\steamapps\common\Unclaimed World\Content"
    "D:\Steam\steamapps\common\Unclaimed World\Content"
    "D:\SteamLibrary\steamapps\common\Unclaimed World\Content"
    "E:\SteamLibrary\steamapps\common\Unclaimed World\Content"
  ) do (
    if exist "%%~P" if "!CONTENT!"=="" set "CONTENT=%%~P"
  )
)

if "%CONTENT%"=="" (
  echo.
  echo Could not find the game's Content folder in any of the usual places.
  echo.
  echo Drag your "Unclaimed World\Content" folder onto this .cmd file, or run:
  echo.
  echo     run.cmd "D:\wherever\Unclaimed World\Content"
  echo.
  popd
pause
  exit /b 2
)

echo Using content: %CONTENT%
echo.

"%~dp0contentprobe.exe" "%CONTENT%" --override "%~dp0effects-gl" --all-effects
set RC=%ERRORLEVEL%

echo.
echo ---------------------------------------------------------------
echo exit code %RC%   ^(the number of assets that failed^)
echo.
if %RC%==-1 echo No graphics device could be created at all - see above.
if not exist "%~dp0contentprobe.log" (
  echo contentprobe.log was NOT written. If nothing printed above either, the most
  echo likely cause is that the .NET 8 runtime is missing:
  echo     https://dotnet.microsoft.com/download/dotnet/8.0
)
echo Send contentprobe.log back.
echo.
popd
pause
