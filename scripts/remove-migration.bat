@echo off

:ASK_CONFIRM
set /p CONFIRM="Are you sure you want to REMOVE the latest migration? (Y/N): "

:: The /i switch makes it case-insensitive (accepts Y or y)
if /i "%CONFIRM%" neq "Y" (
    echo Action cancelled.
    pause
    goto :EOF
)

echo Removing the latest migration...

dotnet ef migrations remove ^
    --project ../src/CezStudentAssistant.Infrastructure ^
    --startup-project ../src/CezStudentAssistant.API

pause