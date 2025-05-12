@echo off
REM CleanupLogs.bat
REM This script deletes log files older than 30 days

set LOGS_DIR=%~dp0logs
set DAYS=30

echo Cleaning up log files older than %DAYS% days from %LOGS_DIR%...

if not exist "%LOGS_DIR%" (
    echo Log directory does not exist. Nothing to clean up.
    goto :end
)

forfiles /p "%LOGS_DIR%" /s /m *.log /d -%DAYS% /c "cmd /c del @path" 2>nul

if %ERRORLEVEL% EQU 0 (
    echo Old log files successfully deleted.
) else if %ERRORLEVEL% EQU 1 (
    echo No log files found older than %DAYS% days.
) else (
    echo Error occurred while cleaning up log files.
)

:end
echo Cleanup completed.
