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

13. Creating a products controller
` Solution Explorer `
` Create: API/Controllers/ProductsController.cs `  
```
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly StoreContext context;

    public ProductsController(StoreContext context)
    {
        this.context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
      // return a product list  
    return await context.Products.ToListAsync();
    }

    [HttpGet("{id:int}")] // api/products/1
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        // return a single product by id
        var product = await context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        // create a new product
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
        // return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

14. Using postman to test our new API Endpoints 
- Postman
  - Create new workspace 
  - name of workspace, only me,
  - new collection
  - skinet -> variables for defining variable for api testing oranization

15. Adding the update and delete endpoints
`API/Controllers/ProductControllers.cs`
```
// update product
    // PUT api/products/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        // update an existing product
        if(product.Id != id) return BadRequest("Cannot update this product");

        context.Entry(product).State = EntityState.Modified;

        await context.SaveChangesAsync();
        
        return NoContent();
    }

    // delete product
    // DELETE api/products/1
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        // delete a product
        var product = await context.Products.FindAsync(id);
        if (product == null) return NotFound();

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return NoContent();
    }
    private bool ProductExists(int id)
    {
        return context.Products.Any(x => x.Id == id);
    }
```

<hr>
API Architecture
<hr>

` Application archeticture `
` The Repository Pattern `
` Seeding Data `
` Migrations and Startup `

19. Introduction to the repository pattern

- The Repository Pattern - Goals
` Decouple business code from data access `
` Separation of concerns `
` Minimise duplicate query logic`
` Testability `

- The Repository pattern - Why use it?
` Avoid messy controllers `
` Simplified testing `
` Increased abstraction `
` Maintainability ` 
` Reduced duplicated code ` 

- The Repository pattern - Downsides?
` Abstraction of an abstraction ` 
` Optimization challenges `
` Optimization challenges `

20. Creating the repository interface and implementation class
` Solution Explorer -> Core/Interfaces ` 
` Solution Explorer -> Core/Interfaces/IProductRepository.cs ` 
- IProductRepository.cs
```
using Core.Entities;
namespace Core.Interfaces;
public interface IProductRepository
{

    Task<IReadOnlyList<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    void AddProduct (Product product);
    void UpdateProduct (Product product);
    void DeleteProduct (Product product);
    bool ProductExists(int id);
    Task<bool> SaveChangesAsync();

}
```
- implement the Interface IProductRepository
` Solution Explorer -> Infrastructure/Data/ProductRepository.cs `
` ProductRepository.cs`
` vscode: highlight + quick fix select-> implement interface`
```
public class ProductRepository : IProductRepository // <-Highlight this 
``` 
`it  will generate default scafolding content in  ProductRepository.cs `
- Add ProductREPOSITORY as service in PROGRAM CLASS
` Program.cs `
```
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

21. Implementing the repository methods

` infrastructure/Data/ProductRepository.cs `
` context -> create & assign field 'context' `
` ProductRepository -> use primary constructor `
remove `private readonly StoreContext context = context; `

22. Using the repository in the controller
` API/Controller/ProductsControllers.cs`
```
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository repo) : ControllerBase
{
    // private readonly StoreContext context;

    // public ProductsController(StoreContext context)
    // {
    //     this.context = context;
    // }
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts()
    {
      // return a product list  
    return Ok(await repo.GetProductsAsync());

    }

    [HttpGet("{id:int}")] // api/products/1
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        // return a single product by id
        // var product = await context.Products.FindAsync(id);
        var product = await repo.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    // create a new product
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        
        repo.AddProduct(product);
        if(await repo.SaveChangesAsync())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        return BadRequest("Problem creating product");

        // context.Products.Add(product);
        // await context.SaveChangesAsync();
        // return product;
        // return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // update product
    // PUT api/products/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        // update an existing product
        if(product.Id != id) return BadRequest("Cannot update this product");

        repo.UpdateProduct(product);

        if(await repo.SaveChangesAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem updating the product");


        // context.Entry(product).State = EntityState.Modified;
        // await context.SaveChangesAsync();
        // return NoContent();
    }

    // delete product
    // DELETE api/products/1
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        // delete a product
        var product = await repo.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        repo.DeleteProduct(product);
        if(await repo.SaveChangesAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem deleting the product");

        // var product = await context.Products.FindAsync(id);
        // context.Products.Remove(product);
        // await context.SaveChangesAsync();
        // return NoContent();
    }
    private bool ProductExists(int id)
    {
        return repo.ProductExists(id);
        // return context.Products.Any(x => x.Id == id);
    }
}
```
`Check Api endpoints in PostMan`

