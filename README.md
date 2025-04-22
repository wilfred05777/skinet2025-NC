To run the program
  - cd api / dotnet run or dotnet watch

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

###### 198. Creating the order components

` IProductRepository.cs `
```
Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort);
```

` ProductRepository.cs `
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

` ProductsController.cs `
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

` Create a generic repository `
` Specification pattern `
` Using the specification pattern `
` Using the debugger `
` Shaping data `

- `about Generics`
    - `Been around shice C# 2.0 (2002)`
    - `Help avoid duplicate code`
    - `Type safety`

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
` Generic Repository can do a simple things `
` but when it comes to more complex queries  bearing in mind for Generic Repositories 'I have a hundred queries' `

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
` implement the interface - highlight this- IGenericRepository `
##### Add to service in program.cs
```
// services section
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
```

###### 30. Implementing the generic repository methods

` Infrastructure/Data/GenericRepository.cs `
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

` Specification pattern `
` Generic Repository is an Anti-Pattern! `
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
` Core/Interfaces/ISpecification.cs `
```
using System;
using System.Linq.Expressions;
namespace Core.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria{ get; }
}

```
- create folder name
` Core/Specifications `
` Core/Specifications/BaseSpecifications.cs `
` implement the interface `
` create and assign field 'criteria' `
` BaseSpecifications(Expression< - highlight Expression - Use primary constructor `
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

` Update Core/Interfaces/IGenericRepository.cs `
```
public interface IGenericRepository<T> where T : BaseEntity
{
    //...
    Task<T?> GetEntityWithSpec(ISpecification<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    //...
}
```
` Update Infrastructure/Data/GenericRepository.cs `
` IGenericRepository = Implement generic methods by Implement Interface `
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

` create Core/Specification/ProductSpecification.cs -> start `

```
using Core.Entities;
namespace Core.Specifications;

public class ProductSpecification : BaseSpecifications<Product>
{

}

```

` adjustment to Core/Specifications/BaseSpecification.cs `
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

` adjust code in  Core/Interaces/ISpecification.cs `
```
using System.Linq.Expressions;
namespace Core.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria{ get; }
}
// making it optional by adding in bool>>? <--
```

` continue -> create Core/Specification/ProductSpecification.cs `

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

` update API/Controllers/ProductsController.cs `
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

` then test Postman Section 4. Specification - Get Products by Brand & Get Products by Type `

###### 36. Adding sorting to the specification
` update Core/Interfaces/ISpecification.cs `
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

` update Infrastructure/Data/SpecificationEvaluator.cs `
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
` update Core/Specifications/ProductSpecification.cs `
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
- ` check API through postman section 4 - Specification - 'Get Products' check if the filter is in Alphabetical order`
- ` check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceAsc price upwards from smallest to highest `

##### !! Issue Encounter & Solution !!: 
` check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceDesc price downwards from highest to lowest ! it got some bugs = because of copy and paste` <br>

- ` solution check: Infrastructure/Data/SpecificationEvaluator.cs -looks good `
- ` solution check: Core/Specifications/ProductSpecification.cs -looks good `
- ` solution check: Core/Specifications/BaseSpecification.cs -the issue is here `
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
- `check API through postman section 4 - Specification - 'Get Products sorted by Price' check {{url}}/api/products?sort=priceDesc price downwards from highest to lowest - functionally good`


###### 37. Using the debugger

` creating debugger - VSCode left handside - 'Run & Debug' -> create a launch.json file` 
` select c# `
` launch.json`
` select add Configuration button + ` 
- `'.NET: attach to .NET process' `
` select add Configuration button + ` 
- `'.NET: launch C# project' `
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
` then create/assign breakpoint in this case its  ProductsController.cs line 18 `
```
  var spec = new ProductSpecification(brand, type, sort);
```
` click debug and run icon - search API and select it then also select .NET Core Attach`
` hit green play icon`

