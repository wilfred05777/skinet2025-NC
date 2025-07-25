To run the program

-  cd api / dotnet run or dotnet watch

---

### API

---

###### creation of .net project

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

-  StoreContext.cs
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

-  create-> Infrastructure/Config/ProductConfiguration.cs
-  `Soulution Explorer`
   -  `higlight->IEntityTypeConfiguration->implement Interface`

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

`terminal: dotnet ef migrations add InitialCreate -s API -p Infrastructure`
create database
`note: Docker should be running/started`
`terminal root: dotnet ef database update -s API -p Infrastructure`

View Database Content
`SQL Server Via VS Code Extension`
`server: localhost`
` username: SA login`
`password: Password@1`
` [skinet].dbo.[__EFMigrationsHistory] right click-> select 1000`

13. Creating a products controller
    `Solution Explorer`
    `Create: API/Controllers/ProductsController.cs`

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

-  Postman
   -  Create new workspace
   -  name of workspace, only me,
   -  new collection
   -  skinet -> variables for defining variable for api testing oranization

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

`Application archeticture`
`The Repository Pattern`
`Seeding Data`
`Migrations and Startup`

19. Introduction to the repository pattern

-  The Repository Pattern - Goals
   `Decouple business code from data access`
   `Separation of concerns`
   ` Minimise duplicate query logic`
   `Testability`

-  The Repository pattern - Why use it?
   `Avoid messy controllers`
   `Simplified testing`
   `Increased abstraction`
   `Maintainability`
   `Reduced duplicated code`

-  The Repository pattern - Downsides?
   `Abstraction of an abstraction`
   `Optimization challenges`
   `Optimization challenges`

20. Creating the repository interface and implementation class
    `Solution Explorer -> Core/Interfaces`
    `Solution Explorer -> Core/Interfaces/IProductRepository.cs`

-  IProductRepository.cs

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

-  implement the Interface IProductRepository
   `Solution Explorer -> Infrastructure/Data/ProductRepository.cs`
   ` ProductRepository.cs`
   ` vscode: highlight + quick fix select-> implement interface`

```
public class ProductRepository : IProductRepository // <-Highlight this
```

`it  will generate default scafolding content in  ProductRepository.cs `

-  Add ProductREPOSITORY as service in PROGRAM CLASS
   `Program.cs`