23. Seeding data
` CA/seed data/delivery.json`
` CA/seed data/products.json`
` add = Infrastructure/Data/SeedData/delivery.json`
` add = Infrastructure/Data/SeedData/products.json`

` create = Infrastructure/Data/StoreContextSeed.json`
```
using System;
using System.Text.Json;
using Core.Entities;

namespace Infrastructure.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context)
    {
        if(!context.Products.Any())
        {
            var productsData = await File.ReadAllTextAsync("../Infrastructure/Data/SeedData/products.json");
            
            var products = JsonSerializer.Deserialize<List<Product>>(productsData);

            if(products == null) return;
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
```

` Program.cs `
```
app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}

app.Run();
```

` local development server: shutdown API dotnet watch/run`
` cd API/ then$: ' dotnet ef database drop -p Infrastructure -s API ' `
encounter issue
` Unable to retrieve project metadata. Ensure it's an SDK-style project. If you're using a custom BaseIntermediateOutputPath or MSBuildProjectExtensionsPath values, Use the --msbuildprojectextensionspath option. `

solution on encounter issue:
` reason I encounter this error is that I was in API I should be in the root folder of Skinet => 'cd ..'`

`if successfully drop data from database 'cd api' again then run 'dotnet watch'`


24. Getting the brands and types
` Core/Interface/IProductRepository.cs `

```
public interface IProductRepository
{
Task<IReadOnlyList<string>> GetBrandsAsync();
Task<IReadOnlyList<string>> GetTypesAsync();
}
```

implement

` Infrastructure/Data/ProductRepository.cs`
```
// IProductRepository => Implement interface 
public class ProductRepository(StoreContext context) : IProductRepository
{
...
 public async Task<IReadOnlyList<string>> GetBrandsAsync()
    {
        return await context.Products.Select(x=> x.Brand)
            .Distinct()
            .ToListAsync();
    }
 public async Task<IReadOnlyList<string>> GetTypesAsync()
    {
        return await context.Products.Select(x=> x.Type)
            .Distinct()
            .ToListAsync();
    }
 ...   
}
```
` API/Controllers/ProductsController.cs `
```
public class ProductsController(IProductRepository repo) : ControllerBase
{
...
 [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        return Ok(await repo.GetBrandsAsync());
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        return Ok(await repo.GetTypesAsync());
    }
...
}
```
` Postman API testing section 3 - Get Products Brands & Get Product Types`

25. Filtering the products by brand = FF

` IProductRepository.cs `
```
public interface IProductRepository
{
    ...
 Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type);
    ...
}
```
` ProductRepository.cs`
```
    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type)
    {   
        var query = context.Products.AsQueryable();

        if(!string.IsNullOrWhiteSpace(brand))
            query = query.Where(x => x.Brand == brand);

        if(!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync();
        // return await context.Products.ToListAsync();
    }
```

` ProductsController.cs`
```
[HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type)
    {
      // return a product list  
    return Ok(await repo.GetProductsAsync(brand,type));

    }
```
` Test it in Postman: Get Products by Brand or Type`
```
{{url}}/api/products?brand=React 
or
{{url}}/api/products?type=boards
```