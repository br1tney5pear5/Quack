rm -rf Migrations
rm Quack.db
dotnet ef migrations add Init -v | exit
dotnet ef database update
