@ECHO OFF
VERIFY ERRORS 2 > NUL
SETLOCAL EnableDelayedExpansion

:: See https://stackoverflow.com/a/36663349/10466817
:: Note this gets "tricked" by PowerShell, but that's a fringe use case.
IF "%console_mode%" EQU "" (
    SET print=false
    FOR %%x IN (%cmdcmdline%) DO (
        IF /i "%%~x" EQU "/c" SET print=true
    )
)

CALL :WritePermissionsCheck
CALL :SetVariables %~1
CALL :SetDirectories
IF %verbose% EQU true CALL :PrintDirectories
FOR /L %%i IN (1,1,%version_count%) DO (
    SET trying_version_folder="!version_names[%%i]!"
    IF NOT EXIST "!folder[%%i]!" GOTO :MissingFolder
)
FOR /L %%i IN (1,1,%file_count%) DO (
    IF NOT EXIST "%target%!files[%%i]!" GOTO :MisplacedScript
)

:: Start the script proper
CALL :PrintIntroduction
:: Get the user's version choice.
ECHO Version list:
FOR /L %%i IN (1,1,%version_count%) DO (
    ECHO     %%i: !version_names[%%i]!
)
CALL :GetSelectionIndex
SET selected_version=!version_names[%index%]!
ECHO You selected: %selected_version%
ECHO/
:: Check that the version folder has all the files.
FOR /L %%i IN (1,1,%file_count%) DO (
    IF NOT EXIST !folder[%index%]!\!files[%%i]! GOTO :MissingFiles %%i
)
:: Copy the files
XCOPY "!folder[%index%]!" "%target%" /sy || GOTO :CopyError
ECHO/
ECHO Version successfully swapped to %selected_version%.
IF NOT "%selected_version%" EQU "%version_names[1]%" (
    CALL :PrintMusicInfo
)
ECHO Run this script again anytime you wish to change the version again.
CALL :PauseIfNeeded
EXIT /b 0

:InsufficientPermissions
    ECHO The script does not have permissions to write files/folders.
    ECHO You can try running this script as admin as a workaround,
    ECHO but most likely the problem is the game install location.
    CALL :PauseIfNeeded
    EXIT /b 1

:MissingFolder
    ECHO Could not find a %trying_version_folder% version folder with the script.
    GOTO :MissingTerminate

:MisplacedScript
    ECHO It appears this script was not placed within a TR2 installation root.
    ECHO (The files to be replaced were not found in the parent folder...)
    GOTO :MissingTerminate

:MissingFiles
    ECHO Could not find one of the files in the %selected_version% folder!
    FOR /L %%i IN (1,1,%file_count%) DO (
        ECHO * `!files[%%i]!`
    )
    GOTO :MissingTerminate

:MissingTerminate
    ECHO/
    ECHO Please re-install the script folder per the instructions here:
    ECHO %git_link%
    CALL :PauseIfNeeded
    EXIT /b 2

:CopyError
    ECHO `xcopy.exe` failed to complete successfully.
    ECHO Depending on the error, your version may or may not have been swapped.
    CALL :PauseIfNeeded
    EXIT /b 3

:: The named sections above are used as `GOTO`s and ultimately end the script.
:: The named sections below are `CALL`ed and used like functions.

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
    ) ELSE (
        SET verbose=false
    )
    SET git_link=https://github.com/TombRunners/tr2-version-swapper
    SET version_names[1]=Multipatch
    SET version_names[2]=Eidos Premier Collection
    SET version_names[3]=Eidos UK Box
    SET version_count=3
    SET files[1]=tomb2.exe
    SET files[2]=data\floating.tr2
    SET files[3]=data\title.pcx
    SET files[4]=data\tombpc.dat
    SET file_count=4
    EXIT /b 0

:SetDirectories
    :: `%~dp0` returns the batch file's absolute directory instead of working.
    SET src=%~dp0
    :: No backslash is appended to version folders here, requiring manual
    :: placement elsewhere in the script. This is still preferred actually
    :: because having the backslash requires placing a leading dot "." at
    :: the end of the `XCOPY` source parameter, which creates ugly output.
    :: For more information: https://stackoverflow.com/a/25841519/10466817
    FOR /L %%i IN (1,1,%version_count%) DO (
        SET folder[%%i]=%src%!version_names[%%i]!
    )
    CALL :GetParentDir target "%src%" "..\"
    EXIT /b 0

:GetParentDir
    :: See https://stackoverflow.com/questions/34942604/get-parent-directory-of-a-specific-path-in-batch-script
    FOR %%I IN ("%~2\%~3") DO SET "%~1=%%~fI"
    EXIT /b 0

:PrintDirectories
    ECHO Using the following directories: [-v]
    ECHO Game: %target%
    ECHO Script: %src%
    ECHO === Versions ===
    FOR /L %%i IN (1,1,%version_count%) DO (
        ECHO !folder[%%i]!\
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
    ECHO This batch script assumes the distributed folder containing the script
    ECHO is placed in a TR2 installation folder per the README file. If placed
    ECHO incorrectly, it may refuse to proceed or do worse by erroneously
    ECHO proceeding if no problems are detected. Thus, it is asked that you be
    ECHO sure to leave the script and the accompanying game files untouched.
    ECHO ================ && ECHO/
    EXIT /b 0

:GetSelectionIndex
    :: Naturally, this hacky solution only works if `%version_count%` <= 9
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
    ECHO music fix available which should fix most music issues. You can find
    ECHO information on the Tomb Runner Discord server or speedrun.com/tr2.

:PauseIfNeeded
    IF %print% EQU true (
        PAUSE
    )
    EXIT /b 0
