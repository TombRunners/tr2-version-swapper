@ECHO OFF
VERIFY ERRORS 2 > NUL
SETLOCAL EnableDelayedExpansion

REM See https://stackoverflow.com/a/36663349/10466817
REM Note this gets "tricked" by PowerShell, but that's a fringe use case.
IF "%console_mode%" EQU "" (
    SET pause=false
    FOR %%x IN (%cmdcmdline%) DO (
        IF /i "%%~x" EQU "/c" SET pause=true
    )
)

REM Perform housekeeping and load variables.
CALL :WritePermissionsCheck
CALL :SetVariables %~1
CALL :SetDirectories
IF %verbose% EQU true CALL :PrintDirectories

REM Check that the parent folder has all the game files.
FOR /L %%i IN (1,1,%file_count%) DO (
    SET "required_file=!files[%%i]!"
    IF %debug% EQU true ECHO Looking for !required_file! in "%target%"
    IF NOT EXIST "%target%!required_file!" GOTO :MisplacedScript
)
IF %debug% EQU true ECHO/

REM Check that all version folders are present.
FOR /L %%i IN (1,1,%version_count%) DO (
    SET "version_folder=!version_names[%%i]!"
    IF %debug% EQU true ECHO Looking for "!version_folder!\" in "%src%"
    IF NOT EXIST "!folder[%%i]!" GOTO :MissingFolder
)
IF %verbose% EQU true ECHO/

REM Start the script proper
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

REM Copy the game files
XCOPY "!folder[%index%]!" "%target%" /sy || GOTO :CopyError
ECHO/
ECHO Version successfully swapped to "%selected_version%".

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
            ECHO Music fix successfully installed. No need to uninstall or
            ECHO modify the relevant files for any future version switch.
            ECHO/
        )
    )
)
ECHO Run this script again anytime you wish to change the version.
CALL :PauseIfNeeded
EXIT /b 0



:InsufficientPermissions
    ECHO The script does not have permissions to write files/folders.
    ECHO You can try running this script as admin as a workaround,
    ECHO but most likely the problem is the game install location.
    CALL :PauseIfNeeded
    EXIT /b 1

:MissingFolder
    ECHO Could not find a "!version_folder!" folder with the script.
    GOTO :ReinstallPrompt

:MisplacedScript
    ECHO/
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
    ECHO `xcopy.exe` failed to complete successfully. Read the error message
    ECHO  above to determine the state of your installation.
    CALL :PauseIfNeeded
    EXIT /b 3



REM The named sections above are used as `GOTO`s and ultimately end the script.
REM The named sections below are `CALL`ed and used like functions.



:WritePermissionsCheck
    COPY NUL foo > NUL
    IF ERRORLEVEL 1 (
        GOTO :InsufficientPermissions
    ) ELSE (
        DEL foo
    )
    EXIT /b 0

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
    SET install_link=https://github.com/TombRunners/tr2-version-swapper/releases
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
    EXIT /b 0

:SetDirectories
    REM `%~dp0` returns the batch file's absolute directory instead of working.
    SET "src=%~dp0"
    SET "music_fix=%src%music_fix"
    SET "patch=%src%patch"
    REM No backslash is appended to version folders here, requiring manual
    REM placement elsewhere in the script. This is still preferred actually
    REM because having the backslash requires placing a leading dot "." at
    REM the end of the `XCOPY` source parameter, which creates ugly output.
    REM For more information: https://stackoverflow.com/a/25841519/10466817
    FOR /L %%i IN (1,1,%version_count%) DO (
        SET "folder[%%i]=%src%!version_names[%%i]!"
    )
    CALL :GetParentDir target "%src%" "..\"
    EXIT /b 0

:GetParentDir
    REM See https://stackoverflow.com/questions/34942604/get-parent-directory-of-a-specific-path-in-batch-script
    FOR %%I IN ("%~2\%~3") DO SET "%~1=%%~fI"
    EXIT /b 0

:PrintDirectories
    ECHO Using the following directories:
    ECHO Game: "%target%"
    ECHO Script: "%src%"
    ECHO Music fix: "!music_fix!\"
    ECHO === Versions ===
    FOR /L %%i IN (1,1,%version_count%) DO (
        ECHO "!folder[%%i]!\"
    )
    ECHO ================
    ECHO/
    EXIT /b 0

:PrintIntroduction
    ECHO Welcome to the TR2 version swapper script!
    ECHO This script's code can be viewed/edited with a text editor.
    ECHO The official files with source control can be found here:
    ECHO %git_link% && ECHO/
    ECHO ==== NOTICE ====
    ECHO This batch script assumes the distributed folder containing it resides
    ECHO in a fresh TR2 Steam installation folder per the README. If placed
    ECHO incorrectly, the script may refuse to work, or do worse by erroneously
    ECHO proceeding if no problems are detected. Thus, it is asked that you be
    ECHO sure to leave the script and the accompanying game files untouched.
    ECHO ================ && ECHO/
    EXIT /b 0

:GetSelectionIndex
    REM Naturally, this hacky solution only works if `%version_count%` <= 9
    SET /p index="Enter the number of your desired version: " < NUL
    :Prompt
    CHOICE /c 0123456789 > NUL
    SET /a "index=%ERRORLEVEL%-1"
    IF %index% GTR %version_count% GOTO :Prompt
    IF %index% LSS 1 GOTO :Prompt
    ECHO %index%
    EXIT /b 0

:PrintMusicInfo
    ECHO/
    ECHO You switched to a non-Multipatch version. You may find that music no
    ECHO longer works, or that the game lags when loading music. There is a
    ECHO music fix available which should fix most music issues. You can learn
    ECHO more on the Tomb Runner Discord server or speedrun.com/tr2.
    ECHO/
    EXIT /b 0

:CheckMusicFiles
    REM Check that the folder has all DLLs.
    FOR /L %%i IN (1,1,%music_file_count%) DO (
        IF NOT EXIST %1\!music_files[%%i]! GOTO :NotTrue
    )
    REM Check that the music folder has all music tracks.
    FOR /L %%i IN (1,1,%music_track_count%) DO (
        IF %%i LEQ 9 (
            SET track=0%%i.wma
        ) ELSE (
            SET track=%%i.wma
        )
        IF NOT EXIST %1\music\!track! GOTO :NotTrue
    )
    SET music_fix_present=true
    GOTO :Exit
    :NotTrue
    SET music_fix_present=false
    :Exit
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
    IF %pause% EQU true (
        PAUSE
    )
    EXIT /b 0