```
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

21. Implementing the repository methods

`infrastructure/Data/ProductRepository.cs`
`context -> create & assign field 'context'`
`ProductRepository -> use primary constructor`
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

`Program.cs`

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
`cd API/ then$: ' dotnet ef database drop -p Infrastructure -s API '`
encounter issue
`Unable to retrieve project metadata. Ensure it's an SDK-style project. If you're using a custom BaseIntermediateOutputPath or MSBuildProjectExtensionsPath values, Use the --msbuildprojectextensionspath option.`

solution on encounter issue:
` reason I encounter this error is that I was in API I should be in the root folder of Skinet => 'cd ..'`

`if successfully drop data from database 'cd api' again then run 'dotnet watch'`

24. Getting the brands and types
    `Core/Interface/IProductRepository.cs`

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

`API/Controllers/ProductsController.cs`

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

`IProductRepository.cs`

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

###### 198. Creating the order components

`IProductRepository.cs`

```
Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort);
```

`ProductRepository.cs`

```
public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort)
{
    // if(!string.IsNullOrWhiteSpace(sort))
    // {
        query = sort switch
        {
            "priceAsc" => query.OrderBy(x => x.Price),
            "priceDesc" => query.OrderByDescending(x => x.Price),
            _ => query.OrderBy(x => x.Name)
        };
    // }
}
```

`ProductsController.cs`

```
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository repo) : ControllerBase
{

 [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
    {
      // return a product list
    return Ok(await repo.GetProductsAsync(brand,type, sort));

    }
}
```

` Postman: Sec. Archeticture - Get Products`
` Postman: Sec. Archeticture - Get Products sorted by price asc`
` Postman: Sec. Archeticture - Get Products sorted by price asc {{url}}/api/products?sort=priceDesc`
` Postman: Sec. Archeticture - Get Products sorted by price asc {{url}}/api/products?sort=priceAsc`

``

#### The specification Pattern

<hr>

###### 28. Introduction

`Create a generic repository`
`Specification pattern`
`Using the specification pattern`
`Using the debugger`
`Shaping data`

-  `about Generics`
   -  `Been around shice C# 2.0 (2002)`
   -  `Help avoid duplicate code`
   -  `Type safety`

##### Generics - We are already using

```
    // e.g 1
    public StoreContext(DbContextOptions<StoreContext> options) : base(options)){

    // => <StoreContext> <=
    }
```

```
    // e.g 2
    pubic DbSet<Product> Products {get; set;}

    // => <Product>
```

```
    services.AddDbContext<StoreContext>(x => x.UseSqlite(_config.GetConnectionString("DefaultConnections)));

    // => <StoreContext>
```

##### Generic Repository

```
public interface IGenericRepository<T> where T: BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
}

// => <T> or T
```

##### Generic Expressions:

###### Product Repository

```
public aync Task<IReadOnlyList<Product>> GetProductsContainingRed()
{
    return await _context.Products
        .Where(x => x.Name.Contains("red"))
        .ToListAsync();
}
```

###### Generic Repository

```
public aync Task<IReadOnlyList<T>> ListAllAsync()
{
    return await _context.Set<T>()
        .OrderBy(p => p.Name)
        .ToListAsync();
        // => p.Name error or there is a change here.
}
```

`Generic Repository can do a simple things`
`but when it comes to more complex queries  bearing in mind for Generic Repositories 'I have a hundred queries'`

###### 29. Creating a generic repository

`Generic Version of IProductRepository.cs & ProductRepository.cs`

`Core/Interfaces/IGenericRepository.cs`

```
using System;
using Core.Entities;

namespace Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> SaveChangesAsync();
    bool Exists(int id);
}

```

###### implementation

`Infrastructure/Data/GenericRepository.cs`

```
using System;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    public void Add(T entity)
    {
        throw new NotImplementedException();
    }

    public bool Exists(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> ListAllAsync()
    {
        throw new NotImplementedException();
    }

    public void Remove(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }

    public void Update(T entity)
    {
        throw new NotImplementedException();
    }
}

```

`implement the interface - highlight this- IGenericRepository`

##### Add to service in program.cs

```
// services section
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
```

###### 30. Implementing the generic repository methods

`Infrastructure/Data/GenericRepository.cs`

```
using System;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public bool Exists(int id)
    {
        return context.Set<T>().Any(x => x.Id == id);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await context.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await context.Set<T>().ToListAsync();
    }

    public void Remove(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void Update(T entity)
    {
        context.Set<T>().Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }
}
0
```

###### 31. Using the generic repository in the controller

`API/Controllers/ProductsController.cs`

```
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
    {
    return Ok(await repo.ListAllAsync());

    }

    [HttpGet("{id:int}")] // api/products/1
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await repo.GetByIdAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    // create a new product
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {

        repo.Add(product);

        if(await repo.SaveAllAsync())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        return BadRequest("Problem creating product");

    }

    // update product
    // PUT api/products/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if(product.Id != id) return BadRequest("Cannot update this product");

        repo.Update(product);

        if(await repo.SaveAllAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem updating the product");

    }

    // delete product
    // DELETE api/products/1
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        // delete a product
        var product = await repo.GetByIdAsync(id);
        if (product == null) return NotFound();

        repo.Remove(product);

        if(await repo.SaveAllAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem deleting the product");

    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        //  TODO: implement method
        return Ok();
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        //  TODO: implement method
        return Ok();
    }

    private bool ProductExists(int id)
    {
        return repo.Exists(id);
    }
}

```

###### `Postman checking`

` Section 4: Specification -> Get Products by Brand`

###### 32. Introduction to the specification pattern

`Specification pattern`
`Generic Repository is an Anti-Pattern!`

```
public interface IRepository<T>
{
    IReadOnlyList<T> ListAllAsync();
    IReadOnlyList <T> FindAsync(Expression<Func<T, bool>> query);
    T GetByID(int id);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}

//FindAsync(Expression<Func<T, bool>> query); // FindAsync = Leaky abstraction or too generic?

// Generic expression e.g: in (Expression<Func<T, bool>> query)
// x=>x.Name.Contains("red")
```

`The specification pattern to the rescue!`

```
- Describes a query in an object
- Returns an IQueryable<T>
- Generic List method takes specification as parameter
- Specification can have meaningful name
    - OrdersWithItemsAndSortingSpecification
```

```
---------------------------
|   Specification:        |
|   that have a brand     |
|   of 'react' and are a  |
|   type of 'gloves'      |
---------------------------
            |
            | down arrow
---------------------------
|                         |
|                         | Generic Repository
|  ListAsync(specifation) |
|                         |
|                         |
---------------------------
    spec |       | IQueryable<T>
         |       |
downarrow|       | up arrow
---------------------------
|                         |
|                         |
|      Evaluator          |
|                         |
|                         |
---------------------------
```

###### 33. Setting up the specification classes

`Core/Interfaces/ISpecification.cs`

```
using System;
using System.Linq.Expressions;
namespace Core.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria{ get; }
}

```

-  create folder name
   `Core/Specifications`
   `Core/Specifications/BaseSpecifications.cs`
   `implement the interface`
   `create and assign field 'criteria'`
   `BaseSpecifications(Expression< - highlight Expression - Use primary constructor`

```
using System;
using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecifications<T>(Expression<Func<T, bool>> criteria) : ISpecification<T>
{
    public Expression<Func<T, bool>>  Criteria => criteria;
}

```

` Infrastructure/Data/SpecificationEvaluator.cs`

```
using System;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class SpecificationEvaluator<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        if( spec.Criteria !=null)
        {
            query = query.Where(spec.Criteria); // x => x.Brand == brand
        }

        return query;

    }
}

```

###### 34. Updating the repository to use the specification

`Update Core/Interfaces/IGenericRepository.cs`

```
public interface IGenericRepository<T> where T : BaseEntity
{
    //...
    Task<T?> GetEntityWithSpec(ISpecification<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    //...
}
```

`Update Infrastructure/Data/GenericRepository.cs`
`IGenericRepository = Implement generic methods by Implement Interface`

```
public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    public async Task<T?> GetEntityWithSpec(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }
    //...

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
       return await ApplySpecification(spec).ToListAsync(); // filter the spec base on the list (spec).ToListAsync(); and return the list.
    }

    //..
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(context.Set<T>().AsQueryable(), spec);
    }

}
```

###### 35. Using the specification pattern

`create Core/Specification/ProductSpecification.cs -> start`

```
using Core.Entities;
namespace Core.Specifications;

public class ProductSpecification : BaseSpecifications<Product>
{

}

```

`adjustment to Core/Specifications/BaseSpecification.cs`

```
using System.Linq.Expressions;
using Core.Interfaces;
namespace Core.Specifications;

// making it optional by adding in bool>>? <--
public class BaseSpecifications<T>(Expression<Func<T, bool>>? criteria) : ISpecification<T>
{
    // empty constructor ProductSpecification
    protected BaseSpecifications() : this(null) {}

    // making it optional by adding in bool>>? <--
    public Expression<Func<T, bool>>?  Criteria => criteria;
}
```

`adjust code in  Core/Interaces/ISpecification.cs`

```
using System.Linq.Expressions;
namespace Core.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria{ get; }
}
// making it optional by adding in bool>>? <--
```

`continue -> create Core/Specification/ProductSpecification.cs`

```
//...
public class ProductSpecification : BaseSpecifications<Product>
{
    // traditional constructor
    public ProductSpecification(string? brand, string? type) : base(x =>
        (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
        (string.IsNullOrWhiteSpace(type) || x.Type == type))
    {

    }
}
```

`update API/Controllers/ProductsController.cs`

```
//...
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
    {
        var spec = new ProductSpecification(brand, type);
        var products = await repo.ListAsync(spec);
        return Ok(products);
    }
}
```

`then test Postman Section 4. Specification - Get Products by Brand & Get Products by Type`

###### 36. Adding sorting to the specification

`update Core/Interfaces/ISpecification.cs`

```
using System.Linq.Expressions;
namespace Core.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
}
```

` Core/Specifications/BaseSpecification.cs`

```
using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    //...
    public Expression<Func<T, object>>? OrderBy {get; private set;}
    public Expression<Func<T, object>>? OrderByDescending {get; private set;}

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        // correction
        OrderByDescending = orderByDescExpression;

        // incorrect
        // OrderBy = orderByDescExpression; // encounter issue seen after the testing

    }
}
```

` Evaluate our expression from BaseSpecification and apply it to the query doing that to our SpecificationEvaluator`

`update Infrastructure/Data/SpecificationEvaluator.cs`

```
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class SpecificationEvaluator<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        //... more code @ top

        if(spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if(spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        return query;

    }
}
```

`update Core/Specifications/ProductSpecification.cs`

```
using Core.Entities;

namespace Core.Specifications;

public class ProductSpecification : BaseSpecifications<Product>
{
    // traditional constructor
    public ProductSpecification(string? brand, string? type, string? sort) : base(x =>
        (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
        (string.IsNullOrWhiteSpace(type) || x.Type == type))
    {
        switch (sort)
        {
            case "priceAsc":
                AddOrderBy(x => x.Price);
                break;
            case "priceDesc":
                AddOrderByDescending(x => x.Price);
                break;
            default:
                AddOrderBy(x => x.Name);
                break;
        }
    }
}
```

` Update API/Controllers/ProductsController.cs file`

```
//more codes top ....

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
    {
        var spec = new ProductSpecification(brand, type, sort); //<--  adding sort here
        var products = await repo.ListAsync(spec);
        return Ok(products);
    }

    //more codes below ....
}
```

##### Sorting/Filtering Specificaton by Checking API through Postman

-  ` check API through postman section 4 - Specification - 'Get Products' check if the filter is in Alphabetical order`
-  `check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceAsc price upwards from smallest to highest`

##### !! Issue Encounter & Solution !!:

` check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceDesc price downwards from highest to lowest ! it got some bugs = because of copy and paste` <br>

-  `solution check: Infrastructure/Data/SpecificationEvaluator.cs -looks good`
-  `solution check: Core/Specifications/ProductSpecification.cs -looks good`
-  `solution check: Core/Specifications/BaseSpecification.cs -the issue is here`

```
//...
public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    //...
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
}
```

-  `check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceDesc price downwards from highest to lowest - functionally good`

###### 37. Using the debugger

` creating debugger - VSCode left handside - 'Run & Debug' -> create a launch.json file`
`select c#`
` launch.json`
`select add Configuration button +`

-  `'.NET: attach to .NET process' `
   `select add Configuration button +`
-  `'.NET: launch C# project' `

```
// default
"configurations": [
        {
            "name": "C#: <project-name> Debug",
            "type": "dotnet",
            "request": "launch",
            "projectPath": "${workspaceFolder}/<relative-path-to-project-folder><project-name>.csproj"
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }


    ]
```

###### `update to`

```
// correction
   "configurations": [
        {
            "name": "C#: API Debug",
            "type": "dotnet",
            "request": "launch",
            "projectPath": "${workspaceFolder}/API.csproj"
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }
    ]
```

`then create/assign breakpoint in this case its  ProductsController.cs line 18`

```
  var spec = new ProductSpecification(brand, type, sort);
```

` click debug and run icon - search API and select it then also select .NET Core Attach`
` hit green play icon`

` test in postman`
`Get Products sorted by price`

```
{{url}}/api/products?sort=priceDesc
to
{{url}}/api/products?sort=priceDesc&type=boards
```

` 'step into' ProductsControllers.cs`

`- Flow of all and correlation about how data is working behind the scene using vscode debugger`

###### 38. Adding projection to the spec part 1

`Enhancing the specification pattern`

`update /Core/Interfaces/ISpecification.cs`

```
//other function on top
public interface ISpecification<T,TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>>? Select { get; }
}
```

`update /Core/Specifcations/BaseSpecification.cs`

```
// other function on top
public class BaseSpecifications<T, TResult>(Expression<Func<T, bool>>? criteria) : BaseSpecifications<T>(criteria), ISpecification<T, TResult>
{
    public Expression<Func<T, TResult>>? Select {get; private set; }
    protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
    {
        Select = selectExpression;
    }
}
```

`update Infrastructure/Data/SpecificationEvaluator.cs `

```
public class SpecificationEvaluator<T> where T: BaseEntity
{
    // more function on top
    public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
    {
        if( spec.Criteria !=null)
        {
            query = query.Where(spec.Criteria); // x => x.Brand == brand
        }

        if(spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if(spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        var selectQuery = query as IQueryable<TResult>;
        if(spec.Select != null)
        {
            selectQuery = query.Select(spec.Select);
        }
        return selectQuery ?? query.Cast<TResult>();

    }
}
```

###### 39. Adding projection to the spec part 2

`update Core/Interfaces/IGenericRepository.cs`

```
public interface IGenericRepository<T> where T : BaseEntity
{
    // more code on top

    Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec);
    Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec);

    // more code on below
}
```

`update Infrastructure/Data/GenericRepository.cs`

```
// implementing interface 'IGenericRepository'

public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    public async Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
    {
       return SpecificationEvaluator<T>.GetQuery<T, TResult>(context.Set<T>().AsQueryable(), spec);
    }
}
```

###### 40. Adding projection to the spec part 3

`update Core/Interface/ISpecification.cs`

```
public interface ISpecification<T>
{
    bool IsDistinct { get; }

}
```

`update Core/Specifications/BaseSpecification.cs`

```
public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    public bool IsDistinct {get; private set;} = false;

    // there is a code here protected void AddOrderByDescending

    protected void AppyDistinct()
    {
        IsDistinct = true;
    }
}
```

`update Infrastructure/Data/SpecificationEvaluator.cs`

```
public class SpecificationEvaluator<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        // more code on top

        if(spec.IsDistinct)
        {
            query = query.Distinct();
        }

        return query;
    }

    public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
    {
        // more code on top

        if(spec.IsDistinct)
        {
            selectQuery = selectQuery?.Distinct();
        }

        return selectQuery ?? query.Cast<TResult>();

    }
}
```

`Adding new file at Core/Specifications/BrandListSpecification.cs`

```
using Core.Entities;

namespace Core.Specifications;

public class BrandListSpecification : BaseSpecifications<Product, string>
{
 public BrandListSpecification()
 {
    AddSelect(x => x.Brand);
    AppyDistinct();
 }
}
```

`Note: Error on BrandListSpecification(){...} to fixed it in BaseSpecification.cs`

```
public class BaseSpecifications<T, TResult>(Expression<Func<T, bool>>? criteria) : BaseSpecifications<T>(criteria), ISpecification<T, TResult>
{
    protected BaseSpecifications() : this(null!) {}
    //more code below...
}
```

`Adding new file at Core/Specifications/TypeListSpecification.cs`

```
using Core.Entities;
namespace Core.Specifications;

public class TypeListSpecification : BaseSpecifications<Product, string>
{
    public TypeListSpecification()
    {
        AddSelect(x => x.Type);
        AppyDistinct();
    }
}
```

` update ProductsController.cs`

```
//...

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    //...

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        var spec = new BrandListSpecification();

        return Ok(await repo.ListAsync(spec));
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeListSpecification();

        return Ok(await repo.ListAsync(spec));
    }

    //...
}
```

`check in postman`
`section 4 - Specification`

-  `Get Product Brands  - {{url}}/api/products/brands`
-  `Get Product Types  - {{url}}/api/products/types`

###### 41. Summary

-  Creating a generic repository
-  Specification pattern
-  Using the specification pattern
-  Using the debugger
-  Shaping data

-  FAQs
   `Question: This is over engineering in action!`
   `Answer; True for now, but we do now have a repository for every entity we create. Imagine we have 100 or 1000 entities, then we have just creatd repositories for all of them.`

`its reusable for every across every future project creating all the example are all generic and reuseable.`

<hr>

### Section 5: Sorting, Filtering, Searching & Pagination

---

###### 42. Introduction

-  API Sorting, Search, Filtering, & Paging

   -  `Sorting`
   -  `Filtering`
   -  `Searching`
   -  `Paging`

-  Goal:
   `To be able to implement sorting, searching and pagination functionality in a list using the Specification parttern`

-  Pagination
   -  `Performance`
   -  `Parameters passed by query string: api/products?pageNumber=2&pageSize=5 `
   -  `Page size should be limited`
   -  `We should always page results`
-  Deferred Execution
   -  ` Query commands are stored in a variable`
   -  `Execution of the query is deffered`
   -  `IQueryable<T> creates an expression tree`
   -  `Execution: `
      -  `ToList(), ToArrya(), ToDictionary() `
      -  `Count() or other singleton queries `

###### 43. Creating product spec parameters

`create Core/Specifications/ProductSpecParams.cs`

```
// prop Full
using System;

namespace Core.Specifications;

public class ProductSpecParams
{
   private List<string> _brands = [];
   public List<string> Brands
   {
        get => _brands; // type=boards, gloves
        set
        {
            _brands = value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).ToList();
        }
   }

   private List<string> _types = [];
   public List<string> Types
   {
        get => _types;
        set
        {
            _types = value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).ToList();
        }
   }

   public string? Sort { get; set; }
}
```

`update Core/Specifications/ProductSpecifications.cs `

```
public class ProductSpecification : BaseSpecifications<Product>
{
    /* old code
    public ProductSpecification(string? brand, string? type, string? sort) : base(x =>
        (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
        (string.IsNullOrWhiteSpace(type) || x.Type == type))
     {
        switch (sort)
        //...
    */
    public ProductSpecification(ProductSpecParams specParams) : base(x =>
        (specParams.Brands.Any() || specParams.Brands.Contains(x.Brand) ) &&
        (specParams.Types.Any() || specParams.Types.Contains(x.Type)))
    {
        // switch (sort)
        switch (specParams.Sort)
        {
            case "priceAsc":
                AddOrderBy(x => x.Price);
                break;
            case "priceDesc":
                AddOrderByDescending(x => x.Price);
                break;
            default:
                AddOrderBy(x => x.Name);
                break;
        }
    }
}
```

` going back to ProductsController.cs`

```
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet]
    /* old code
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
    {
        // var spec = new ProductSpecification(brand, type, sort);
    }
    */
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams specParams)
    {
        // var spec = new ProductSpecification(brand, type, sort);
        var spec = new ProductSpecification(specParams);
        var products = await repo.ListAsync(spec);
        return Ok(products);

    }

    //...
}
```

`Check api (Postman) at Section 5: Get Products by Brand` - `{{url}}/api/products?brands=Angular,React `

`Get Products by Brand and Types` - `{{url}}/api/products?brands=Angular,React&types=Boots,Gloves `

###### 44. Adding pagination part 1

`update Infrastructure/Data/ProductRepository.cs`

```
public class ProductRepository(StoreContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort)
    {
        //...
        return await query.Skip(5).Take(5).ToListAsync();
    }
}
```

`update Core/Interface/ISpecification.cs`

```
public interface ISpecification<T>
{
    //...
        int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }

}
```

`update Core/Specifications/BaseSpecifications.cs`

```
// implement interface

public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    //...
    public int Take {get; private set;}
    public int Skip {get; private set;}
    public bool IsPagingEnabled { get; private set; }

    //...
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        // IsPagingEnabled = true;
    }
}
```

`update Infrastructure/Data/SpecificationEvaluator.cs`

```


public class SpecificationEvaluator<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        //...

        if(spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        //...
    }

    public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
    {
        //...

        if(spec.IsPagingEnabled)
        {
            selectQuery = selectQuery?.Skip(spec.Skip).Take(spec.Take);
        }

        //...
    }

}
```

###### 45. Adding pagination part 2

`update Core/Specifications/ProductSpecParams.cs`

```
public class ProductSpecParams
{
    private const int MaxPageSize = 50;
    public int PageIndex { get; set; } = 1;
    private int _pageSize = 6;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    //...
}
```

`update Core/Specifications/ProductSpecification.cs`

```
public class ProductSpecification : BaseSpecifications<Product>
{

    public ProductSpecification(ProductSpecParams specParams) : base(x =>
        (specParams.Brands.Any() || specParams.Brands.Contains(x.Brand) ) &&
        (specParams.Types.Any() || specParams.Types.Contains(x.Type))
    )
    {
        //...
        ApplyPaging(specParams.PageSize *(specParams.PageIndex - 1), specParams.PageSize);
        //...
    }
}
```

`Create API/RequestHelpers/Pagination.cs`

```
public class Pagination<T>(int PageIndex, int pageSize, int count, IReadOnlyList<T> data)
{
    public int PageIndex { get; set; } = PageIndex;
    public int PageSize {get; set;} = pageSize;
    public int Count { get; set; } = count;
    public IReadOnlyList<T> Data {get; set;} = data;
}
```

###### 46. Adding pagination part 3

`update Core/Interfaces/IGenericRepository.cs`

```
public interface IGenericRepository<T> where T : BaseEntity
{
    //...
    Task<int> CountAsync(ISpecification<T> spec);
}
```

`update Core/Insterfaces/ISpecification.cs `

```
public interface ISpecification<T>
{
    //...
    IQueryable<T> ApplyCriteria(IQueryable<T> query);
}
```

`update Core/Specifications/BaseSpecification.cs`

```
// ISpecification<T> -> implement interface

public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    //...

    public IQueryable<T> ApplyCriteria(IQueryable<T> query)
    {
        if(Criteria != null)
        {
            query = query.Where(Criteria);
        }
        return query;
    }
}
```

`update Infrastructure/Data/GenericRepository.cs `

```
// IGenericRepository<T> - implement interface

public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    //...
    public async Task<int> CountAsync(ISpecification<T> spec)
    {
        var query = context.Set<T>().AsQueryable();

        query = spec.ApplyCriteria(query);

        return await query.CountAsync();
    }
    //...
}
```

`update API/Controllers/ProductsController.cs `

```
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet]

    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams specParams)
    {
        var spec = new ProductSpecification(specParams);
        var products = await repo.ListAsync(spec);

        /** new update start here ======= */
        var count = await repo.CountAsync(spec);
        var pagination = new Pagination<Product>(specParams.PageIndex, specParams.PageSize, count, products);

        return Ok(pagination);
        /** new update end here ======= */
    }
    //...
}
```

`testing API via Postman Section 5 - Paging, sorting, and filtering :`

-  `Get Paged Products Page 0 Size 5 -> {{url}}/api/products?pageSize=3&pageIndex=1`
-  `not showing count: 0 upon testing the api`

###### 47. Creating a Base API controller

`create API/Controllers/BaseApiController.cs`

```
using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected async Task<ActionResult> CreatePageResult<T>(IGenericRepository<T> repo, ISpecification<T> spec, int pageIndex, int pageSize) where T : BaseEntity
    {
        var items = await repo.ListAsync(spec);
        var count = await repo.CountAsync(spec);

        var pagination = new Pagination<T>(pageIndex, pageSize, count, items);
        return Ok(pagination);

    }
}
```

`to use/update in API/Controllers/ProductsController.cs`

```
// Derive BaseAPiController

public class ProductsController(IGenericRepository<Product> repo) : BaseApiController
{
    /* to the code below update : much more cleaner and modularize/ granularize */

    return await CreatePageResult(repo, spec, specParams.PageIndex, specParams.PageSize);

    /*  update this page below:
       var products = await repo.ListAsync(spec);
       var count = await repo.CountAsync(spec);

       var pagination = new Pagination<Product>(specParams.PageIndex, specParams.PageSize, count, products);

       return Ok(pagination);
    */
}
```

-  `Sect. 5 Issue : Solution Sect. 5 Issue`

`testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=2&types=boards`
`testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=1&types=gloves`

-  `BUG: issue is that count & data : not returning anything. Status: not yet solve !FF`
-  `get all products bug not showing after the applying Section 5: Sorting, Filtering, Pagination`

###### 48. Adding the search functionality

`update Core/Specifications/ProductSpecParams.cs`

```
public class ProductSpecParams
{

    //...
    private string? _search;
    public string Search
    {
        get => _search ?? "";
        set => _search = value.ToLower();
    }

}
```

`update Core/Specifications/ProductSpecification.cs`

```
public class ProductSpecification : BaseSpecifications<Product>
{

    public ProductSpecification(ProductSpecParams specParams) : base(x =>
        // update
        (string.IsNullOrEmpty(specParams.Search) || x.Name.ToLower().Contains(specParams.Search)) &&

        //...
        (specParams.Brands.Any() || specParams.Brands.Contains(x.Brand) ) &&
        (specParams.Types.Any() || specParams.Types.Contains(x.Type))
    )
    {
        //...
    }
}
```

`testing in postman GetProducts with search term: {{url}}/api/products?search=red`

`refer back to :  Sect. 5 Issue : for hints`
` S`

`Solution Sect. 5 Issue: is in ProductSpecification.cs`

```
public class ProductSpecification : BaseSpecifications<Product>
{
    public ProductSpecification(ProductSpecParams specParams) : base(x =>
        (string.IsNullOrEmpty(specParams.Search) || x.Name.ToLower().Contains(specParams.Search)) &&

         // correction is here .Count == 0
        (specParams.Brands.Count == 0 || specParams.Brands.Contains(x.Brand) ) &&
        // correction is here .Count == 0
        (specParams.Types.Count == 0 || specParams.Types.Contains(x.Type))
    )
}
```

```
    !!!! Skinet issue start here:
    44. Adding pagination part 1: FF
    commit b5ae77a362185f6f883f3693eba84426b4c8f45c
    Author: Wilfred Erdo Bancairen <wilfred05777@gmail.com>
    Date:   Wed Apr 16 03:18:54 2025 +0800
```

###### git learning or re-learning

-  `git log // show time details of each commits `
-  `git certain commit ######`
-  `git reset --hard commit ###### //reset back to a certain save point`
-  `note: be careful using reset and revert - first you should have branch for it before using the main branch`

<hr>

### 6 Error handling on the API

<hr>

###### 50. Introduction

-  Error handling and exceptions
-  Validation errors
-  Http response errors
-  Middleware - catching as defense for handling error
-  CORS

```
 /* Http response codes */

 200 range => ok
 300 range => Redirection
 400 range => Client error
 500 range => Server error

    - 500 Internal server error
```

###### 51. Adding a test controller for error handling

`create API/Controllers/BuggyControllers.cs`

```
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;

public class BuggyController : BaseApiController
{
    [HttpGet("unauthorized")]
    public IActionResult GetUnauthorized()
    {
        return Unauthorized("This is an unauthorized response");
    }

    [HttpGet("badrequest")]
    public IActionResult GetBadRequest()
    {
        return BadRequest("Not a good request");
    }

    [HttpGet("notfound")]
    public IActionResult GetNotFound()
    {
        return NotFound();
    }

    [HttpGet("internalerror")]
    public IActionResult GetInternalError()
    {
        throw new Exception("This is a test exception");
    }

    [HttpPost("validationerror")]
    public IActionResult GetValidationError(Product product)
    {
        return Ok();
    }
}
```

`postman checking API: Section 6- Get Notfound , Get Bad Request & Validation Error`

-  `{{url}}/api/buggy/notfound`
-  `{{url}}/api/buggy/badrequest`
-  `{{url}}/api/buggy/validationerror`

###### 52. Exception handling middleware

```
<!-- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-9.0 -->

```

`create API/Errors/ApiErrorResponse.cs`

```
namespace API.Errors;

public class ApiErrorResponse(int statusCode, string message, string? details)
{
    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;
    public string? Details { get; set; } = details;
}
```

`create API/Middleware/ExceptionMiddleware.cs`

```
// generate the method for HandleExceptionAsync

namespace API.Middleware;

public class ExceptionMiddleware(IHostEnvironment env, RequestDelegate next)
{
    public async Task InvokeAsync (HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex, IHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = env.IsDevelopment()
            ? new ApiErrorResponse(context.Response.StatusCode, ex.Message, ex.StackTrace)
            : new ApiErrorResponse(context.Response.StatusCode, ex.Message, "Internal Server Error");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}

// HandleExceptionAsync make it static

```

`going back to API/program.cs class to use the functionality forom ExceptionMiddleware.cs`

```
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
//...
```

`postman: Get Internal server error = {{url}}/api/buggy/internalerror`

###### 53. Validation error responses

`create API/DTOs/CreateProductDto.cs`

```
using System.ComponentModel.DataAnnotations;
namespace API.DTOs;
public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]    // required doesn't work in number types
    public decimal Price { get; set; }

    [Required]
    public string PictureUrl { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity in stock must be at least 1")]    // required doesn't work in number types
    public int QuantityInStock { get; set; }
}

//Products.cs
//copy to CreateProductDto.cs and modify
/* old code
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
*/
```

`update BuggyController.cs`

```
public class BuggyController : BaseApiController
{

[HttpPost("validationerror")]
    public IActionResult GetValidationError(CreateProductDto product)
    {
        return Ok();
    }
}
```

`Postman: Section 6: Get Validation Error =>  {{url}}/api/buggy/validationerror`

###### 54. Adding CORS support on the API

`update API/Program.cs class `

```
// Add services to the container.
//...

builder.Services.AddCors();
//...

/* next */
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

// must in between | below

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

// must in between | above
app.MapControllers();

//...
```

`Postman testing APi: section 6: Check Cors is enabled => {{url}}/api/products => scripts tab`

-  `Scripts tab then on headers tab will see the Access-Control-Allow-Origin: value: https://localhost:4200 `

-  `Sect. 5 Issue | not showing anything on count & data`

###### 55. Summary

-  Error Handling objective

   -  Goal to handle errors so we can configure the UI in the client for all errors generated by the API

   -  client side of things

` update Core/Specifications/ProductSpecParams.cs`

```
public class ProductSpecParams : PagingParams //<= derive PagingParams.cs>
{
    /* remove code start here and move to PaginParams.cs*/
    private const int MaxPageSize = 50;
    public int PageIndex { get; set; } = 1;

    private int _pageSize = 6;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    /* remove code end here and move to PaginParams.cs*/

    //...
}
```

`create Core/Specifications/PagingParams.cs`

```
namespace Core.Specifications;
public class PagingParams
{
    private const int MaxPageSize = 50;
    public int PageIndex { get; set; } = 1;

    private int _pageSize = 6;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}
```

<hr>

### 7: Angular Setup

<hr>

###### 56: Introduction

-  Install the Angular CLI
-  Creating the Angular project
-  Setting up VS Code for Angular\*
-  Setting up Angular to use HTTPS
-  Adding Angular Material and Tailwind CSS

-  ###### Goal
-  To have a working Angular application running on HTTPS
-  To understand angular standalone components and how we use them to build an app.

-  ###### Angular Release Schedule
   -  Major release every 6 months
   -  1-3 minor releases for each major release
   -  A patch release build almost every week
   -  At time of recording Angular is on v18

###### 57. Creating the angular project

-  [angular.dev](https://angular.dev/)
-  [Angular versioning and releases](https://angular.dev/reference/releases)
-  [Version compatibility](https://angular.dev/reference/versions)

-  [The Angular CLI](https://angular.dev/tools/cli)
-  [Setting up the local environment and workspace](https://angular.dev/tools/cli/setup-local)

   -  `npm install -g @angular/cli`
   -  `node --version or -v // check node version`
   -  `npm --version or -v // check npm version`
   -  `ng version // check angular version`

-  ###### create angular project

   -  `ng new client`
   -  `Would you like to share pseudonymous usage data abou: NO`
   -  `Would you like to share pseudonymous usage data abou: NO`
   -  `Which stylesheet format would you like to use?: Sass(SCSS)`
   -  `Do you want to enable Server-Side Rendering (SSR) and Static Site Generation (SSG/Prerendering)?: No`

   -  ` ng serve`
   -  `ng`
   -  ` npm link @angular/cli`

###### 58. Reviewing the Angular project files

-  just and overview of basic fundamental files in angular

###### 59. Using HTTPS with the Angular project

-  [Mkcertificate](https://github.com/FiloSottile/mkcert)
-  gitbash: choco install mkcert
-  `cd client`
-  `mkdir ssl`
-  `mkcert localhost`

```
// optional // switching to MS-Edge browser | chrome to test https works better than firefox
mkcert -install
```

-  `client/angular.json`

```
"serve":{
    //..
    "options":{
        "ssl": true,
        "sslCert":"ssl/localhost.pem",
        "sslKey":"ssl/localhost-key.pem"
    },
    //...
}
```

-  `ng serve`

##### Angular Styling Configuration:

###### 60. Adding Angular Material and Tailwind CSS

-  [Angular Material UI](https://material.angular.io/)
-  [Angular Material UI](https://material.angular.io/components/categories)

-  `cd client/ then => ng add @angular/material`
-  `The package @angular/material@19.2.11 will be installed and executed.
Would you like to proceed? : YES`
-  `Choose a prebuilt theme name, or "custom" for a custom theme: Azure/Blue`
-  `Set up global Angular Material typography styles?: No`
-  `Include the Angular animations module?: Yes`

###### Installing Tailwind CSS In Angular Project

-  [Install Tailwind CSS with Angular](https://tailwindcss.com/docs/installation/framework-guides/angular)

-  side note: For Utility Classes
-
-  `npm install -D tailwindcss postcss autoprefixer // it does not work anymore`
-  `npx tailwindcss init // it does not work anymore`

-  [How to set up Angular & Tailwind CSS 4 in VS Code with Intellisense](https://www.youtube.com/watch?v=s-TAV5pQfcU)
-  `npm install tailwindcss @tailwindcss/postcss postcss --force `
-  `create client/.postcssrc.json

```
{
  "plugins": {
    "@tailwindcss/postcss": {}
  }
}
```

-  `update client/src/style.scss

```
@import "tailwindcss";

/* code on top is working as of this current try out April 25, 2025 */

/*
Code below: No longer works as of this current testing & experimentation

@tailwind base;
@tailwind components;
@tailwind utilities;

*/
```

###### 61. Adding VS Code extensions for Angular and Tailwind

-  `Extension : Angular Language Service: by angular.dev`
-  ` Extension : Tailwind CSS IntelliSense: by Tailwind Labs tailwindcss.com // For some reason - I restarted the extension but I turns off its auto suggestions`

###### 62. Summary

-  `Installing the Angular CLI`
-  `Creating the Angular project`
-  `Setting up VS Code for Angular*`
-  `Setting up Angular to use HTTPS`

<hr>

### Section 8: Angular Basics

<hr>

-  `Adding components`
-  `Http client module`
-  `Observables => note: doesn't use promise base but observables - its benefits, usage; also there is abetter tools SignalR `
-  `Typescript: => type safety like C# and Java better for development at earlier stage`

```
Goal:

To be able to use the http client to retrieve data from the API.

To understand the basics of observables and Typescript

```

###### 64. Setting up the folder structure and creating components

-  `create src/app/core/`
-  `create src/app/shared/`
-  `create src/app/features/`
-  `create src/app/layout/`

```
cd client/
angular cli commands

- ng help
- ng generate help
- ng g c layout/header --dry-run
- ng g c layout/header --skip-tests // skip the test file

    - `app/layout/header.component.html `
    - `app/layout/header.component.scss `
    - `app/layout/header.component.ts `

- vsCode: Explorer: Compact Folders - uncheck
```

###### 65. Adding a Header component

-  `udpate client/src/app/layout/header.component.html`

```
<header class="border-b p-3 w-full ">
  <div class="flex align-middle items-center justify-between max-w-screen-2xl mx-auto">
     <img src="/images/logo.png" alt="app logo" class="max-h-16" />
     <nav class="flex gap-3 my-2 uppercase text-xl">
      <a>Home</a>
      <a>Shop</a>
      <a>Contact</a>
     </nav>
     <div class="flex gap-3 align-middle">
       <a matBadge="5" matBadgeSize="large">
         <mat-icon>shopping_cart</mat-icon>
        </a>
      <button mat-stroked-button>Login</button>
      <button mat-stroked-button>Login</button>
     </div>
  </div>
</header>
```

-  `udpate client/src/app/layout/header.component.ts`

```
import { Component } from '@angular/core';
/* in oder to use the material ui components it need to be importe first then*/
import { MatIcon} from '@angular/material/icon';
import { MatButton } from '@angular/material/button';
import { MatBadge } from '@angular/material/badge';

@Component({
  selector: 'app-header',
  imports: [
    /* then add the import here but there should be a auto add imports if vs-code extension works well*/
    MatIcon,
    MatButton,
    MatBadge
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

}
```

###### 66. Improving the header component

-  `add images like logo in client/public`
-  `if a logo recently added would not show - a restart on ng serve will suffice as first step to troubleshoot it`

-  `update file header.component.scss`

```
//...
    <div class="flex gap-3 align-middle">
    <a matBadge="5" matBadgeSize="large" class="custom-badge mt-2 mr-2"> // added class="custom-badge mt-2 mr-2"
//...
```

-  `update file header.component.scss`

```
// for styling of number icon sign badge on top of the cart icon
.custom-badge .mat-badge-content{
  width: 24px;
  height: 24px;
  font-size: 14px;
  line-height: 24px;
}

// for styling the cart icon size
.custom-badge .mat-icon{
  font-size: 32px;
  width: 32px;
  height: 32px;
}
```

-  `styling global css in angular  `

-  `client/src/styles.css `

```
@use '@angular/material' as mat; /*1*/

@import "tailwindcss";

$customTheme: mat.define-theme(); /*2*/

@include mat.core(); /*3*/

/* 4 */
.custom-theme {
  @include mat.all-component-themes($customTheme);

  // override the button styles
  .mdc-button, .mdc-raised-button, .mdc-stroke-button, .mdc-flat-button {
    @apply rounded-md; /* Equivalent to Tailwind's rounded-md */
  }
}
```

-  `then implement it in the entire website/app via top level html `
-  `client/src/index.html`

```
/* add the custom-theme class */
<html lang="en" class="custom-theme">
```

-  ` incouter some issue: Visual studio code  : Git: fatal: unable to access 'https:/github.com/wilfred05777/skinet2025-NC.git/': Could not resolve proxy: proxy.server.com`

```
@ Git Bash terminal:

Fix Options:

✅ Option 1: Remove Proxy Configuration in Git
Run these commands in your terminal to remove the proxy settings:

git config --global --unset http.proxy
git config --global --unset https.proxy

✅ Option 2: Check Your Git Proxy Settings
To see if Git is using a proxy:

git config --global --get http.proxy
git config --global --get https.proxy

```

###### 67. Making http requests in Angular

-  ["Installing Node using NVM"](https://gist.github.com/MichaelCurrin/5c2d59b2bad4573b26d0388b05ab560e)
-  `nvm list`
-  `nvm install 22.15`
-  `nvm use 22.5`

-  `update client/src/app/app.config.ts`

```
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    //...
    provideHttpClient(),
  ],
};
```

-  `update client/src/app/app.component.ts`

```
//...
import { Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

//...
export class AppComponent {
  //...
  baseUrl = 'https://localost:5001/api/'
  private http = inject(HttpClient);
  //...

}

```

-  /_ implementing observable app.components.ts_/

```
ngOnInit():void {
  this.http.get(this.baseUrl + 'products').subscribe({
    next: data => console.log(data),
    error: error => console.log(error),
    complete: () => console.log('complete')
  })
}
```

-  `to test go broswer console`
-  ` encounter error - Cross Origin Request Blocked: on laptop`
-  `update note: working sa desktop node --version 22.13.1`

-  ` /* Display actual product from API to UI(client - angular project) */`
-  `update app.component.ts`

```
export class AppComponent implement OnInit{
  //...
  products: any[] = [];

  //ngOnInit()...
  this.http.get<any>(this.baseUrl + 'products').subscribe({
    next: response => this.products = response.data,
  })

}
```

-  `update client/src/app/app.component.html`

```
/* container will be used connect the global styling*/
<div class="container mt-6">
  <h2 class="text-3xl font-bold underline">Welcome to {{ title }}</h2>

  <ul>
    /* it will loop the product and display each using li*/

    @for(product of products; track product.id){
      <li>{{ product.name }}</li>
    }

  </ul>
</div>
```

-  `update client/src/styles.scss`

```
//...
.container {
  @apply mx-auto max-w-screen-2xl
}
```

###### 68. Introduction to observables

-  `Note:`

```
Observable -
A sequence of items that arrive asynchronously over time.
- counter part of promises in javascript

    Promises                vs          Obeservables
    -Has one pipeline                   - Are cancellable
    -Typically used with                - Stream data in multiple pipelines
    -async data return                  - Arrays like operations
    -Not easy to cancle                 - Can be created from other sources like events
                                        - They can be subscribed to

Promises
                         ------->  Succeed(data)
                        /
==================< Then
                        \
                         ------->  Fail


Observables                  Cancel
                         -------X
                        /
==================< Then ======================
                        \  Fail
                          --------------|------------|-------|--> data
                           Succeed   subscribe()   map()  filter()

```

-  `Observable`

```
 _____________________      GET api/products            ________________
|                     |------------------------------->|               |
| Angular HTTP Client |                                |      API      |
|_____________________|<-------------------------------|_______________|
 Observable of Products[]  HTTP Response: Products[]
            \
             \
              \  Subscribe
               \ __________________                 ____________________
                |                  |                |                   |
                | Shop Component   |                | Displays data in  |
                |__________________|                |   the Browser     |
                                                    |___________________|
```

-  `HTTP, Observables and RxJS`

```
1. HTTP Get request from ShopService
2. Receive the Observable and cast it into a Products Array
3. Subscribe to the Obeservable from the component
4. Assign the Products array to a local variable for use in the components template
```

-  `RxJS`

```
    -Reactive Extensions for JavaScript
    -Utility library for working with observables, similar to lodash or
     underscore for javascript objects and arrays.
    -Uses the pipe() method to chain RxJS operators together
```

###### 69. Introduction to TypeScript

-  `Typescript`

```
    Pros - Typescript Rocks!        and             Cons - Typescript is Annoying!
    -Strong Typing                                  -More upfront code
    -Object Oriented                                -3rd party libraries
    -Better intellisense                            -Strict mode is...strict! (insurance)
    -Access Modifiers
    -Future Javascript features
    -Catches silly mistakes in
    development
    -3rd party libraries
```

###### 70. Typescript demo

-  `create client/src/demo.ts `

```
/* superset of javascript */

/* superset of javascript */

// let message: string | number = "Hello";
let message = "Hello"; // inferred type
// let isComplete: boolean = false;
let isComplete = false;

// message = 42; // This is allowed because message can be a string or a number

// isComplete = 1;


/* interface Todo {
  {
    id: number;
    title: string;
    completed: boolean;
  }
  they are equivalent to type 'Todo = {' below
*/


type Todo = {
  id: number;
  title: string;
  completed: boolean;
}


let todos: Todo[] = [];

function addTodo(title: string): Todo {
  const newTodo: Todo ={
    id: todos.length + 1,
    title: title,
    completed: false
  }
  todos.push(newTodo);
  return newTodo;
}


/* if function does not return anything we return 'void' */
function toggleTodo(id: number): void {
  const todo  = todos.find(todo => todo.id === id);
  if(todo){
    todo.completed = !todo.completed;
  }

}


addTodo("Learn TypeScript");
addTodo("Publish app");
toggleTodo(1);


console.log(todos);

// ` to compile it  - npx tsc src/demo.ts

```

###### 71. Using Types in our Project

-  `creating Product models -> client/src/app/shared/models/products.ts`

```
export type Product = {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  brand: string;
  type: string;
  quantityInStock: number;
}

// in interface Product
export interface Product1 {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  brand: string;
  type: string;
  quantityInStock: number;
  rating: number;
  reviewsCount: number;
  isFavorite: boolean;
}
```

-  `creating Paginantion models -> client/src/app/shared/models/pagination.ts`

```
//<T> is a generics the same with c# but it can also be used in Typescript
export type Pagination<T> = {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}
```

-  `update src/app/app.component.ts`

```
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { HttpClient } from '@angular/common/http';
import { Product } from './shared/models/products';
import { Pagination } from './shared/models/pagination';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  baseUrl = 'https://localhost:5001/api/'
  private http = inject(HttpClient);
  title = 'Skinet';
  // products:any[] = [];

    /* instead of any[] we will be using/implementing the Product[] model */
  products: Product[] = [];

  ngOnInit():void {
    /* implementing Pagination and Product models */
    this.http.get<Pagination<Product>>(this.baseUrl + 'products').subscribe({

      // next: data => console.log(data),

      /* reponse. will give as access to data, count, pageIndex, pageSize */
      next: response => this.products = response.data,

      error: error => console.log(error),

      complete: () => console.log('Complete')
    })
  }
}
```

-  `update client/src/app/app.component.html`

```
<app-header></app-header>
<router-outlet></router-outlet>

<div class="container mt-6">
  <h2 class="text-3xl font-bold underline">Welcome to {{ title }}</h2>
  <ul>
    @for (product of products; track product.id){
      <li>{{ product.name }}</li>
    }
  </ul>
</div>
```

###### 72. Summary

-  `Goal:`

```
Goal:
To be able to use the http client to retrieve data from the API

To understand the basics of obeservables and Typescript
```

-  `FAQs`

```
    Question:
    Can I use <CssFramework>
    instead of Angular Material and Tailwind?

    Answer:
    Only with great difficulty.
    This is heavily intergrated into the app.
```

-  ` fixing Tailwind Compile issue at global style.scss`

```
/* Fixing tailwind import issue = FF Solution with chatgpt's help */

// @import "tailwindcss";
@use "tailwindcss" as *;
```

<hr>

### Section 9: Building the User Interface for the shop

<hr>

-  ` 73. Introduction : Client Building the UI`

```
    -Angular services (AS) - Singleton, for this project we don't redux (state management) to remember things utilize AS for this project but we can if we want to.
    -Building the UI for the shop
    -Material components
    -Pagination
    -Filtering, Sorting & Search
    -Input properties - child components

```

###### 74. Introduction to Angular services

-  `create angular service for reusable logic, ng service is a singleton,`
-  `' ng g service core/services/shop.services.ts ' => go to core component <---/* working */`
-  ` ' ng g service core/services/shop.services.ts ' --dry-run <- this command will generate service at app/core/services/shop <---/* not working */`
-  ` ' ng g service core/services/shop.services.ts '  --dry-run --skip-tests <---/* not working */ <- this command will generate service at app/core/services/shop.services.ts`

-  ` create client/src/app/core/services/shop.services.ts`

```
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  baseUrl = 'https://localhost:5001/api/'
  private http = inject(HttpClient);

  constructor() { }
}
```

-  ` update app/app.component.ts`

```
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { Product } from './shared/models/products';
import { ShopService } from './core/services/shop.service';
import { Pagination } from './shared/models/pagination';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  /* implement shop service */
  private shopService = inject(ShopService);

  /* refactor the code for service method
  baseUrl = 'https://localhost:5001/api/'
  private http = inject(HttpClient);
  */
  title = 'Skinet';
  products: Product[] = [];

  ngOnInit():void {

    // this.http.get<Pagination<Product>>(this.baseUrl + 'products').subscribe({

    this.shopService.getProducts().subscribe({
      // return service from shop.service.ts to fix the error
      next: response => this.products = response.data,
      error: error => console.log(error),

      // complete: () => console.log('Complete')
    })
  }
}

```

-  ` update client/src/app/core/services/shop.services.ts`

```
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Pagination } from '../../shared/models/pagination';
import { Product } from '../../shared/models/products';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  baseUrl = 'https://localhost:5001/api/'
  private http = inject(HttpClient);

  getProducts() {
    // add a return to make the method be identified in the app.component.ts
    return this.http.get<Pagination<Product>>(this.baseUrl + 'products')
  }
}
```

###### 75. Designing the shop page

-  ` create features/shop.component.ts`
-  `ng g component features/shop --dry-run / --skip-tests`
-  ` shop.component.ts`

```
import { Component, inject } from '@angular/core';
import { ShopService } from '../../core/services/shop.service';
import { Product } from '../../shared/models/products';

@Component({
  selector: 'app-shop',
  imports: [],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent {  /* implement shop service */
  private shopService = inject(ShopService);
  products: Product[] = [];

  ngOnInit():void {
    this.shopService.getProducts().subscribe({
      next: response => this.products = response.data,
      error: error => console.log(error)
    })
  }
}
```

-  `update src/app/app.component.ts`

```
// cutting code below:
/* implement shop service */

  private shopService = inject(ShopService);

  title = 'Skinet';
  products: Product[] = [];

  ngOnInit():void {
    this.shopService.getProducts().subscribe({
      next: response => this.products = response.data,
      error: error => console.log(error)
    })
  }
```

-  `update app.component.html`

```
<app-header></app-header>
<router-outlet></router-outlet>

<div class="container mt-6">
  <app-shop></app-shop>
</div>

<!-- old code below -->
<!-- <div class="container mt-6">
  <app-shop></app-shop>
  <h2 class="text-3xl font-bold underline">Welcome to {{ title }}</h2>
  <ul>
    @for (product of products; track product.id){
      <li>{{ product.name }}</li>
    }
  </ul>
</div> -->
```

-  `update src/app/app.component.ts`

```
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { Product } from './shared/models/products';
import { ShopService } from './core/services/shop.service';
import { ShopComponent } from "./features/shop/shop.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ShopComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = "Skinet";
}
```

-  ` client/src/app/features/shop/shop.component.ts`

```

```

-  ` client/src/app/features/shop/shop.component.html`

```
                /* grid-cols-5 display five columns in UI*/
<div class="grid grid-cols-5 gap-4">
  @for (product of products; track product.id) {
    <mat-card appearance="raised">
      <img src="{{  product.pictureUrl }}" alt="alt imge of {{ product.name }}" class="w-full h-48 object-cover" />
    </mat-card>
  }
</div>
```

-  ` update shop.component.ts`

```
import { Component, inject } from '@angular/core';
import { ShopService } from '../../core/services/shop.service';
import { Product } from '../../shared/models/products';
import { MatCard } from '@angular/material/card';

@Component({
  selector: 'app-shop',
  imports: [
    MatCard
  ],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent {  /* implement shop service */
  private shopService = inject(ShopService);
  products: Product[] = [];

  ngOnInit():void {
    this.shopService.getProducts().subscribe({
      next: response => this.products = response.data,
      error: error => console.log(error)
    })
  }
}
```

-  `update client/src/app/core/services/shop.services.ts`

```
    getProducts() {

    new return this.http.get<Pagination<Product>>(this.baseUrl + 'products?pageSize=20')

    <!--old return this.http.get<Pagination<Product>>(this.baseUrl + 'products') -->

  }
```

###### 76. Adding a product item component

-  `ng g c features/shop/product-item --skip-tests`

```
    - src/app/features/shop/product-item/product-item.component.ts
    - src/app/features/shop/product-item/product-item.component.scss
    - src/app/features/shop/product-item/product-item.component.html
```

-  `src/app/features/shop/product-item/product-item.component.ts`

```
import { Component, Input } from '@angular/core';
import { Product } from '../../../shared/models/products';

@Component({
  selector: 'app-product-item',
  imports: [

  ],
  templateUrl: './product-item.component.html',
  styleUrl: './product-item.component.scss'
})
export class ProductItemComponent {
  @Input() product?: Product;
}
```

-  `update product-item.component.html`

```
@if (product) {
  <mat-card appearance="raised">
    <img src="{{  product.pictureUrl }}" alt="alt imge of {{ product.name }}" class="w-full h-48 object-cover" />
  </mat-card>
}
```

-  ` update import @ shop.component.ts`

```
@Component({
  selector: 'app-shop',
  imports: [
    MatCard,
    ProductItemComponent // <-- import to shop.component.ts
],
```

-  ` update product-item.component.ts`

```
import { MatCard, MatCardContent } from '@angular/material/card';

@Component({
  selector: 'app-product-item',
  imports: [
    MatCard, // <-- import
    MatCardContent, // <-- import
    CurrencyPipe, // <-- import
    MatCardActions, // <-- import
    MatButton, // <-- import
    MatIcon // <-- import
  ],
})
```

-  `updae product-item.component.html`

```
@if (product) {
  <mat-card appearance="raised">
    <img src="{{  product.pictureUrl }}" alt="alt imge of {{ product.name }}" class="w-full h-48 object-cover" />

    <!-- udpated code start -->
    <mat-card-content class="text-sm font-semibold uppercase">
      {{ product.name }}
      <p class="font-light">{{ product.price | currency }}</p>
    </mat-card-content>
    <mat-card-actions>
      <button mat-stroked-button class="w-full">
        <mat-icon>add_shopping_cart</mat-icon>
        Add to cart
      </button>
    </mat-card-actions>
    <!-- udpated code end -->

  </mat-card>
}
```

-  `API/Program.cs`

```
/*
Cross-Origin Request Blocked: The Same Origin Policy disallows reading the remote resource at https://localhost:5001/api/products?pageSize=20. (Reason: CORS request did not succeed). Status code: (null)
*/
//// desktop working and not working in laptop
// app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
//     .WithOrigins("http://localhost:4200", "https://localhost:4200"));

// laptop testing config
app.UseCors(x => x
    .WithOrigins("http://localhost:4200", "https://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod());

```

###### 77. Getting the types and brands lists

-  `update client/src/app/core/services/shop.services.ts`

```
export class ShopService {
    //...
    types: sting[] = [];
    brands: string[] = []

    //...
    getBrands(){
        if(this.brands.length > 0) return;
        return this.http.get<string[]>(this.baseUrl + 'products/brands').subscribe({
            next: response => this.brands = response
        })
    }

    getTypes(){
        if(this.types.length > 0) return;
        return this.http.get<string[]>(this.baseUrl + 'products/types/').subscribe({
            next: response => this.types = reponse
        })
    }
}
```

-  ` update client/src/app/features/shop/shop.components.ts`

```
export class ShopComponent implements OnInit {

    //...
    initializeShop() {
        this.shopService.getBrands();
        this.shopService.getTypes();
        this.shopService.getProducts().subscribe({
            next: response => this.products = response.data,
            error: error => console.log(error)
        })
    }
}
```

-  `second update client/src/app/features/shop/shop.components.ts`

```
export class ShopComponent{

    //...

    ngOnInit():void{
        this.initializeShop(); /* <-- insert the initializeShop() function here */

        /* this will be move to initializeShop(){} see below
        this.shopService.getProducts().subscribe({
         next: response => this.products = response.data,
        error: error => console.log(error)
        })
    }

    initializeShop(){
        this.shopService.getBrands();
        this.shopService.getTypes();

        /* this code below comes from ngOnInit from the top */
        this.shopService.getProducts().subscribe({
        next: response => this.products = response.data,
        error: error => console.log(error)
        })
    }

}
```

###### 78. Adding the filtering functionality using Material Dialog

[Angular MAterial Ui Dialog](https://material.angular.io/components/dialog)

-  `create dialog cd client - ng g c features/shop/filters-dialog --skip-tests`

-  `client/src/app/features/shop/filters-dialog/filters-dialog.components.ts`

```
//...
import { ShopService } from '../../../core/services/shop.services';
import { MatDivider } from '@angular/material/divider';
import { MatListOption, MatSelectionList } from '@angular/material/list';
import { MatButton } from '@angular/material/button';

@Component({
    //...
    imports:[
        MatDivider,
        MatSelectionList,
        MatListOption,
        MatButton
    ]
})
//...
export class FiltersDialogComponent {
    shopService = inject(shopService)
}
```

-  `update filters-dialog.components.html `

```
<div>
    <h3 class="text-3xl text-center pt-6 mb-3"> Filters</h3>
    <mat-divider></mat-divider>
    <div class="flex p-4">
        <div class="w-1/2>
            <h4 class="font-semibold text-xl text-primary">
                Brands
            </h4>
            <mat-selection-list>
                @for (brand of shopService.brands; track $index) {
                    <mat-list-option>{{brand}}</mat-list-option>
                }
            </mat-selection-list>
        </div>

        <div class="w-1/2>
            <h4 class="font-semibold text-xl text-primary">
                Types
            </h4>
            <mat-selection-list>
                @for (type of shopService.types; track $index) {
                    <mat-list-option>{{ type}}</mat-list-option>
                }
            </mat-selection-list>
        </div>
    </div>

    <div class="flex justify-end p-4">
        <button mat-flat-butotn>Apply Filters</button>
    </div>
</div>
```

###### 79. Adding the filtering functionality using Material Dialog part 2: FF

-  `update shop.components.ts`

```
//...
import { MatDialog } from '@angular/material/dialog';
import { FiltersDialogComponent } from './filters-dialog/filters-dialog.component';
import { MatButton } from '@angular/material/butotn';
import { MatIcon } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';

@component({
    //...
    imports: [
        //...
        ProductItemComponent,
        MatButton,
        MatIcon
    ]
})

//...
export class ShopComponent{
    //...
    private dialogService = inject(MatDialog)
    //...

    //...
    openFiltersDialog(){
        const dialogRef = this.dialogSerice.open(FiltersDialogComponent, {
            minWidth: '500px'
        })
    }
}
```

-  `update shop.component.html`
-  `google search: Material Symbols and Icons`
   [Material Design Icon](https://fonts.google.com/icons)

```
<div class="flex flex-col gap-3">
    <div class="flex justify-end>
        <button mat-stroked-button (click)="openFiltersDialog()">
            <mat-icon>filter_list</mat-icon>
            Filters
        </button>
    </div>

    <div class="grid grid-cols-5 gap-4">
        @for (product of products; track product.id){
            <app-product-item [product]="product"></app-product-item>
        }
    </div>
</div>
//...
```

###### 80. Hooking up the filters to the service

-  `update filters-dialog.component.ts`

```
//...
import { FormsModule } from '@angular/forms';

@component({
    //...
    imports:[
        //...
        FormsModule
    ]
})
```

-  `update filters-dialog.components.html`

```
/*
    # notes: about square brackets & parenthesis in angular
    #1 square brackets in angular [] represents input properties *
    #2 parethesis represents output properties or events in angular *
*/

<div class="flex p-4">
    <div class="w-1/2">
        //...
        <mat-selection-list [(ngModel)]="selectedBrands" [multiple]="true">
            @for (brand of shopService.brands; track $index){
            <mat-list-option [value]="brand">{{brand}}</mat-list-option>
            }
        </mat-selection-list>
        //...

    <div class="w-1/2">
            //...
        <mat-selection-list [(ngModel)]="selectedTypes" [multiple]="true">
                @for (type of shopService.types; track $index){
                    <mat-list-option [value]="type">{{type}}</mat-list-option>
                }
        </mat-selection-list>
    //...

    //...
    <div class="flex justify-end p-4">
                            /* (click)="applyFilters()" here for click event */
        <button mat-flat-button (click)="applyFilters()">Apply Filter</button>
    </div>
</div>
```

-  `update shop.service.ts`

```
    //...
    getProducts(brands?: string[], types?: string[]) {
        let params = new HttpParams();

        if (brands && brands.length > 0 ) {
        params = params.append('brands', brands.join(','));
        }

        if (types && types.length > 0 ) {
        params = params.append('types', types.join(','));
        }
        params = params.append('pageSize', 20 ) //  filters to show 20 items with angular params

        return this.http.get<Pagination<Product>>(this.baseUrl + 'products', {params})
        // return this.http.get<Pagination<Product>>(this.baseUrl + 'products?pageSize=20') // filters to show 20 items with api usage i'm not really sure what to call this ATM
    }
    //...
```

-  ` update shop.components.ts`

```
    //...

    openFiltersDialog(){

        //...
        dialogRef.afterClosed().subscribe({
            next: result =>{
                if(result) {
                    //...
                    //apply filters
                    this.shopService.getProducts(this.selectedBrands, this.selectedTypes).subscribe({
                        next: response => this.products = response.data,
                        error: error => console.log(error)
                    })
                }
            }
        })
    }
    //...

```

###### 81. Adding the sorting functionality

-  `update shop.components.ts`

```
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';
import { MatSelectionList, MatSelectionList, MatSelectionListChange } from '@angular/material/list';

@Component({
    imports: [
        //...
        MatMenu,
        MatSelectionList,
        MatListOption,
        MatMenuTrigger
    ]
})

export class ShopComponent{
    //...
    selectedSort: string= 'name';
    sortOptions =[
        { name: 'Alphabetical', value: 'name' },
        { name: 'Price: Low-High', value: 'priceAsc' },
        { name: 'Price: High-Low', value: 'priceDesc' },
    ]

    //...
    //initialiseShop(){
        this.shopService.getTypes();
        this.shopService.getBrands();
        this.getProducts(); // updated code

        //... code from here and move inside getProducts(){ move here code below }
        // move to getProducts(){ }
        this.shopService.getProducts().subscribe({
            next: resonse => this.products = response.data,
            error: error => console.error(erorr)
        })
    }

    /* updated */
    getProducts(){
        // these code below comes from initialiseShop(){ // imsert here }

        this.shopService.getProducts(this.selectedBrands, this.selectedTypes, this.selectedSort).subscribe({
            next: resonse => this.products = response.data,
            error: error => console.error(erorr)
        })
    }

    onSortChange(event: MatSelectionListChange){
        const selectedOption = event.options[0]; // grab the first elemen on the list [0]
        if(selectedOption){
            this.selectedSort = selectedOption.value;
            this.getProducts(); // updated code
            console.log(this.selectedSort); /* optional for console testing*/
        }
    }

    openFiltersDialog(){
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.selectedBrands,
        selectedTypes: this.selectedTypes
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if(result) {
          // console.log(result);

          this.selectedBrands = result.selectedBrands;
          this.selectedTypes = result.selectedTypes;
          this.getProducts(); /* <-- update code here *********/

          // apply filters
          /*
          this.shopService.getProducts(this.selectedBrands, this.selectedTypes).subscribe({
            next: response => this.products = response.data,
            error: error => console.log(error)
          })
          */
        }
      }
    });
  }
}
```

-  `update shop.components.html`

```
<div class="flex justify-end gap-3">
   //...

    <button mat-stroked-button [matMenuTriggerFor]="sortMenu">
      <mat-icon>swap_vert</mat-icon>
      Sort
    </button>
</div>

<!-- below -->
<mat-menu #sortMenu="matMenu">
    <mat-selection-list [multiple]="false" (selectionChange)="onSortChange($event)">
        @for (sort of sortOptions; track $index){
            <mat-list-option [value]="sort.value" [selected]="selectedSort === sort.value">
                {{ sort.name }}
            </mat-list-option>

        }
    <mat-selection-list>
</mat-menu>

```

-  `update shop.service.ts`

```
//...
getProducts(//..., //..., sort?: string){
    //...if

    if(sort){
        params = params.append('sort', sort)
    }
    // params
}

//...
```

###### 82. Using a class to supply the parameters for the API request

-  [Angular Paginator](https://material.angular.dev/components/paginator/overview)

-  `create client/src/app/shared/models/shopParams.ts`

```
export class ShopParams {
    brands string[] = []
    types string[] = []
    sort ='name';
    pageNumber = 10;
    pageSize = 10;
    search = '';
}
```

-  `update client/src/app/features/shop/shop.components.ts`

```

export class ShopComponents {
    // selectedBrands: string[] = []; // 4th update removed
    // selectedTypes: string[] = []; // 4th update removed
    //selectedSort: string = 'name' // 4th update removed

    //... sortOptions = [...]

    shopParams = new ShopParams(); // 1st update

    //... ngOnInit(){...}

    getProducts(){
        /* old
        this.shopService.getProducts(this.selectedBrands, this.selectedTypes, this.selectedSort)
        */

        /* 2nd update */
        this.shopService.getProducts(this.shopParams).subscribe({
            next: response => this.products = response.data;
            error: error => console.error(error)
        })
    }

    onSortChange(event: MatSelectionListChange){
        if(selectedOption){
            this.selectedSort = selectedOption.value;  // <== updated
            this.getProducts();
            console.log(this.selectedSort); /* removable console testing only */
        }
    }

    openFiltersDialog(){
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.shopParams.brands,
        selectedBrands: this.shopParams.types
        //selectedTypes: this.selectedTypes // 5th update
        //selectedTypes: this.selectedTypes // 5th update
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if(result) {
           this.shopParams.brands = result.selectedBrands;  //3rd update
           this.shopParams.types = result.selectedTypes;    //3rd update
        // this.selectedBrands = result.selectedBrands; // old
        // this.selectedTypes = result.selectedTypes; // old
          this.getProducts();
        }
      }
    });
  }
}
```

-  `update client/src/app/features/shop/shop.components.html`

```
<mat-menu #sortMenu="matMenu">
  <mat-selection-list [multiple]="false" (selectionChange)="onSortChange($event)">
    @for (sort of sortOptions; track $index){

      <!-- <mat-list-option [value]="sort.value" [selected]="selectedSort === sort.value"> -->
      mat-list-option [value]="sort.value" [selected]="shopParams.sort === sort.value">
        {{  sort.name }}
      </mat-list-option>
    }
  </mat-selection-list>
</mat-menu>

```

-  `update client/src/app/core/services/shop.services.ts`

```

import { ShopParams } from '../../shared/models/shopParams';

export class ShopService {
  baseUrl = 'https://localhost:5001/api/';
  private http = inject(HttpClient);
  types: string[] = [];
  brands: string[] = [];

  getProducts(shopParams: ShopParams) { // update
  // getProducts(brands?: string[], types?: string[], sort?: string) { // old

    let params = new HttpParams();

    if (shopParams.brands && shopParams.brands.length > 0) { // update

//  if ( shopParams.brands.length > 0) { // NC solution - update
//  if (brands && brands.length > 0) {

      params = params.append('brands', shopParams.brands.join(',')); // update
      //params = params.append('brands', brands.join(',')); // old

    }

    if (types && types.length > 0) { // updated my solution

    //  if (shopParams.length > 0) { // NC solution update
    //  if (types && types.length > 0) { // original state

    params = params.append('types', shopParams.types.join(',')); // updated
    //params = params.append('types', types.join(',')); // old

    }

    if (shopParams.sort) { // updated
    //if (sort) { // old

    params = params.append('sort', shopParams.sort); // updated
    //params = params.append('sort', sort); // old

    }

    params = params.append('pageSize', '20');

    return this.http.get<Pagination<Product>>(this.baseUrl + 'products', { params });
  }

  getBrands() {
    if (this.brands.length > 0) return;
    return this.http.get<string[]>(this.baseUrl + 'products/brands').subscribe({
      next: response => (this.brands = response),
    });
  }

  getTypes() {
    if (this.types.length > 0) return;
    return this.http.get<string[]>(this.baseUrl + 'products/types').subscribe({
      next: response => (this.types = response),
    });
  }
}

```

###### 83. Adding pagination to the client using Material

-  `update client/src/app/core/services/shop.serives.ts`

```
export class ShopService{
    getProducts(shopParams: ShopParams){
        //... if(shopParams.srot){ ... }

        params = params.append('pageSize', shopParams.pageSize); // update
        params = params.append('pageIndex', shopParams.pageNumber); // update
        //params = params.append('pageSize', '20'); // old

        //return this.http.get<Pagination>
    }
}
```

-  `update client/src/app/features/shop/shop.component.ts `

```
import { MatPaginator } from '@angular/material/paginator'; // update

@Component({
    //...
    imports: [
        //...,
        MatPagninator // update
    ]
})
```

-  `update client/src/app/features/shop/shop.component.html `

```
<div class="flex flex-col">
 <div class="flex justify-between"> <!--update-->
    <mat-paginator
        class="bg-white"
        (page)="handlePageEvent($event)"
        [length]="products?.count"  // update
        <!--[lenght]="totalCount" old -->

        [pageSize]="showParams.pageSize" // update
        [showFirstLastButtons]="true" // update
        [pageSizeOptions]="pageSizeOptions"  // update
        [pageIndex]="shopParams.pageNumber - 1 " // update
        arial-label="Select page" // update
    >
    </mat-paginator>

 <!-- <div class="flex justify-end gap-3"> old-->
    <mat-paginator
      (page)="handlePageEvent($event)"
      [lenght]="totalCount"
    >

    <div class="flex gap-3">
        <div class="flex justify-between gap-3"> <!-- update -->
        <!-- <div class="flex justify-end gap-3"> old -->
            <button mat-stroked-button (click)="openFiltersDialog()">
                <mat-icon>filter_list</mat-icon>
                    Filters
            </button>
            <button mat-stroked-button [matMenuTriggerFor]="sortMenu">
                <mat-icon>swap_vert</mat-icon>
                    Filters
            </button>
        </div>
    </div>
</div>
```

-  `update client/src/app/features/shop/shop.component.ts `

```
import { MatPaginator, PageEvent } from '@angular/material/paginator'; // 4th update auto import

import { Pagination } from '../../shared/models/pagination'; // 2nd update auto import

export Class ShopComponent {
    //... private dialogService = inject(MatDialog);

    products?: Pagination<Product>; // 2nd update
    // old products: Product[] = [];

    //... sortOptions = [...]

    //... shopParams = new ShopParams();
    pageSizeOptions = [5, 10, 15, 20] // 3rd update

    //...

    getProducts(){
        this.shopService.getProducts(this.shopParams).subscribe({
            next: response => this.products = response, // 1st update
        //  next: response => this.products = response.data, // legacy
            error: error => console.log(error)
        })
    }

    // 4th update below
    handlPageEvent(event: PageEvent){ //4th update
        this.shopParams.pageNumber = event.pageIndex + 1; //4th update
        this.shopParams.pageSize = event.pageSize; //4th update
        this.getProducts(); //4th update
    } //4th update

    onSortChange(event: MatSelectionListChange){
    const selectedOption = event.options[0]; // grab the first elemen on the list [0]
    if(selectedOption){
      this.shopParams.sort = selectedOption.value;
      this.shopParams.pageNumber = 1; // update
      this.getProducts();
    }
  }

  openFiltersDialog(){
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.shopParams.brands,
        selectedTypes: this.shopParams.types,
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if(result) {
          // console.log(result);
          this.shopParams.brands = result.selectedBrands;
          this.shopParams.types = result.selectedTypes;
          this.shopParams.pageNumber = 1; // update
          this.getProducts();
        }
      }
    });
  }
}
```

-  `update client/src/app/features/shop/shop.component.html `

```
<div class="flex flex-col gap-3">
    <div class="grid grid-cols-5 gap-4>
        @for (product of products?.data; track product.id) { /* update */
        <!-- @for (product of products; track product.id) { old -->
        <app-product-item [product]="product"></app-product-item>
    }
    </div>
</div>
```

-  `style default bg color of mat-paginator client/tailwind.config.ts`

```
module.exports = {
    content: [
        "./src/**/*.{html,ts}",
    ],
    theme: {
        extend: {},
    },
    plugins: [],
    important: true // for tailwind to take effect styling overiding material styles
}
```

###### 84. Adding the search functionality to the client

-  `Update shop.component.ts  `

```
@Component({
  selector: 'app-shop',
  imports: [
    //...
    FormsModule // update
],
})

export class ShopComponent {

    //... getProducts(){...}

    onSearchChange(){
        this.shopParams.pagenumber = 1
        this.getProducts();
    }

    //... handlePageEvent(event: PageEvent){...}
}
```

-  `Update shop.component.html `

```
<div class="flex flex-col gap-3">
  <div class="flex justify-between gap-3">

    //... </mat-paginator>

    <form
      #searchForm="ngForm"
      (ngSubmit)="onSearch()"
      class="relative flex items-center w-full max-w-md mx-4"
    >
      <input
          type="search"
          class="block w-full p-4 text-sm text-gray-900 border border-gray-300 rounded-lg"
          placeholder="Search"
          name="search"
          [(ngModel)]="shopParams.search"
      />
      <button mat-icon-button type="submit"
        class="absolute inset-y-0 right-8 top-2 flex items-center pl-3"
      >
        <mat-icon>search</mat-icon>
      </button>
    </form>

    // <div class="flex gap-3"></div>
  </div>
</div>
```

-  `Update shop.services.ts`

```
    //...if (shopParams.sort) { params = params.append('sort', shopParams.sort);  }

    if(shopParams.search) {
      params = params.append('search', shopParams.search);
    }

    //... params = params.append('pageSize', shopParams.pageSize);
```

-  `update styles.scss`

```
.text-primary{
  color: #7d00fa;
}

button.match-input-height {
  height: var(--mat-form-field-container-height) !important;
}

```

-  ` update shop.component.html`

```
// 'match-input-height' = aligns the input and button for search in UI
<button class="match-input-height" mat-stroked-button (click)="openFiltersDialog()">...
</button>

<button class="match-input-height" mat-stroked-button [matMenuTriggerFor]="sortMenu">...
</button>

```

-  `!Issue/Solution for not displaying all items in ui `

```
export class ShopParams {
  brands: string[] = [];
  types: string[] = [];
  sort = 'name';
  pageNumber = 1; // error when it is set to 10; correcting to 1;
  pageSize = 10;
  search = '';
}

```

###### 85. Summary

-  Angular Services
-  Building the UI for the loop
-  Material components
-  Pagination
-  Filtering, Sorting & Search
-  Input properties

<hr>

### Section 10: Angular Routing

<hr>

###### : 86. Introduction

-  Adding new feature components
-  Setting up routes
-  Nav links
-  This way to the shop!

###### 87. Creating components and routes

-  `cd client create 'ng g c features/home --skip-tests'`
-  `cd client create 'ng g c features/shop/product-detail --skip-tests'`

-  `client/src/app/app.routes.ts`

```
export const routes: Routes = [
    { path: '', component: HomeCompnent },
    { path: 'shop', component: ShopCompnent },
    { path: 'shop/:id', component: ProductDetailsCompnent },
    { path: '**', redirectTo: '', pathMatch: 'full' },
]
```

-  `tracing client/src/app/app.component.ts`

```
//
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ShopComponent],
  templateUrl: './app.component.html', // right click 'go to defination' in vs code
  styleUrl: './app.component.scss'
})
```

-  `tracing client/src/app/app.component.ts`

```
<app-header></app-header>
<div class="container mt-6">
  <router-outlet></router-outlet> // <!-- update here -->
</div>
```

-  `client/src/app/app.config.ts`

```
// nothing to change here but its a good way to understand the structure for routes in angular
```

-  for testing the routes
-  https://localhost:4200/
-  https://localhost:4200/shop
-  https://localhost:4200/shop/none

###### 88. Setting up the links in the app

-  `update client/src/app/layout/header/header.component.html`

```
<!-- ...  -->
    <img routerLink="/" src="/images/logo.png" alt="app logo" class="max-h-16" />
     <nav class="flex gap-3 my-2 uppercase text-2xl">
      <a routerLink="/"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{exact: true}"
        >Home</a>
      <a routerLink="/shop" routerLinkActive="active">Shop</a>
      <a routerLink="/contact" routerLinkActive="active">Contact</a>
     </nav>
<!-- ... -->
```

-  `update client/src/app/layout/header/header.component.ts`

```
import { RouterLink, RouterLinkActive } from '@angular/router';
@Component({
    .../,
    imports:[
        RouterLink,
        RouterLinkActive
    ]
})
```

-  `update client/src/app/layout/header/header.component.scss`

```
a {
    &.active{
        color: #7d00fa;
    }
}
```

-`client/src/app/features/shop/shop.component.html flickering issue`

```
@if(products) {
    //... insert all entire component code here and remove ? from products?.count
}
```

###### 89. Getting an individual product using Route params

-  `update shop.service.ts`

```
//... getProducts(shopParams: ShopParams){}

getProduct(id: number){
    return this.http.get<Product>(this.baseUrl + 'products/' + id);
}

//... getBrands(){... }
```

-  `update product-details.components.ts`

```
import { Component, inject, OnInit } from '@angular/core';

export class ProductDetailsComponent implements OnInit {

    // update
    /* 1st */
    private shopService = Inject(ShopService);
    private activatedRoute = Inject(ActivatedRoute);
    product?: Product;

    /* 3rd */
    ngOninit(): void {
        this.loadProduct();
    }

     /* 2nd */
    loadProduct(){
        const id = this.activatedRoute.snapshot.paramMap.get('id'); // its refering to id in app.routes.ts
        if(!id) return;
        this.shopService.getProduct(+id).subscribe({
            next: product => this.product = product,
            error: error => console.log(error)
        })
    }
}
```

-  `connection with app.routes.ts from produc-details.component.ts`

```
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'shop', component: ShopComponent },
  { path: 'shop/:id', component: ProductDetailsComponent }, // its refering to this id
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
```

-  `update product-details.components.html`

```
@if(product){
    <h1 class="text-2xl">{{ product?.name }}</h1>
}
    <h1 class="text-2xl">{{ product?.name }}</h1> // product?. is called optional chaining

```

-  `update product-item.component.ts`

```
@Component({
  selector: 'app-product-item',
  imports: [
    //...MatIcon,
    RouterLink // add
  ],
 //....
})
```

-  `update product-item.component.html`

```
@if (product) {
  <mat-card appearance="raised" routerLink="/shop/{{ prdouct.id }}" class="product-card"> //  update code
  </mat-card>
  //...
}
```

-  `update product-item.component.scss`

```
.product-card{
    transition: transform 0.2s, box-shadow 0.2s;
}
.product-card:hover{
    transform: translateY(-10px);
    box-shadow: 0 4px 8px rgba(0, 0 , 0, 0.2);
    cursor: pointer;
}
```

###### 90. Designing the product details page

-  `update product-details.component.ts`

```
@if(product){
    <section class="py-8">
        <div class="max-w-screen-2xl px-4 mx-auto">
            <div class="grid grid-cols-2 gap-8">
                <div class="max-w-xl mx-auto">
                    <img class="w-full" src={{product.pictureUrl}} alt="product image">
                </div>

                <div>
                    <h1 class="text-2xl font-semibold text-gray-900" >{{ product.name }}</h1>
                    <div class="mt-4 items-center gap-4 flex">
                        <p class="text-3xl font-extrabold text-gray-900">
                            {{  product.price | currency }}
                        </p>
                    </div>

                    <div class="flex gap-4 mt-6">
                        <button mat-flat-button class="match-input-height">
                            <mat-icon> shopping_cart </mat-icon>
                            Add to cart
                        </button>

                        <mat-form-field apperance="outline" class="flex">
                            <mat-label> Quantity </mat-label>
                            <input matInput type="number">
                        </mat-form-field>
                    </div>

                    <mat-divider></mat-divider>

                    <p class="mt-6 text-gray-500">
                        {{  product.description }}
                    </p>
                </div>

            </div>
        </div>
    </section>
}
```

-  `Update product-details.component.ts`

```
import { CurrencyPipe } from '@angular/common';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/materila/icon';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@components({
    //...
    imports:[
        CurrencyPipe,
        MatButton,
        MatIcon,
        MatFormField,
        MatInput,
        MatLabel,
        MatDivider,
    ]
})
```

-  `update styles.scss`

```
//... button.match-input-height{...}

