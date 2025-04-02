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

11. Connecting to the Sql Server from the app
```
Solution Explorer:
                  -skinet2025/API/appsettings.json/appsettings.Development.json
```
appsettings.Development.json
``` 
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=skinet;User Id=SA;Password=Password@1;TrustServerCertificate=True;"
  }, 
```

install package
`https://www.nuget.org/packages/dotnet-ef`

`terminal: dotnet tool install --global dotnet-ef --version 9.0.3"`
` stop the running APP & restart it again: dotnet watch or dotnet run`

check if there is a dotnet ef tool
`at terminal root folder skinet: dotnet ef`

migratations
`note: api runnig should be stop befor running the command below at the terminal` 
`dotnet ef migrations add InitialCreate -s API -p Infrastructure`

To undo this action, use `'dotnet ef migrations remove -s API -p Infrastructure'`
`note: docker tool should run in order to excute properly the removal of migration`


12. Configuring the entities for the migration
- StoreContext.cs
`override space selectOnModelCreating(ModelBuilder modelBuilder)) `
```
namespace Infrastructure.Data;
using Core.Entities; // Ensure the Core project is referenced in the Infrastructure project
using Infrastructure.Config;
using Microsoft.EntityFrameworkCore;


public class StoreContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
    }
}
```

- create-> Infrastructure/Config/ProductConfiguration.cs
- `Soulution Explorer`
  - `higlight->IEntityTypeConfiguration->implement Interface`
```
using System;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}
```
` terminal: dotnet ef migrations add InitialCreate -s API -p Infrastructure `
create database
`note: Docker should be running/started`
`terminal root: dotnet ef database update -s API -p Infrastructure`

View Database Content
`SQL Server Via VS Code Extension`
` server: localhost `
` username: SA login`
` password: Password@1 `
` [skinet].dbo.[__EFMigrationsHistory] right click-> select 1000`