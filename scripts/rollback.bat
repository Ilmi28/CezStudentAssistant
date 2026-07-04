@echo off

:ASK_MIGRATION
set /p MIGRATION="Enter the target migration name to roll back to (or '0' to wipe all): "

if "%MIGRATION%"=="" (
    echo Target migration cannot be empty.
    goto :ASK_MIGRATION
)

:ASK_CONFIRM
set /p CONFIRM="Are you sure you want to ROLLBACK the database to '%MIGRATION%'? (Y/N): "

:: Check input (case-insensitive)
if /i "%CONFIRM%" neq "Y" (
    echo Action cancelled.
    pause
    goto :EOF
)

echo Rolling back database to '%MIGRATION%'...

dotnet ef database update %MIGRATION% ^
    --project ../src/CezStudentAssistant.Infrastructure ^
    --startup-project ../src/CezStudentAssistant.API

echo Database rollback complete.
pause