.mdc-notched=outline__notch{
    border-right-style: none !important;
}
```

###### 91. Summary

-  Adding new feature components
-  Setting up routes
-  Nav links
-  Q: What about lazy loading?
   -  A: Optimization comes at the end, not the beginning

<hr>

### Section 11: Client Side error handling and loading

<hr>

###### 92. Introduction

-  `Error handling in Angular`
-  `Http interceptors`
-  `Adding toast notifications`
-  `Adding loading indicators`
-  `Goal`

```
Goal:

To handle errors we receive from the API
centrally and handled by the Http interceptor.

To understand how to troubleshoot API Errors

```

###### 93. Creating a test error component

-  `@ client create new component = ng g c features/test-erorr --skip-tests`

-  `update app.routes.ts`

```
//...
import { TestErrorComponent } from './features/test-error/test-error.component';

export const routes: Route =[
    {...},
    {path: 'test-error', component: TestErrorComponent},
    {...},
]
```

-  ` update header.component.html`

```
    <a routerLink="/test-error" routerLinkActive="active">Errors</a> //for testing purposes only
    <!-- <a routerLink="/contact">Contact</a>// old -->
```

-  ` update test-components.ts`

```
@Component({
    ...
    imports:[
        MatButton
    ],
    ....
})

export class TestErrorComponent{
    baseUrl = 'https://localhost:5001/api/';
    private http = inject(HttpClient);

    get404Error(){
        this.http.get(this.baseUrl + 'buggy/notfound').subscribe({
            next: response => console.log(reponse),
            error: error =>  console.log(error)
        })
    }

    get400Error(){
        this.http.get(this.baseUrl + 'buggy/badrequest').subscribe({
            next: response => console.log(reponse),
            error: error =>  console.log(error)
        })
    }

    get401Error(){
    this.http.get(this.baseUrl + 'buggy/unauthorized').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
        })
    }

    get500Error(){
        this.http.get(this.baseUrl + 'buggy/internalerror').subscribe({
            next: response => console.log(reponse),
            error: error =>  console.log(error)
        })
    }

     // old
    get400ValidationError(){
        this.http.get(this.baseUrl + 'buggy/validationerror').subscribe({
            next: response => console.log(reponse),
            error: error =>  console.log(error)
        })
    }

    // update
    get400ValidationError(){
        this.http.post(this.baseUrl + 'buggy/validationerror', {}).subscribe({
            next: response => console.log(reponse),
            error: error =>  console.log(error)
        })
    }
}
```

-  `refer to BuggyControllers.cs API`

-  `update test-error.component.html`

```
<div class="mt-5 flex justify-center gap-4">
    <button mat-stroked-button (click)="get500Error()">Test 500 error</button>
    <button mat-stroked-button (click)="get404Error()">Test 404 error</button>
    <button mat-stroked-button (click)="get400Error()">Test 400 error</button>
    <button mat-stroked-button (click)="get401Error()">Test 401 error</button>
    <button mat-stroked-button (click)="get400ValidationError()">Test validation error</button>
</div>
```

###### 94. Creating a NotFound and Server Error component

-  `ng g c shared/components/not-found --skip-tests `

```
<div class="container mt-5 ">
    <h1 class="text-3xl"> Not found </h1>
</div>
```

-  `ng g c shared/components/server-error --skip-tests `

```
<div class="container mt-5 ">
    <h1 class="text-3xl"> Internal server error </h1>
</div>
```

-  `update app.routes.ts`

```
//...
export const routes: Routes =[
    {...},
    { path: 'test-error', component: TestErrorComponent },
    { path: 'not-found', component: NotFoundComponent },
    { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
]
```

###### 95. Creating an HTTP Interceptor for handling API errors

-  `cd /client folder  'ng g interceptor core/interceptors/error --dry-run'`
-  ` ng g interceptor core/interceptors/error --skip-tests`

```
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if(err.status === 400){
        alert(err.error.title || err.error);
      }
      if(err.status === 401){
        alert(err.error.title || err.error);
      }
      if(err.status === 404){
        router.navigateByUrl('/not-found');
      }
      if(err.status === 500){
        router.navigateByUrl('/server-error');
      }
      return throwError(() => err);
    })
  );
};
```

-  `update app.config.ts`

```
//...
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    //...
    provideHttpClient(withInterceptors([errorInterceptor])),
  ],
};
```

###### 96. Adding toast (snackbar) notifications

[Angular Snackbar](https://material.angular.dev/components/snack-bar/overview)

-  ` create new service 'ng g s core/services/snackbar --skip-tests'`

```
import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  private snackbar = inject(MatSnackBar);

  error(message: string) {
    this.snackbar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['snackbar-error'],
    })
  }

  success(message: string) {
    this.snackbar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['snackbar-success'],
    })
  }
}
```

-  ` update style.scss`

```
//... .mdc-notche-outline__notch{...}

.mat-mdc-snack-bar-container.snackbar-error{
  --mdc-snackbar-container-color: red;
  --mat-snackbar-button-color: #fff;
  --mdc-snackbar-supporting-text-color: #fff;
}

.mat-mdc-snack-bar-container.snackbar-success{
  --mdc-snackbar-container-color: green;
  --mat-snackbar-button-color: #fff;
  --mdc-snackbar-supporting-text-color: #fff;
}

```

-  `update error.interceptor.ts`

```
export const errorInterceptor: HttpInterceptorFn = (req, next) =>{
    ...
    const snackbar = inject(SnackbarService);

    return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if(err.status === 400){
        snackbar.error(err.error.title || err.error);
      }
      if(err.status === 401){
        snackbar.error(err.error.title || err.error);
      }

      //...
    })
  );
}
```

###### 97. Handling validation errors from the API

-  `update error.interceptor.ts`

```
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  //....

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if(err.status === 400){
        // update starts here =================
        if(err.error.errors){
            const modelStateErrors = [];
            for ( const key in err.error.errors){
                if (err.error.errors[key]){
                    modelStateErrors.push(err.error.errors[key])
                }
            }
            throw modelStateErrors.flat();
        } else {
            snackbar.error(err.error.title || err.error);
        }
        // update ends here ===================
      }
    //....
```

-  ` update test-error.components.ts`

```
export class TestErrorComponent {
    //....
    validationErrors?: string[];

    //...
    get400ValidationError(){
        this.http.post(this.baseUrl + 'buggy/validationerror', {}).subscribe({
        next: response => console.log(response),
        error: error => this.validationErrors = error // update here
        })
    }
}
```

-  `update test-error.components.html template`

```

//...
@if(validationErrors){
    <div class="mx-auto max-w-lg mt-5 bg-red-100">
        <ul class="space-y-2 p-2">
            @for ( error of validationErrors; track $index){
                <li class="text-red-800">{{ error }} </li>
            }
        </ul>
    </div>
}
```

###### 98. Configuring the server error page

-  `update error-interceptor.ts`

```
//...

import { NavigationExtras, Router } from '@angular/router';

export const errorInterceptor: HttpInteceptorFn = (req, next) =>{
    catchError((err.HttpErrorResponse)) =>{
        //...

        if(err.status == 500){
            //update starts here =============
            const navigationExtras: NavigationExtras = { state: {error: err.error}}
            router.navigationByUrl('/sever-error', navigationExtras);
            //update ends here =============
        }

        return throwError(()=>err)
    }
}
```

-  `update server-error-components.ts`

```
import { HttpErrorReponse } from '@angular/common/http';
import { Router } from '@angular/router';

//...
@Component({
    selector: 'app-server-error',
    standalone: true,
    imports: [
        MatCard
    ],
    //...
})

export class ServerComponent {
    <!-- error?: HttpErrorResponse -->
    error?: any;

    constructor(private router: Router){
        const navigation = this.router.getCurrentNavigation();
        this.error = navigation?.extras.state?.['error'];
    }
}
```

-  ` update server-error.component.html template`

```
<div class="container mt-5 p-4 bg-gray-100 rounded shadow-lg">
    <h1 class="text-2xl font-semibold mb-4">
        Interna server error
    </h1>

    @if (error) {
        <h5 class="text-red-600"> Error: {{  error.message }}</h5>
        <p class="font-bold mb-2"> This error comes from the server, not Angular</p>
        <p class="mb-2"> What to do next? </p>
        <ol class="list-decimal ml-5 mb-4">
            <li class="mb-1"> Check the network tab in chrome dev tools </li>
            <li class="mb-1"> Reproduce the error in postman. If same error, don't waste time in troubleshooting angular code. </li>
        </ol>
        <h5 class="text-lg font-semibold mb-2"> Stack trace</h5>
        <mat-card class="p4 -bg-white">
            <code class="block whitespace-pre-wrap">
                {{  error.details }}
            </code>
        </mat-card>
    }
</div>
```

###### 99. Configuring the Not found page

-  `update not-found-component.html`

```
<div class="flex items-center justify-center min-h-96 bg-gray-100">
    <div class="text-center">
      <mat-icon class="text-purple-700 icon-display"> error_ outline</mat-icon>
      <h1 class="text-4xl font-bold text-gray-800 mt-4">404</h1>
      <p class="text-lg text-gray-600 mt-2">
        Page not found
      </p>
      <button routerLink="/shop" mat-flat-button class="mt-4">Back to shop</button>
    </div>
</div>
```

-  `update not-found-component.ts`

```
//...
import { MatButton } from '@angular/material/button'; // update here
import { MatIcon } from '@angular/material/icon'; // update here
import { RouterLink } from '@angular/router'; // update here

@Component({
  selector: 'app-not-found',
  imports: [
    MatIcon, // update here
    MatButton, // update here
    RouterLink // update here
  ],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss'
})
```

-  ` update not-found-component.scss`

```
.icon-display{
  transform: scale(3);
}
```

###### 100. Adding an HTTP Interceptor for loading

-  `create "ng g interceptor core/interceptors/loading --skip-tests"`

```
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { delay, finalize } from 'rxjs';
import { BusyService } from '../services/busy.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService); // needs busy-services

  busyService.busy();

  return next(req).pipe(
    delay(500),
    finalize(() => busyService.idle())
  );
};
```

-  ` app.config.ts`

```
import { loadingInterceptor } from './core/interceptors/loading.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        errorInterceptor,
        loadingInterceptor // UPDATE HERE
    ])),
  ],
};
```

-  `create "ng g s core/services/busy --skip-tests " `

```
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  loading = false;
  busyRequestCount = 0;

  busy(){
    this.busyRequestCount++;
    this.loading = true;
  }

  idle(){
    this.busyRequestCount--;
    if(this.busyRequestCount <= 0){
      this.busyRequestCount = 0;
      this.loading = false;
    }
  }
}
```

###### 101. Adding a progress bar to indicate loading

[Material UI - Progressbar](https://material.angular.dev/components/progress-bar/overview)

-  `update header.component.ts`

```
import { MatProgressBar } from '@angular/material/progress-bar'; // update

@Component({
  selector: 'app-header',
  imports: [
    //...
    MatProgressBar // update
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})

export class HeaderComponent {
  busyService = inject(BusyService); // update
}
```

-  `update header.component.html`

```
<header>
...
</header>

// update below code
@if(busyService.loading){
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
}
```

##### 102. Making the header fixed to the top

-  ` update header.component.html`

```
<header class="border-b shadow-md p-3 w-full fixed top-0 z-50 bg-white">
</header>

@if(busyService.loading){
  <mat-progress-bar mode="indeterminate" class="fixed top-20 z-50"></mat-progress-bar>
}

```

-  ` update app.component.html`

```
<div class="container mt-24"> // update from mat-6 to mat-24
</div>
```

###### 103. Summary

```
Goal:

To handle errors we receive from the API centrally and handled by the Http interceptor.
To understand how to troubleshoot API errors

```

-  Question: Would we create an errors component in a 'real' app?
-  Answer: Probably not, but it is helpful

<hr>

### Module 12 - API - Shopping Cart

<hr>

###### 104. Introduction

In this module

```
- Where to 'store' the cart
- Adding Redis to the API
- Creating the Cart service & controller
```

Goal:

```
To set up and configure Redis to
store the customer cart in server
memory and create the supporting
service and controller
```

Where to store the basket?

```
Options:
    - Database ()
    - Local Storage (downside: on client side)
    - Cookie (can stor cart data not typically use this days)
    - Redis ( for storing our shopping cart's item in memory data store 'fast' and persistence, also in has service side features )
```

What is Redis?

```
- In-memory data structure store
    - and Ideal for caching

- Suppors strings, hashes, lists, sets, etc.

- Key/Value store

- Fast

- Perists data by using snapshots every minute

- Data can be given time to live

- Great for caching data
```

###### 105. Creating a Redis instance to use in our app

-  `update docker-compose.yml`

```
services:
  sql:
    image: mcr.microsoft.com/azure-sql-edge
    environment:
      ACCEPT_EULA: "1"
      MSSQL_SA_PASSWORD: "Password@1"
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql # update persist data to redis
  redis: # persist data to redis
    image: redis:latest
    ports:
      - "6379:6379" # update persist data to redis
    volumes:
      - redis-data:/data

volumes:
  redis-data:
  sql-data:
```

-  `terminal cmd: cd root folder`
-  `docker compose down` // delete only the current container and doesn't include the volumes
-  `docker compose up -d` // restarts container and volumes
-  `docker compose down`
-  `docker compose down -v` // delete the volumes

###### 106. Using Redis with .Net

-  ` install redis via nuget -> Infrastructure.csproject [-]`
-  `update API Program.cs`

```
using StackExchange.Redis; // add this library and run ' dotnet add package StackExchange.Redis '

//...builder.Services.AddCors();
// update code below:
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Redis");
    /**if (connString == null) throw new Exception("Cannot get redis connection string"); // Use coalesce expression below is ther conversion */
    var connString = builder.Configuration.GetConnectionString("Redis") ?? throw new Exception("Cannot get redis connection string");

    var configuration = ConfigurationOptions.Parse(connString, true);
    return ConnectionMultiplexer.Connect(configuration);
});

//...builder.Services.AddOpenApi();
```

-  `in API folder install ' dotnet add package StackExchange.Redis '`
-  ` update Program.cs`

```
//... using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
```

-  ` update appsettings.Development.json`

```
"ConnectionStrings": {
    "DefaultConnection":"...",
    "Redis": "localhost" // update
}

```

-  `restart api 'dotnet watch'`

###### 107. Creating the Cart classes

-  `Redis is not relational database and for this project app use as a key value and store`

-  `create new class API/Core/Entities/ShoppingCart.cs `

```
namespace Core.Entities;

public class ShoppingCart
{
    public required string Id { get; set; }
    public List<CartItem> Items {get; set;} = [];
}
```

-  `create new class API/Core/Entities/CartItem.cs `

```
namespace Core.Entities;

public class CartItem
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public required string PictureUrl { get; set; }
    public required string Brand { get; set; }
    public required string Type { get; set; }
}
```

###### 108. Creating a Cart service

-  `create service @ Core/Interfaces/create ICartService.cs`

```
using Core.Entities;
namespace Core.Interfaces;

public interface ICartService
{
    Task<ShoppingCart?> GetCartAsync(string key);
    Task<ShoppingCart?> SetCartAsync(ShoppingCart cart);
    Task<bool> DeleteCartAsync(string key);
}
```

-  `create service @ Infrastructure/Services/CartService.cs `

```
using System.Text.Json;
using Core.Entities;
using Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class CartService(IConnectionMultiplexer redis) : ICartService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<bool> DeleteCartAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<ShoppingCart?> GetCartAsync(string key)
    {
        var data = await _database.StringGetAsync(key);

        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ShoppingCart>(data);

    }

    public async Task<ShoppingCart?> SetCartAsync(ShoppingCart cart)
    {
        var created = await _database.StringSetAsync(cart.Id, JsonSerializer.Serialize(cart), TimeSpan.FromDays(30));

        if (!created)
        {
            return null;
        }

        return await GetCartAsync(cart.Id);
    }
}
```

-  ` Update Program.cs`

```
// builder.Services.AddSingleton<IConnectionMultiplexer>({...})
builder.Services.AddSingleton<ICartService, CartService>();

```

`Issue & Solution : IConnectionMultiplexer `

```
    - encounter issue IConnectionMultiplexer
    - install redis via nuget -> Infrastructure.csproject [-]`

```

###### 109. Creating the Cart controller

-  ` API/Controllers/CartController.cs`

```
    public class Controller(ICartService cartService): BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetCartById(string id) // generate id on the client side
        {
            var cart = await cartService.GetCartAsync(id); // we get the cart if it lives in the redis database
                                                           // if it does returning the shopping cart
            return Ok(cart ?? new ShoppingCart{Id = id}); // then it sets the id in the client side
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateCart(ShoppingCart cart)
        {
            var updatedCart = await cartService.SetCartAsync(cart);

            if(updatedCart == null) return BadRequest("Problem with cart");

            return updatedCart;

        }

        [HttpDelete]
        public async Task<ActionResult> DeleteCart(string id)
        {
            var result = await cartService.DeleteCartAsync(id);

            if(!result) return BadRequest("Problem deleting cart");

            return Ok();
        }
    }
```

###### 110. Testing the Cart in Postman

-  At postman

```
Get Cart = {{url}}/api/cart?id=cart1

Update Cart = {{url}}/api/cart
     - think of this as a client side storage and focus on productId and quantity because we are going to validate this in our API, for checking 'Get Cart'

Delete Cart =

```

-  ` add redis extension in VSCode - Redis by Dunn`
-  ` redis explorer - click Add button`

###### 111. Summary

Goal

```
To setup and configure Redis to
store the customer cart in server
memory and create the supporting
service and controller

FAQs

Question: Isn't Redis overkill here?
            (over engineer?)

Answer: Maybe, but we will use it for
        something else later.
```

<hr>

### Section 13 - Client - Shopping Cart

<hr>

###### 112. Introduction

```
- Adding the cart feature
- Angular signals = added in angular v16 - react to an update a value in NG
- Environment variables -
```

-  Goal

```
To add the cart feature to the
angular app.
To understand the usage of signals in Angular
```

###### 113. Creating the cart components

-  `cd client 'ng g s core/services/cart --skip-tests`

```

```

-  `'ng g c features/cart --skip-tests`

```

```

-  `update app.routes.ts`

```
import { cartComponent } from './features/cart/cart.component';

export const routes: Routes =[
    //... 'shop/:id'
    {path: 'cart', component: CartComponent}
    //...
]
```

-  `update header.component.html`

```
//...
     <div class="flex gap-3 align-middle">
       <a
            routerLink="/cart"          // update
            routerLinkActive="active"   // update
            matBadge="5"
            matBadgeSize="large"
            class="custom-badge mt-2 mr-2">
                <mat-icon>shopping_cart</mat-icon>
        </a>
        //...
        //...
     </div>
//...
```

-  `client/src/app/shared/models/cart.ts`

```
import { nanoid } from 'nanoid';

export type CartType = {
  id: string;
  items: CartItem[];
}

export type CartItem = {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  pictureUrl: string;
  brand: string;
  type: string;
}

export class Cart implements CartType {
  // id = ''; // generate a random id in this example nanoid package
  id = nanoid(); // implment nanoid package to generate a random id
  items: CartItem[] = []; // CartItem[] is an array while = [] is an empty array
}
```

-  `install a utility packages that generate a random id`
-  `npm install nanoid`

###### 114. Introduction to Angular Signals

```
- State that can be observed and reacted to

- Clean API for state management

- Avoid the complexity of observables

=====================================
Signal      Computed        Effect
=====================================
const count = signal(0);

//  Signals are getter functions - calling them reads their value.
console.log('The count is:' + count());

//  Set a new value
count.set(3)

//  Update a value
count.update(value => value + 2)


Pros                    Cons
- Simplicity            - Limited flexibility
- Performance           - Scalability
- Readability           -

```

###### 115. Adding the Cart service methods

-  `update cart.service.ts`

```
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Cart } from '../../shared/models/cart';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  cart = signal<Cart | null>(null);

  getCart(id: string)
  {
    return this.http.get<Cart>(this.baseUrl + 'cart?id=' +id).subscribe({
      next: cart => this.cart.set(cart),
    })
  }

  setCart(cart: Cart) {
    return this.http.post<Cart>(this.baseUrl + 'cart', cart).subscribe({
      next: cart => this.cart.set(cart),
    })
  }
}
```

-  `ng g --help`
-  `ng g environments`
-  `create client/src/environment.development.ts`

```
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/'
};
```

-  `create client/src/environment.ts`

```
export const environment = {
  production: true,
  apiUrl: 'api/'
};
```

###### 116. Adding an item to the cart

-  `update cart.service.ts`

```
export class CartService {
    //...setCart(cart: Cart) {...}

    addItemToCart(item: CartItem | Product, quantity = 1){
    const cart = this.cart() ?? this.createCart();
    if (this.isProduct(item)) {
      item = this.mapProductToCartItem(item);
    }
    cart.items = this.addOrUpdateItem(cart.items, item, quantity);
    this.setCart(cart);
  }

  private addOrUpdateItem(items: CartItem[], item: CartItem, quantity: number): CartItem[] {
    const index = items.findIndex( x=> x.productId === item.productId);
    if (index === -1) {
      item.quantity = quantity;
      items.push(item);
    } else {
      items[index].quantity += quantity;
    }
    return items;
  }

  private mapProductToCartItem(item: Product): CartItem {
    return {
      productId: item.id,
      productName: item.name,
      price: item.price,
      quantity: 0,
      pictureUrl: item.pictureUrl,
      brand: item.brand,
      type: item.type,
    }
  }

  private isProduct(item: CartItem | Product): item is Product{
    return (item as Product).id !== undefined;
  }

  private createCart(): Cart {
    const cart = new Cart();
    // persist in the local storage
    localStorage.setItem('cart_id', cart.id);
    return cart;
  }

}
```

###### 117. Using the add item functionality in the product item

-  `update product-item.component.ts`

```
import { Component, inject, Input } from '@angular/core';
import { CartService } from '../../../core/services/cart.service';

export class ProductItemComponent {
  @Input() product?: Product;
  cartService = inject(CartService);
}

```

-  `update product-item.component.html`

```
//...
    <button mat-stroked-button class="w-full" (click)="cartService.addItemToCart(product)">
        <mat-icon>add_shopping_cart</mat-icon>
        Add to cart
    </button>
//...
```

-

```
note to check it:
in the browser,

- go to network tab
- click 'cart' post
- requerst tab ( payload ) -> you will see details from there
```

-  Issue: About after clicking 'Add to cart' button in shop page
-  `update product-item.component.html`

```
<mat-card-actions (click)="$event.stopPropagation()"> // prevents from interfere to redirect kay inside may sya sa routerLink which is <mat-card></mat-card>

    // ignore the code below for now, focus on the top commented code.
    <button mat-stroked-button class="w-full" (click)="cartService.addItemToCart(product)">
        <mat-icon>add_shopping_cart</mat-icon>
        Add to cart
    </button>
</mat-card-actions>
```

###### 118. Persisting the cart

-  `to check go to browser tools`
-  `storage-> Local Storage`
-  `app initializer`
-  `update app.config.ts`

```
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        errorInterceptor,
        loadingInterceptor
    ])),
    // {
    //   provider: APP_INITIALIZER,
    //   userFactory
    // }
  ],
};
```

-  `create new server 'ng g s core/services/init --skip-tests' => init.service.ts `

