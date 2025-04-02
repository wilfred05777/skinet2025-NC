To run the program
  - cd api / dotnet run or dotnet watch
---

API
<hr>

creation of .net project 
```
dotnet sln add Core
dotnet new webapi -o API -controllers
dotnet new sln
dotnet new classlib -o Infrastructure
dotnet sln add API 
dotnet new classlib -o Core
dotnet sln add Core // add to main file using dotnet sln list if it is seen it is link or reference on the main project
dotnet sln list // show attacheh folders
dotnet new gitignore // add git .gitignore file
```

```
cd API folder
- dotnet add reference ../Infrastructure

cd ..
cd Core folder
-dotnet add reference ../Core
```

```
cd root folder
-dotnet restore
- dotnet build
```

config on APi Controllers .net v9
```
- api/properties 
- launchSettings.json
- 
"http": {
      "applicationUrl": "http://localhost:5000; http://localhost:5001",
    },
-- bug in this section: 
https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45148203#notes
```


```
dotnet dev-certs https
dotnet dev-certs https --trust
dotnet dev-certs https --clean
```

8. Creating the Product Entity
```
 - Solution Explorer
 - Core / Class1.cs - delte
 - create new folder & file core/Entities/product.cs
```
product.cs
```
namespace Core.Entities;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string Type { get; set; }
    public required string Brand { get; set; }   
    public int QuantityInStock { get; set; }
}
```

9. Setting up entity framework
```
- Solution Explorer
-- Infractructure folder
--- VS Code - Nuget tab / Uncheck Pre-release
---- Search for -> Microsoft.EntityFrameworkCore.SqlServer -> install in Infrastructure project
---- Search for -> Microsoft.EntityFrameworkCore.Design -> install in API project
---- In Infrastructure folder - removed Class1.cs

---- Infrastructure/Data/StoreContext.cs
---- click yellow bulb in VSCode ->Generate constructor(storeContext(optional))
---- refactor the generated code to primary constructor
```
Connection String @ Program.cs dbContext
```
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt =>{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
```
```
- issue cd infrastructure then -> dotnet add reference ../Core/Core.csproj
```

10. Setting up Sql Server
```
- docker
- root file skinnet2025/docker-compose.yml
```
docker-compose.yml
```
services: 
  sql:
    image: mcr.microsoft.com/azure-sql-edge
    environment:
      ACCEPT_EULA: "1"
      MSSQL_SA_PASSWORD: "Password@1"
    ports:
      - "1433:1433"
      
```
running docker-compose.yaml file
```
 root terminal: docker compose up -d
```