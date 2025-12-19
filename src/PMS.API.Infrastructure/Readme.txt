For adding, update and removing migrations you have to change the Default Project to "src\PMS.API.infrastructure" from drop down.

-----------------------------Add Migrations-----------------------------------

dotnet ef migrations add InitialMigration --project src\PMS.API.infrastructure --startup-project src\PMS.API.web

----------------------------Update Database----------------------------------

dotnet ef  database update --project src\PMS.API.infrastructure --startup-project src\PMS.API.web

----------------------------Remove Migration----------------------------------
dotnet ef migrations remove --project src\PMS.API.infrastructure --startup-project src\PMS.API.web


cd /var/www/PMS-API/src && dotnet ef database update --project PMS.API.Infrastructure/PMS.API.Infrastructure.csproj --startup-project PMS.API.Web/PMS.API.Web.csproj