```
import { inject, Injectable } from '@angular/core';
import { CartService } from './cart.service';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InitService {

 private cartService = inject(CartService);

 init(){
  const cartId = localStorage.getItem('cart_id');
  const cart$ = cartId ? this.cartService.getCart(cartId) : of(null);

  return cart$;
 }
}
```

-  `update cart.service.ts`

```
  getCart(id: string)
  {
    return this.http.get<Cart>(this.baseUrl + 'cart?id=' +id)
        .pipe(map( cart => { // update
        this.cart.set(cart);  // update
        return cart;  // update in init.services this will resolve the issue to observable instead of Subscription
    })
    )

    /**  // old
    return this.http.get<Cart>(this.baseUrl + 'cart?id=' +id).subscribe({
      next: cart => this.cart.set(cart),
    }) **/
  }

```

-  ` update app.config.ts`

```
//...
import { InitService } from './core/services/init.service';
import { lastValueFrom } from 'rxjs';

function initializeApp(initService: InitService){
  return () => lastValueFrom(initService.init()).finally(() =>{
    const splash = document.getElementById('initial-splash'); // this will reflect on html browser
    if (splash) {
      splash.remove();
    }
  })
}

//...
export const appConfig: ApplicationConfig = {
    providers: [
        providerZoneChangeDetection({ eventCoalescing: true }),
        //...
        {
            provide: APP_INITIALIZER,
            useFactory: initializeApp,
            multi: true,
            deps: [InitService]
        }
    ]
}
```

-  ` update index.html`

```
<body>
  <div id="initial-splash">
    <div class="flex items-center justify-center h-screen">
      <div class="flex-col items-center align-middle">
        <img src="../images/logo.png" alt="logo">
      </div>
    </div>
  </div>
  <app-root></app-root>
  //...
```

###### 119. Updating the nav bar with the cart item count

-  ` update cart.service.ts`

```
import {
  computed, // update
  inject,
  Injectable,
  signal
} from '@angular/core';

//...

export class CartService{
  //... cart = signal ...
  itemCount = computed(() => {

    return this.cart()?.items.reduce((sum, item) => sum + item.quantity, 0);

    // - reduce() method will reduce an array of items into a number
    // (sum, item)  sum is an accumulator keeps the running total and in this case it start at 0
    // each time we use this call back function in one of the items in the array it executes `sum + item.quantity` the current sum + item.quantity

  })
}
```

-  ` update header.component.ts`

```
import { CartService } from '../../core/services/cart.service';

//... @Component({..})

export class HeaderComponent{
  //... busyService
  cartService = inject(CartService);
}
```

-  ` update header.component.html`

```
//...
      <div class="flex gap-3 align-middle">
         <a routerLink="/cart" routerLinkActive="active"
            matBadge="{{ cartService.itemCount() }}" // and its because its a signal we need to include () parenthesis
            matBadgeSize="large"
            class="custom-badge mt-2 mr-2">
         <mat-icon>shopping_cart</mat-icon>
        </a>
      <button mat-stroked-button>Login</button>
      <button mat-stroked-button>Register</button>
     </div>

//...</div>

```

###### 120. Styling the cart

-  `update cart.component.ts`

```
import { Component, inject } from '@angular/core';
import { CartService } from '../../core/services/cart.service';

export class CartComponent{
   cartService = inject(CartService);
}
```

-  `update cart.component.html`

```
<section>
  <div class="max-auto max-w-screen-xl">
    <div class="flex w-full items-start gap-6 mt-32">
      <div class="w-full">
        @for(item of cartService.cart()?.items; track item.productId){
          <div>{{ item.productName }} - {{ item.quantity }}</div>
        }
      </div>
    </div>
  </div>
</section>
```

-  `ng g c features/cart/cart-item --skip-tests`

-  ` update cart-item.component.ts`

```
import { Component, input } from '@angular/core';
import { CartItem } from '../../../shared/models/cart';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { CurrencyPipe } from '@angular/common';

@Component({
  //...
  imports:[
    RouterLink, // this is needed by routerLink in the template
    MatButton,
    MatIcon,
    CurrencyPipe
  ]
})

export class CartItemComponent {
  item = input.required<CartItem>();
}
```

-  ` update cart-item.component.html`

```
<div class="rounded-lg border border-gray-200 bg-white p-4 shadow-sm mb-4">
  <div class="flex items-center justify-between gap-6">
    <a
      routerLink="/shop/{{ item().productId }}" // routerLink needs RouterLink at .ts file
      class="shrink order-1"
      >
      <img
        [src]="item().pictureUrl"
        alt="product image"
        class="h-20 w-20"
        />
    </a>
    <div class="flex items-center justify-between order-3">
      <div class="flex items-center align-middle gap-3">
        <button mat-icon-button>
            <mat-icon class="text-red-600">remove</mat-icon>
        </button>
        <div class="font-semibold text-xl mb-1">{{ item().quantity }}</div>

        <button mat-icon-button>
            <mat-icon class="text-green-600">add</mat-icon>
        </button>
      </div>

      <div class="text-end order-4 w-32">
          <p class="font-bold text-xl">{{ item().price | currency }}</p>
      </div>
    </div>

    <div class="w-full flex-1 space-y-4 order-2 max-w-md">
      <a
        routerLink="/shop/{{ item().productId }}"
        class="font-medium"
      >
        {{ item().productName }}
      </a>

      <div class="flex items-center gap-4">
        <button
          mat-button color="warn">
          <mat-icon>delete</mat-icon>
          Delete
        </button>
      </div>
    </div>

  </div>
</div>
```

-  ` update cart.component.html`

```
<section>
  <div class="max-auto max-w-screen-xl">
    <div class="flex w-full items-start gap-6 mt-32">
      <div class="w-full">

        @for(item of cartService.cart()?.items; track item.productId) {

          <app-cart-item [item]="item"></app-cart-item> // update

          <!-- <div>{{ item.productName }} - {{ item.quantity }}</div> --> // old
        }
      </div>
    </div>
  </div>
</section>
```

-  ` update cart.component.ts`

```
@Component({
  //...
  imports: [CartItemComponent],
  //...
})
```

###### 121. Creating the order summary component

-  `create 'ng g c shared/components/order-summary --skip-tests`

-  ` update order-summary.component.html`

```
<div class="mx-auto max-w-4xl flex-1 space-y-6 w-full">
  <div class="space-y-4 rounded-lg border border-gray-200 bg-white shadow-sm">
    <p class="text-xl font-semi-bold">Order Summary</p>
    <div class="space-y-4">
      <div class="space-y-2">
        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Subtotal</dt>
          <dd class="font-medium text-gray-900">$200.00</dd>
        </dl>

        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Discount</dt>
          <dd class="font-medium text-gray-900">-$0.00</dd>
        </dl>

        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Delivery fee</dt>
          <dd class="font-medium text-gray-900">$10.00</dd>
        </dl>

        <dl class="flex items-center justify-between gap-4 border-t border-gray-200 pt-2">
          <dt class="font-medium text-gray-500">Total</dt>
          <dd class="font-medium text-gray-900">$210.00</dd>
        </dl>

      </div>
    </div>
  </div>
</div>
```

-  `update cart.component.html`

```
<section>
  <div class="max-auto max-w-screen-xl">
    <div class="flex w-full items-start gap-6 mt-32">
      <div class="w-full"> // update to class="w-3/4"
      // ...
      </div>
      // update starts here
      <div class="w-1/4">
        <app-order-summary></app-order-summary>
      </div>
      // update ends here
    </div>
  </div>
</section>
```

###### 122. Creating the order summary component part 2

-  `update order-summary.component.html`

```
<div class="mx-auto max-w-4xl flex-1 space-y-6 w-full">
  <div class="space-y-4 rounded-lg border border-gray-200 p-4 bg-white shadow-sm"> // adding p-4
    <div class="space-y-4">
    //...
      <dl class="flex items-center justify-between gap-4">
        <dt class="font-medium text-gray-500">Discount</dt>
        <dd class="font-medium text-green-500">-$0.00</dd> // update to green-500
      </dl>
    // <dl> = definition list
    </div>

    // add button
    <div class="flex flex-col gap-2">
        <button routerLink="/checkout" mat-flat-button>Checkout</button>
        <button routerLink="/shop" mat-button>Continue Shopping</button>
      </div>
  </div>

  // voucher code
<div class="space-y-4 rounded-lg border border-gray-200 bg-white shadow-sm">
    <form class="space-y-2 flex flex-col p-2">
      <label class="mb-2 block text-sm font-medium">
        Do you have a voucher code?
      </label>
      <mat-form-field appearance="outline">
        <mat-label>Voucher code</mat-label>
        <input type="text" matInput> // has issue on the UI there is a vertical line on it.
      </mat-form-field>
      <button mat-flat-button> Apply Code</button>
    </form>
  </div>
```

-  `update order-summary.component.ts`

```
import { MatButton } from '@angular/material/button';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-summary',
  imports: [
    MatButton, // update
    RouterLink, // update
    MatFormField, // update
    MatLabel, // update
  ],
  templateUrl: './order-summary.component.html',
  styleUrl: './order-summary.component.scss'
})
```

###### 123. Creating the order totals

-  `update cart.service.ts`

```
export class CartService {
  //itemCount...

  //update below
  totals = computed(() => {
    const cart = this.cart();
    if(!cart) return null;
    const subTotal = cart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const shipping = 0;
    const discount = 0;

    return {
      subTotal,
      shipping,
      discount,
      total: subTotal + shipping - discount
    }
  })

  //...
}
```

-  ` update order-summary.component.ts`

```
import { Component, inject } from '@angular/core';
import { CartService } from '../../../core/services/cart.service';
import { CurrencyPipe } from '@angular/common';

@component({
  ...,
  imports: [
    MatButton,
    RouterLink,
    MatFormField,
    MatLabel,
    MatInput,
    CurrencyPipe // update
  ],
  ...
})

export class OrderSummaryComponent {
  cartService = inject(CartService);
}
```

-  ` update order-summary.component.html`

```
//...
  <div class="space-y-4">
    <div class="space-y-2">
        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Subtotal</dt>
          <dd class="font-medium text-gray-900">
            {{ cartService.totals()?.subTotal | currency}} // update
          </dd>
        </dl>

        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Discount</dt>
          <dd class="font-medium text-green-500">
            {{ cartService.totals()?.discount | currency}} // update
          </dd>
        </dl>

        <dl class="flex items-center justify-between gap-4">
          <dt class="font-medium text-gray-500">Delivery fee</dt>
          <dd class="font-medium text-gray-900">
            {{ cartService.totals()?.shipping | currency }} // update
          </dd>
        </dl>

        <dl class="flex items-center justify-between gap-4 border-t border-gray-200 pt-2">
          <dt class="font-medium text-gray-500">Total</dt>
          <dd class="font-medium text-gray-900">
            {{ cartService.totals()?.total | currency }} // update
          </dd>
        </dl>
      </div>
  </div>
//...
```

-  !!Bug - cart-item.component.html issue - visual issue - minor = status: !resolve

```
Discovered: bug @ cart-item.component.html visual issue on visual minus sign in cart should be red and plus sign should be green, my theory is the tailwind is not detected.
```

-  `update cart.service.ts`

```
// addItemToCart(item: CartItem | Product, quantity = 1){...}

  removeItemFromCart(productId: number, quantity = 1) {
    const cart = this.cart();
    if(!cart) return;

    const index = cart.items.findIndex(x => x.productId === productId);
    if (index !== -1) {
      if (cart.items[index].quantity > quantity){
        cart.items[index].quantity -= quantity;
      } else {
        cart.items.splice(index, 1);
      }

      //
      if(cart.items.length === 0){
        this.deleteCart();
      } else{
        this.setCart(cart);
      }
    };
  }

// removal of cart items from local storage and redis server
  deleteCart() {
    this.http.delete(this.baseUrl + 'cart?id=' + this.cart()?.id).subscribe({
      next: () => {
        localStorage.removeItem('cart_id');
        this.cart.set(null);
      }
    })
  }

  private addOrUpdateItem(items: CartItem[], item: CartItem, quantity: number): CartItem[] {...}
```

###### 125. Adding these functions to the cart

-  `update cart-item.component.ts `

```
import { ..., inject, ... } from '@angular/core';
import { CartService } from '../../../core/services/cart.service';

export class CartItemComponent {
  //... item = input.required<CartItem>();

  // update starts here
  cartService = inject(CartService);
  incrementQuantity(){
    this.cartService.addItemToCart(this.item());
  }

  decrementQuantity(){
    this.cartService.removeItemFromCart(this.item().productId);
  }

  removeItemFromCart(){
    this.cartService.removeItemFromCart(this.item().productId, this.item().quantity);
  }
  // update ends here
}
```

-  ` update cart-item.component.html`

```
//...
<div class="flex items-center align-middle gap-3">
  <button mat-icon-button (click)="decrementQuantity()"> // add click event
      <mat-icon class="text-red-600">remove</mat-icon>
  </button>
  <div class="font-semibold text-xl mb-1">{{ item().quantity }}</div>

  <button mat-icon-button (click)="incrementQuantity()"> // add click event
      <mat-icon class="text-green-600">add</mat-icon>
  </button>
</div>
//...

//...
<div class="flex items-center gap-4">
<button
  mat-button color="warn" (click)="removeItemFromCart()"> // add click event
  <mat-icon>delete</mat-icon>
  Delete
</button>
```

-  `check or test in https://localhost:4200/cart`

###### 126. Adding the update cart functionality to the product details

-  `update product-details.component.ts`

```
import { CartService } from '../../../core/services/cart.service'; // update import
import { FormsModule } from '@angular/forms'; // update import

@Component({
  selector: 'app-product-details',
  imports: [
    // MatLabel,
    FormsModule // update import
  ],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss'
})

export class ProductDetailsComponent implements OnInit {
  //...
  private cartService = inject(CartService);
  //...product?: Product;
  quantityInCart = 0;
  quantity = 1;

  loadProduct(){
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) return;
      {
      this.shopService.getProduct(+id).subscribe({
        next: product => {  // update
          this.product = product; // update
          this.updateQuantityInCart(); // update
        }, // update
        error: error => console.error(error),
      });
    }
  }

  // update starts here
  updateCart(){
    if( !this.product) return;

    if(this.quantity > this.quantityInCart){
      const itemsToAdd = this.quantity - this.quantityInCart;
      this.quantityInCart += itemsToAdd;
      this.cartService.addItemToCart(this.product, itemsToAdd);
    } else {
      const itemsToRemove = this.quantityInCart - this.quantity;
      this.quantityInCart -= itemsToRemove;
      this.cartService.removeItemFromCart(this.product.id, itemsToRemove);
    }
  }

  updateQuantityInCart() {
    this.quantityInCart = this.cartService.cart()?.items.find(x => x.productId === this.product?.id)?.quantity || 0;
    this.quantity = this.quantityInCart || 1;

  }

  getButtonText() {
    return this.quantityInCart > 0 ? 'Update Cart' : 'Add to Cart';
  }
  // update ends here

}
```

-  `update product-details.component.html`

```
//...
    <div class="flex gap-4 mt-6">
      <button
          [disabled]="quantity=== quantityInCart"
          (click)="updateCart()"
          mat-flat-button class="match-input-height">
          <mat-icon> shopping_cart </mat-icon>
          {{ getButtonText() }}
      </button>

      <mat-form-field apperance="outline" class="flex">
          <mat-label> Quantity </mat-label>
          <input matInput min="0"
          [(ngModel)]="quantity"
          type="number"
          >
      </mat-form-field>
    </div>
//...
```

-  `To test you go to e.g https://localhost:4200/shop/1`

###### 127. Creating the checkout components

-  `cd client/ ng g s core/services/checkout --skip-tests`
-  `cd client/ ng g c features/checkout --skip-tests`

-  ` update app.routes.ts`

```
import { CheckoutComponent } from './features/checkout/checkout.component';

export const routes: Routes = [
  //...{ path: 'cart', component: CartComponent },
  { path: 'checkout', component: CheckoutComponent }, // update
];
```

-  `update checkout.component.html `

```
<div class="mt-5">
  <h1 class="text-2xl"> Only authorized users should be able to see this!</h1>
</div>
```

-  `to test go to e.g: https://localhost:4200/checkout`

###### 128. Summary

```
Goal:

To add the cart feature to the angular app.
To understand the usage of signals in Angular

```

<hr>

### Section 14: API - Identity

<hr>

###### 129. Introduction

```
In this module

- Setting up ASP.NET Identity
- Using the UserManager & SignInManager
- Claims
- Extension methods

--Storing user accounts in the app - why do it?
  --- Full control
  --- Customization
  --- No dependencies
  --- Cost

--Storing user accounts in the app - cons?
  --- Security risks
  --- Compliance
  --- Overhead  -.Net Identity API Endpoints
                - The MapIdentityApi<TUser> endpoints
                - The call to MapIdentityApi<TUser> adds the following endpoints to the app:
                e.g POST /register; /login; /refresh

Goal:

To implement ASP.NET Identity to
allow clients to login and register
to our app and receive a Cookie
which can be used to authenticate
against certain classes/methods
in the API.

- we used cookie base because we are using a browser,
- on the other hand if it is mobile we use diferently like: json base (which i have no idea yet.)

```

###### 130. Setting up identity

[How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0)

-  ` alternative: Azure Identity`

-  ` Nuget -> search: microsoft.extensions.identity.stores (Note: 9.0.3 currently development) -> add to: Core.csproj`
-  `Nuget -> search: microsoft.aspnetcore.identity.EntityFrameworkCore (Note: 9.0.3 currently development) -> add to: Infrastructure.csproj`

-  `API/Core/Entities/AppUser.cs`

```
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class AppUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

```

-  `update StoreContenxt.cs`

```
// instead of DBContext derive to IdentityDBContext()
// public class StoreContext(DbContextOptions options) : DbContext(options)
public class StoreContext(DbContextOptions options) : IdentityDBContext<AppUser>(options)
{
  //...
}
```

-  `update Program.cs`

```
//... builder.Services.AddSingleton<ICartService, CartService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
.AddEntityFrameworkStores<StoreContext>();

//... app.MapControllers();
app.MapIdentityApi<AppUser>();
```

###### 131. Updating the DB and testing the endpoints

-  `root folder new or re-migration "dotnet ef migrations add IdentityAdded -s API -p Infrastructure" `

```
for testing the Identity API go to postman section 14


- Defualt endpoints - register => POST {{url}}/register
- Default endpoints -> Login => POST {{url}}/login
  - blots a token

- Default endpoints - login with cookie => POST {{url}}/login?useCookies=true
  - cookie link in postman


```

###### 132. Adding a custom register endpoint

-  `create API/DTO/RegisterDto.cs`

```
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;

}
```

-  ` create API/Controllers/AccountController.cs`

```
using API.DTOs;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
    [HttpDelete("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok();
    }
}
```

-  `update Program.cs`

```
//...app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>(); // api/login //update with app.MapGroup("api").
```

-  Test in postman

```
Register as Tom {{url}}/api/account/register
Body: raw:

{
	"firstName": "Tom",
    "lastName": "Smith",
	"email": "tom@test.com",
	"password": "Pa$$w0rd"
}

Login as Bob - get token
Login as Bob - get cookie

```

-  `!BUG: 404 not found upon testing at Postman ( desktop workstation ) `

```
// find the bug here AccountController.cs
public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
    [HttpPost("register")] <== HttpDelete to HttpPost
    //...
}
```

###### 133. Testing the authentication

-  `update BuggyController.cs`

```
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class BuggyContoller : BaseApiController{

  [Authorize]
  [HttpGet("secrete")]
  public IActionResult GetSecret()
    {
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok("Hello " + name + " with the id of " + id);
    }
}
```

-  `test at postman`

```
- step 1: Login as Bob - get cookie - to test -> remove cookies - manage cookies/x or clear all cookies
- step 2: Get Secret from buggy - {{url}}/api/buggy/secret - 401 upon delatiom of cookie
- repeat step 1 & 2

note: Postman console for more detail
```

###### 134. Creating additional user endpoints

```
Note:
at this current stage we don't have a way to delete our cookie from the users browsers when a user logs out and
what we are going to do next is to create a method that will do that

```

-  ` update AccountController.cs`

```
using System.Security.Claims;

public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
  //...
  // [Authorize]
  [HttpPost("logout")] // it should be [HttpPost] and not [HttpGet]
  public async Task<ActionResult> Logout()
  {
      await signInManager.SignOutAsync();

      return NoContent();
  }

  [HttpGet("user-info")]
    public async Task<ActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated == false) return NoContent();

        var user = await signInManager.UserManager.Users
            .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        if (user == null) return Unauthorized();

        return Ok( new
        {
            user.FirstName,
            user.LastName,
            user.Email,
        });
    }

    [HttpGet]
    public ActionResult GetAuthState()
    {
        return Ok( new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }

}
```

