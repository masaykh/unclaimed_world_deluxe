@echo off
setlocal EnableDelayedExpansion
rem ============================================================================================
rem  Sets up a SELF-CONTAINED Unclaimed World port installation, without modifying your game.
rem
rem    setup.cmd
rem    setup.cmd "D:\Games\Unclaimed World"
rem
rem  Same job as setup.ps1, in plain cmd.exe, for machines where PowerShell is disabled by
rem  policy, restricted by an execution policy nobody can change, or blocked by security
rem  software. It uses only what ships with Windows: robocopy, findstr, for /f.
rem
rem  It finds your installed game, copies Content\ and data\ out of it into THIS folder, and
rem  stops. Your game folder is only ever READ - nothing in it is written, replaced or deleted,
rem  so Steam's "Verify integrity of game files" stays clean and the retail game keeps working.
rem  To uninstall the port, delete this folder.
rem
rem  data\ is copied rather than linked because --export-data writes into data\BaseData\, and a
rem  link would put that in your real game folder - the thing this script exists to avoid.
rem ============================================================================================

set "HERE=%~dp0"
if "%HERE:~-1%"=="\" set "HERE=%HERE:~0,-1%"

echo.
echo === Unclaimed World port setup ===============================================
echo.

rem ---- 1. this folder must be the extracted port ------------------------------------------
if not exist "%HERE%\UnclaimedWorld.exe" goto :notport
if not exist "%HERE%\port-content"      goto :notport

rem ---- 2. locate the original game --------------------------------------------------------
set "GAME="
if not "%~1"=="" (
    set "GAME=%~1"
    if not exist "!GAME!\Content" (
        echo REFUSING: "!GAME!" has no Content folder.
        goto :fail
    )
    goto :gotgame
)

rem Default Steam locations first.
call :trygame "%ProgramFiles(x86)%\Steam\steamapps\common\Unclaimed World"
if defined GAME goto :gotgame
call :trygame "%ProgramFiles%\Steam\steamapps\common\Unclaimed World"
if defined GAME goto :gotgame

