@ECHO OFF

dotnet ef database update --startup-project src\Hosts\Api --project src\Modules\Infrastructure --verbose

pause