[Use the GET /manage/info endpoint](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0#use-the-get-manageinfo-endpoint)

-

```
To Test => Postman
- Get Current User => {{url}}/api/account/user-info
- Post Logout => {{url}}/api/account/logout => [body] to remove the cookie -> !405 Bug not working |
                  it should remove the cookie on this endpoint and 204 error should fixed it.
                  (solution:  [HttpPost("logout")] // it should be [HttpPost] and not [HttpGet] )

- Login as Tom => {{url}}/api/login?useCookies=true => to get a new cookie
- Get Secret from Buggy => {{url}}/api/buggy/secret =>
- Logout - to verify the cookie has been deleted.
```

###### 135. Creating extension methods

-  `create API/Extensions/ClaimsPrincipleExtensions.cs`

```
using System.Security.Authentication;
using System.Security.Claims;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ClaimsPrincipleExtensions // we typically use static when don't want a new instance of it
{

public static async Task<AppUser> GetUserByEmail(this UserManager<AppUser> userManager,
      ClaimsPrincipal user)
  {
      var userToReturn = await userManager.Users.FirstOrDefaultAsync(x =>
          x.Email == user.GetEmail());

      if (userToReturn == null) throw new AuthenticationException("User not found");

      return userToReturn;

  }

  public static string GetEmail(this ClaimsPrincipal user)
  {

    var email = user.FindFirstValue(ClaimTypes.Email);

    // use coalasce expression in if
    // if (email == null) throw new AuthenticationException("Email claim not found");

    // use coalasce expression in if = transform to these code below:
    var email = user.FindFirstValue(ClaimTypes.Email)
    ?? throw new AuthenticationException("Email claim not found");

    return email;

  }
}
```

-  `update AccountController.cs`

```
public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
    //...

    [HttpGet("user-info")]
    public async Task<ActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated == false) return NoContent();

        var user = await signInManager.UserManager
            .GetUserByEmail(User);
            // .Users.FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        // because the var user - in no longer a null we don't need the if check condition below:
        // if (user == null) return Unauthorized();
        return Ok(new
        {
            user.FirstName, // 'user' is not null here by hovering the user.
            user.LastName, // 'user' is not null here
            user.Email, // 'user' is not null here
        });
    }
    //...
}
```

-  `Postman Checking`

```
test in Postman

- Get - Get Secret from buggy = {{url}}/api/buggy/secret
- Get - Get Current User = {{url}}/api/account/user-info

```

###### 136. Validation errors

```
- go to postman
- Register with weak password {{ url }}/api/account/register
-
  {
    "firstName": "Bob",
      "lastName": "Bobbity",
    "email": "bob@test.com",
    "password": "letmein" // weak password
  }

- Login as Tom bad password  {{url}}/api/login

```

-  `update AccountController.cs`

```
//... BaseApiController has default error generation which is why we can call ModelState.AddModelError
// AddModelError

  public async Task<IActionResult> Register(RegisterDto registerDto)
  {

  // if (!result.Succeeded) return BadRequest(result.Errors); // before

    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
          ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
  }
//...
```

-  Postman testing

```
// Register with weak password
// {{url}}/api/account/register

{
	"firstName": "Bob",
    "lastName": "Bobbity",
	"email": "bob@test.com",
	"password": "letmein" // weak password
}
```

```
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "PasswordRequiresDigit": [
            "Passwords must have at least one digit ('0'-'9')."
        ],
        "PasswordRequiresUpper": [
            "Passwords must have at least one uppercase ('A'-'Z')."
        ],
        "PasswordRequiresNonAlphanumeric": [
            "Passwords must have at least one non alphanumeric character."
        ]
    },
    "traceId": "00-5c7d991a0f472322c71f965ea80c20b4-cdde6aa8ec4af742-00"
}
```

###### 137. Adding a user address class

-  ` create Core/Entities/Address.cs`

```
namespace Core.Entities;

public class Address : BaseEntity
{
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
}
// note: later gamiton ni para sa stripe payment
```

-  `then connect to AppUser.cs`

```
public class AppUser : IdentityUser
{
  //... public string? LastName { get; set; }

  public Address? Address { get; set; } // connect here
}
```

-  ` update StoreContext.cs`

```
public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    //... public DbSet<Product> Products { get; set; }

    public DbSet<Address> Addresses { get; set; } // connnect here;
    // option to query the Address w/out involving the AppUser
}

// note: if there a new entity need to update the database via migration
// cd .. root
// dotnet ef migrations add AddressAdded -s API -p Infrastructure
```

###### 138. Adding an endpoint to update the user address

-  ` update API/DTOs/AddressDto.cs`

```
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class AddressDto
{
    [Required] // its a required anotation
    public string Line1 { get; set; } = string.Empty;

    [Required] // its a required anotation
    public string? Line2 { get; set; }

    [Required] // its a required anotation
    public string City { get; set; } = string.Empty;

    [Required] // its a required anotation
    public string State { get; set; } = string.Empty;

    [Required] // its a required anotation
    public string PostalCode { get; set; } = string.Empty;

    [Required] // its a required anotation
    public string Country { get; set; } = string.Empty;
}

```

-  `then update AccountController.cs`

```
public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
  //... public ActionResult GetAuthState(){...}

    [Authorize]
    [HttpGet("address")] // correct =>[HttpPost("address")] and wrong=>[HttpGet] this one causes 401 error
    public async Task<ActionResult<Address>> CreateOrUpdateAddress(AddressDto addressDto)
    {
        var user = await signInManager.UserManager.GetUserByEmail(User);

        // for this approach - creating an extension method approach
        // ` create Extension API/Extensions/AddressMappingExtensions.cs `

        if (user.Address == null)
        {
            user.Address = addressDto.ToEntity();
        }
        else
        {
            user.Address.UpdateFromDto(addressDto);
        }

        var result = await signInManager.UserManager.UpdateAsync(user);

        if (!result.Succeeded) return BadRequest("Problem updating user address");

        return Ok(user.Address.ToDto());
        }
    }
}
```

-  ` update ClaimsPrincipleExtensions.cs`

```
public static class ClaimsPrincipleExtensions
{
    //...public static async Task<AppUser> GetUserByEmail(this UserManager<AppUser> userManager,
        ClaimsPrincipal user)
    {...}

    // update below code:
    public static async Task<AppUser> GetUserByEmailWithAddress(this UserManager<AppUser> userManager,
        ClaimsPrincipal user)
    {
        var userToReturn = await userManager.Users
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.Email == user.GetEmail());

        if (userToReturn == null) throw new AuthenticationException("User not found");

        return userToReturn;
    }
}
```

-  `create Extension API/Extensions/AddressMappingExtensions.cs`

```
// auto-mapper is has shakey reputation
// so we are using the Method Extension approach
using API.DTOs;
using Core.Entities;

namespace API.Extensions;

public static class AddressMappingExtensions
{
    // 1st extension method
    public static AddressDto ToDto(this Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));

        return new AddressDto
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            Country = address.Country,
            PostalCode = address.PostalCode
        };
    }

    // 2nd extension method
    public static Address ToEntity(this AddressDto addressDto)
    {
        if (addressDto == null) throw new ArgumentNullException(nameof(addressDto));

        return new Address
        {
            Line1 = addressDto.Line1,
            Line2 = addressDto.Line2,
            City = addressDto.City,
            State = addressDto.State,
            Country = addressDto.Country,
            PostalCode = addressDto.PostalCode
        };
    }

    // 3rd extension method
    public static void UpdateFromDto(this Address address, AddressDto addressDto)
    {
        if (addressDto == null) throw new ArgumentNullException(nameof(addressDto));
        if (address == null) throw new ArgumentNullException(nameof(address));

        address.Line1 = addressDto.Line1;
        address.Line2 = addressDto.Line2;
        address.City = addressDto.City;
        address.State = addressDto.State;
        address.Country = addressDto.Country;
        address.PostalCode = addressDto.PostalCode;

    }
}
```

###### 139. Updating the user address part 2

-  `update AccountController.cs`

```
public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
{
  //...

  public static Address ToEntity(this AddressDto addressDto)
  {
    //...

    return Ok(new
      {
          user.FirstName,
          user.LastName,
          user.Email,
          Address = user.Address?.ToDto()
      });
  }

}

```

-  ` update AddressMappingExtensions.cs`

```
public static class AddressMappingExtensions
{
    // 1st extension method

    // set to nullable AddressDto?
    public static AddressDto? ToDto(this Address? address)
    // enable Address to optional by adding ?

    {
      if (address == null) return null; // update to this
      // if (address == null) throw new ArgumentNullException(nameof(address));

    }
}
```

-  Bug - !FF not working need to recheck all of the files here

-  `postman testing | Add User Address (tom) - {{ url }}/api/account/address`

```
- Add User Address (tom) - {{ url }}/api/account/address
-
```

-  ` update AddressDto.cs`

```
// accidentally set to required that prevents
public class AddressDto
{
    //...

    //[Required] // removed this
    public string? Line2 { get; set; } // because string is set to optional string?
    //the question mark, data anotation

    //...
}

```

-  Postman - Get Current User {{ url }}/api/account/user-info

-  ` update AcountController.cs`

```
// ...
       var user = await signInManager.UserManager
            .GetUserByEmailWithAddress(User); // update .GetUserByEmail(User) to  .GetUserByEmailWithAddress(User);

//...
```

-  `update Program.cs`

```
app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials() // update and add
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));
```

-  `Bug fixed`

```
[Authorize]
[HttpGet("address")] // correct =>[HttpPost("address")] and wrong=>[HttpGet] this one causes 401
```

###### 140. Summary

```
Goal:
To implement ASP.NET Identity to
allow clients to login and register
to our app and recieve a Cookie
which can be used to authenticate
against certain classes/methods
in the API

Q:  Is this secure?
A:  Yes, but the general consesus
    is what we should use 3rd party
    identity providers and offload the
    user authentication there

```

<hr>

### Module 15: Client - Identity

<hr>

###### 141: Introduction

```
In this module

- Adding an Account feature
- Forms in Angular
- Reactive forms
- Reusable form components
- Client side validation

```

###### 142. Creating the account components

-  `cd client  'ng g s core/services/account --skip-tests'`
-  `client 'ng g c features/account/login --skip-tests'`
-  `client 'ng g c features/account/register --skip-tests'`

-  `create routes for each at app.routes.ts`

```
//...
import { LoginComponent } from './features/account/login/login.component';
import { RegisterComponent } from './features/account/register/register.component';

export const routes: Routes = [
  //...,
  { path: 'account/login', component: LoginComponent },
  { path: 'account/register', component: RegisterComponent },
  //...
];
```

-  ` update header.component.html`

```
  //...

    <button routerLink="/account/login" mat-stroked-button>Login</button>
    <button routerLink="/account/register"  mat-stroked-button>Regiser</button>

  //...
```

-  `create client/src/app/shared/models/user.ts`

```
export type User = {
  firstName: string;
  lastName: string;
  email: string;
  address: Address;
}

export type Address = {
  line1: string;
  line2?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}
```

-  ` update account.service.ts`

```
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Address, User } from '../../shared/models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient)
  currentUser = signal<User | null>(null);

  login(values: any){
    let params = new HttpParams();
    params = params.append('useCookies', true);
    return this.http.post<User>(this.baseUrl + 'login', values, {params});
  }

  register(values: any){
    return this.http.post(this.baseUrl + 'account/register', values);
  }

  getUserInfo(){
    return this.http.get<User>(this.baseUrl + 'account/user-info').subscribe({
      next: user => this.currentUser.set(user)
    })
  }

  logout(){
    return this.http.post(this.baseUrl + 'account/logout', {});
  }

  updateAddress(address: Address){
    return this.http.post(this.baseUrl + 'account/address', address);
  }
}
```

-  ` Take-away and realization`

```
- API can be only tested or trace bugs using postman or other api tester
- 401 or other issue regarding http verbs - kay sa api ang issue maybe the httpGet is dapat httpPost something ingon ani na mga issue
```

###### 143. Introduction to Angular forms

-  Angular Forms

```
______________________                    _______________________
|                     |                   |                     |
| Forms Module        |                   | ReactiveFormsModule |
| (Template-driven)   |                   |     (Reactive)      |
|_____________________|                   |_____________________|
  - Template driven                             - Flexible
  - Easy to use                                 - Immutable data model
  - 2 way binding                               - Uses observable operators
  - NgModule directive                          - More component code
  - Minimal component code                      - Easier to test
  - Automatic tracking by Angular               - Reactive transformations (debounce)
  - Testing is difficult


______________________      ______________________    __________________
|                     |    |                     |    |                 |
|    FormControl      |    |      FormGroup      |    |     FormArray   |
|                     |    |                     |    |                 |
|_____________________|    |_____________________|    |_________________|

```

###### 144. Creating the login form

-  `update login.component.ts`

```
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard } from '@angular/material/card';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { AccountService } from '../../../core/servies/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatCard,
    MatFormField,
    MatInput,
    MatLabel,
    MatButton
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: [''], // '' empty string
    password: ['']
  })

  onSubmit(){
    this.accountService.login(this.loginForm.value).subscribe({
      next: () =>{
        this.accountService.getUserInfo();
        this.router.navigateByUrl('/shop');
      }
    })
  }
}
```

-  ` update login.component.html`

```
<mat-card class="max-w-lg mx-auto mt-32 p-8 bg-white">
  <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
    <div class="text-center mb-6">
      <h1 class="text-3xl font-semibold text-primary">Login</h1>
    </div>
    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Email</mat-label>
      <input formControlName="email" type="email" placeholder="name@example.com" matInput/>
    </mat-form-field>

     <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Password</mat-label>
      <input formControlName="password" type="password" placeholder="Password" matInput/>
    </mat-form-field>
    <button mat-flat-button type="submit" class="w-full py-2">Sign in</button>
  </form>
</mat-card>
```

-  `update account.services.ts `

```
login(values: any){
  //...
                                                              // add withCredentials: true
  return this.http.post<User>(this.baseUrl + 'login', values, {params, withCredentials: true});

}

getUserInfo(){
                                                            // add {withCredentials: true}
return this.http.get<User>(this.baseUrl + 'account/user-info', {withCredentials: true}).subscribe({
      next: user => this.currentUser.set(user)
    })
 }

  logout(){
  return this.http.post(this.baseUrl + 'account/logout', {}, {withCredentials: true});
  }

```

-  `to test login client`

```
- https://localhost:4200/account/login
- "email": "tom@test.com",
	"password": "Pa$$w0rd"

Browser:
check network tab:
Chrome: Application Tab => cookies
Firefox: Storage tab => cookies

- 201 has issue or test has failed
- 200 server reponse is successfully getting something and working good upon login
- at Network tab
  - user-info
  - preview and Reponse = 200 means good or successful test
```

###### 145. Updating the header component

-  ` update header.component.ts`

```
//...
import { Router, ... , ...} from '@angular/router';
import { AccountService } from '../../core/servies/account.service';


export class HeaderComponent {
  //...
  accountService = inject(AccountService);
  private router = inject(Router);

  logout(){
    this.accountService.logout().subscribe({
      next: () =>{
        this.accountService.currentUser.set(null);
        this.router.navigateByUrl('/');
      }
    })
  }
}

```

-  ` update header.component.html`

```
  <a> ... </a>
  @if (accountService.currentUser() ) {
    <button mat-stroked-button (click)="logout()">Logout</button>
  } @else {
    <button routerLink="/account/login" mat-stroked-button>Login</button>
    <button routerLink="/account/register" mat-stroked-button>Register</button>
  }
```

-  `key take away sa 145: Updating header`

```
- kani kay pag erase lang sa cookie
- pero wala pay persist pag i-refresh sa user ang browser
- dili ma-disable ang login bisan pag naka login na
```

###### 146. Persisting the login

-  `ideas from NC`

```
- call an API endpoints that sends the cookie

```

-  ` update init.services.ts`

```
import { AccountService } from '../servies/account.service';

export class InitService {
  //... private cartService = inject(CartService);
  private accountService = inject(AccountService);
  //...

  init(){
  const cartId = localStorage.getItem('cart_id');
  const cart$ = cartId ? this.cartService.getCart(cartId) : of(null);

  // forkJoin allows us to wait for multiple observables to complete
  // multiple requests can be made here, such as fetching user info
  return forkJoin({
    cart: cart$,
    user: this.accountService.getUserInfo()
  })
  // return cart$;
 }

}
```

-  `update account.service.ts`

```
import { map } from 'rxjs';

export class AccountService{

  getUserInfo(){

    // update para mo connect sa init service gamiton ang pipe instead of subscribe
    return this.http.get<User>(this.baseUrl + 'account/user-info', {withCredentials: true}).pipe(
      map(user => {
        this.currentUser.set(user);
        return user;
      })
    )

    // old
    //return this.http.get<User>(this.baseUrl + 'account/user-info', {withCredentials: true}).subscribe({
    //  next: user => this.currentUser.set(user)
    //})
  }
}
```

-  `testing the login https://localhost:4200/account/login`

-  ` update login.component.ts`

```

  onSubmit(){
    this.accountService.login(this.loginForm.value).subscribe({
      next: () =>{

        // add subscribe() para mo take reflect on the first login
        this.accountService.getUserInfo().subscribe();

        // old
        // this.accountService.getUserInfo();
        this.router.navigateByUrl('/shop');
      }
    })
  }
```

-  `test again login https://localhost:4200/account/login`

###### 147. Adding an auth interceptor

```
- ang objective ani kay ma centralize ang cookie nga dili redundant sa oban controllers
-
```

-  `cd client create 'ng g interceptor core/interceptors/auth --skip-tests' `

```
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // update start here
  const clonedRequest = req.clone({
    withCredentials: true
  })

  return next(clonedRequest);
  // update end here
};
```

-  ` update app.config.ts`

```
//...
import { authInterceptor } from './core/interceptors/auth.interceptor';
//...

provideHttpClient(
      withInterceptors([
        errorInterceptor,
        loadingInterceptor,
        authInterceptor // add  authInterceptor
    ])),

}
```

-  `update account.services.ts`

```
  login(values: any){
    let params = new HttpParams();
    params = params.append('useCookies', true);
    return this.http.post<User>(this.baseUrl + 'login', values, {params}); // remove withCredentials:...

    //return this.http.post<User>(this.baseUrl + 'login', values, {params, withCredentials: true});
  }

  getUserInfo(){
                                            // removed {withCredentials: true}
    return this.http.get<User>(this.baseUrl + 'account/user-info').pipe( // update code

    // old code
    // return this.http.get<User>(this.baseUrl + 'account/user-info', {withCredentials: true}).pipe(
      map(user => {
        this.currentUser.set(user);
        return user;
      })
    )
  }

  logout(){
            /// removed {withCredentials: true},  update code below
    return this.http.post(this.baseUrl + 'account/logout', {});

    //return this.http.post(this.baseUrl + 'account/logout', {}, {withCredentials: true}); // old
  }
```

###### 148. Adding an Angular Material Menu

-  `material.angular.io/components/menu/overview`
   [Angular Menu Docs](https://material.angular.dev/components/menu/overview)

-  ` update header.components.ts`

```
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatDivider } from '@angular/material/divider';

@Component({
  selector: 'app-header',
  imports: [
    //...,
    MatMenuTrigger,
    MatMenu,
    MatDivider,
    MatMenuItem
  ],
  //...

  })
```

-  ` update header.component.html`

```
// ...
        @if (accountService.currentUser() ) {

          // update code
          <button mat-button [matMenuTriggerFor]="menu" >
            <mat-icon> arrow_drop_down</mat-icon>
            <span>{{ accountService.currentUser()?.email }}</span>
          </button>

          // old
          // <button mat-stroked-button (click)="logout()">Logout</button>
        } @else {
          <button routerLink="/account/login" mat-stroked-button>Login</button>
          <button routerLink="/account/register" mat-stroked-button>Register</button>
        }
//...

// lower part

<mat-menu #menu="matMenu" class="px-5">
  <button mat-menu-item class="px-3" routerLink="/cart">
    <mat-icon>shopping_cart</mat-icon>
    My cart
  </button>

  <button mat-menu-item class="px-3" routerLink="/orders">
    <mat-icon>history</mat-icon>
    My orders
  </button>

  <mat-divider></mat-divider>
  <button mat-menu-item class="px-3" (click)="logout()">
    <mat-icon>logout</mat-icon>
    Logout
  </button>
</mat-menu>
```

###### 149. Adding the register form

-  ` update register.component.ts`

```
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard } from '@angular/material/card';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { AccountService } from '../../../core/servies/account.service';
import { Router } from '@angular/router';
import { SnackbarService } from '../../../core/services/snackbar.service';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule, // update
    MatCard,  // update
    MatFormField,  // update
    MatLabel,  // update
    MatInput,  // update
    MatButton,  // update
    JsonPipe    // update
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {

  // inject the services here below
  private fb = inject(FormBuilder);  // update
  private accountService = inject(AccountService);  // update
  private router = inject(Router);  // update
  private snack = inject(SnackbarService);   // update

  // create registerForm method
  registerForm = this.fb.group({
    fristName: [''],
    lastName: [''],
    email: [''],
    password: [''],
  })

  // create onSubmit method
  onSubmit(){
    this.accountService.register(this.registerForm.value).subscribe({
        next: () => {
          this.snack.success('Registration successful - you can now log in');
          this.router.navigateByUrl('/account/login');
        },
      })
    }
}

```

-  ` update register.component.html`

```
<mat-card class="max-w-lg mx-auto mt-32 p-8 bg-white">
  <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
    <div class="text-center mb-6">
      <h1 class="text-3xl font-semibold text-primary">Register</h1>
    </div>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>First name</mat-label>
      <input formControlName="firstName" type="text" placeholder="John" matInput/>
    </mat-form-field>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Last name</mat-label>
      <input formControlName="lastName" type="text" placeholder="Smith" matInput/>
    </mat-form-field>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Email address</mat-label>
      <input formControlName="email" type="email" placeholder="name@example.com" matInput/>
    </mat-form-field>

     <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Password</mat-label>
      <input formControlName="password" type="password" placeholder="Password" matInput/>
    </mat-form-field>
    <button mat-flat-button type="submit" class="w-full py-2">Register</button>
  </form>
</mat-card>

<mat-card class="max-w-lg mx-auto mt-3 p-2">
  <pre>Form values: {{ registerForm.value | json }}</pre>
  <pre>Form status: {{ registerForm.status }}</pre>
</mat-card>

```

###### 150. Form validation part 1

-  ` client side validation`

-  ` update register.component.ts`

```
  //...private snack = inject(SnackbarService);

  // add this below
  validationErrors?: string[];

  //...
  onSubmit(){
    this.accountService.register(this.registerForm.value).subscribe({
        next: () => {
          this.snack.success('Registration successful - you can now log in');
          this.router.navigateByUrl('/account/login');
        },

        // update code below
        error: errors => this.validationErrors = errors
      })
    }
```

-  ` update register.component.html`

```
<mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Password</mat-label>
      <input formControlName="password" type="password" placeholder="Password" matInput/>
    </mat-form-field>

    // update starts here
    @if (validationErrors){
      <div class="mb-3 p-4 bg-red-100 text-red-600">
        <ul class="list-disc px-3">
          @for (error of validationErrors; track $index) {
            <li>{{ error }}</li>
          }
        </ul>
      </div>
    }
    // update ends here

    <button mat-flat-button type="submit" class="w-full py-2">Register</button>
```

###### 151. Form validation part 2

-  `client side validation`
-  ` update register.component.ts`

```
import { MatError, ..., ... } from '@angular/material/form-field';

@Component({
  selector: 'app-register',
  imports: [
    MatError
  ],
    templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})

  // updated code below
  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required]],
    password: ['', Validators.required], // we can explore this Valditor.pattern later
  })

  // old
    registerForm = this.fb.group({
    firstName: [''],
    lastName: [''],
    email: [''],
    password: [''],
  })
```

-  test the UI here https://localhost:4200/account/register

-  ` update register.component.html`

```
//...
  <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>First name</mat-label>
      <input formControlName="firstName" type="text" placeholder="John" matInput/>

        // update code below:

        @if (registerForm.get('firstName')?.hasError('required')) {
          <mat-error>First name is required</mat-error>
        }
    </mat-form-field>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Last name</mat-label>
      <input formControlName="lastName" type="text" placeholder="Smith" matInput/>

        // update code below:
        @if (registerForm.get('lastName')?.hasError('required')){
          <mat-error>Last name is required</mat-error>
        }
    </mat-form-field>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Email address</mat-label>
      <input formControlName="email" type="email" placeholder="name@example.com" matInput/>

        // update code below:
        @if (registerForm.get('email')?.hasError('required')) {
          <mat-error>Email is required</mat-error>
        }
    </mat-form-field>

    <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Password</mat-label>
      <input formControlName="password" type="password" placeholder="Password" matInput/>

        // update code below:
        @if(registerForm.get('password')?.hasError('required')) {
          <mat-error>Password is required</mat-error>
        }
    </mat-form-field>
//...

// Note: naa pay better way sa akoang solution para ma-manage ang mga validators
```

###### 152. Creating a re-usable text input

-  `create ' ng g c shared/components/text-input --skip-test '`
-  `update text-input.component.ts`

```
import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-text-input',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatInput,
    MatError,
    MatLabel
  ],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.scss'
})
            // TextInputComponent need to implement
export class TextInputComponent implements ControlValueAccessor {

  // use decoratorts instead of signal
  @Input() label: string = '';
  @Input() type: string = 'text';

  // need constructor for the decorators to work
  constructor(@Self() public controlDir: NgControl) {
    this.controlDir.valueAccessor = this; // set the value accessor
  }

  writeValue(obj: any): void {

  }
  registerOnChange(fn: any): void {

  }
  registerOnTouched(fn: any): void {

  }

  get control() {
    return this.controlDir.control as FormControl; // return the control from the directive
  }
}
```

-  `update text-input.component.html`

```
<mat-form-field appearance="outline" class="w-full mb-4">
  <mat-label>{{ label }}</mat-label>

  // kini ang tama - dapat no gap ang type={{type}} anything ingon anig format placeholder={{label}}
  <input [formControl]="control" type={{type}} placeholder={{label}} matInput/>
  // code below dili mo gana apg ang type={{ type }} naay space dapat no space or gap
  <!-- <input [formControl]="control" type={{ type }} placeholder={{ label }} matInput/> -->

  @if (control.hasError('required')) {
    <mat-error>{{ label }} is required</mat-error>
  }
  @if (control.hasError('email')) {
    <mat-error>email is invalid</mat-error>
  }
</mat-form-field>
```

-  `update register.component.html`

```
<mat-card class="max-w-lg mx-auto mt-32 p-8 bg-white">
  <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
    <div class="text-center mb-6">
      <h1 class="text-3xl font-semibold text-primary">Register</h1>
    </div>

    <app-text-input label="First name" formControlName="firstName" />
    <app-text-input label="Last name" formControlName="lastName" />
    <app-text-input label="Email address" formControlName="email" />
    <app-text-input label="Password" formControlName="password" type="password"/>

    <!-- <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>First name</mat-label>
      <input formControlName="firstName" type="text" placeholder="John" matInput/>
      @if (registerForm.get('firstName')?.hasError('required')) {
        <mat-error>First name is required</mat-error>
      }
    </mat-form-field> -->

    <!-- <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Last name</mat-label>
      <input formControlName="lastName" type="text" placeholder="Smith" matInput/>
      @if (registerForm.get('lastName')?.hasError('required')){
        <mat-error>Last name is required</mat-error>
      }
    </mat-form-field> -->

    <!-- <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Email address</mat-label>
      <input formControlName="email" type="email" placeholder="name@example.com" matInput/>
      @if (registerForm.get('email')?.hasError('required')) {
        <mat-error>Email is required</mat-error>
      }
    </mat-form-field> -->

    <!-- <mat-form-field appearance="outline" class="w-full mb-4">
      <mat-label>Password</mat-label>
      <input formControlName="password" type="password" placeholder="Password" matInput/>
      @if(registerForm.get('password')?.hasError('required')) {
        <mat-error>Password is required</mat-error>
      }
    </mat-form-field> -->

    @if (validationErrors){
      <div class="mb-3 p-4 bg-red-100 text-red-600">
        <ul class="list-disc px-3">
          @for (error of validationErrors; track $index) {
            <li>{{ error }}</li>
          }
        </ul>
      </div>
    }

    <button
        disabled="registerForm.invalid"
        mat-flat-button
        type="submit"
        class="w-full py-2">
        Register
    </button>
  </form>
</mat-card>

<!-- <mat-card class="max-w-lg mx-auto mt-3 p-2">
  <pre>Form values: {{ registerForm.value | json }}</pre>
  <pre>Form status: {{ registerForm.status }}</pre>
</mat-card> -->

```

-  `to test localhost:4200/account/register`

-  `update register.component.ts`

```
@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatCard,
    // MatFormField,
    // MatLabel,
    // MatInput,
    MatButton,
    // JsonPipe,
    // MatError,
    TextInputComponent
],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
```

###### 153. Creating an auth guard

```
all about:

client side security using auth guard
- hide things from the user on the clien side, active a route or deactivate a route
- example: hey you need to be login first
```

-  `ng g help`
-  `' ng g g core/guards/auth --dry-run ' testing`
-  `select: CanActivate`

-  `creating guard ' ng g g core/guards/auth --skip-test '`
-  `select: CanActivate`

```
// auth.gaurd.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../servies/account.service';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (accountService.currentUser()){
    return true;
  } else {
    router.navigate(['/account/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
};

// note: to use this go to app.routes.ts
```

-  ` update app.routes.ts`

```
//...
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  //...
  { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard] },
  //...
]

// next we go to login.component.ts
```

-  `update login.component.ts`

```
import { AccountService } from '../../../core/servies/account.service';
import { ActivatedRoute, Router } from '@angular/router';

export class LoginComponent {
  //...
    private accountService = inject(AccountService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  returnUrl = '/shop';

  constructor(){
    const url = this.activatedRoute.snapshot.queryParams['returnUrl'];
    if (url) this.returnUrl = url;
  }

    onSubmit(){
    this.accountService.login(this.loginForm.value).subscribe({
      next: () =>{
        this.accountService.getUserInfo().subscribe();
        this.router.navigateByUrl(this.returnUrl);
        // this.router.navigateByUrl('/shop');
      }
    })
  }
}

```

-  ` to test`

```
- go to https://localhost:4200/cart then checkout button
- you will be redirected here if not login: https://localhost:4200/account/login?returnUrl=%2Fcheckout

- logout and refresh and login again
- there is weird issue: after that it won't redirect (this is using signal)
```

###### 154. Updating the auth guard to use observables

```
- signal is a synchronous it about timing issue

- observable is asynchronous
```

-  `update auth.guard.ts`

```
import { of } from 'rxjs';
export const authGuard: CanActivateFn = (route, state) => {
  //...
  if (accountService.currentUser()){
    return of(true);
  } else {...}
}
```

-  `API - AccountController.cs = explain and connected with the timing issue`

```
    public ActionResult GetAuthState()
    {
        return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }
```

-  `update account.service.ts`

```
export class AccountService {

  //... updateAddress(...){...}

  getAuthSate(){
    return this.http.get<{isAuthenticated: boolean}>(this.baseUrl + 'account/auth-status');
  }
}

// back to the auth.guard.ts
```

-  ` update auth.guard.ts`

```
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../servies/account.service';
import { map, of } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (accountService.currentUser()){
    return of(true);
  } else {
    return accountService.getAuthSate().pipe(
      map(auth => {
        if (auth.isAuthenticated) {
          return true;
        } else{
          router.navigate(['/account/login'], { queryParams: { returnUrl: state.url } });
          return false;
        }
      })
    )
  }
};

// issue encounter during the testing
// to fix it go to account.service.ts
// then AccountController.cs
```

-  ` update AccountController.cs`

```
    // no router here [HttpGet()] should add "auth-status"
    [HttpGet("auth-status")]

    public ActionResult GetAuthState()
    {
      //... return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }
```

###### 155. Challenge - empty cart guard

```
- what if the basket is empty? because currently right now the checkout section is showing/displaying the order summary
- Objective is to hide it if its empty and show if there is something on the cart/basket.
```

-  ` update checkout.component.html`

```
<div class="mt-5">
  <h1 class="text-2xl"> Only authorized users and those with something in the cart should be able to see this!</h1>
</div>
```

###### 156. Challenge solution

-  `creating guard ' ng g g core/guards/empty-cart --skip-test '`

```
import { CanActivateFn, Router } from '@angular/router';
import { CartService } from '../services/cart.service';
import { inject } from '@angular/core';
import { SnackbarService } from '../services/snackbar.service';

export const emptyCartGuard: CanActivateFn = (route, state) => {
  const cartService = inject(CartService);
  const router = inject(Router);
  const snack = inject(SnackbarService);

  if (!cartService.cart() || cartService.cart()?.items.length === 0) {
    snack.error("Your cart is empty. Please add items to your cart before proceeding.");
    router.navigate(['/cart']);
    return false;
  }
  return true;
};

// then use it app.router.ts
```

-  `update app.router.ts`

```
//...
import { emptyCartGuard } from './core/guards/empty-cart.guard';

export const routes: Routes = [
  //...
  { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard, emptyCartGuard] },
  //...
]
```

###### 157. Adding an empty state component

```
Note: better approach when we don't have anything to display in cart
```

-  `create ' ng g c shared/components/empty-state --skip-test '`
-  ` empty-state.component.ts`

```
import { Component } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  imports: [
    MatIcon,
    MatButton,
    RouterLink
  ],
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.scss'
})
export class EmptyStateComponent {

}
```

-  ` update empty-state.component.html`

```
<div class="max-w-screen-xl mx-auto mt-32 px-10 py-4 bg-white rounded-lg shadow-md w-full">
  <div class="flex flex-col items-center justify-center py-12 w-full">
    <mat-icon class="icon-display mb-8">shopping_cart</mat-icon>
    <p class="text-gray-600 text-lg font-semibold mb-4">
      Your shopping cart is empty
    </p>
    <button mat-flat-button>Go Shopping!</button>
  </div>
</div>
```

-  `empty-state.component.scss`

```
.icon-display{
  transform: scale(3);
}
```

-  `  update cart.component.html - important`

```
<section>
  // wierd issue on this part (cartService.cart()?.items?.length > 0 )
  // to solve go to cart.services.ts
  // if sometimes typescript is showin silly error you can override it. by adding ! at cartService.cart()?.items?.length! > 0
  @if (cartService.cart()?.items?.length! > 0 ) {

    <div class="max-auto max-w-screen-xl">
      <div class="flex w-full items-start gap-6 mt-32">
        <div class="w-3/4">
          @for(item of cartService.cart()?.items; track item.productId) {
            <app-cart-item [item]="item"></app-cart-item>
            <!-- <div>{{ item.productName }} - {{ item.quantity }}</div> -->
          }
        </div>
        <div class="w-1/4">
          <app-order-summary></app-order-summary>
        </div>
      </div>
    </div>
  } @else {
    <app-empty-state></app-empty-state>
  }
</section>
```

-  ` not so important in this lecture:  update cart.component.ts`

```
import { EmptyStateComponent } from "../../shared/components/empty-state/empty-state.component";
@Component({
  selector: 'app-cart',
  imports: [
    //...OrderSummaryComponent,
    EmptyStateComponent // import this
  ],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
```

###### 158. Summary

```
  - ability to login and register

- Adding an Account feature
- Forms in Angular
- Reactive forms
- Reusable form components
- Client side validation

FAQs:

Q:  Why don't you convert template forms?
A:  They don't give us any functionality we can't have in
    Reactive forms. Reactive forms are more powerful.

```

<hr>

### API & Client Checkout : Module 16

<hr>

###### Module 16 : API & Client Checkout = 159. Introduction

-  `Ideas `

```
- PCI
- Strong Customer Authentication
```

-  `In this module`

```
- Creating a Stripe account
- PCI DSS Compliance
- Strong Customer Authentication -
- Setting up Payment Intents
- Using Stripe Elements
- Confirming Card Payments
```

-  ` Goal:`

```
To be able to accept payments
securely globally that complies
with EU regulations and PCI DSS
regulations.

- Payment Card Industry Data Security Standard ( PCI DSS)
    - Set of industry standards
    - Designed to protect payment card data
    - Increased protection for customers and reduced
      risk of data breaches involving personal card data
    -12 broad requirements and collectively more than 200
      line item requiremnts

    - 6 key ares:
      - Building and maitaining a secure network
      - Protecting cardholder data
      - Maintaining a vulnerability management program
      - Implementing strong access control measures
      - Regular monitor and test networks
      - Maintaining an information security policy

      Note: stripe is a payment processor

PCI DSS non-compliance consequences
  - Monthly financial penalties from $5,0000 to $100,000
  - Infringement consequences ($50 to $90 per card
      holder whose informatin has been endangered)
  - Compensation costs
  - Legal Action
  - Damage reputation
  - Revenue loss
  - Federal audits

* Strong Customer Authentication

- EU standards authenticating online payments
- Requires two of three elements:
  - Something the customer knows (password or pin)
  - Something the customer has (phone or hardware token)
  - Something the customer is (fingerprint or facial recognition)

- Banks will decline payments that require SCA and
  don't meet this criteria

============================================================
Strip without SCA ( USA and Canadian payment only)

1) Create order on API.
2) If success make payment to Stripe
3) Stripe returns one time use token if payment succeeds
4) Client sends token to the API
5) API verifies token with Stripe
6) Stripe confirms token.
7) On success/failure result sent to client from API

 _________       __________
|         |     |          |
|   API   |     |   STRIPE |
|_________|     |__________|


[ client ]
=============================================================

below approach that we are going to use for our project

Stripe with SCA - Accept payment globally

1) Create payment intent with API (before payment)
2) API sends payment intent to Stripe
3) Stripe creates payment intent return client secret
4) API returns client secret to client
5) Client sends payment to stripe using the client secret
6) Stripe Sends confirmation to client payment was successful
7) Client creates order with API
8) Stripe sends confirmation to API that payment was successfull - wehbook
9) Payment confirmed and can be shipped

 _________       __________
|         |---->|          |
|   API   |<----|   STRIPE |
|_________|     |__________|
  ^               ^
  |              /
   \            /
    \          /
    [ client  ]

```

###### 160. Creating the delivery methods in the API

-  `create new Entity ' Core/Entities/DeliveryMethod.cs'`

```
namespace Core.Entities;

public class DeliveryMethod : BaseEntity
{

    public required string ShortName { get; set; }
    public required string DeliveryTime { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    // note: kay tungod naa daw ang decimal mao ng mag buhat ug DeliveryMethodConfiguration.cs
}
```

-  `create new ' Infrastructure/Config/DeliveryMethodConfiguration.cs ' `

```
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class DeliveryMethodConfiguration : IEntityTypeConfiguration<DeliveryMethod>
{
    public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
    {
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
    }
}

// unya adto ta sa StoreContext.cs
```

-  ` update StoreContext.cs`

```
public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
  // ...
  public DbSet<DeliveryMethod> DeliveryMethods { get; set; }

  // ...
}

// note: Infrastructure/Data/SeedData/delivery.json i-seed ang mga data diri.
// buhaton nato ni sa StoreContextSeed.cs
```

-  `update StoreContextSeed.cs`

```
public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context)
    {
      //...if (!context.Products.Any()) {...}

      if (!context.DeliveryMethods.Any())
        {
            var dmData = await File.ReadAllTextAsync("../Infrastructure/Data/SeedData/delivery.json");

            var methods = JsonSerializer.Deserialize<List<DeliveryMethod>>(dmData);

            if (methods == null) return;
            context.DeliveryMethods.AddRange(methods);
            await context.SaveChangesAsync();
        }
    }
}
```

-  `cd API root  'dotnet ef migrations add DeliveryMethodsAdded -p Infrastructure -s API'`

```
Issue: on old EF version
- updating a t entity framework tools version 'x.x.x' is older ...

Solution:
- dotnet tool update dotnet-ef -g
```

-  Test Data DeliveryMethodsAdded.cs

```
- To check if data has been seeded in the database
- go Infrastructure/Migrations/######_DeliveryMethodsAdded.cs
- re-run in the terminal cd api / dotnet watch
- the logs below will show

            info: Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand //...

- it means successful.
```

###### 161. Setting up Stripe

-  `https://stripe.com/pricing`
-  `https://dashboard.stripe.com/test/dashboard - wilfred057777@gmail.com`

```
-> In Strip sandbox - click the top left - Shop icon->
-> the create new account -> 'Other accounts' ->  + Create account ( fill up the form)
-> after that:

-> at Home tab-> API key(right side at this current time - June2025 UI )
-> Publishable key: pk_test_51RaRRpQ4ykDn46yOi2ItBhEXc7gWqmfj8tV3mfFaUAllbGzNG9tc0pEqs7jvPv3uwggX8GUDAl8GA7U4gbSOCYWM00aqPSWy9D

-> Secret key: sk_test_51RaRRpQ4ykDn46yOxDUqEkKMSkAxBpFBU4g7AL3TJZEGWF4Gy4r56vA2tcJnRf4yTiCmfqHiwQ4v9vx2Ps7M8TU500wlrybuus

// then we go to: 
```
- ` API/appsettings.Development.json `
```
  "ConnectionStrings":{...},
  "StripeSettings":{
    "PublishableKey":"pk_test_51RaRRpQ4ykDn46yOi2ItBhEXc7gWqmfj8tV3mfFaUAllbGzNG9tc0pEqs7jvPv3uwggX8GUDAl8GA7U4gbSOCYWM00aqPSWy9D",
    "SecretKey":"sk_test_51RaRRpQ4ykDn46yOxDUqEkKMSkAxBpFBU4g7AL3TJZEGWF4Gy4r56vA2tcJnRf4yTiCmfqHiwQ4v9vx2Ps7M8TU500wlrybuus"
  },
```
- ` update API/appsettings.json`
```
{
  //"Loggin": {...},
  "StripeSettings":{
  "PublishableKey":"pk_test_51RaRRpQ4ykDn46yOi2ItBhEXc7gWqmfj8tV3mfFaUAllbGzNG9tc0pEqs7jvPv3uwggX8GUDAl8GA7U4gbSOCYWM00aqPSWy9D",
  "SecretKey":"sk_test_51RaRRpQ4ykDn46yOxDUqEkKMSkAxBpFBU4g7AL3TJZEGWF4Gy4r56vA2tcJnRf4yTiCmfqHiwQ4v9vx2Ps7M8TU500wlrybuus"
  },
}
```
- ` note for stripe Publishable and Secrete keys: you will see test on the generated keys but in production you will not see the test on the keys!!` 

- ` in order to used the stripe key in the application project we need a package for stripe`
```
=> go to nuget -> stripe.net - by Jayme Davis ( for this project )
=> add to Infrastructure.csproj
=> to utilize the stripe for the C# backend we gonna create a payment service
```
- `=> Solution Explorer: create -> Core/Interfaces/IPaymentService.css`
```
using Core.Entities;

namespace Core.Interfaces;

public interface IPaymentService
{
    Task<ShoppingCart> CreateOrUpdatePaymentIntent(string cartId);
}

```
- ` next the we create the implmentation class`
- ` go to Infrastructure/Services/PaymentService.cs`
```

using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Services;

             // implement the IPaymentService interface
public class PaymentService : IPaymentService
{
    public Task<ShoppingCart> CreateOrUpdatePaymentIntent(string cartId)
    {
        throw new NotImplementedException();
    }
}
```
- ` Then add the service to the program.cs class`
```
//... add the service below this code
//... builder.Services.AddIdentityApiEndpoints<AppUser>().AddEntityFrameworkStores<StoreContext>();

builder.Services.AddScoped<IPaymentService, PaymentService>(); // add the stripe service here
```

###### 162. Implementing the payment intent
- ` update to return null at the moment the following files:`
- ` PaymentService.cs & IPaymentService.cs` 
```
public class PaymentService : IPaymentService
{
    //update ShoppingCart to null by adding '?'
    public Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {}
}
```
- `Update IPaymentService.cs`
```
public interface IPaymentService
{
    //update ShoppingCart to null by adding '?'
    Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId);
}
```
- `next go to Core/Entities/ShoppingCart.cs and update the code`
```
namespace Core.Entities;

public class ShoppingCart
{
  //... public
  public int? DeliveryMethodId { get; set; }
  public string? ClientSecret { get; set; }
  public string? PaymentIntentId { get; set; }
}
```
- ` next we go to PaymentService.cs`
```
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class PaymentService(
    IConfiguration config,
    ICartService cartService,
    // setting Product to not be ambiguous with Core.Entities.Product (below code:)
    IGenericRepository<Core.Entities.Product> productRepo, 
    IGenericRepository<DeliveryMethod> dmRepo) 
    : IPaymentService
{
    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null) return null;

        var shippingPrice = 0m;

        if (cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await dmRepo.GetByIdAsync((int)cart.DeliveryMethodId);

            if (deliveryMethod == null) return null;

            shippingPrice = deliveryMethod.Price;
        }

        // validate the items for the cart
        foreach (var item in cart.Items)
        {
            var productItem = await productRepo.GetByIdAsync(item.ProductId);
            if (productItem == null) return null;

            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }

        // variable for service
        var service = new PaymentIntentService();
        PaymentIntent? intent = null;

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                        + (long)shippingPrice * 100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            intent = await service.CreateAsync(options);

            // update the cart with the payment intent id and client secret
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                        + (long)shippingPrice * 100,
            };
            intent = await service.UpdateAsync(cart.PaymentIntentId, options);
        }

        await cartService.SetCartAsync(cart);

        return cart; // dapat naa return para dili mag problema ang CreateOrUpdatePaymentIntent class kay pagwala ni return, mag error siya nga wala daw return type
    }
}
```
[Implementing the payment intent](https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45151441#notes)
- `note to myself: balikan ni na video 'Implementing the payment intent' kay gi explain niya ang dagan sa pagbuhat ug PaymentService thoroughly ` 

###### 163. Creating a payment controller

- ` create new controller ' API/Controllers/PaymentsController.cs ` 
```
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PaymentsController(
        IPaymentService paymentService,
        IGenericRepository<DeliveryMethod> dmRepo
    ) : BaseApiController
{
    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem with your cart");

        return Ok(cart);
    }

    [HttpGet("deliveryMethods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await dmRepo.ListAllAsync());
    }

}

```
- ` check Stripe https://dashboard.stripe.com/test/payments `

- ` test API via Postman Section 16: Payments `
```
- Update cart : {{url}}/api/cart 
- json raw files
```

```
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 1,
            "quantity": 2,
            "pictureUrl": "https://localhost:5001/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ]
}
```
```
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 1,
            "quantity": 2,
            "pictureUrl": "https://localhost:5001/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    "deliveryMethodId": null,
    "clientSecret": null,
    "paymentIntentId": null
}
```
- `Postman => Create payment intent as tom {{url}}/api/payments/cart1`
```
- Create payment intent as tom`
- Login as top first - Section 14: Login as tom => {{url}}/api/login?useCookies=true => 200 ok response
- create payment as tom => {{url}}/api/payments/cart1
- Status: 200 ok

- Body return:
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 150.00,
            "quantity": 2,
            "pictureUrl": "https://localhost:5001/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    
    // get this 3 items and add to `Update Cart - Add payment intent details`
    "deliveryMethodId": null,
    "clientSecret": "pi_3Rap5XQ4ykDn46yO27Ar5LkA_secret_nvCwFM39efPgMyV3w4setY1CV",
    "paymentIntentId": "pi_3Rap5XQ4ykDn46yO27Ar5LkA"
}

// check the stripe https://dashboard.stripe.com/test/payments or in the transactions tab
// update the cart 
// kaning Update cart intent for the mean time we do it manually sa Postman later we create UI after ni
// `Update Cart - Add payment intent details` - Add payment intent details => {{url}}/api/cart

{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 1,
            "quantity": 10,
            "pictureUrl": "https://localhost:5001/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    "deliveryMethodId": null,
    "clientSecret": "pi_3Rap5XQ4ykDn46yO27Ar5LkA_secret_nvCwFM39efPgMyV3w4setY1CV",
    "paymentIntentId": "pi_3Rap5XQ4ykDn46yO27Ar5LkA"
}
// send this request then 
// next
```
- `Update payment intent as bob => {{url}}/api/payments/cart1 `
```
- at postman send this request {{url}}/api/payments/cart1 
{}
//

// return response coming Status: 200 Ok
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 150.00,
            "quantity": 10,
            "pictureUrl": "https://localhost:5001/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    "deliveryMethodId": null,
    "clientSecret": "pi_3Rap5XQ4ykDn46yO27Ar5LkA_secret_nvCwFM39efPgMyV3w4setY1CV",
    "paymentIntentId": "pi_3Rap5XQ4ykDn46yO27Ar5LkA"
}

// then check again the Stripe 
// https://dashboard.stripe.com/test/payments/ 
// price before the update was $300 the it became $1,500

```
- this Chapters needed to be rewatch for better understanding how he create services and controllers to connect to Stripe payment
[162. Implementing the payment intent](https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45151441#content)
[163. Creating a payment controller](https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45151443#content)


###### 164. Checkout page layout

- ` update checkout.component.html`
```
<div class="flex mt-32">
  <div class="w-3/4">
    Checkout Stepper
  </div>
  <div class="w-1/4">
    <app-order-summary></app-order-summary>
  </div>
</div>
```
- ` update checkout.component.ts`
```
import { OrderSummaryComponent } from "../../shared/components/order-summary/order-summary.component";

@Component({
  //...
  imports: [OrderSummaryComponent],
  //...
})
```

- ` update cart.component.html `
```
  // removed this because its restricting the full width of the certain section
   <div class="max-auto max-w-screen-xl"> 
   //...
   </div>
```
[Angular Material Stepper](https://material.angular.dev/components/stepper/overview)

- ` To use the Angular Material Stepper ` 
- ` in this case we go to checkout.component.ts`

```
import { MatStepperModule } from '@angular/material/stepper';
@Component({
  selector: 'app-checkout',
  imports: [
    //...OrderSummaryComponent,
    MatStepperModule
  ],
  //...
})
```
- ` update checkout.component.html`
```
<div class="flex mt-32 gap-6">
  <div class="w-3/4">
    <!-- Checkout Stepper -->
     <mat-stepper #stepper class="bg-white border border-gray-200 shadow-sm">
      <mat-step label="Address"> Address form</mat-step>
      <mat-step label="Shipping"> Delivery form</mat-step>
      <mat-step label="Payment"> Payment form</mat-step>
      <mat-step label="Confirmation"> Review form</mat-step>
     </mat-stepper>
  </div>
  <div class="w-1/4">
    <app-order-summary></app-order-summary>
  </div>
</div>
```
- ` Issue User Checkout hide checkout button when in user checkout page `
```
  - update order-summary.component.ts purpose is to hide the UI if a user is the checkout stepper
  - 
```
- `update order-summary.component.ts`
```
//...
import { CurrencyPipe, Location } from '@angular/common';
//...

export class OrderSummaryComponent {
  //...cartService = inject(CartService);

  location = inject(Location); // update
}
```
- ` update order-summary.component.html `
```
//...put this inside the if (location.path() !== '/checkout'){ ... }
<div class="flex flex-col gap-2">
  <button routerLink="/checkout" mat-flat-button>Checkout</button>
  <button routerLink="/shop" mat-button>Continue Shopping</button>
</div>

// take away for some reason it didn't work on its own location.path '.path' has issue at order-summary.component.ts 
// must add `PathLocationStrategy` to 
// import { CurrencyPipe, Location, PathLocationStrategy } from '@angular/common';
// 1- read the terminal prompting issue
// 2- if unfamiliar go to google and search or use AI/
// 3- vscode - terminal `[ERROR] NG9: Property 'path' does not exist on type 'Location'.`
// 4- google and just scroll the searches that are comming related to the bug prompt and lucky lead me to 
// 5- https://angular.dev/api/common/Location and which I find it more reliable since its coming from 
//    angular documentations and just trying to hook the class or library 

// update code below:
@if (location.path() !== '/checkout'){
  <div class="flex flex-col gap-2">
    <button routerLink="/checkout" mat-flat-button>Checkout</button>
    <button routerLink="/shop" mat-button>Continue Shopping</button>
  </div>
}
//...
```

###### 165. Adding client side Stripe

- ` install : ' npm install @stripe/stripe-js ' at client: cd client `
- ` create  : ' ng g s core/services/stripe --skip-test' `

- ` stripe.service.ts but needs enviroment.ts setup by adding publisable key coming from stripe `
```

```
- ` update cart.ts `
```
export type CartType = {
  //...id: string;
  //...items: CartItem[];
  deliveryMethodId?: number; // update
  paymentIntentId?: string; // update 
  clientSecret?: string; // update
}

//...

  // id = ''; // generate a random id in this example nanoid package
  //...id = nanoid(); // implment nanoid package to generate a random id
  //...items: CartItem[] = []; // CartItem[] is an array while = [] is an empty array
  
  deliveryMethodId?: number; // update
  paymentIntentId?: string; // update
  clientSecret?: string; // update
```

- ` update environment.development.ts & environment.ts ` 
```
// this should be done first before stripe.service.ts
// get the publishable key at stripe

export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/',
  // get this publish key from your Stripe dashboard
  stripePublicKey:
  'pk_test_51RaRRpQ4ykDn46yOi2ItBhEXc7gWqmfj8tV3mfFaUAllbGzNG9tc0pEqs7jvPv3uwggX8GUDAl8GA7U4gbSOCYWM00aqPSWy9D',
};
```
- `update environment.ts `
export const environment = {
  production: true,
  apiUrl: 'api/',
  // update code below with the publishable key
  stripePublicKey:
  'pk_test_51RaRRpQ4ykDn46yOi2ItBhEXc7gWqmfj8tV3mfFaUAllbGzNG9tc0pEqs7jvPv3uwggX8GUDAl8GA7U4gbSOCYWM00aqPSWy9D',
};

```
 need to rewatch this again with out typing the code and just try to understand the flow
 very important in setting or connecting stripe api how it is created

