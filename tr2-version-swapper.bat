@ECHO OFF
VERIFY ERRORS 2 > NUL
SETLOCAL EnableDelayedExpansion

REM See https://stackoverflow.com/a/36663349/10466817.
REM Note this gets "tricked" by PowerShell, but that's a fringe use case.
IF "%console_mode%" EQU "" (
    SET pause=false
    FOR %%x IN (%cmdcmdline%) DO (
        IF /i "%%~x" EQU "/c" SET pause=true
    )
)

REM See https://stackoverflow.com/a/12264592/10466817.
NET FILE 1>NUL 2>NUL & IF ERRORLEVEL 1 GOTO :InsufficientPermissions

REM Load variables and directories.
CALL :SetVariables %~1
CALL :SetDirectories
IF %verbose% EQU true CALL :PrintDirectories

REM Check that the parent folder has all the game files.
FOR /L %%i IN (1,1,%file_count%) DO (
    SET "required_file=!files[%%i]!"
    IF %debug% EQU true ECHO Looking for !required_file! in "%target%"...
    IF NOT EXIST "%target%!required_file!" GOTO :MisplacedScript
)
IF %debug% EQU true ECHO/

REM Check that all version folders are present.
FOR /L %%i IN (1,1,%version_count%) DO (
    SET "version_folder=!version_names[%%i]!"
    IF %debug% EQU true ECHO Looking for "!version_folder!\" in "%src%versions\"...
    IF NOT EXIST "!folder[%%i]!" GOTO :MissingFolder
)
IF %debug% EQU true ECHO/

REM Ensure the game whose files will be overwritten is not running.
CALL :Tomb2TaskCheck

REM Start the script proper.
CALL :PrintIntroduction
REM Get the user's version choice.
ECHO Version list:
FOR /L %%i IN (1,1,%version_count%) DO (
    ECHO     %%i: !version_names[%%i]!
)
CALL :GetSelectionIndex
SET "selected_version=!version_names[%index%]!"
ECHO You selected: "%selected_version%"
ECHO/

REM Check that the selected version folder has all the game files.
FOR /L %%i IN (1,1,%file_count%) DO (
    SET "required_file=!files[%%i]!"
    IF %verbose% EQU true ECHO Looking for !required_file! in "!folder[%index%]!"
    IF NOT EXIST "!folder[%index%]!\!required_file!" GOTO :MissingFiles
)
IF %verbose% EQU true ECHO/

REM Copy the game files.
XCOPY "!folder[%index%]!" "%target%" /sy || GOTO :CopyError
ECHO/
ECHO === Success ===
ECHO Version successfully swapped to "%selected_version%".
ECHO ===============
ECHO/

REM Copy the patch file if present and desired.
SET "patch=!patch_folder!\!patch_file!"
IF %debug% EQU true ECHO Looking for "%patch%".
IF NOT EXIST "%patch%" (
    REM This is not a terminating GOTO section because the music fix can
    REM still be successfully installed even if the patch cannot.
    ECHO Unfortunately, the patch file was not found and thus it cannot be
    ECHO installed with this script right now. Your other files are fine.
    ECHO/
) ELSE (
    CALL :GetPatchInstallChoice
    IF !patch_choice! EQU 1 (
        XCOPY "%patch%" "!target!" /sy || CALL :PrintCopyErrorMessage
        ECHO/
        ECHO === Success ===
        ECHO Patch successfully installed.
        ECHO ===============
        ECHO/
    ) ELSE (
        ECHO Skipping patch installation...
        ECHO/
    )
)

REM Copy the music files if applicable and desired.
IF NOT "%selected_version%" EQU "Multipatch" (
    CALL :CheckMusicFiles "%target%"
    IF !music_fix_present! NEQ true (
        CALL :PrintMusicInfo
        CALL :CheckMusicFiles "!music_fix!"
        IF !music_fix_present! NEQ true (
            GOTO :MissingMusicFixFiles
        )
        CALL :GetMusicInstallChoice
        IF !music_choice! EQU 1 (
            XCOPY "!music_fix!" "!target!" /sy || GOTO :CopyError
            ECHO/
            ECHO === Success ===
            ECHO Music fix successfully installed. No need to uninstall or
            ECHO modify the relevant files for any future version switch.
            ECHO ===============
            ECHO/
        ) ELSE (
            ECHO Skipping music fix installation...
            ECHO/
        )
    ) ELSE (
        ECHO The music fix appears to be already installed.
        ECHO/
    )
)
ECHO Run this script again anytime you wish to change or patch the version.
CALL :PauseIfNeeded
EXIT /b 0



