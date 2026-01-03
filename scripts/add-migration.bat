@echo off
set /p MIGRATION_NAME="Enter migration name: "

if "%MIGRATION_NAME%"=="" (
    echo Error: Migration name cannot be empty.
    goto :EOF
)

echo Creating migration '%MIGRATION_NAME%' in Persistence/Migrations...

dotnet ef migrations add "%MIGRATION_NAME%" ^
    --project ../src/CezStudentAssistant.Infrastructure ^
    --startup-project ../src/CezStudentAssistant.API ^
    --output-dir Persistence\Migrations

pause