```
[165. Adding client side Stripe](https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45151525#overview)

###### 166. Creating the address element

- ` update stripe.service.ts`
```
import { loadStripe, Stripe, StripeAddressElement, StripeAddressElementOptions, StripeElements } from '@stripe/stripe-js';

export class StripeService {
  //private elements?: StripeElements;
  private addressElement?: StripeAddressElement;
}

  //... async initializeElements() {...}

  async createAddressElement() {
    if (!this.addressElement){
      const elements = await this.initializeElements();
      if(elements) {
        const options: StripeAddressElementOptions = {
          mode: 'shipping',
        };
        this.addressElement = elements.create('address', options);
      } else{
        throw new Error('Elements instance not been loaded');
      }
    }
    return this.addressElement;
  }

  //...createOrUpdatePaymentIntent(){...}
  createOrUpdatePaymentIntent(){
    const cart = this.cartService.cart();
    if (!cart) throw new Error('Problem with cart');
    // 500 error is due to return , but it should be + for concatenation 
    // this.http.post<Cart>(this.baseUrl + 'payments/', cart.id, {}).pipe( 
    return this.http.post<Cart>(this.baseUrl + 'payments/' + cart.id, {}).pipe(
      map(cart => {
        this.cartService.cart.set(cart);
        return cart;
      })
    );
  }
}
```
- ` update checkout.component.html`
```
//...
    <mat-stepper #stepper class="bg-white border border-gray-200 shadow-sm">
      <mat-step label="Address">
        <div class="address-element">
        </div>
        <div class="flex justify-between mt-6">
        <button routerLink="/shop" mat-stroked-button>Continue shopping</button>
        <button matStepperNext mat-flat-button>Next</button>
      </div>
    </mat-step>
//...
```
- ` update checkout.component.ts`
```
import { Component, inject, OnInit } from '@angular/core';
import { OrderSummaryComponent } from "../../shared/components/order-summary/order-summary.component";
import { MatStepperModule } from '@angular/material/stepper';
import { MatButton } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { StripeService } from '../../core/services/stripe.service';
import { StripeAddressElement } from '@stripe/stripe-js';
import { SnackbarService } from '../../core/services/snackbar.service';


