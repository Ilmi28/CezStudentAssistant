@echo off

:ASK_CONFIRM
set /p CONFIRM="Are you sure you want to UPDATE the database? (Y/N): "

:: Check input (case-insensitive)
if /i "%CONFIRM%" neq "Y" (
    echo Action cancelled.
    pause
    goto :EOF
)

echo Updating database to the latest migration...

dotnet ef database update ^
    --project ../src/CezStudentAssistant.Infrastructure ^
    --startup-project ../src/CezStudentAssistant.API

echo Database update complete.
pause