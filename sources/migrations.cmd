@ECHO OFF

dotnet ef migrations %* --startup-project src\Hosts\Api --project src\Modules\Infrastructure --verbose

pause