@Component({
  selector: 'app-checkout',
  imports: [
    OrderSummaryComponent,
    MatStepperModule,
    MatButton,
    RouterLink
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  // inject stripe into our component
  private stripeService = inject(StripeService);
  private snackBar = inject(SnackbarService);
  addressElement?: StripeAddressElement;

  async ngOnInit() {
    try {
      this.addressElement = await this.stripeService.createAddressElement();
      this.addressElement.mount('#address-element');
    } catch (error: any) {
      this.snackBar.error(error.message);
    }
  }
}
```
- ` test ui https://localhost:4200/checkout `
- ` encouter issues:`
```
  - on address it create 404 on api
  
  - browser prompt:
    - Request path: POST http://localhost:5000/api/payments, Response status code: 404 
  
    - POST http://localhost:5000/api/payments [HTTP/1.1 404 Not Found 0ms] 
    
```

###### 167. Populating the address in the address form

```
- postman
- Section 14 
  - Add User Address (tom) => {{url}}/api/account/address
  - 
  {
    "line1": "100 Park Lane",
    "city": "London",
    "state": "London",
    "postalCode": "SW1 1BD",
    "country": "GB"
  }

- Get Current User => {{url}}/api/account/user-info
- 
  {
      "firstName": "Tom",
      "lastName": "Smith",
      "email": "tom@test.com",
      "address": {
          "line1": "100 Park Lane",
          "line2": null,
          "city": "London",
          "state": "London",
          "postalCode": "SW1 1BD",
          "country": "GB"
      }
  }
```

- ` update stripe.service.ts`
```
//...
import { AccountService } from '../servies/account.service';

export class StripeService {

  //...
    private accountService = inject(AccountService);
  //...

    async createAddressElement() {
    if(elements) {
        const user = this.accountService.currentUser(); // update
        let defaultValues: StripeAddressElementOptions['defaultValues'] = {}; // update

        if(user) {
          defaultValues.name = user.firstName + ' ' + user.lastName; // update
        }

        if(user?.address){ // update 
          defaultValues.address = { // update
            line1: user.address.line1, // update
            line2: user.address.line2, // update
            city: user.address.city, // update
            state: user.address.state, // update
            country: user.address.country, // update
            postal_code: user.address.postalCode // update
          }
        }

        const options: StripeAddressElementOptions = {
          //... mode: 'shipping',
          defaultValues  // update
        };
    }
}

// at localhost:4200/checout 
// under addres it should populate the address which in this case i haven't fix the issue yet.
// 
```

- ` just fixed the issue about register.component.html `
```
[disabled]="registerForm.invalid" => adding [] in disabled
```

- ` update stripe.service.ts`
```
export class StripeService {

  //...

  disposeElements(){
    this.elements = undefined;
    this.addressElement = undifined;
  }
}

// to use this we go to checkout.component.ts
```
- ` update checkout.component.ts => para ni sa different user nga mo login dili mahabilin ilahang cookie`
```
import {...Component, ...inject, OnDestroy, ...OnInit } from '@angular/core';
//...

export class CheckoutComponent implements OnInit, OnDestroy {

  //...

  ngOnDestroy(): void {
    this.stripeService.disposeElements();
  }
}
```
- ` ngOndestroy is iyahang ipang remove ang cookie of a certian user`
```
- to test this is dapat imoha ipangLogin then ilogout with different user
```

###### 168. Save the address as default address

- `save address input into our database`

- [Checkbox Material Ui](https://material.angular.dev/components/checkbox/overview)

- `update checkout.component.ts`
```
//...
import {MatCheckboxChange, MatCheckboxModule} from '@angular/material/checkbox';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { Address } from '../../shared/models/user';

@Component({
  selector: 'app-checkout',
  imports: [
    //...RouterLink
    MatCheckboxModule
  ],
  //...templateUrl:'...';
})
export class CheckoutComponent implements OnInit, OnDestroy {
  //...addressElement?: StripeAddressElement;
  saveAddress = false;

  //... async ngOnInit(){...}

  async onStepChange(event:StepperSelectionEvent){
      if (event.selectedIndex === 1){
        if (this.saveAddress){
          // const address = await this.addressElement?.getValue();
          const address = await this.getAddressFromStripeAddress();
        }
      }
    }
    private async getAddressFromStripeAddress(): Promise<Address | null> {
      const result = await this.addressElement?.getValue();
      const address = result?.value.address;

      if (address){
        return {
          line1: address.line1,
          line2: address.line2 || undefined,
          city: address.city,
          country: address.country,
          state: address.state,
          postalCode: address.postal_code,
        }
      } else return null;
    }

    onSaveAddressCheckboxChange(event: MatCheckboxChange){
      this.saveAddress = event.checked;
    }

  //... ngOnDestroy():void{...}
}

```
- ` update checkout.component.html`
```
      
    <div class="w-3/4">
      <!-- Checkout Stepper -->
        <mat-stepper
        (selectionChange)="onStepChange($event)" // update
        #stepper
      class="bg-white border border-gray-200 shadow-sm">
        <mat-step label="Address">
          <div id="address-element"></div>
          
          <!--  // update start -->
          <div class="flex justify-end mt-1">
            <mat-checkbox
              [checked]="saveAddress"
              (change)="onSaveAddressCheckboxChange($event)"
            >
              Save as default address
            </mat-checkbox>
          </div>
          <!--  // update end -->
      //...
```

- `update account.service.ts `

```
//...
import { map, tap } from 'rxjs';

  updateAddress(address: Address){
    // return this.http.post(this.baseUrl + 'account/address', address);
    return this.http.post(this.baseUrl + 'account/address', address).pipe(

      // using 'tap' because we don't need to interfere with the incoming data
      tap(()=> {
        this.currentUser.update(user => {
          if (user) user.address = address
          return user;
        })
      })
    );
  }

/// going back to checkout.component.ts
```
- ` 168. Save the address as default address => checkout.component.ts`
```
//...
import { AccountService } from '../../core/servies/account.service';  // update

export class CheckoutComponent implements OnInit, OnDestroy {
  //... private snackbar = inject(SnackbarService);
  private accountService = inject(AccountService); // update
  //... addressElement?: StripeAddressElement;

  //...
  async onStepChange(event:StepperSelectionEvent){
    if (event.selectedIndex === 1){
      if (this.saveAddress){
        const address = await this.getAddressFromStripeAddress();
        address && firstValueFrom(this.accountService.updateAddress(address)); // update
      }
    }
  }

  //...
}
```

- ` Doing Address Stepper test below are the steps: `
```
- doing a test in the localhost:4200/checkout
- fill in the Address stepper
- tick => checkbox: set to default address 
- save, if it is successful all data will be save once you try to refresh the browser
```

###### 169. Creating the delivery component part 1

- `Step-1-a:169 In client folder => ' ng g c features/checkout/checkout-delivery --skip-tests ' `

- `step-1-b:169 we need to create a checkoutDelivery models => client/src/app/shared/models/deliveryMetho.ts `
```
// this below is our property for our deliver checkout-delivery.component.ts 
export type DeliveryMethod = {
  shortName: string;
  deliveryTime: string;
  description: string;
  price: number;
  id: number;
}
```
- `step-2:169 update the checkout.service.ts `
```
// step-2:169 
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { DeliveryMethod } from '../../shared/models/deliveryMethods';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  deliveryMethods: DeliveryMethod[] = [];


  getDeliveryMethods(){

    if (this.deliveryMethods.length > 0) return of(this.deliveryMethods)
    return this.http.get<DeliveryMethod[]>(this.baseUrl + 'payments/delivery-methods').pipe(
      map(methods => {
        this.deliveryMethods = methods.sort((a, b) => b.price - a.price);
        return methods
      })
    )
  }
}

```

- `step-3: checkout-delivery.component.ts`
```
import { Component, inject, OnInit } from '@angular/core';
import { CheckoutService } from '../../../core/services/checkout.service';

@Component({
  selector: 'app-checkout-delivery',
  imports: [],
  templateUrl: './checkout-delivery.component.html',
  styleUrl: './checkout-delivery.component.scss'
})
export class CheckoutDeliveryComponent implements OnInit {
  checkoutService = inject(CheckoutService);

  ngOnInit(): void {
    this.checkoutService.getDeliveryMethods().subscribe();
  }
}
```

- `step-4: checkout.component.html`
```
  //...    
    <!-- <mat-step label="Shipping">...</mat-step> update the content inside ... -->

      <mat-step label="Shipping">
        <!-- Delivery form -->
         <app-checkout-delivery></app-checkout-delivery>
          <div class="flex justify-between mt-6">
            <button matStepperPrevious mat-stroked-button>Back</button>
            <button matStepperNext mat-flat-button>Next</button>
          </div>
      </mat-step>
    
  //...
```
###### 170. Creating the delivery component part 2
- ` step-5: create radio buttons - go to angular material - radio button `
- [ng material - radio button](https://material.angular.dev/components/radio/overview)
- [Api ref. for angular Material radio](https://material.angular.dev/components/radio/api)
- ` import {MatRadioModule} from '@angular/material/radio'; insert this to `

- ` step-5:170 - update checkout-delivery.component.ts ` 
```
//... step-5.b:170
import {MatRadioModule} from '@angular/material/radio';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-checkout-delivery',
  imports: [
    MatRadioModule, // udpate
    CurrencyPipe // udpate
  ],
  // templateUrl:...
})

//...
```
- ` step-6:170 update template checkout-delivery-component.html `
```
<div class="w-full">
  <mat-radio-button class="grid grid-cols-2 gap-4">
    @for (method of checkoutService.deliveryMethods; track method.id) {
      <label class="p-3 border border-gray-200 cursor-pointer w-full h-full hover:bg-purple-100">
        <mat-radio-button class="w-full h-full" [value]="method">
          <div class="flex flex-col w-full h-full">
            <strong>{{ method.shortName }} - {{ method.price | currency }}</strong>
          </div>
        </mat-radio-button>
      </label>
    }
  </mat-radio-button>
</div>
```

- Issue/Bug spotted @ 170. Creating the delivery component part 2: !FF = functional but the UI is broken

```
- bug-170-a: stepper Header and stepper container: - background color is not white instead #FEF8FC <= wrong #ffffff
- bug-170-b: shipping tab UI display is not properly align to 2column and 2rows size distribution

```

- Objective: 
```
- upon selecting any one of the shipping fee the 'Order summary' section the 'Delivery fee' should update
 
```

###### 171. Creating the delivery component part 3

- ` step-7:171 Update cart.service.ts `

```
//...
import { DeliveryMethod } from '../../shared/models/deliveryMethods';

// create another signal
  seletedDelivery = signal<DeliveryMethod | null>(null);
  totals = computed(() => {
    //...const cart = this.cart();
    const delivery = this.seletedDelivery();

    if(!cart) return null;
    //... const subTotal = cart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const shipping = delivery ? delivery.price : 0; // update
    //... const discount = 0;
  })
```

- ` step-6b:171 update checkout-delivery.component.ts `
```
//...
import { CartService } from '../../../core/services/cart.service'; // update
import { DeliveryMethod } from '../../../shared/models/deliveryMethods'; // update

export class CheckoutDeliveryComponent implements OnInit {
  //...
  cartService = inject(CartService);   // update 

  ngOnInit(): void {
    // update below
    this.checkoutService.getDeliveryMethods().subscribe({
      next: methods => {
        if (this.cartService.cart()?.deliveryMethodId) {
          const method = methods.find(x => x.id === this.cartService.cart()?.deliveryMethodId);
          if(method){
            this.cartService.seletedDelivery.set(method);
          }
        }
      }
    });
  }

  // update below code
  updateDeliveryMethod(method: DeliveryMethod){
    // update our signal
    this.cartService.seletedDelivery.set(method);
    const cart = this.cartService.cart();
    if(cart) {
      cart.deliveryMethodId = method.id;
      this.cartService.setCart(cart);
    }
  }

}

```

- `step-6c:171 Update checkout-delivery.component.html `
```
<div class="w-full">
  <mat-radio-group
    [value]="cartService.seletedDelivery()?.id" // update
    (change)="updateDeliveryMethod($event.value)" // update
    class="grid grid-cols-2 gap-4"
  >
  @for (method of checkoutService.deliveryMethods; track method.id) {
      <label class="p-3 border border-gray-200 cursor-pointer w-full h-full hover:bg-purple-100">
        <mat-radio-button
          class="w-full h-full" [value]="method" // update
          [checked]="cartService.seletedDelivery() === method" // update
          [value]="method" 
          >
          <div class="flex flex-col w-full h-full">
            <strong>{{ method.shortName }} - {{ method.price | currency }}</strong>
            <span class="text-sm">{{ method.description }}</span>
          </div>
        </mat-radio-button>
      </label>
    }
  </mat-radio-group>
</div>
```

- ` step-6d:171 update checkout.component.ts ` 
```
  async onStepChange(event:StepperSelectionEvent){
    if (event.selectedIndex === 1){
      //...
    }

    // update code below
        if(event.selectedIndex === 2){
      // update payment intent
      await firstValueFrom(this.stripeService.createOrUpdatePaymentIntent());
    }
  }
```
- `step-6e:171 update stripe.service.ts `
```
  createOrUpdatePaymentIntent(){
    const cart = this.cartService.cart();
    if (!cart) throw new Error('Problem with cart');
    return this.http.post<Cart>(this.baseUrl + 'payments/' + cart.id, {}).pipe(
      map(cart => {
        this.cartService.setCart(cart); // this pass the redis // updated this line
        // this.cartService.cart.set(cart); // while this pass only on local browser
        return cart;
      })
    );
  }
```
- ` step-6f:171 testing via browser`
```
- localhost:4200/checkout
- address
- shipping
  - ticking any will update the following:
    - Order summary
      - Delivery fee
      - Total will compute base on the delivery/shipping fee

- then test on strip payment dashboard 
  - it will reflect the payment
  - https://dashboard.stripe.com/test/payments
  - in my case: $210/$205 - June 29,2025 - if it reflect the value here it means it is successful.

-bug fFix also the issue of 2x2 UI

- next on step is on payment tab for checkout page

```

###### 172. Creating the payment element

- ` 172-Update stripe.service.ts `
```
export class StripeService {
  //...
  private paymentElement?: StripePaymentElement;

  //... async initializeElements() {//...}
  
  async createPaymentElement(){
    if(!this.paymentElement){
      const elements = await this.initializeElements();
      if(elements){
        this.paymentElement = elements.create('payment');
      } else {
        throw new Error("Elements instance has not been initialized");
      }
    }
    return this.paymentElement;
  }
  //...async createAddressElement() {//...}

  //...

  disposeElements(){
    //...this.addressElement = undefined;
    this.paymentElement = undefined;
  }
}
```

- ` 172-update checkout.component.html`
```
      //...
      <mat-step label="Payment">
        <!-- Payment form -->
          <div id="payment-element"></div>
            <div class="flex justify-between mt-6">
            <button matStepperPrevious mat-stroked-button>Back</button>
            <button matStepperNext mat-flat-button>Next</button>
          </div>
      </mat-step>
    //....
```
- ` 172-update: checkout.component.ts `
```

export class CheckoutComponent implements OnInit, OnDestroy { 
  //...
  paymentElement?: StripePaymentElement;
}

async ngOnInit(){
  try{
    //...this.addressElement.mount('#address-element');
    
    this.paymentElement = await this.stripeService.createPaymentElement();
    this.paymentElement.mount('#payment-element');
  } catch {
    //...
  }
}
```
- 172-Testing UI
```
- https://localhost:4200/checkout
  - Payment
  - downside of using stripe api on console
    - there alot poping console issues
  - stripe -> settings(cog icon)
  - Search Product settings category
    -> Payments
    -> Payment methods tab
    -> Map suggestion in console will auto suggest for map location address automatically by 
        hooking up stripe payment
  
  - note for development purposes: in chrome console in filter icon type - minus e.g '-utf8 -apple -google -thrid-party' not an ideal solution but good thing to know

  - search about "stripe third part cookies"
```

###### 173. Creating the review component

- ` cd client ' ng g c features/checkout/checkout-review --skip-tests ' `
- ` Update-173: checkout.component.html `
```
    //...

      <mat-step label="Confirmation">
      <!-- Review form -->
        // update code start
        <app-checkout-review></app-checkout-review> // update code
        <div class="flex justify-between mt-6">
          <button matStepperPrevious mat-stroked-button>Back</button>
          <button mat-flat-button>Pay {{ cartService.totals()?.total | currency }}</button>
        </div>
        // update code end
      </mat-step>

      //...
```

- ` Update-173: checkout.component.ts `
```
import { CartService } from '../../core/services/cart.service';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-checkout',
  imports: [
    //...CheckoutDeliveryComponent,
    CheckoutReviewComponent, // update 
    CurrencyPipe // update 
],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})

export class CheckoutComponent implements OnInit, OnDestroy {
  //...private accountService = inject(AccountService);
  cartService = inject(CartService); // update
  //...addressElement?: StripeAddressElement;
}
```

- ` update-173: checkout-review.component.ts `
```
import { Component, inject } from '@angular/core';
import { CartService } from '../../../core/services/cart.service';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-checkout-review',
  imports: [
    CurrencyPipe
  ],
  templateUrl: './checkout-review.component.html',
  styleUrl: './checkout-review.component.scss'
})
export class CheckoutReviewComponent {
  cartService = inject(CartService);
}

```
- ` update-173: checkout-review.component.html `
```
<div class="mt-4 w-full">
  <h4 class="text-lg font-semibold">Billing and delivery information</h4>
  <dl>
    <dt class="font-medium">Shipping address</dt>
    <dd class="mt-1 text-gray-500">Shipping address goes here</dd>
    <dt class="font-medium">Payment details</dt>
    <dd class="mt-1 text-gray-500">Payment details goes here</dd>
  </dl>
</div>

<div class="mt-6 mx-auto">
  <div class="border-b border-gray-200">
    <table class="w-full text-center">
      <tbody class="divide-y divide-gray-200">
        @for (item of cartService.cart()?.items; track item.productId) {
          <tr>
            <td class="py-4">
              <div class="flex items-center gap-4">
                <img src="{{item.pictureUrl}}" alt="product image" class="w-10 h-10"/>
                <span>{{item.productName}}</span>
              </div>
            </td>
            <td class="p-4">x{{ item.quantity }}</td>
            <td class="p-4 text-right">{{ item.price | currency }}</td>
          </tr>
        }
      </tbody>
    </table>
  </div>
</div>

```
- ` testing go to localhost:4200/checkout => stepper => confirmation `

- Take aways for 173: 
``` 
- take aways at the moment: is that when we are able to laid the C# api properly
- calling or styling the UI makes it easy.
- 
```

###### 174. Stripe address auto complete functionality

- ` if I don't want some stripe payment additional functionality `
- ` just go to https://dashboard.stripe.com/test/settings/payment_methods/pmc_1RaRSMQ4ykDn46yOpZsYmoHv and turn it off `
- ` payment option the link will be turn off`
- ` Address section address line 1 suggestion: user that doesn't have an address `
- ` iframe issue on the address auto suggestion when creating a new user like sam@test.com - difficult to style`

- ` update-174: checkout.component.html`
```
//  adding z-0 is sovling issus about auto suggest address odd layout ouput on UI 
<button class="z-0" routerLink="/shop" mat-stroked-button>Continue shopping</button>
<button class="z-0" matStepperNext mat-flat-button>Next</button>
```
- ` next will be disabling the stepper to not go or jump around to other tabs like: Shipping  | Payment | Confirmation` 

###### 175. Validating step completion part 1
- ` update-175: checkout.component.ts `
```
//...
import { //..., StripeAddressElementChangeEvent, //... } from '@stripe/stripe-js';
import { //...CurrencyPipe, JsonPipe } from '@angular/common';

@Component({
  selector: 'app-checkout',
  imports: [
    //...CurrencyPipe,
    JsonPipe
],
  //...templateUrl: './checkout.component.html',
  //...styleUrl: './checkout.component.scss'
})

export class CheckoutComponent implements OnInit, OnDestroy {

  //...saveAddress = false;

  completionStatus = signal<{address: boolean, card: boolean, delivery:boolean}>(
    {address: false, card: false, delivery: false}
  )

  /*constructor(){
    this.handleAddressChange = this.handleAddressChange.bind(this)
  }*/

  async ngOnInit() {
    try {
      //...this.addressElement = await this.stripeService.createAddressElement();
      //...this.addressElement.mount('#address-element');
      // update code below
      this.addressElement.on('change', this.handleAddressChange);

      //...this.paymentElement = await this.stripeService.createPaymentElement();
      //...this.paymentElement.mount('#payment-element');
    } //...catch (error: any) {
      //...this.snackbar.error(error.message);
    }
  }

  // same effect as constructor () {...}

  handleAddressChange = (event: StripeAddressElementChangeEvent) => {
    this.completionStatus.update(state => {
      state.address = event.complete;
      return state;
    })
  }

  /* old code
  handleAddressChange(event: StripeAddressElementChangeEvent){
    this.completionStatus.update(state => {
      state.address = event.complete;
      return state;
    })
  }
  */
}
```

- ` update-175: checkout.component.html `
```
    //...
     </mat-stepper>
      // update code below
     <pre>{{completionStatus() | json}}</pre>
     //...
```
- ` testing address the address status will reflect and it will reflect to true in the UI `

- ` next-step: update-175: checkout.component.ts `
```
import { //..., StripePaymentElementChangeEvent } from '@stripe/stripe-js';

export class CheckoutComponent implements OnInit, OnDestroy {
//...
  async ngOnInit() {
    try {
      //...

      //...this.paymentElement = await this.stripeService.createPaymentElement();
      //..this.paymentElement.on('change', this.handlePaymentChange);

      this.paymentElement.mount('#payment-element'); // update 
      
    } catch (error: any) {
      //...this.snackbar.error(error.message);
    }
  }
  //   handleAddressChange  =() =>{...}
  
  /* updated code below */
  handlePaymentChange = (event: StripePaymentElementChangeEvent) => {
    this.completionStatus.update(state => {
      state.card = event.complete;
      return state;
    })
  }
}
```

- `delivery: false update to true when the value is true`
- `next-step-> update-175: checkout-delivery.component.ts`
```
import { //...OnInit, output } from '@angular/core';

export class CheckoutDeliveryComponent implements OnInit {

  deliveryComplete = output<boolean>(); // update

  ngOnInit(): void {
    this.checkoutService.getDeliveryMethods().subscribe({
      next: methods => {
        if (this.cartService.cart()?.deliveryMethodId) {
          const method = methods.find(x => x.id === this.cartService.cart()?.deliveryMethodId);
          if(method){
            this.cartService.seletedDelivery.set(method);
            this.deliveryComplete.emit(true) // update
          }
        }
      }
    });
  }

  updateDeliveryMethod(method: DeliveryMethod){
    // update our signal
    this.cartService.seletedDelivery.set(method);
    const cart = this.cartService.cart();
    if(cart) {
      cart.deliveryMethodId = method.id;
      this.cartService.setCart(cart);
      this.deliveryComplete.emit(true);  // update
    }
  }

}
```

- `next-step: update:175 checkout-component.ts `
```

//...handlePaymentChange = (event: StripePaymentElementChangeEvent) => {...}
  
  /* update code */
  handleDeliveryChange(event: boolean){
    this.completionStatus.update(state =>{
      state.delivery = event;
      return state;
    })
  }

  //... async onStepChange(event:StepperSelectionEvent){...}

```

- ` update-175: template checkout.component.html `
```
      <mat-step label="Shipping">
        <!-- Delivery form -->
                      /* update (deliveryComplete="handleDeliveryChange($event)" 
                        - checking is if cart has item it will have "delivery":true
                      */
         <app-checkout-delivery (deliveryComplete)="handleDeliveryChange($event)"></app-checkout-delivery>
          <div class="flex justify-between mt-6">
            <button matStepperPrevious mat-stroked-button>Back</button>
            <button matStepperNext mat-flat-button>Next</button>
          </div>
      </mat-step>
```
- Check and testing
```
- UI -> localhost:4200/checkout
- Address - it will set to 'true' if it has an input in it
- card - it will set to 'true' if it has an input in it
- delivery - it will become 'true' if the cart is not empty

{
  "address": true,
  "card": true,
  "delivery": true
}

```

###### 176. Validating step completion part 2

- ` implement prevent a user from moving forward in stepper if anything is not meet accordingly `
- ` but we will allow a user to move backward step`

- `update-176 step-1: checkout.component.html ` 
```
<div class="flex mt-32 gap-6">
  <div class="w-3/4">
    <!-- Checkout Stepper -->
     <mat-stepper
      (selectionChange)="onStepChange($event)"

      [linear]="true" /* update: what this does it will prevent the stepper to move forward */
      
      //...
      //class="bg-white border border-gray-200 shadow-sm">

        /* updated code below: */
        <mat-step label="Address" [completed]="completionStatus().address">
                      //...
                      <button
              [disabled]="!completionStatus().address"
              class="z-0" matStepperNext mat-flat-button>Next</button>
        </mat-step>
          
        <mat-step label="Address" [completed]="completionStatus().delivery">
          //...
              <button
              [disabled]="!completionStatus().delivery"
              class="z-0" matStepperNext mat-flat-button>Next
              </button>
        </mat-step>

        <mat-step label="Address" [completed]="completionStatus().card>
              <button
              [disabled]="!completionStatus().card"
              class="z-0" matStepperNext mat-flat-button>Next
              </button>
        </mat-step>
         /* updated code above: */

    </mat-stepper>
  </div>
</div>
```

###### 177. Creating a Stripe confirmation token

- ` update-177: stripe.service.ts `
```
//...async createAddressElement(){...}

  async createConfimationToken(){
    const stripe = await this.getStripeInstance();
    const elements = await this.initializeElements();
    const result = await elements.submit();
    if (result.error) throw Error(result.error.message);
    if(stripe){
      return await stripe.createConfirmationToken({elements});
    } else {
      throw new Error('Stripe not available');
    }
  }

//...createOrUpdatePaymentIntent(){...}
```

- `update-177: checkout.component.ts `
```
import { ConfirmationToken, ..., ... } from '@stripe/stripe-js';

export class CheckoutComponent implements OnInit, OnDestroy {

  //... completionStatus = signal<{address: boolean, card: boolean, delivery:boolean}>(...)

  confirmationToken?: ConfirmationToken;

  //...async ngOnInit() {...}

    async getConfirmationToken(){
    try {
      if(Object.values(this.completionStatus()).every(status => status === true)){
        const result = await this.stripeService.createConfimationToken();
        if(result.error) throw new Error(result.error.message);
        this.confirmationToken = result.confirmationToken;
        console.log(this.confirmationToken);
      }
    } catch (error:any) {
      this.snackbar.error(error.message);
    }
  }

  async onStepChange(event:StepperSelectionEvent){
    if(event.selectedIndex === 2){
      //...
    }
    if(event.selectedIndex === 3) {
      await this.getConfirmationToken();
    }
  }

} 
```
- ` Test-177: `
```
- go to checkout payment
- open console in browser
- you will se Object with token
- navigate to paymnent_method_preview
  - card
  - shipping
```

###### 178. Updating the review component with the token information

- ` update-178: checkout-review-component.ts `
```
//...
import { ConfirmationToken } from '@stripe/stripe-js';

export class CheckoutReviewComponent {
  //...cartService = inject(CartService);
  @Input() confirmationToken?: ConfirmationToken;
}
```

- ` update-178: checkout.component.html`
```
    //...
      <mat-step label="Confirmation">
      <!-- Review form -->
        // update => [confirmationToken]="confirmationToken"
        <app-checkout-review [confirmationToken]="confirmationToken"></app-checkout-review>
        <div class="flex justify-between mt-6">
          <button matStepperPrevious mat-stroked-button>Back</button>
          <button mat-flat-button>Pay {{ cartService.totals()?.total | currency }}</button>
        </div>
      </mat-step>
    //...
```

- ` update-178: checkout-review.component.html`
```
<div class="mt-4 w-full">
  <h4 class="text-lg font-semibold">Billing and delivery information</h4>
  <dl>
    <dt class="font-medium">Shipping address</dt>
    
    /* pag-gamit sa address.pipe.ts mao ning access point sa gi response gikan sa stripe via DOM/Console kay ang address.pipe.ts diri ang factory mahitabo below ang pag join together:*/
    <dd class="mt-1 text-gray-500">{{confirmationToken?.shipping | address}}</dd> 
    
    //<dd class="mt-1 text-gray-500">{{confirmationToken?.shipping?.address}}</dd> // dili mo gana need ug angular-pipe ani ng approach
    // testing the it via UI ang result kay [object Object]

    <!-- <dd class="mt-1 text-gray-500">Shipping address goes here</dd> -->
    <dt class="font-medium">Payment details</dt>
    <dd class="mt-1 text-gray-500">Payment details goes here</dd>
  </dl>
</div>

```

- `issue: [object Object] -> solution is to look for ng pipe`
```
- cd client => ng g --help
  - ng g p shared/pipes/address --dry-run
  - ng g p shared/pipes/address --skip-tests
```
- ` update-178: adress.pipe.ts`
```
import { Pipe, PipeTransform } from '@angular/core';
import { ConfirmationToken } from '@stripe/stripe-js';

@Pipe({
  name: 'address'
})
export class AddressPipe implements PipeTransform {

  /*
  * diri ma-access ang kadtong gipang response sa Console sa browser nga gikan sa stripe
  * kaning transform(value: ConfirmationToken) ang tawag ani kay angular pipe
  * para magamit ni need ni pang inject next sa checkout-review.component.html
  * 
  * then iimport ni sa sa checkout-review.component.ts:  import { AddressPipe } from "../../../shared/pipes/address.pipe";
  *
  * 
  */
  transform(value?: ConfirmationToken['shipping'], ...args: unknown[]): unknown {
    if(value?.address && value.name){
      const {line1, line2, city, state, country, postal_code} = value.address;
      return `${value.name}, ${line1}${line2 ? ', ' + line2 : ''},
        ${city}, ${state}, ${postal_code}, ${country}`
    } else {
      return 'Unknown address'
    }
  }
}
```

###### 179. Confirming the payment
- ` step-1a-179: create new payment-card.pipe.ts `
```
/*
- cd client => ng g --help
  - ng g p shared/pipes/payment-card --dry-run
*/

import { Pipe, PipeTransform } from '@angular/core';
import { ConfirmationToken } from '@stripe/stripe-js/dist/api/confirmation-tokens';

@Pipe({
  name: 'paymentCard'
})
export class PaymentCardPipe implements PipeTransform {

  transform(value?: ConfirmationToken['payment_method_preview'], ...args: unknown[]): unknown {
    if(value?.card){
      const {brand, last4, exp_month, exp_year} = value.card;
      return `${brand.toUpperCase()} **** **** **** ${last4}, Exp: ${exp_month}/${exp_year}`;
    } else {
      return 'Unknown payment method'
    }
  }
}
```

- ` step-1b-179: update checkout-review.component.html `
```
<div class="mt-4 w-full">
  <h4 class="text-lg font-semibold">Billing and delivery information</h4>
  <dl>
    //...<dt class="font-medium">Shipping address</dt>
    //...<dd class="mt-1 text-gray-500">{{confirmationToken?.shipping | address}}</dd>

    /* update code below: */
    <dt class="font-medium">Payment details</dt>
    <dd class="mt-1 text-gray-500">Payment details goes here</dd> // update here
    </dl>
</div>
```

- ` step-1c-179: update checkout-review.component.ts `
```
import { PaymentCardPipe } from "../../../shared/pipes/payment-card.pipe"; // update import here

@Component({
  selector: 'app-checkout-review',
  imports: [
    //...AddressPipe,
    PaymentCardPipe // update import here
],
  //...templateUrl: './checkout-review.component.html',
  //...styleUrl: './checkout-review.component.scss'
})
```

- `step-2-update-179: stripe.service.ts `
```
  //...async createConfimationToken(){...}

  async confirmPayment(confirmationToken: ConfirmationToken){
    const stripe = await this.getStripeInstance();
    const elements = await this.initializeElements();
    const result = await elements.submit();
    if (result.error) throw Error(result.error.message);

    const clientSecret = this.cartService.cart()?.clientSecret;

    if(stripe && clientSecret) {
      return await stripe.confirmPayment({
        clientSecret: clientSecret,
        confirmParams: {
          confirmation_token: confirmationToken.id
        },
        redirect: 'if_required'
      })
    } else {
      throw new Error('Unable to load stripe');
    }
  }

  //... createOrUpdatePaymentIntent(){...}
```

- `step-3-update-179: checkout.component.ts`
```
import { MatStepper, ... } from '@angular/material/stepper';
import { Router, ...RouterLink } from '@angular/router';

export class CheckoutComponent implements OnInit, OnDestroy{
  //...private snakbar 
  private router = inject(Router);
  //...private accountService

  //...   async onStepChange(event:StepperSelectionEvent){...}

  async confirmPayment(stepper: MatStepper){
    try {
      if(this.confirmationToken){
        const result = await this.stripeService.confirmPayment(this.confirmationToken);
        if(result.error){
          throw new Error(result.error.message);
        } else {
          this.cartService.deleteCart();
          this.cartService.seletedDelivery.set(null);
          this.router.navigateByUrl('/checkout/success');
        }
      }
    } catch (error: any) {
      this.snackbar.error(error.message || 'Something went wrong');
      stepper.previous();
    }
  }

  private async getAddressFromStripeAddress(): Promise<Address | null> {...}
}
```

- `step-4-179: update checkout.component.html `
```
//...

      <mat-step label="Confirmation">
      <!-- Review form -->
        <app-checkout-review [confirmationToken]="confirmationToken"></app-checkout-review>
        <div class="flex justify-between mt-6">
          <button matStepperPrevious mat-stroked-button>Back</button>
          <button
              (click)="confirmPayment(stepper)" // updated code this line only works because of the #stepper assign on  <mat-stepper #stepper ></mat-stepper>
              mat-flat-button>Pay {{ cartService.totals()?.total | currency }}</button>
        </div>
      </mat-step>

      //...
```
- ` step-5-179: at client => create checkout-success `
```
- cd client => ' ng g c features/checkout/checkout-success --skip-tests '
```

- ` step-6-179: app.routes.ts `
```
import { CheckoutSuccessComponent } from './features/checkout/checkout-success/checkout-success.component';

export const routes: Routes = [ 
  //...{ path: 'checkout', component: CheckoutComponent, canActivate: [authGuard, emptyCartGuard] },

  { path: 'checkout/success', component: CheckoutSuccessComponent, canActivate: [authGuard] }, // update code
  
  //...{ path: 'account/login', component: LoginComponent },
]
```

- ` step-7-179: Test the UI `
```
- localhost:4200/checkout/
  - address
  - shipping
  - payment
    - card
  - Confirmation stepper
    - Payment details
      -  hit 'Pay' button

- if it is successful
  - in dashboard.stripe.com/test/payments
    - it will show : Succeeded
```

###### 180. Loading and error notifications

- [Stripe Docs - Cards by brand](https://docs.stripe.com/testing)

- `/* mag add ug loading sa pay button section*/`

- ` step-1-180: update => checkout.component.ts `
```
//...
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner'; // update code

@Component({
  selector: 'app-checkout',
  imports: [
  //...,
    MatProgressSpinnerModule // update code
],
  //templateUrl: './checkout.component.html',
  //styleUrl: './checkout.component.scss'
})

export class CheckoutComponent implements OnInit, OnDestroy {
  //... confirmationToken?: ConfirmationToken;

  loading = false; // update code

  async confirmPayment(stepper: MatStepper){
    this.loading = true;
    try {
      if(this.confirmationToken){
        const result = await this.stripeService.confirmPayment(this.confirmationToken);
        if(result.error){
          throw new Error(result.error.message);
        } else {
          this.cartService.deleteCart();
          this.cartService.seletedDelivery.set(null);
          this.router.navigateByUrl('/checkout/success');
        }
      }
    } catch (error: any) {
      this.snackbar.error(error.message || 'Something went wrong');
      stepper.previous();
    } finally { // update code
      this.loading = false; // update code
    } // update code
  }
}
```
- [Material-NG: Progress Spinner](https://material.angular.dev/components/progress-spinner/overview)

- `step-2-180: update => checkout-component.html `
```
    //...
    <mat-stepper>
      //...

      <mat-step label="Confirmation">
      <!-- Review form -->
        <app-checkout-review [confirmationToken]="confirmationToken"></app-checkout-review>
        <div class="flex justify-between mt-6">
          <button matStepperPrevious mat-stroked-button>Back</button>
          <button
              [disabled]="!confirmationToken || loading" // update
              (click)="confirmPayment(stepper)" mat-flat-button
            >
              /* update code start here*/
              @if (loading) { 
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                <span>
                  Pay {{ cartService.totals()?.total | currency }}
                </span>
              }
              /* update code end here*/
            </button>
        </div>
      </mat-step>
     </mat-stepper>
    //...
```
- `step-3-180: testing in UI https://localhost:4200/checkout`
```
- for successful payment:
  - card : 5555 5555 5555 4444
  - https://dashboard.stripe.com/test/payments
  - it should reflect in stripe as success

- for unsuccessful payment:
  - https://docs.stripe.com/testing#declined-payments
  - Insufficient funds decline	4000000000009995	card_declined	insufficient_funds
    - you will see a snakbar of 'insufficient funds' red prompting !!
      - Generic decline	4000000000000002	card_declined	generic_decline
      - Stolen card decline	4000000000009979	card_declined	stolen_card

  -https://docs.stripe.com/testing#regulatory-cards
    - Always authenticate	4000002760003184	This card requires authentication on all transactions, regardless of how the card is set up. 
    - pop-up -3d secure 2 Test page' - test fail or complete button
      - if fail = it will not proceed and you will re-redirected to payment/card
    - else if it is successful - it will procced to 'checkout-success page'

  - then check in stripe for verification of payments status: https://dashboard.stripe.com/test/payments
```

###### 181. Checkout success page

- `step-1a-181: update => checkout-success.component.ts`
```
import { Component } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-checkout-success',
  imports: [
    MatButton,
    RouterLink
  ],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent {

}
```

- `step-1b-181: update => checkout-success.component.html`
```
<section class="bg-white py-16">
  <div class="mx-auto max-w-2xl px-4">
    <h2 class="font-semibold text-2xl mb-2">
      Thanks for your order!
    </h2>
    <p class="text-gray-500 mb-8">Your order <span class="font-medium">#42</span>
      will never be processed as this is a fake shop. we will not notify you once your order has not shipped.
    </p>
    <div class="space-y-2 rounded-lg border border-gray-100 bg-gray-50 p-6 mb-8">
      <dl class="flex items-center justify-between gap-4">
        <dt class="font-normal text-gray-500">Date</dt>
        <dd class="font-medium text-gray-900 text-end">20 Sep 2025</dd>
      </dl>
      <dl class="flex items-center justify-between gap-4">
        <dt class="font-normal text-gray-500">Payment method</dt>
        <dd class="font-medium text-gray-900 text-end">Visa</dd>
      </dl>
      <dl class="flex items-center justify-between gap-4">
        <dt class="font-normal text-gray-500">Address</dt>
        <dd class="font-medium text-gray-900 text-end">Address goes here</dd>
      </dl>
      <dl class="flex items-center justify-between gap-4">
        <dt class="font-normal text-gray-500">Amount</dt>
        <dd class="font-medium text-gray-900 text-end">$450</dd>
      </dl>
    </div>
    <div class="flex items-center space-x-4">
      <button routerLink="/orders/42" mat-flat-button>View your order</button>
      <button routerLink="/shop" mat-stroked-button>Continue shopping</button>
    </div>
  </div>
</section>
```
- `step-2-181: Check and Testing`
```
- https://localhost:4200/checkout/success
```

###### 182. Summary
- goal
```
- To be able to accept payments
securely globally that complies
with EU regulations and PCI DSS
regulations.
```
- FAQs
```
Q:  Why didn't you include <PaymentProcessor>?

A:  Stripe covers the majority of
    card payments globally and is
    priced similarly to others. You
    could add extra if you wish

Q2: How can I become PCI 
    complaint so that I can avoid
    payment processor costs.

A2: if your is turning over 
    lots of money then this is 
    something to think about.
    Outside the scope of this course.     
```
<hr>

### Section 17: API - Orders
<hr>

###### 183. Introduction
- In this module
```
- Adding the Order Entity
- Aggregate Entities
- ' Owned Entities ' - we don't need a multiple/separte table for a shipping address nor do we need separate for shipping address if we use Own Entities.
- Unit of Work pattern - refactor our code, streamline the process.
```

###### 184. Creating the order aggregate part 1

- `step-1a-184: go to Solution explorer `

- `step-2a-184: create Core/Entities/OrderAggregate/ShippingAddress.cs 1st part`
```
namespace Core.Entities.OrderAggregate;

public class ShippingAddress
{
    public required string Name { get; set; }
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
}
``` 
- `step-2b-184: create | Class | Core/Entities/OrderAggregate/ProductItemOrdered.cs 2nd part`
```
namespace Core.Entities.OrderAggregate;

public class ProductItemOrdered
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string PictureUrl { get; set; }
}
```

- `step-2c-184: create | Enum | Core/Entities/OrderAggregate/OrderStatus.cs  `
```
namespace Core.Entities.OrderAggregate;

public enum OrderStatus
{
    Pending,
    PaymentReceived,
    PaymentFailed
}
```

- `step-2d-184: create | class | Core/Entities/OrderAggregate/OrderItem.cs  `
```
namespace Core.Entities.OrderAggregate;

public class OrderItem : BaseEntity
{
    public ProductItemOrdered ItemOrdered { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
}
```

- `step-2d-184: create | class | Core/Entities/OrderAggregate/PaymentSummary.cs  `
```
namespace Core.Entities.OrderAggregate;

public class PaymentSummary
{
    public int Last4 { get; set; }
    public required string Brand { get; set; }
    public int ExpMonth { get; set; }
    public int Year { get; set; }
}
```

###### 185. Creating the order aggregate part 2

- `step-1-185: create | class | Core/Entities/OrderAggregate/Order.cs  `
```
namespace Core.Entities.OrderAggregate;

public class Order : BaseEntity
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required string BuyerEmail { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public DeliveryMethod DeliveryMethod { get; set; } = null!;
    public PaymentSummary PaymentSummary { get; set; } = null!;
    public IReadOnlyList<OrderItem> OrderItems { get; set; } = [];
    public decimal SubTotal { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required string PaymentIntentId { get; set; }
}
```

###### 186. Configuring the order entities

- ` step-1-186: create | Infrastructure/Config/OrderConfiguration.cs `
```
using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsOne(x => x.ShippingAddress, o => o.WithOwner());
        builder.OwnsOne(x => x.PaymentSummary, o => o.WithOwner());
        builder.Property(x => x.Status).HasConversion(
            o => o.ToString(),
            o => (OrderStatus)Enum.Parse(typeof(OrderStatus), o)
        );
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18, 2)");
        builder.HasMany(x => x.OrderItems).WithOne().OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.OrderDate).HasConversion(
            d => d.ToUniversalTime(),
            d => DateTime.SpecifyKind(d, DateTimeKind.Utc)
        );
    }
}
```

- ` step-2-186: create | Infrastructure/Config/OrderItemConfiguration.cs `
```
using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.OwnsOne(x => x.ItemOrdered, o => o.WithOwner());
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
    }
}
```

- ` step-3-186: Update StoreContext.cs => Infrastructure/Data/StoreContext.cs `
```
//...using Core.Entities;
using Core.Entities.OrderAggregate; // update
//...using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    //... public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
    
    public DbSet<Order> Orders { get; set; } // update 
    public DbSet<OrderItem> OrderItems { get; set; } // update

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      //....
    }
}
```

- ` step-4-186: Update Migration `
```
- cd => root folder => " dotnet ef migrations add OrderAggregateAdded -p Infrastructure -s API "
- check Infrastructure/Migration/20250707124655_OrderAggregateAdded.cs
```

###### 187. Introducing the Unit of work

```
Pros - Current Usage of Repo
  - Generic Repository
  - Specification pattern

Cons 
  - Could end up with partial updates
  - 
  
```
- `Contoller`
```
  Controller
[Unit of Work] ====>   Database []
    |
    | =======> Repository<Order>
    |
    | =======> Repository<Product>
    |
    | =======> Repository<Delivery>

1. Unit of Work (UoW) instantiates DbContext instance
2. UoW Creates Repositories as needed
```

- `Unit of Work Pattern`
```
- UoW creates repository instances as needed.
- EF Tracks the entities state (add, update, remove).
- At the end of the transaction UoW.Complete()
- Dispose the DBContext
- Uses same lifetime as repository (scoped)
```

###### 188. Implementing the unit of work

- ` step-1a-188: solution-explorer Create | type - Interface | Interface Core/Interfaces/IUnitOfWork.cs`
- ` step-1b-188: IUnitOfWork.cs the implement IDisposable `
```
using Core.Entities;

namespace Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<bool> Complete();
}
```

- ` step-2a-188: solution-explorer Create | Class | Infrastructure/Data/IUnitOfWork.cs`
```
using System.Collections.Concurrent;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class UnitOfWork(StoreContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        context.Dispose();
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;

        return (IGenericRepository<TEntity>)_repositories.GetOrAdd(type, t =>
        {
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(TEntity));
            return Activator.CreateInstance(repositoryType, context)
            ?? throw new InvalidOperationException($"Could not create repository for {t}");
        });
    }
}
```

- ` step-3-188: add this as a service update 'API/Program.cs' `
```
/*
    encounter some issues in the 189: GenericRepository
    GenericRepository => added using infrastructure;
    typeof(GenericRepository<>))

*/
using Infrastructure;

//...builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); //update
//...builder.Services.AddCors();
```

###### 189. Using the unit of work

/* https://www.udemy.com/course/learn-to-build-an-e-commerce-app-with-net-core-and-angular/learn/lecture/45151681?start=0#overview 
*/
- ` step-1-189: update IGenericRepository.cs `
```
/* by removing Task<bool> SaveChangesAsync(); */

public interface IGenericRepository<T> where T : BaseEntity
{
    void Remove(T entity);
    // Task<bool> SaveChangesAsync(); // gi remove ni kay ibalhin sa UnitOfWork ang controll
    bool Exists(int id);
}
```

- ` step-2-189: update GenericRepository.cs `
/* by removing the SaveAllAsync*/
```
public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : BaseEntity
{
    /*remove below */
    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
    
}
```
- ` step-3-189: update ProductsController.cs `
```
/*        replace: IGenericRepository<Product> to (IUnitOfWork unit)  
  update  ProductsController(IGenericRepository<Product> repo)
  
  then update all the error in 'repo' = unit.Repository<Product>()
  
  update repo.SaveAllAsync() to unit.Complete()

*/

using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using API.RequestHelpers;

namespace API.Controllers;

public class ProductsController(IUnitOfWork unit) : BaseApiController
{
    [HttpGet]

    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams specParams)
    {

        var spec = new ProductSpecification(specParams);

        return await CreatePageResult(unit.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);

    }

    [HttpGet("{id:int}")] // api/products/2
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    // create a new product
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        unit.Repository<Product>().Add(product);
        
        if(await unit.Complete())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        return BadRequest("Problem creating product");

    }

    // update product
    // PUT api/products/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if(product.Id != id) return BadRequest("Cannot update this product");

        unit.Repository<Product>().Update(product);

        if(await unit.Complete())
        {
            return NoContent();
        }
        return BadRequest("Problem updating the product");
    }

    // delete product
    // DELETE api/products/1
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product == null) return NotFound();

        unit.Repository<Product>().Remove(product);
        if(await unit.Complete())
        {
            return NoContent();
        }
        return BadRequest("Problem deleting the product");

    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {   
        var spec = new BrandListSpecification();

        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {   
        var spec = new TypeListSpecification();

        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }

    private bool ProductExists(int id)
    {
        return unit.Repository<Product>().Exists(id);
    }
}
```

- ` step-4-189: update PaymentsController.cs `
```
/*
    replace IGenericRepository<DeliveryMethod> dmRepo to  IUnitOfWork unit
    replace dmRepo to unit.Repository<DeliveryMethod>()
*/
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PaymentsController(
        IPaymentService paymentService,
        IUnitOfWork unit
    ) : BaseApiController
{
    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem with your cart");

        return Ok(cart);
    }

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

}
```

- ` step-5-189: update PaymentService.cs `
```
/*
    replace 
    IGenericRepository<Core.Entities.Product> productRepo, 
    IGenericRepository<DeliveryMethod> dmRepo

    to 
    IUnitOfWork unit

    then replace dmRepo to unit.Repository<DeliveryMethod>()

    while 
    on productRepo replace to unit.Repository<Core.Entities.Product>().
*/

public class PaymentService(
    IConfiguration config,
    ICartService cartService,
    IUnitOfWork unit    /* update here */

    // setting Product to not be ambiguous with Core.Entities.Product (below code:)
    //IGenericRepository<Core.Entities.Product> productRepo, 
    //IGenericRepository<DeliveryMethod> dmRepo
    ) 
    : IPaymentService
{

    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null) return null;

        var shippingPrice = 0m;

        if (cart.DeliveryMethodId.HasValue)
        {
                                    /* update here */
            var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync((int)cart.DeliveryMethodId); 

            if (deliveryMethod == null) return null;

            shippingPrice = deliveryMethod.Price;
        }  
    }
}
```

- ` step-6-189: via postman `
```
- section 17 - Orders

- go to section 2 - Add Product
                  {
                      "name": "{{$randomProduct}}",
                      "description": "{{$randomLoremParagraph}}",
                      "price": {{$randomPrice}},
                      "pictureUrl": "{{$randomImageUrl}}",
                      "type": "Some type",
                      "brand": "Some brand",
                      "quantityInStock": {{$randomInt}}
                  }
- go to section 2 - Get Product {{url}}/api/products/2 to {{url}}/api/products/19 in our case its 1002 {{url}}/api/products/1002

- go to section 2 - Get Products -> Get Product {{url}}/api/products' // get all the products

- go to section 2 - Update Product ->  {{url}}/api/products/1002 = 204 'no Content status'
                        {
                            "id": 1002,
                            "name": "{{$randomProduct}} Updated",
                            "description": "{{$randomLoremParagraph}}",
                            "price": {{$randomPrice}},
                            "pictureUrl": "{{$randomImageUrl}}",
                            "type": "Some type",
                            "brand": "Some brand",
                            "quantityInStock": {{$randomInt}}
                        }
- go to section 2 - Get Product -> Get Product {{url}}/api/products/1002'

- go to section 2 - Delete Product -> Get Product {{url}}/api/products/1002'
                  - this will delete the product  '204 No content' should be the status

- go to section 2 - Get Products -> Get Product {{url}}/api/products'

```

###### 190. Creating the order controller

- ` step-1-190: solution-explorer Create | Class | API/Controllers/OrdersController.cs `
```
/*
    - derive from BaseApiController
    - CreateOrder at DTO
    - then continue back to  CreateOrderDto.cs
    - (ICartService cartService, IUnitOfWork unit)

*/
using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class OrdersController(ICartService cartService, IUnitOfWork unit) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto orderDto)
    {
        var email = User.GetEmail();

        var cart = await cartService.GetCartAsync(orderDto.CartId);

        if (cart == null) return BadRequest("Cart not found");

        if (cart.PaymentIntentId == null) return BadRequest("No payment intent for this order");

        var items = new List<OrderItem>();

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);

            if (productItem == null) return BadRequest("Problem with the order");

            var itemOrdered = new ProductItemOrdered
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl
            };

            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity
            };
            items.Add(orderItem);
        }

        var deliveryMethod = await unit.Repository<DeliveryMethod>()
            .GetByIdAsync(orderDto.DeliveryMethodId);

        if (deliveryMethod == null) return BadRequest("No delivery method selected");

        var order = new Order
        {
            OrderItems = items,
            DeliveryMethod = deliveryMethod,
            ShippingAddress = orderDto.ShippingAddress,
            SubTotal = items.Sum(x => x.Price * x.Quantity),
            PaymentSummary = orderDto.PaymentSummary,
            PaymentIntentId = cart.PaymentIntentId,
            BuyerEmail = email
        };

        unit.Repository<Order>().Add(order);

        if (await unit.Complete())
        {
            return order;
        }

        return BadRequest("Problem creating order");
    }
}
```

- ` step-1b-190: solution-explorer Create | Class | API/DTO/CreateOrderDto.cs `
```
using System.ComponentModel.DataAnnotations;
using Core.Entities.OrderAggregate;

namespace API.DTOs;

public class CreateOrderDto
{
    [Required]
    public string CartId { get; set; } = string.Empty;

    [Required]
    public int DeliveryMethodId { get; set; }

    [Required]
    public ShippingAddress ShippingAddress { get; set; } = null!;

    [Required]
    public PaymentSummary PaymentSummary { get; set; } = null!;
}
```

###### 191. Debugging the order creation

- `step-1-191: `  
```
- add a breakpoint to var email = User.GetEmail();
- Debugger -> .NET Core Attach -> search -> API -> AirPlayUIAgent
- the go to postman section 17 Orders
- postman: Update Cart {{ url }}/api/cart
-  postman: Section 14: Login as Tom {{url}}/api/login?useCookies=true
- postman: {{url}}/api/cart
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 1,
            "quantity": 3,
            "pictureUrl": "/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    "deliveryMethodId": null,
    "clientSecret": null,
    "paymentIntentId": null // need this payment intent for th cart
}

- Create payment intent: {{ url }}/api/payments/cart1
{
    "id": "cart1",
    "items": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "price": 150.00,
            "quantity": 3,
            "pictureUrl": "/images/products/boot-ang2.png",
            "brand": "Angular",
            "type": "Boots"
        }
    ],
    "deliveryMethodId": null,
    "clientSecret": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6_secret_LAzqmVeFHX8qxLzhIDlvjUUjh",
    "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6" // id is here
}

- create order: {{ url }}/api/orders
{
    "orderDate": "2025-07-17T18:58:12.7216706Z",
    "buyerEmail": "tom@test.com",
    "shippingAddress": {
        "name": "Tom Smith",
        "line1": "100 Centre Street",
        "line2": null,
        "city": "New York",
        "state": "NY",
        "postalCode": "10013",
        "country": "US"
    },
    "deliveryMethod": {
        "shortName": "UPS1",
        "deliveryTime": "1-2 Days",
        "description": "Fastest delivery time",
        "price": 10.00,
        "id": 1
    },
    "paymentSummary": { // kani ang importante para sa kani na process
        "last4": 4444,
        "brand": "Mastercard",
        "expMonth": 12,
        "year": 0
    },
    "orderItems": [
        {
            "itemOrdered": {
                "productId": 17,
                "productName": "Angular Purple Boots",
                "pictureUrl": "/images/products/boot-ang2.png"
            },
            "price": 150.00,
            "quantity": 3,
            "id": 1
        }
    ],
    "subTotal": 450.00,
    "status": 0,
    "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
    "id": 1
}

- then go to CreateOrderDto.cs

- after seding the send order in postman: Create Order: {{url}}/api/orders

- .net debugger local dapat naa ni sulod kay mo dagan mn diri
- which is why ang kaning akoang machine OS kay windows man 
- unsay equivalent sa AirplayUIAgent (appleMac ang example) how about sa windows?
-
```

- ` step-2-191: Core/Entities/OrderAggregate/PaymentSumamry.cs`
```
namespace Core.Entities.OrderAggregate;

public class PaymentSummary
{
    public int Year { get; set; } // wrong
    public int ExpYear { get; set; } // corrected
} 
// kay sa output sa orders sa Postman 0 ang year
// then after ma correct usabon ang database migration
// cd solotion folder 'dotnet ef migrations add PaymentSummaryCorrection -p Infrastructure -s API'
 
//json file
{
    "orderDate": "2025-07-17T23:11:58.9369522Z",
    "buyerEmail": "tom@test.com",
    "shippingAddress": {
        "name": "Tom Smith",
        "line1": "100 Centre Street",
        "line2": null,
        "city": "New York",
        "state": "NY",
        "postalCode": "10013",
        "country": "US"
    },
    "deliveryMethod": {
        "shortName": "UPS1",
        "deliveryTime": "1-2 Days",
        "description": "Fastest delivery time",
        "price": 10.00,
        "id": 1
    },
    "paymentSummary": {
        "last4": 4444,
        "brand": "Mastercard",
        "expMonth": 12,
        "year": 0 // 0 ang year which is need to correct in the PaymentSummary.cs
    },
    "orderItems": [
        {
            "itemOrdered": {
                "productId": 17,
                "productName": "Angular Purple Boots",
                "pictureUrl": "/images/products/boot-ang2.png"
            },
            "price": 150.00,
            "quantity": 3,
            "id": 1002
        }
    ],
    "subTotal": 450.00,
    "status": 0,
    "paymentIntentId": "pi_3Rm0y0Q4ykDn46yO2tjDASJs",
    "id": 1002
}
```

###### 192. Adding the get order methods

- ` step-1b-192: update OrdersController.cs `
```
[Authorize]
public class OrdersController(ICartService cartService, IUnitOfWork unit) : BaseApiController
{
    //.... more on top

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrdersForUser()
    {
        var spec = new OrderSpecification(User.GetEmail());

        var orders = await unit.Repository<Order>().ListAsync(spec);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);

        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        return Ok(order);
    }
}
```

- ` step-1a-192: Create | class | Core/Specifications/OrderSepcifications.cs`
```
using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderSpecification : BaseSpecifications<Order>
{
    public OrderSpecification(string email) : base(x => x.BuyerEmail == email)
    {

    }

    public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
    {

    }
}
```
- ` step-2-192: postman cheking `
```
- login as a tom
- Get Orders For User - {{url}}/api/orders
[
    {
        "orderDate": "2025-07-17T18:58:12.7216706Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": null,
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [],
        "subTotal": 450.00,
        "status": 0,
        "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
        "id": 1
    },
    {
        "orderDate": "2025-07-17T19:05:02.9614261Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": null, // issue here related property projection or alternative: eager loading and includes
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0 // will be corrected in the coming development
        },
        "orderItems": [], // shouldn't be empty projection or eager loading
        "subTotal": 450.00,
        "status": 0, 
        "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
        "id": 2
    },
    {
        "orderDate": "2025-07-17T23:11:58.9369522Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": null,
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [],
        "subTotal": 450.00,
        "status": 0,
        "paymentIntentId": "pi_3Rm0y0Q4ykDn46yO2tjDASJs",
        "id": 1002
    }
]

```

###### 193. Updating the spec for eager loading
- ` step-1-193: testing postman: Get Order For user = {{url}}/api/orders/1`

- ` step-1b-193: check Core/Interface/ISpecification.cs `
```public interface ISpecification<T>
{
    //Expression<Func<T, bool>>? Criteria{ get; }
    //Expression<Func<T, object>>? OrderBy { get; }
    //Expression<Func<T, object>>? OrderByDescending { get; }
    
    List<Expression<Func<T, object>>> Indcludes { get; }
    List<string> IncludeStrings { get; } // For ThenInclude
}
```

- ` step-1bc-193: for demo purposes 'Infrastructure/Data/ProductRepository.cs ' `
```
   public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort)
    {   
        var query = context.Products.AsQueryable();

        // query.Include(x=> x.Brand).ThenInclude(x=>x.AsQueryable<>) //demo purpose only

        // if (!string.IsNullOrWhiteSpace(brand))
        // query = query.Where(x => x.Brand == brand);
    }
```

- `step-2a-193: Update Core/Specifications/BaseSpecification.cs `
```
// - Implete Interface ISpecification<T>
public class BaseSpecifications<T>(Expression<Func<T, bool>>? criteria) : ISpecification<T>
{
    public List<Expression<Func<T, object>>> Indcludes { get; } = [];
    public List<string> IncludeStrings { get; } = [];

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Indcludes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString); // For ThenInclude
    }
}
```

- ` step-3-193: update Infrastructure/Data/SpecificationEvaluator.cs `
```
public class SpecificationEvaluator<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        //if(spec.IsPagingEnabled)
        //{
        //    query = query.Skip(spec.Skip).Take(spec.Take);
        //}

        /* updated code below */
        query = spec.Indcludes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        //return query;
    }
}
```

###### 194. Updating the controller to eagerly load in the get methods

- `step-1a-194: Update Core/Specifications/OrderSpecification.cs`
```
public class OrderSpecification : BaseSpecifications<Order>
{
    public OrderSpecification(string email) : base(x => x.BuyerEmail == email)
    {
        AddInclude(x => x.OrderItems);
        AddInclude(x => x.DeliveryMethod);
        AddOrderByDescending(x => x.OrderDate);
    }

    public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod"); // downside is no type safety
    }
}
```

- ` step-2a-194: Go to postman: `
```
- test for postman: Get Orders for user: {{url}}/api/orders
"statusCode": 500,
    "message": "Collection was of a fixed size.",
```

- ` step-2b-194: check/inspect Core/Entities/OrderAggregate/Order.cs `
```
public class Order : BaseEntity
{ 
    public List<OrderItem> OrderItems { get; set; } = []; // correct
    //public IReadOnlyList<OrderItem> OrderItems { get; set; } = []; // wrong
}
```

- ` step-2c-194: Go to postman: Get Orders for user: {{url}}/api/orders `
```
- test for postman: Get Orders for user: {{url}}/api/orders
// result on postman below:
[
    {
        "orderDate": "2025-07-17T23:11:58.9369522Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": {
            "shortName": "UPS1",
            "deliveryTime": "1-2 Days",
            "description": "Fastest delivery time",
            "price": 10.00,
            "id": 1
        },
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [ // order items here
            {
                "itemOrdered": {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png"
                },
                "price": 150.00,
                "quantity": 3,
                "id": 1002
            }
        ],
        "subTotal": 450.00,
        "status": 0, // status incorrect
        "paymentIntentId": "pi_3Rm0y0Q4ykDn46yO2tjDASJs",
        "id": 1002
    },
    {
        "orderDate": "2025-07-17T19:05:02.9614261Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": {
            "shortName": "UPS1",
            "deliveryTime": "1-2 Days",
            "description": "Fastest delivery time",
            "price": 10.00,
            "id": 1
        },
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [
            {
                "itemOrdered": {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png"
                },
                "price": 150.00,
                "quantity": 3,
                "id": 2
            }
        ],
        "subTotal": 450.00,
        "status": 0,
        "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
        "id": 2
    },
    {
        "orderDate": "2025-07-17T18:58:12.7216706Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": {
            "shortName": "UPS1",
            "deliveryTime": "1-2 Days",
            "description": "Fastest delivery time",
            "price": 10.00,
            "id": 1
        },
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [
            {
                "itemOrdered": {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png"
                },
                "price": 150.00,
                "quantity": 3,
                "id": 1
            }
        ],
        "subTotal": 450.00,
        "status": 0,
        "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
        "id": 1
    }
]
```

- ` step-2d-194: Go to postman: Get Order for user: {{url}}/api/orders/1 `
```
Postman result below:
{
    "orderDate": "2025-07-17T18:58:12.7216706Z",
    "buyerEmail": "tom@test.com",
    "shippingAddress": {
        "name": "Tom Smith",
        "line1": "100 Centre Street",
        "line2": null,
        "city": "New York",
        "state": "NY",
        "postalCode": "10013",
        "country": "US"
    },
    "deliveryMethod": {
        "shortName": "UPS1",
        "deliveryTime": "1-2 Days",
        "description": "Fastest delivery time",
        "price": 10.00,
        "id": 1
    },
    "paymentSummary": {
        "last4": 4444,
        "brand": "Mastercard",
        "expMonth": 12,
        "expYear": 0
    },
    "orderItems": [
        {
            "itemOrdered": {
                "productId": 17,
                "productName": "Angular Purple Boots",
                "pictureUrl": "/images/products/boot-ang2.png"
            },
            "price": 150.00,
            "quantity": 3,
            "id": 1
        }
    ],
    "subTotal": 450.00,
    "status": 0,
    "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6",
    "id": 1
}
```

###### 195. Shaping the data to return

- `step-1b-195: create DTO | class | 'API/DTOs/OrderDtos.cs' `
```
using Core.Entities.OrderAggregate;

namespace API.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public required string BuyerEmail { get; set; }
    public required ShippingAddress ShippingAddress { get; set; } 
    public required string DeliveryMethod { get; set; }
    public decimal ShippingPrice { get; set; }
    public required PaymentSummary PaymentSummary { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } 
    public decimal SubTotal { get; set; }
    public required string Status { get; set; }
    public required string PaymentIntentId { get; set; }
}

// - Generate class 'OrderItemDto'
// - Move type to OrderItemDto.cs
// - OrderItemDto - go to defination

```

- `step-1a-195: check Core/Entities/OrderAggregate/Order.cs `
```
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required string BuyerEmail { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public DeliveryMethod DeliveryMethod { get; set; } = null!;
    public PaymentSummary PaymentSummary { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = [];
    public decimal SubTotal { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required string PaymentIntentId { get; set; }
```

- ` step-2-195: API/DTOs/OrderItemDto.cs ` 
```
namespace API.DTOs;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string PictureUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

- `step-3-195: |class| Create API/Extensions/OrderMappingExtension.cs`
```
using API.DTOs;
using Core.Entities.OrderAggregate;

namespace API.Extensions;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerEmail = order.BuyerEmail,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            PaymentSummary = order.PaymentSummary,
            DeliveryMethod = order.DeliveryMethod.Description,
            ShippingPrice = order.DeliveryMethod.Price,
            OrderItems = order.OrderItems.Select(x => x.ToDto()).ToList(),
            SubTotal = order.SubTotal,
            Status = order.Status.ToString(),
            PaymentIntentId = order.PaymentIntentId
        };
    }

    public static OrderItemDto ToDto(this OrderItem orderItem)
    {
        return new OrderItemDto
        {
            ProductId = orderItem.ItemOrdered.ProductId,
            ProductName = orderItem.ItemOrdered.ProductName,
            PictureUrl = orderItem.ItemOrdered.PictureUrl,
            Price = orderItem.Price,
            Quantity = orderItem.Quantity,
        };
    }
}
```

- `step-4-195: Update API/Controllers/OrdersController.cs`
```
    /*
      Updated start code below:
    */
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUser()
    {
        var spec = new OrderSpecification(User.GetEmail());

        var orders = await unit.Repository<Order>().ListAsync(spec);

        var ordersToReturn = orders.Select(o => o.ToDto()).ToList();

         return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);

        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        return order.ToDto();
    }

    /* Updated end code:*/

    /*
      Old code below:
    */
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrdersForUser()
    {
        var spec = new OrderSpecification(User.GetEmail());

        var orders = await unit.Repository<Order>().ListAsync(spec);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);

        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        return Ok(order);
    }
```


- `step-test-195: Postman testing `
```
- restart api / dotnet watch
- postman: 
  - Get Orders For User {{url}}/api/orders
      [
        {
            "id": 1002,
            "orderDate": "2025-07-17T23:11:58.9369522Z",
            "buyerEmail": "tom@test.com",
            "shippingAddress": {   // naa nay content nga gi-return question nako assa ni gikan?
                "name": "Tom Smith",
                "line1": "100 Centre Street",
                "line2": null,
                "city": "New York",
                "state": "NY",
                "postalCode": "10013",
                "country": "US"
            },
            "deliveryMethod": "Fastest delivery time", // naa na sulod
            "shippingPrice": 10.00,
            "paymentSummary": {
                "last4": 4444,
                "brand": "Mastercard",
                "expMonth": 12,
                "expYear": 0
            },
            "orderItems": [ // naa na sulod
                {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png",
                    "price": 150.00,
                    "quantity": 3
                }
            ],
            "subTotal": 450.00, // naa na sulod
            "status": "Pending", // naa na sulod
            "paymentIntentId": "pi_3Rm0y0Q4ykDn46yO2tjDASJs" // naa na sulod
        },
        {
            "id": 2,
            "orderDate": "2025-07-17T19:05:02.9614261Z",
            "buyerEmail": "tom@test.com",
            "shippingAddress": {
                "name": "Tom Smith",
                "line1": "100 Centre Street",
                "line2": null,
                "city": "New York",
                "state": "NY",
                "postalCode": "10013",
                "country": "US"
            },
            "deliveryMethod": "Fastest delivery time",
            "shippingPrice": 10.00,
            "paymentSummary": {
                "last4": 4444,
                "brand": "Mastercard",
                "expMonth": 12,
                "expYear": 0
            },
            "orderItems": [
                {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png",
                    "price": 150.00,
                    "quantity": 3
                }
            ],
            "subTotal": 450.00,
            "status": "Pending",
            "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6"
        },
        {
            "id": 1,
            "orderDate": "2025-07-17T18:58:12.7216706Z",
            "buyerEmail": "tom@test.com",
            "shippingAddress": {
                "name": "Tom Smith",
                "line1": "100 Centre Street",
                "line2": null,
                "city": "New York",
                "state": "NY",
                "postalCode": "10013",
                "country": "US"
            },
            "deliveryMethod": "Fastest delivery time",
            "shippingPrice": 10.00,
            "paymentSummary": {
                "last4": 4444,
                "brand": "Mastercard",
                "expMonth": 12,
                "expYear": 0
            },
            "orderItems": [
                {
                    "productId": 17,
                    "productName": "Angular Purple Boots",
                    "pictureUrl": "/images/products/boot-ang2.png",
                    "price": 150.00,
                    "quantity": 3
                }
            ],
            "subTotal": 450.00,
            "status": "Pending",
            "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6"
        }
    ]


- Get Order For User {{url}}/api/orders/1
  - 
    {
        "id": 1,
        "orderDate": "2025-07-17T18:58:12.7216706Z",
        "buyerEmail": "tom@test.com",
        "shippingAddress": {
            "name": "Tom Smith",
            "line1": "100 Centre Street",
            "line2": null,
            "city": "New York",
            "state": "NY",
            "postalCode": "10013",
            "country": "US"
        },
        "deliveryMethod": "Fastest delivery time",
        "shippingPrice": 10.00,
        "paymentSummary": {
            "last4": 4444,
            "brand": "Mastercard",
            "expMonth": 12,
            "expYear": 0
        },
        "orderItems": [
            {
                "productId": 17,
                "productName": "Angular Purple Boots",
                "pictureUrl": "/images/products/boot-ang2.png",
                "price": 150.00,
                "quantity": 3
            }
        ],
        "subTotal": 450.00,
        "status": "Pending",
        "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6"
    }
```

- `step-7-195: send the subtotal update Core/Entities/OrderAggregate/Order.cs`
```
namespace Core.Entities.OrderAggregate;

public class Order : BaseEntity
{
    //... public required string PaymentIntentId { get; set; }

    public decimal GetTotal()
    {
        return SubTotal + DeliveryMethod.Price;
    }
}
```

- `step-8-195: udpate API/DTOs/OrderDto.cs `
```
public class OrderDto
{
  //...  public required string Status { get; set; }

   public decimal Total { get; set; } // update

  //...  public required string PaymentIntentId { get; set; }
}
```

- `step-9-195: udpate API/Extensions/OrderMappingExtensions.cs `
```
public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            //...SubTotal = order.SubTotal,
            Total = order.GetTotal(), // update
            //....Status = order.Status.ToString(),
        };
    }
}
```

- `step-10-195: postman test `
```
/*
    Get Order For User - {{url}}/api/orders/1
*/