rem Then any extra Steam library folders. Lines in libraryfolders.vdf look like
rem      "path"        "D:\\SteamLibrary"
rem so splitting on the quote character puts the path in token 4, and the doubled backslashes
rem have to be collapsed.
set "VDF=%ProgramFiles(x86)%\Steam\steamapps\libraryfolders.vdf"
if exist "%VDF%" (
    for /f "usebackq tokens=4 delims=^"" %%A in (`findstr /i /c:"\"path\"" "%VDF%"`) do (
        set "LIB=%%A"
        set "LIB=!LIB:\\=\!"
        if not defined GAME call :trygame "!LIB!\steamapps\common\Unclaimed World"
    )
)
if defined GAME goto :gotgame

echo Could not find your Unclaimed World installation.
echo.
echo Pass it on the command line:
echo     setup.cmd "C:\Path\To\Unclaimed World"
goto :fail

:gotgame
rem Normalise and compare - setting up on top of the game itself is the one thing this must not
rem do, since the whole point is leaving it alone.
for %%I in ("%GAME%") do set "GAME=%%~fI"
if /i "%GAME%"=="%HERE%" (
    echo REFUSING: this folder IS the game installation.
    echo setup.cmd builds a SEPARATE self-contained copy and must be run from its own folder.
    goto :fail
)
if not exist "%GAME%\data" (
    echo REFUSING: "%GAME%" has no data folder - is this a complete installation?
    goto :fail
)

echo ==^> original game: %GAME%
echo ==^> setting up in: %HERE%

rem ---- 3. game version --------------------------------------------------------------------
rem The port is compiled from ONE game version's code and its content is version-specific. A
rem mismatch shows up as a bare cast exception at content load - "Unable to cast object of type
rem 'SpriteSheet' to type 'LightSourceSpriteSheet'" was a real report, caused by game 1.0.4.8
rem deleting that class. Nothing in that message says "wrong game version".
rem
rem Reading a file version without PowerShell is awkward: wmic could do it but is deprecated and
rem absent from current Windows 11. So this compares what it can and says so when it cannot,
rem rather than pretending to have checked.
set "EXPECTED="
if exist "%HERE%\game-version.txt" (
    for /f "usebackq tokens=* delims=" %%V in ("%HERE%\game-version.txt") do (
        set "LINE=%%V"
        if not "!LINE!"=="" if not "!LINE:~0,1!"=="#" if not defined EXPECTED set "EXPECTED=!LINE!"
    )
)
if defined EXPECTED (
    echo ==^> this port is built for game version !EXPECTED!
    echo     ^(cmd cannot read the exe's version; if setup finishes but the game fails at
    echo      content load with a cast exception, your game is a different version^)
)

rem ---- 4. refuse to clobber silently ------------------------------------------------------
if exist "%HERE%\Content" (
    echo.
    echo Content\ already exists here. Delete it first, or re-run after removing it.
    goto :fail
)
if exist "%HERE%\data" (
    echo.
    echo data\ already exists here. Delete it first, or re-run after removing it.
    goto :fail
)

rem ---- 5. copy ----------------------------------------------------------------------------
rem robocopy rather than xcopy: it handles long paths, reports a usable exit code, and does not
rem prompt. Exit codes 0-7 are success (8 and above are real failures), which is why the check
rem below is "geq 8" and not "neq 0".
echo ==^> Content\  ^(copying 317 MB, this takes a minute^)
robocopy "%GAME%\Content" "%HERE%\Content" /E /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 (
    echo COPY FAILED for Content\ - robocopy exit code %errorlevel%.
    goto :fail
)

echo ==^> data\  ^(copying 102 MB - always a copy, it is written to^)
robocopy "%GAME%\data" "%HERE%\data" /E /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 (
    echo COPY FAILED for data\ - robocopy exit code %errorlevel%.
    goto :fail
)

rem Needed for Steamworks to attach. Without it achievements are unavailable; the game logs it
rem and carries on (PORT DEVIATION 4), but there is no reason to lose them.
if exist "%GAME%\steam_appid.txt" (
    copy /y "%GAME%\steam_appid.txt" "%HERE%\steam_appid.txt" >nul
    echo ==^> steam_appid.txt
)

rem ---- 6. verify --------------------------------------------------------------------------
echo ==^> verifying
set "BAD="
if not exist "%HERE%\Content\multiTex.xnb"      set "BAD=Content is incomplete"
if not exist "%HERE%\data"                      set "BAD=data is missing"
if not exist "%HERE%\port-content\multiTex.xnb" set "BAD=port-content is missing its effects"
if defined BAD (
    echo SETUP INCOMPLETE: !BAD!
    goto :fail
)

rem The shaders the game loads live in port-content\; Content\ keeps its original MGFX v8 copies
rem and that is correct - see PORT DEVIATION 18 in PORTING-NOTES.md.
rem /r, because two of the effects live in port-content\GUI\ - a top-level-only glob reported 17
rem of 19 and looked like a truncated copy.
set /a FX=0
for /r "%HERE%\port-content" %%F in (*.xnb) do set /a FX+=1
echo     Content\ and data\ copied, !FX! converted effect^(s^) in port-content\

rem ---- 7. actually load some content ------------------------------------------------------
rem This is the real version check, and it is better than comparing version strings: it loads
rem representative assets through the game's own content pipeline, including the effects via the
rem override. It catches a version mismatch (the cast exception mentioned above), a truncated
rem copy, and content belonging to the wrong build - all as one pass/fail.
if exist "%HERE%\tools\contentprobe.exe" (
    echo ==^> checking the content actually loads
    "%HERE%\tools\contentprobe.exe" "%HERE%\Content" --override "%HERE%\port-content" >"%TEMP%\uwprobe.txt" 2>&1
    if errorlevel 1 (
        echo.
        echo CONTENT CHECK FAILED - the game is unlikely to start. Details:
        type "%TEMP%\uwprobe.txt"
        echo.
        echo The most common cause is a game version other than !EXPECTED!.
        del "%TEMP%\uwprobe.txt" >nul 2>&1
        goto :fail
    )
    for /f "usebackq tokens=*" %%L in (`findstr /c:"loaded," "%TEMP%\uwprobe.txt"`) do echo     %%L
    del "%TEMP%\uwprobe.txt" >nul 2>&1
)

echo.
echo Done. Your game folder was not modified.
echo Run the port with:  %HERE%\UnclaimedWorld.exe
echo.
echo To remove it: delete this folder.
echo.
endlocal
exit /b 0

rem --------------------------------------------------------------------------------------------
:trygame
rem Sets GAME when %1 looks like a complete installation.
if exist "%~1\Content" if exist "%~1\data" set "GAME=%~1"
exit /b 0

:notport
echo REFUSING: this does not look like the extracted port archive.
echo Expected UnclaimedWorld.exe and port-content\ next to this script.
echo Run setup.cmd from the folder you unzipped, not from an empty directory.
goto :fail

:fail
echo.
echo Setup did not complete. Your game folder was not modified.
echo.
endlocal
exit /b 1