:InsufficientPermissions
    ECHO You must right-click and select "RUN AS ADMINISTRATOR" or run from
    ECHO a CMD or PS with admin permissions to use this batch file.
    CALL :PauseIfNeeded
    EXIT /b 1

:MissingFolder
    ECHO Could not find a "!version_folder!" version folder with the script.
    GOTO :ReinstallPrompt

:MisplacedScript
    ECHO It appears this script was not placed within a TR2 installation root.
    ECHO This script should be in a folder called `tr2-version-swapper` along
    ECHO with all of the other files in the archive.
    GOTO :ReinstallPrompt

:MissingFiles
    ECHO Could not find "%required_file%" in the "%selected_version%" folder!
    GOTO :ReinstallPrompt

:MissingMusicFixFiles
    ECHO Unfortunately, the music fix files are incomplete and thus cannot be
    ECHO installed with this script right now. Your other files are fine.
    GOTO :ReinstallPrompt

:ReinstallPrompt
    ECHO/
    ECHO You are advised to re-install the latest release to fix the issue:
    ECHO %install_link%
    CALL :PauseIfNeeded
    EXIT /b 2

:CopyError
    CALL :PrintCopyErrorMessage
    CALL :PauseIfNeeded
    EXIT /b 3



REM The named sections above are used as `GOTO`s and ultimately end the script.
REM The named sections below are `CALL`ed and used like functions.



:SetVariables
    IF "%~1" EQU "-v" (
        SET verbose=true
        SET debug=false
    ) ELSE (
        IF "%~1" EQU "-d" (
            SET verbose=true
            SET debug=true
        ) ELSE (
            SET verbose=false
            SET debug=false
        )
    )
    SET git_link=https://github.com/TombRunners/tr2-version-swapper
    SET install_link=https://github.com/TombRunners/tr2-version-swapper/releases/latest
    SET "version_names[1]=Multipatch"
    SET "version_names[2]=Eidos Premier Collection"
    SET "version_names[3]=Eidos UK Box"
    SET version_count=3
    SET files[1]=tomb2.exe
    SET files[2]=data\floating.tr2
    SET files[3]=data\title.pcx
    SET files[4]=data\tombpc.dat
    SET file_count=4
    SET music_files[1]=fmodex.dll
    SET music_files[2]=winmm.dll
    SET music_file_count=2
    SET music_track_count=61
    SET patch_file=tomb2.exe
    EXIT /b 0

:SetDirectories
    REM `%~dp0` returns the batch file's absolute directory instead of working.
    SET "src=%~dp0"
    SET "music_fix=%src%utilities\music_fix"
    SET "patch_folder=%src%utilities\patch"
    REM No backslash is appended to version folders here, requiring manual
    REM placement elsewhere in the script. This is still preferred actually
    REM because having the backslash requires placing a leading dot "." at
    REM the end of the `XCOPY` source parameter, which creates ugly output.
    REM See https://stackoverflow.com/a/25841519/10466817.
    FOR /L %%i IN (1,1,%version_count%) DO (
        SET "folder[%%i]=%src%versions\!version_names[%%i]!"
    )
    CALL :GetParentDir target "%src%" "..\"
    EXIT /b 0

:GetParentDir
    REM See https://stackoverflow.com/a/34948844/10466817.
    FOR %%A IN ("%~2\%~3") DO SET "%~1=%%~fA"
    EXIT /b 0

:Tomb2TaskCheck
    IF %verbose% EQU true ECHO Checking for Tomb2 running in Game folder...
    :TaskKillLoop
    REM Read the output of WMIC through a file per https://www.dostips.com/forum/viewtopic.php?t=4490#p25709.
    REM Use 2>NUL to suppress the "No Instance(s) Available" output when no match is found.
    2>NUL WMIC PROCESS WHERE NAME="!files[1]!" GET ExecutablePath,ProcessID /format:csv > file.tmp
    FOR /F "SKIP=2 TOKENS=1,2 DELIMS=," %%i IN ('TYPE file.tmp') DO (
        IF %debug% EQU true ECHO Found running Tomb2 at "%%j".
        REM Flag /I runs a case-insensitive comparison.
        IF /I "%%j" EQU "%target%!files[1]!" (
            ECHO Tomb Raider II is currently being run from "%target%!files[1]!".
            ECHO Please close this instance of the game, then use any key to continue.
            ECHO Hint: You may need to kill a background task.
            PAUSE
            ECHO/
            DEL file.tmp
            GOTO :TaskKillLoop
        )
    )
    IF %verbose% EQU true ECHO No match against "%target%!files[1]!". && ECHO/
    DEL file.tmp
    EXIT /b 0