{
    "id": 1,
    "orderDate": "2025-07-17T18:58:12.7216706Z",
    "buyerEmail": "tom@test.com",
    "shippingAddress": {
        "name": "Tom Smith",
        "line1": "100 Centre Street",
        "line2": null,
        "city": "New York",
        "state": "NY",
        "postalCode": "10013",
        "country": "US"
    },
    "deliveryMethod": "Fastest delivery time",
    "shippingPrice": 10.00,
    "paymentSummary": {
        "last4": 4444,
        "brand": "Mastercard",
        "expMonth": 12,
        "expYear": 0
    },
    "orderItems": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "pictureUrl": "/images/products/boot-ang2.png",
            "price": 150.00,
            "quantity": 3
        }
    ],
    "subTotal": 450.00,  
    "status": "Pending",
    "total": 460.00, // update here
    "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6"
}
```

###### 196. Summary

```
Summary

- Adding the Order entity
- Aggregate Entities
- Owned Entities
- Unit of Work pattern

- Comming up Next
  - Orders + Payments
```

<hr>

### Section 18: Orders & Payments
<hr>

###### 197. Introduction

- `In this module`
```
- Client side orders
- Webhooks
  - communicate with the api server for the confirmation of the payment

- SignalR 

[ API ]       [Stripe]  
   |          /  |
   |        /    |
   |      /      |
[Client ]        |

1. Create payment intent with API (before payment)
2. API sends payment intent to Stripe
3. Stripe creates payment intent returns client secret
4. API returns client secret to client
5. Client sends payment to Stripe using the client secret
6. Stripe Sends confimation to client payment was successful
7. Client creates order with API
8. Stripe sends confirmation to API that payment was successful
9. Payment confirmed and can be shipped

SignalR
/* real time notification*/
- Provides real time-functionality
- Good for:
    - Dashboards
    - Monitoring apps
    - Apps that require notifications
    - Chat apps
- SignalR features
  - Connection management
  - Supports:
    - Websockets
    - Server sent events
    - Long polling
```

###### 198. Creating the order components
- `step-1-198: cd client | create order service | ' ng g s core/services/order --skip-tests '`
- `step-1b-198: cd client | create order features | ' ng g c features/orders/order --skip-tests --flat'`
- `step-1c-198: cd client | create order order-detailed | ' ng g c features/orders/order-detailed --skip-tests --flat'`

- `step-2-198: check at postman`
```
- section 17: Get Order For user {{ url }}/api/orders/1
- copy to clipboard below:
{
    "id": 1,
    "orderDate": "2025-07-17T18:58:12.7216706Z",
    "buyerEmail": "tom@test.com",
    "shippingAddress": {
        "name": "Tom Smith",
        "line1": "100 Centre Street",
        "line2": null,
        "city": "New York",
        "state": "NY",
        "postalCode": "10013",
        "country": "US"
    },
    "deliveryMethod": "Fastest delivery time",
    "shippingPrice": 10.00,
    "paymentSummary": {
        "last4": 4444,
        "brand": "Mastercard",
        "expMonth": 12,
        "expYear": 0
    },
    "orderItems": [
        {
            "productId": 17,
            "productName": "Angular Purple Boots",
            "pictureUrl": "/images/products/boot-ang2.png",
            "price": 150.00,
            "quantity": 3
        }
    ],
    "subTotal": 450.00,
    "status": "Pending",
    "total": 460.00,
    "paymentIntentId": "pi_3Rlwz8Q4ykDn46yO0mX9ptU6"
}

- then go to jsontots in google https://transform.tools/json-to-typescript
- paste the clipboard copy
- and then copy the typescript output
- then go create a order.ts file at client/src/app/shared/models/order.ts
```

- `step-3-198: create client/src/app/shared/order.ts `
```
export interface Order {
  id: number
  orderDate: string
  buyerEmail: string
  shippingAddress: ShippingAddress
  deliveryMethod: string
  shippingPrice: number
  paymentSummary: PaymentSummary
  orderItems: OrderItem[]
  subTotal: number
  status: string
  total: number
  paymentIntentId: string
}

export interface ShippingAddress {
  name: string
  line1: string
  line2?: string
  city: string
  state: string
  postalCode: string
  country: string
}

export interface PaymentSummary {
  last4: number
  brand: string
  expMonth: number
  expYear: number
}

export interface OrderItem {
  productId: number
  productName: string
  pictureUrl: string
  price: number
  quantity: number
}

export interface OrderToCreate{
  cartId: string;
  deliveryMethodId: number;
  shippingAddress: ShippingAddress;
  paymentSummary: PaymentSummary;
}
```

- `step-4-198: update order.services.ts `
```
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Order, OrderToCreate } from '../../shared/models/order';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  createOrder(orderToCreate: OrderToCreate){
    return this.http.post<Order>(this.baseUrl = 'orders', orderToCreate);
  }

  getOrdersForUser(){
    return this.http.get<Order[]>(this.baseUrl + 'orders');
  }

  getOrderDetailed(id: number){
    return this.http.get<Order>(this.baseUrl + 'orders/' + id);
  }

}
```

- `step-5-198: update client/src/app/app.routes.ts`
```
import { OrderComponent } from './features/orders/order.component';
import { OrderDetailedComponent } from './features/orders/order-detailed.component';

export const routes: Routes = [
  //{ path: 'checkout/success', component: CheckoutSuccessComponent, canActivate: [authGuard] },
  
  { path: 'orders', component: OrderComponent, canActivate: [authGuard] }, // update
  { path: 'orders/:id', component: OrderDetailedComponent, canActivate: [authGuard] }, // update
  
  //{ path: 'account/login', component: LoginComponent },
]
```

###### 199. Submitting the order
- `step-1-199: go to checkout.component.ts then createOrderModel method`
```

  private async createOrderModel():Promise<OrderToCreate> {
    const cart = this.cartService.cart();
    const shippingAddress = await this.getAddressFromStripeAddress() as ShippingAddress
    const card = this.confirmationToken?.payment_method_preview?.card;

    if(!cart?.id || !cart?.deliveryMethodId || !card || !shippingAddress){
      throw new Error("Problem creating order")
    }

    return {
      cartId: cart.id,
      paymentSummary: {
        last4: +card.last4,
        brand: card.brand,
        expMonth: card.exp_month,
        expYear: card.exp_year
      },
      deliveryMethodId: cart.deliveryMethodId,
      shippingAddress
    }
  } 

  /*
  - adding ShippingAddress solves the issue as ShippingAddress from createOrderModel 
  - remove the ShippingAddress from import of stripe
  - import { ConfirmationToken, StripeAddressElement, StripeAddressElementChangeEvent, StripePaymentElement, StripePaymentElementChangeEvent } from '@stripe/stripe-js';
  - then import add Import From shared models instead: 
    - import { ShippingAddress } from '../../shared/models/order'; 
    - it should be an interface and not the stripe
  -

  */

  private async getAddressFromStripeAddress(): Promise<Address| ShippingAddress | null> {
    const result = await this.addressElement?.getValue();
    const address = result?.value.address;

    if (address){
      return {
        name: result.value.name, // newly added
        line1: address.line1,
        line2: address.line2 || undefined,
        city: address.city,
        country: address.country,
        state: address.state,
        postalCode: address.postal_code,
      }
    } else return null;
  }

  /*
  - private async getAddressFromStripeAddress(): Promise<Address null> {} // old
  - update the inside it like the address
    -
      if (address){
        return {
          name: result.value.name, // update
          line1: address.line1,
          line2: address.line2 || undefined,
          city: address.city,
          country: address.country,
          state: address.state,
          postalCode: address.postal_code,
        }
      } else return null; 
  */


  /*  
  
  async onStepChange(event:StepperSelectionEvent){
    const address = await this.getAddressFromStripeAddress() as Address; // update - set as Address
    // const address = await this.getAddressFromStripeAddress();- old needs to update
  }  
  */

  -then add to createOrderModel = const card = this.confirmationToken?.payment_method_preview?.card;

  -then create a check statement
    - 
      if(!cart?.id || !cart?.deliveryMethodId || !card || !shippingAddress){
        throw new Error("Problem creating order")
      }

  - then
    return {
      cartId: cart.id,
      paymentSummary: {
        last4: +card.last4,
        brand: card.brand,
        expMonth: card.exp_month,
        expYear: card.exp_year
      },
      deliveryMethodId: cart.deliveryMethodId,
      shippingAddress
    }
  
  - then 
    private async createOrderModel(){} to adjust to promise to OrderToCreate below initation
    private async createOrderModel(): Promise<OrderToCreate> {} to adjust to promise to OrderToCreate


  - then use id to the confirmPayment(){} method
  - inside the try create a checking if(result.paymentIntent?.status === 'succeeded'){}
  - since it is in async loading at start turn on and turn off at the finally block not needed to subscribe to it and will make more confusion
  - then add = private orderService = inject(OrderService);
  - then 

  async confirmPayment(stepper: MatStepper){
    this.loading = true;
    try {
      if(this.confirmationToken){
        const result = await this.stripeService.confirmPayment(this.confirmationToken);

        if(result.paymentIntent?.status === 'succeeded'){
          const order = await this.createOrderModel();
          const orderResult = await firstValueFrom();
          if(orderResult){
            this.cartService.deleteCart();
            this.cartService.seletedDelivery.set(null);
            this.router.navigateByUrl('/checkout/success');
          } else {
            throw new Error('Order creation failed');
          } else if (result.error) {
            //coming from stripe error
            throw new Error(result.error.message);
        } else {
          throw new Error('Something went wrong')
        } 

        }

        /*
        - this will be remove because of modification above
        if(result.error){
          throw new Error(result.error.message);
        } else {
          /* transfer this to if(orderResult) */
          this.cartService.deleteCart();
          this.cartService.seletedDelivery.set(null);
          this.router.navigateByUrl('/checkout/success');
        }
        */

      }
    } catch (error: any) {
      this.snackbar.error(error.message || 'Something went wrong');
      stepper.previous();
    } finally {
      this.loading = false;
    }    
  }  

```
- `step-2-198: test in the UI/UX`
```
- shop - add to chart -
  - 5555 5555 5555 4444 - mastercard payment
  - not working upon payment 
    - 'issue Http failure response for https://localhost:4200/orders: 404 OK' - (fixed)
      - to fixed the issue i need to run over again and do some testing and found out the issue was in order.service.ts
      /*
        // correct one  
        createOrder(orderToCreate: OrderToCreate){
          
          return this.http.post<Order>(this.baseUrl + 'orders', orderToCreate);
        }

        - instead of + its = after this.baseUrl = 'orders'
        // wrong one 
        createOrder(orderToCreate: OrderToCreate){
          return this.http.post<Order>(this.baseUrl = 'orders', orderToCreate);
        }
      */

- database - localhost - skinet - tables - dbo.Orders - select top 1000
```

- `protoype messy code of checkout.component.ts `
```
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { OrderSummaryComponent } from "../../shared/components/order-summary/order-summary.component";
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatButton } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { StripeService } from '../../core/services/stripe.service';
import { ConfirmationToken, StripeAddressElement, StripeAddressElementChangeEvent, StripePaymentElement, StripePaymentElementChangeEvent } from '@stripe/stripe-js';
import { SnackbarService } from '../../core/services/snackbar.service';
import {MatCheckboxChange, MatCheckboxModule} from '@angular/material/checkbox';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { Address } from '../../shared/models/user';
import { firstValueFrom } from 'rxjs';
import { AccountService } from '../../core/servies/account.service';
import { CheckoutDeliveryComponent } from "./checkout-delivery/checkout-delivery.component";
import { CheckoutReviewComponent } from "./checkout-review/checkout-review.component";
import { CartService } from '../../core/services/cart.service';
import { CurrencyPipe, JsonPipe } from '@angular/common';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import { OrderToCreate, ShippingAddress } from '../../shared/models/order';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  imports: [
    OrderSummaryComponent,
    MatStepperModule,
    MatButton,
    RouterLink,
    MatCheckboxModule,
    CheckoutDeliveryComponent,
    CheckoutReviewComponent,
    CurrencyPipe,
    JsonPipe,
    MatProgressSpinnerModule
],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit, OnDestroy {
  // inject stripe into our component
  private stripeService = inject(StripeService);
  private snackbar = inject(SnackbarService);
  private router = inject(Router);
  private accountService = inject(AccountService);
  private orderService = inject(OrderService);
  cartService = inject(CartService);
  addressElement?: StripeAddressElement;
  paymentElement?: StripePaymentElement;
  saveAddress = false;

  completionStatus = signal<{address: boolean, card: boolean, delivery:boolean}>(
    {address: false, card: false, delivery: false}
  )

  confirmationToken?: ConfirmationToken;
  loading = false;

  async ngOnInit() {
    try {
      this.addressElement = await this.stripeService.createAddressElement();
      this.addressElement.mount('#address-element');
      this.addressElement.on('change', this.handleAddressChange);

      this.paymentElement = await this.stripeService.createPaymentElement();
      this.paymentElement.mount('#payment-element');
      this.paymentElement.on('change', this.handlePaymentChange);
    } catch (error: any) {
      this.snackbar.error(error.message);
    }
  }

  handleAddressChange = (event: StripeAddressElementChangeEvent) => {
    this.completionStatus.update(state => {
      state.address = event.complete;
      return state;
    })
  }

  handlePaymentChange = (event: StripePaymentElementChangeEvent) => {
    this.completionStatus.update(state => {
      state.card = event.complete;
      return state;
    })
  }

  handleDeliveryChange(event: boolean){
    this.completionStatus.update(state =>{
      state.delivery = event;
      return state;
    })
  }

  async getConfirmationToken(){
    try {
      if(Object.values(this.completionStatus()).every(status => status === true)){
        const result = await this.stripeService.createConfimationToken();
        if(result.error) throw new Error(result.error.message);
        this.confirmationToken = result.confirmationToken;
        console.log(this.confirmationToken);
      }
    } catch (error:any) {
      this.snackbar.error(error.message);
    }
  }

  async onStepChange(event:StepperSelectionEvent){
    if (event.selectedIndex === 1){
      if (this.saveAddress){
        // const address = await this.addressElement?.getValue();
        const address = await this.getAddressFromStripeAddress() as Address;
        address && firstValueFrom(this.accountService.updateAddress(address));
      }
    }
    if(event.selectedIndex === 2){
      // update payment intent
      await firstValueFrom(this.stripeService.createOrUpdatePaymentIntent());
    }
    if(event.selectedIndex === 3) {
      await this.getConfirmationToken();
    }
  }

  async confirmPayment(stepper: MatStepper){
    this.loading = true;
    try {
      if(this.confirmationToken){
        const result = await this.stripeService.confirmPayment(this.confirmationToken);

        if(result.paymentIntent?.status === 'succeeded'){
          const order = await this.createOrderModel();
          const orderResult = await firstValueFrom(this.orderService.createOrder(order));
          if(orderResult){
            this.cartService.deleteCart();
            this.cartService.seletedDelivery.set(null);
            this.router.navigateByUrl('/checkout/success');
          } else {
            throw new Error('Order creation failed');
          }
        } else if (result.error) {
            //coming from stripe error
            throw new Error(result.error.message);
        } else {
          throw new Error('Something went wrong')
        }

        // if(result.error){
        //   throw new Error(result.error.message);
        // } else {
        //   this.cartService.deleteCart();
        //   this.cartService.seletedDelivery.set(null);
        //   this.router.navigateByUrl('/checkout/success');
        // }
      }
    } catch (error: any) {
      this.snackbar.error(error.message || 'Something went wrong');
      stepper.previous();
    } finally {
      this.loading = false;
    }
  }

  private async createOrderModel():Promise<OrderToCreate> {
    const cart = this.cartService.cart();
    const shippingAddress = await this.getAddressFromStripeAddress() as ShippingAddress
    const card = this.confirmationToken?.payment_method_preview?.card;

    if(!cart?.id || !cart?.deliveryMethodId || !card || !shippingAddress){
      throw new Error("Problem creating order")
    }

    return {
      cartId: cart.id,
      paymentSummary: {
        last4: +card.last4,
        brand: card.brand,
        expMonth: card.exp_month,
        expYear: card.exp_year
      },
      deliveryMethodId: cart.deliveryMethodId,
      shippingAddress
    }
    // const order: OrderToCreate = {
    //   cartId: cart.id,
    //   paymentSummary: {
    //     last4: +card.last4,
    //     brand: card.brand,
    //     expMonth: card.exp_month,
    //     expYear: card.exp_year
    //   },
    //   deliveryMethodId: cart.deliveryMethodId,
    //   shippingAddress
    // }
  }

  private async getAddressFromStripeAddress(): Promise<Address| ShippingAddress | null> {
    const result = await this.addressElement?.getValue();
    const address = result?.value.address;

    if (address){
      return {
        name: result.value.name,
        line1: address.line1,
        line2: address.line2 || undefined,
        city: address.city,
        country: address.country,
        state: address.state,
        postalCode: address.postal_code,
      }
    } else return null;
  }

  onSaveAddressCheckboxChange(event: MatCheckboxChange){
    this.saveAddress = event.checked;
  }

  ngOnDestroy(): void {
    this.stripeService.disposeElements();
  }
}
```

###### 200. Designing the order component to display orders 
- `step-1-200: Open order.component.ts`
```
/*
  - goal is to get the order from the API
  - display in the User interface
*/

import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../shared/models/order';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-order',
  imports: [
    RouterLink,
    DatePipe,
    CurrencyPipe
  ],
  templateUrl: './order.component.html',
  styleUrl: './order.component.scss'
})
export class OrderComponent implements OnInit {
  private orderService = inject(OrderService);
  orders: Order[] = [];

  ngOnInit(): void {
    this.orderService.getOrdersForUser().subscribe({
      next: orders => this.orders = orders

    })
  }
}
```

- `step-2-200: update = order.component.html`
```
<div class="mx-auto mt-32">
  <h2 class="font-semibold text-2xl mb-6 text-center">
    My Orders
  </h2>
  <div class="flex flex-col">
    <div class="w-full">
      <table class="min-w-full divide-y divide-gray-200 cursor-pointer">
        <thead class="bg-gray-50">
          <tr class="uppercase text-gray-600 text-sm">
            <th class="text-center px-6 py-3">Order</th>
            <th class="text-left">Date</th>
            <th class="text-left">Total</th>
            <th class="text-left">Status</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divid-gray-200">
          @for (order of orders; track order.id) {
            <tr routerLink="/orders/{{order.id}}" class="hover:bg-gray-100">
              <th class="px-6 py-3"># {{order.id}}</th>
              <td>{{ order.orderDate | date: 'medium' }}</td>
              <td>{{ order.total | currency }}</td>
              <td>{{ order.status }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  </div>
</div>
```

- `step-3-200: checking the UI`
```
/*
    https://localhost:4200/orders
*/
```

###### 201. Creating the order detailed page

- `step-1-201: https://localhost:4200/orders/2`

- `step-1a-201: update order-detailed.component.ts`
```
- steps-1a-201:
  - create private orderService = inject(OrderService);
  - create private activateRouter = inject(ActivateRouter);
  - create order?: Order;
  - implements OnInit
    -ngOnInit(): void(){
      // create first loadOrder method
      // insert loadOrder
      this.loadOrder();

    }

  - create loadOrder(){
    const id = this.activateRouter.snapshot.paramMap.get('id');
    if(!id) return;
    this.orderService.getOrderDetailed(+id).subscribe({
      next: order => this.order = order,
    })
  }

/*
  order-detailed.component.ts full code below:
*/ 
import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../core/services/order.service';
import { ActivatedRoute } from '@angular/router';
import { Order } from '../../shared/models/order';

@Component({
  selector: 'app-order-detailed',
  imports: [],
  templateUrl: './order-detailed.component.html',
  styleUrl: './order-detailed.component.scss'
})
export class OrderDetailedComponent implements OnInit {
  private orderService = inject(OrderService);
  private activateRouter = inject(ActivatedRoute);
  order?: Order;

  ngOnInit(): void {
    this.loadOrder();
  }

  loadOrder() {
    const id = this.activateRouter.snapshot.paramMap.get('id');
    if(!id) return;
    this.orderService.getOrderDetailed(+id).subscribe({
      next: order => this.order = order,
    })
  }
}
```

- `step-1b-201: update - order-detailed.component.ts `
```
/*
  - imports: [MatCardModule,  MatButton ]
*/
import { MatCardModule } from '@angular/material/card';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-order-detailed',
  imports: [
    MatCardModule,
    MatButton
  ],
  templateUrl: './order-detailed.component.html',
  styleUrl: './order-detailed.component.scss'
})

/*
    next go to template definition order.detailed.component.html
*/
```

- `step-1c-201: update  order.detailed.component.html `
```
/*
  - check first there is an order via if(){...}
  - because of flex everything <div class="mt-8 py-3 border-t border-gray-200 flex gap-16">...</div>
    will adjust automatically to left and right <div class="space-y-2"></div> there are two of it 
  - to save a time copy checkout-review.component.html = <table class="w-full text-center">...</div>
    and modify some component in it.
      like: 
        @for (item of cartService.cart()?.items; track item.productId) to  
        @for (item of order.orderItems; track item.productId)
  - go to order-summary.component.html and copy to clipboard
      <div class="space-y-4 rounded-lg border border-gray-200 p-4 bg-white shadow-sm">...</div>
      then paste to order-detailed.component.ts
*/

@if(order){
  <mat-card class="bg-white py-8 shadow-md max-w-screen-lg mx-auto">
      <div class="px-4 w-full">
        <h2 class="text-2xl text-center font-semibold">Order summary for order #{{ order.id }}</h2>
        <div class="mt-8 py-3 border-t border-gray-200 flex gap-16">
          <div class="space-y-2">
            <h4 class="text-lg font-semibold">Billing and delivery information</h4>
            <dl>
              <dt class="font-medium">Shipping address</dt>
              <dd class="mt-1 font-light">Address goes here</dd>
            </dl>
            <dl>
              <dt class="font-medium">Payment info</dt>
              <dd class="mt-1 font-light">payment info goes here</dd>
            </dl>
          </div>
          <div class="space-y-2">
            <h4 class="text-lg font-semibold">Order details</h4>
            <dl>
              <dt class="font-medium">Email address</dt>
              <dd class="mt-1 font-light">{{ order.buyerEmail }}</dd>
            </dl>
            <dl>
              <dt class="font-medium">Order status</dt>
              <dd class="mt-1 font-light">{{ order.status }}</dd>
            </dl>
            <dl>
              <dt class="font-medium">Order date</dt>
              <dd class="mt-1 font-light">{{ order.orderDate | date: 'medium' }}</dd>
            </dl>
          </div>
        </div>

        <div class="mt-4">
          <div class="order-y border-gray-200">
            <table class="w-full text-center">
              <tbody class="divide-y divide-gray-200">
                @for (item of order.orderItems; track item.productId) {
                  <tr>
                    <td class="py-4">
                      <div class="flex items-center gap-4">
                        <img src="{{item.pictureUrl}}" alt="product image" class="w-10 h-10"/>
                        <span>{{item.productName}}</span>
                      </div>
                    </td>
                    <td class="p-4">x{{ item.quantity }}</td>
                    <td class="p-4 text-right">{{ item.price | currency }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <div class="space-y-4 rounded-lg border-y border-gray-200 p-4 bg-white shadow-sm">
          <p class="text-xl font-semi-bold">Order Summary</p>
          <div class="space-y-4">
            <div class="space-y-2">
              <dl class="flex items-center justify-between gap-4">
                <dt class="font-medium text-gray-500">Subtotal</dt>
                <dd class="font-medium text-gray-900">
                  {{ order.subTotal | currency}}
                </dd>
              </dl>

              <dl class="flex items-center justify-between gap-4">
                <dt class="font-medium text-gray-500">Discount</dt>
                <dd class="font-medium text-green-500">
                  - $0.00
                </dd>
              </dl>

              <dl class="flex items-center justify-between gap-4">
                <dt class="font-medium text-gray-500">Delivery fee</dt>
                <dd class="font-medium text-gray-900">
                  {{ order.shippingPrice | currency }}
                </dd>
              </dl>

              <dl class="flex items-center justify-between gap-4 border-t border-gray-200 pt-2">
                <dt class="font-medium text-gray-500">Total</dt>
                <dd class="font-medium text-gray-900">
                  {{ order.total | currency }}
                </dd>
              </dl>
            </div>

          </div>
        </div>
      </div>
  </mat-card>
}
```

###### 202. Updating the address pipe with type guards

- `step-1-202: update order-detailed.component.html`
```
- update Shipping address

          <div class="space-y-2">
            <h4 class="text-lg font-semibold">Billing and delivery information</h4>
            <dl>
              <dt class="font-medium">Shipping address</dt>
              <dd class="mt-1 font-light">Address goes here</dd> // needs modification below:
              <dd class="mt-1 font-light">{{ order.shippingAddress | address }}</dd>
            </dl>
            <dl>
              <dt class="font-medium">Payment info</dt>
              <dd class="mt-1 font-light">{{ order.paymentSummary | paymentCard }}</dd> 
              // modify
                // encounter error which we need to fixed in 'step-2-202 paymentCard issue address.pine.ts'
                // step-3-202: paymentSummary issue is next - payment-card.pipe.ts  
            </dl>
          </div>

```

- `step-2-202: modify address.pipe.ts `
```
import { ShippingAddress } from '../models/order';

export class AddressPipe implements PipeTransform {
  /* modifide code below: */
    transform(value?: ConfirmationToken['shipping'] | ShippingAddress, ...args: unknown[]): unknown {
    if(value && 'address' && value.name){
      const {line1, line2, city, state, country, postal_code} =
        (value as ConfirmationToken['shipping'])?.address!;
      return `${value.name}, ${line1}${line2 ? ', ' + line2 : ''},
        ${city}, ${state}, ${postal_code}, ${country}`
    } else if(value && 'line1' in value){
      const {line1, line2, city, state, country, postalCode} =
        value as ShippingAddress;
      return `${value.name}, ${line1}${line2 ? ', ' + line2 : ''},
        ${city}, ${state}, ${postalCode}, ${country}`
    }
    else {
      return 'Unknown address'
    }
  }

  /* old code below */
  transform(value?: ConfirmationToken['shipping'], ...args: unknown[]): unknown {
    if(value?.address && value.name){
      const {line1, line2, city, state, country, postal_code} = value.address;
      return `${value.name}, ${line1}${line2 ? ', ' + line2 : ''},
        ${city}, ${state}, ${postal_code}, ${country}`
    } else {
      return 'Unknown address'
    }
  }

}
```

- `step-3-202: Update payment-card.pipe.ts `
```
import { PaymentSummary } from '../models/order';

/* modified code below: */
export class PaymentCardPipe implements PipeTransform {

  transform(value?: ConfirmationToken['payment_method_preview'] | PaymentSummary, ...args: unknown[]): unknown {
    if(value && 'card' in value) {
      const {brand, last4, exp_month, exp_year} =
        (value as ConfirmationToken['payment_method_preview']).card!;
          return `${brand.toUpperCase()} **** **** **** ${last4}, Exp: ${exp_month}/${exp_year}`;
    } else if(value && 'last4' in value) {
      const {brand, last4, expMonth, expYear} =  value as PaymentSummary;
        return `${brand.toUpperCase()} **** **** **** ${last4}, Exp: ${expMonth}/${expYear}`;
    } else {
      return 'Unknown payment method'
    }
  }
}


/*old code below:

    export class PaymentCardPipe implements PipeTransform {

  transform(value?: ConfirmationToken['payment_method_preview'], ...args: unknown[]): unknown {
    if(value?.card){
      const {brand, last4, exp_month, exp_year} = value.card;
      return `${brand.toUpperCase()} **** **** **** ${last4}, Exp: ${exp_month}/${exp_year}`;
    } else {
      return 'Unknown payment method'
    }
  }
}

*/
```

- `step-1b-202: add button in order-detailed.component.html`
```
/*
- add routerlink should be added in the imports @order-detailed.component.ts 
    RouterLink
    @Component({
  selector: 'app-order-detailed',
  imports: [
  //...PaymentCardPipe,
    RouterLink
],
  templateUrl: './order-detailed.component.html',
  styleUrl: './order-detailed.component.scss'
})
*/

<div class="px-4 w-full">
        <div class="flex justify-between items-center align-middle"> // modify here styles
          <h2 class="text-2xl text-center font-semibold">Order summary for order #{{ order.id }}</h2>
          <button routerLink="/orders" mat-stroked-button>Return to orders</button> // modify here the routerlink
        </div>
        <div class="mt-8 py-3 border-t border-gray-200 flex gap-16">
        ...
        </div>
        //...
</div>

```
- ` issue on my end https://localhost:4200/orders/2003 `
```
- always loading and not showing any population of data from stripe in the UI/UX

- working redo the payment process and its all good tested only once
```