` test in postman`
` Get Products sorted by price `
```
{{url}}/api/products?sort=priceDesc
to 
{{url}}/api/products?sort=priceDesc&type=boards
```
` 'step into' ProductsControllers.cs`

` - Flow of all and correlation about how data is working behind the scene using vscode debugger `

###### 38. Adding projection to the spec part 1
` Enhancing the specification pattern `

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

` update Core/Interfaces/IGenericRepository.cs `
```
public interface IGenericRepository<T> where T : BaseEntity
{
    // more code on top

    Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec);
    Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec);
    
    // more code on below
}
```
` update Infrastructure/Data/GenericRepository.cs `
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
` update Core/Interface/ISpecification.cs `
```
public interface ISpecification<T>
{
    bool IsDistinct { get; }

}
```
` update Core/Specifications/BaseSpecification.cs `
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
` update Infrastructure/Data/SpecificationEvaluator.cs `
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
` Note: Error on BrandListSpecification(){...} to fixed it in BaseSpecification.cs `
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
` check in postman `
` section 4 - Specification `
- ` Get Product Brands  - {{url}}/api/products/brands `
- ` Get Product Types  - {{url}}/api/products/types `

###### 41. Summary
- Creating a generic repository
- Specification pattern
- Using the specification pattern
- Using the debugger
- Shaping data

- FAQs
` Question: This is over engineering in action! `
` Answer; True for now, but we do now have a repository for every entity we create. Imagine we have 100 or 1000 entities, then we have just creatd repositories for all of them. `

` its reusable for every across every future project creating all the example are all generic and reuseable. `
<hr>

### Section 5: Sorting, Filtering, Searching & Pagination
--- 
###### 42. Introduction
- API Sorting, Search, Filtering, & Paging
    - ` Sorting `
    - ` Filtering `
    - ` Searching `
    - ` Paging `

- Goal: 
``` To be able to implement sorting, searching and pagination functionality in a list using the Specification parttern ```

- Pagination
    - ` Performance `
    - ` Parameters passed by query string: api/products?pageNumber=2&pageSize=5  `
    - ` Page size should be limited `
    - ` We should always page results `
- Deferred Execution
    - ` Query commands are stored in a variable`
    - ` Execution of the query is deffered `
    - ` IQueryable<T> creates an expression tree `
    - ` Execution:  `
        - ` ToList(), ToArrya(), ToDictionary()  `
        - ` Count() or other singleton queries  `

###### 43. Creating product spec parameters

` create Core/Specifications/ProductSpecParams.cs `
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

` Check api (Postman) at Section 5: Get Products by Brand ` 
    - `{{url}}/api/products?brands=Angular,React `

` Get Products by Brand and Types ` 
    - `{{url}}/api/products?brands=Angular,React&types=Boots,Gloves `


###### 44. Adding pagination part 1
` update Infrastructure/Data/ProductRepository.cs `
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
` update Core/Interface/ISpecification.cs `
```
public interface ISpecification<T>
{
    //...
        int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }

}
```

` update Core/Specifications/BaseSpecifications.cs `
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

` update Infrastructure/Data/SpecificationEvaluator.cs `
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
` update Core/Specifications/ProductSpecification.cs `
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

` Create API/RequestHelpers/Pagination.cs `
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

` update Core/Specifications/BaseSpecification.cs `
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
` testing API via Postman Section 5 - Paging, sorting, and filtering : `
   - ` Get Paged Products Page 0 Size 5 -> {{url}}/api/products?pageSize=3&pageIndex=1 `
    - ` not showing count: 0 upon testing the api `


###### 47. Creating a Base API controller
` create API/Controllers/BaseApiController.cs `
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
` to use/update in API/Controllers/ProductsController.cs `
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
` testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=2&types=boards `
` testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=1&types=gloves `

   - ` BUG: issue is that count & data : not returning anything. Status: not yet solve !FF `
   - ` get all products bug not showing after the applying Section 5: Sorting, Filtering, Pagination `