:PrintDirectories
    ECHO Using the following directories:
    ECHO =======================
    ECHO Game: "%target%"
    ECHO Script: "%src%"
    ECHO === Utility Folders ===
    ECHO Music fix: "!music_fix!\"
    ECHO Patch: "!patch_folder!\"
    ECHO === Version Folders ===
    FOR /L %%i IN (1,1,%version_count%) DO (
        ECHO "!folder[%%i]!\"
    )
    ECHO =======================
    ECHO/
    EXIT /b 0

:PrintIntroduction
    ECHO ======= WELCOME =======
    ECHO Welcome to the TR2 version swapper script v2.1.0.
    ECHO This script's code can be viewed/edited with a text editor.
    ECHO The official files with source control can be found here:
    ECHO %git_link%
    ECHO ======= CAUTION =======
    ECHO This script assumes you followed INSTALLATION INSTRUCTIONS in HOW-TO-USE.txt.
    ECHO If the script was installed incorrectly, it may refuse to work or it may
    ECHO proceed with unpredictable outcomes and/or errors.
    ECHO =======================
    ECHO/
    EXIT /b 0

:GetSelectionIndex
    REM Naturally, this hacky solution only works if `%version_count%` <= 9.
    SET /p index="Enter the number of your desired version: " < NUL
    :Prompt
    CHOICE /c 0123456789 > NUL
    SET /a "index=%ERRORLEVEL%-1"
    IF %index% GTR %version_count% GOTO :Prompt
    IF %index% LSS 1 GOTO :Prompt
    ECHO %index%
    EXIT /b 0

:PrintCopyErrorMessage
    ECHO `xcopy.exe` failed to complete successfully. Read the error message
    ECHO above to determine the state of your installation.
    EXIT /b 0

:GetPatchInstallChoice
    ECHO (Optional) Install CORE's Patch 1? This is a separate EXE that is not required,
    SET /p patch_choice="but may be used on top of English game versions. [0 = no, 1 = yes]: " < NUL
    :PatchPrompt
    CHOICE /c 0123456789 > NUL
    SET /a "patch_choice=%ERRORLEVEL%-1"
    IF %patch_choice% GTR 1 GOTO :PatchPrompt
    ECHO %patch_choice%
    EXIT /b 0

:PrintMusicInfo
    ECHO You switched to a non-Multipatch version. You may find that music no
    ECHO longer works, or that the game lags when loading music. There is a
    ECHO music fix available which should fix most music issues. You can learn
    ECHO more on the Tomb Runner Discord server or speedrun.com/tr2.
    ECHO/
    EXIT /b 0

:CheckMusicFiles
    REM Check that the folder has all DLLs.
    FOR /L %%i IN (1,1,%music_file_count%) DO (
        IF %debug% EQU true ECHO Looking for !music_files[%%i]! in "%~1"...
        IF NOT EXIST "%~1\!music_files[%%i]!" GOTO :NotTrue
    )

    REM Check that the music folder has all music tracks.
    FOR /L %%i IN (1,1,%music_track_count%) DO (
        REM Add leading zeroes to one-digit numbers.
        IF %%i LEQ 9 (
            SET track=0%%i.wma
        ) ELSE (
            SET track=%%i.wma
        )
        IF %debug% EQU true ECHO Looking for \music\!track! in "%~1"...
        IF NOT EXIST "%~1\music\!track!" GOTO :NotTrue
    )
    SET music_fix_present=true
    GOTO :Exit

    :NotTrue
    IF %verbose% EQU true (
        ECHO Music fix file not found in "%~1".
        ECHO/
    )
    SET music_fix_present=false

    :Exit
    IF %debug% EQU true (
        IF %music_fix_present% EQU true ECHO/
    )
    EXIT /b 0

:GetMusicInstallChoice
    SET /p music_choice="Install the music fix? [0 = no, 1 = yes]: " < NUL
    :MusicPrompt
    CHOICE /c 0123456789 > NUL
    SET /a "music_choice=%ERRORLEVEL%-1"
    IF %music_choice% GTR 1 GOTO :MusicPrompt
    ECHO %music_choice%
    EXIT /b 0

:PauseIfNeeded
    IF %pause% EQU true PAUSE
    EXIT /b 0
