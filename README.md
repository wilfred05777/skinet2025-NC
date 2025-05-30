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
- ` Sect. 5 Issue : Solution Sect. 5 Issue `

` testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=2&types=boards `
` testing API via postman : {{url}}/api/products?pageSize=3&pageIndex=1&types=gloves `

   - ` BUG: issue is that count & data : not returning anything. Status: not yet solve !FF `
   - ` get all products bug not showing after the applying Section 5: Sorting, Filtering, Pagination `


###### 48. Adding the search functionality

` update Core/Specifications/ProductSpecParams.cs `
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
` update Core/Specifications/ProductSpecification.cs `
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
` testing in postman GetProducts with search term: {{url}}/api/products?search=red `

` refer back to :  Sect. 5 Issue : for hints `
` S`

` Solution Sect. 5 Issue: is in ProductSpecification.cs `
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

- `git log // show time details of each commits `
- `git certain commit ######`
- `git reset --hard commit ###### //reset back to a certain save point` 
- `note: be careful using reset and revert - first you should have branch for it before using the main branch`
    

<hr>

### 6 Error handling on the API
<hr>

###### 50. Introduction

- Error handling and exceptions
- Validation errors
- Http response errors
- Middleware - catching as defense for handling error
- CORS

```
 /* Http response codes */

 200 range => ok
 300 range => Redirection
 400 range => Client error
 500 range => Server error

    - 500 Internal server error
```

###### 51. Adding a test controller for error handling 
` create API/Controllers/BuggyControllers.cs `
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
` postman checking API: Section 6- Get Notfound , Get Bad Request & Validation Error `
 - ` {{url}}/api/buggy/notfound `
 - ` {{url}}/api/buggy/badrequest `
 - ` {{url}}/api/buggy/validationerror `

###### 52. Exception handling middleware

```
<!-- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-9.0 -->

```

` create API/Errors/ApiErrorResponse.cs `

```
namespace API.Errors;

public class ApiErrorResponse(int statusCode, string message, string? details)
{
    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;
    public string? Details { get; set; } = details;
}
```
` create API/Middleware/ExceptionMiddleware.cs `
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
` going back to API/program.cs class to use the functionality forom ExceptionMiddleware.cs `
```
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
//... 
```

` postman: Get Internal server error = {{url}}/api/buggy/internalerror `

###### 53. Validation error responses
` create API/DTOs/CreateProductDto.cs `

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
` update BuggyController.cs `
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
` Postman: Section 6: Get Validation Error =>  {{url}}/api/buggy/validationerror `

###### 54. Adding CORS support on the API
` update API/Program.cs class  `
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
` Postman testing APi: section 6: Check Cors is enabled => {{url}}/api/products => scripts tab `
- `Scripts tab then on headers tab will see the Access-Control-Allow-Origin: value: https://localhost:4200 `

- ` Sect. 5 Issue | not showing anything on count & data `

###### 55. Summary
- Error Handling objective
    - Goal to handle errors so we can configure the UI in the client for all errors generated by the API 
    
    - client side of things

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
` create Core/Specifications/PagingParams.cs `
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

- Install the Angular CLI
- Creating the Angular project
- Setting up VS Code for Angular*
- Setting up Angular to use HTTPS
- Adding Angular Material and Tailwind CSS

- ###### Goal
- To have a working Angular application running on HTTPS
- To understand angular standalone components and how we use them to build an app.

- ###### Angular Release Schedule
    - Major release every 6 months
    - 1-3 minor releases for each major release
    - A patch release build almost every week
    - At time of recording Angular is on v18

###### 57. Creating the angular project

- [angular.dev](https://angular.dev/)
- [Angular versioning and releases](https://angular.dev/reference/releases)
- [Version compatibility](https://angular.dev/reference/versions)

- [The Angular CLI](https://angular.dev/tools/cli)
- [Setting up the local environment and workspace](https://angular.dev/tools/cli/setup-local)
    - ` npm install -g @angular/cli `
    - ` node --version or -v // check node version `
    - ` npm --version or -v // check npm version `
    - ` ng version // check angular version `

- ###### create angular project 
    - ` ng new client ` 
    - ` Would you like to share pseudonymous usage data abou: NO `
    - ` Would you like to share pseudonymous usage data abou: NO `
    - ` Which stylesheet format would you like to use?: Sass(SCSS) `
    - ` Do you want to enable Server-Side Rendering (SSR) and Static Site Generation (SSG/Prerendering)?: No `

    - ` ng serve`
    - ` ng `
    - ` npm link @angular/cli`



###### 58. Reviewing the Angular project files 

- just and overview of basic fundamental files in angular


###### 59. Using HTTPS with the Angular project

- [Mkcertificate](https://github.com/FiloSottile/mkcert) 
- gitbash: choco install mkcert
- `cd client`
- `mkdir ssl`
- `mkcert localhost`

```
// optional // switching to MS-Edge browser | chrome to test https works better than firefox
mkcert -install
```

- ` client/angular.json `
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
- `ng serve`

##### Angular Styling Configuration:

###### 60. Adding Angular Material and Tailwind CSS
- [Angular Material UI](https://material.angular.io/)
- [Angular Material UI](https://material.angular.io/components/categories)

- ` cd client/ then => ng add @angular/material `
- ` The package @angular/material@19.2.11 will be installed and executed.
Would you like to proceed? : YES `
- `Choose a prebuilt theme name, or "custom" for a custom theme: Azure/Blue`
- `Set up global Angular Material typography styles?: No`
- `Include the Angular animations module?: Yes`

###### Installing Tailwind CSS In Angular Project
- [Install Tailwind CSS with Angular](https://tailwindcss.com/docs/installation/framework-guides/angular)

- side note: For Utility Classes 
- 
- ` npm install -D tailwindcss postcss autoprefixer // it does not work anymore `
- ` npx tailwindcss init // it does not work anymore `


- [How to set up Angular & Tailwind CSS 4 in VS Code with Intellisense](https://www.youtube.com/watch?v=s-TAV5pQfcU)
- `npm install tailwindcss @tailwindcss/postcss postcss --force `
- `create client/.postcssrc.json
```
{
  "plugins": {
    "@tailwindcss/postcss": {}
  }
}
```
- `update client/src/style.scss
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

- ` Extension : Angular Language Service: by angular.dev `
- ` Extension : Tailwind CSS IntelliSense: by Tailwind Labs tailwindcss.com // For some reason - I restarted the extension but I turns off its auto suggestions`

###### 62. Summary

- ` Installing the Angular CLI `
- ` Creating the Angular project `
- ` Setting up VS Code for Angular* `
- ` Setting up Angular to use HTTPS `

<hr>

### Section 8: Angular Basics
<hr>

- `Adding components`
- `Http client module`
- `Observables => note: doesn't use promise base but observables - its benefits, usage; also there is abetter tools SignalR ` 
- `Typescript: => type safety like C# and Java better for development at earlier stage`

 
```
Goal:

To be able to use the http client to retrieve data from the API.

To understand the basics of observables and Typescript

```

###### 64. Setting up the folder structure and creating components

- `create src/app/core/`
- `create src/app/shared/`
- `create src/app/features/`
- `create src/app/layout/`

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
- ` udpate client/src/app/layout/header.component.html `
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

- ` udpate client/src/app/layout/header.component.ts `
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

- ` add images like logo in client/public `
- ` if a logo recently added would not show - a restart on ng serve will suffice as first step to troubleshoot it `


- ` update file header.component.scss `
```
//...
    <div class="flex gap-3 align-middle">
    <a matBadge="5" matBadgeSize="large" class="custom-badge mt-2 mr-2"> // added class="custom-badge mt-2 mr-2"
//...
```
- ` update file header.component.scss `
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

- `styling global css in angular  `

- ` client/src/styles.css  `
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
- ` then implement it in the entire website/app via top level html  `
- ` client/src/index.html `
```
/* add the custom-theme class */
<html lang="en" class="custom-theme"> 
```
- ` incouter some issue: Visual studio code  : Git: fatal: unable to access 'https:/github.com/wilfred05777/skinet2025-NC.git/': Could not resolve proxy: proxy.server.com`
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

- ["Installing Node using NVM"](https://gist.github.com/MichaelCurrin/5c2d59b2bad4573b26d0388b05ab560e)
- ` nvm list `
- ` nvm install 22.15 `
- ` nvm use 22.5 `

- ` update client/src/app/app.config.ts ` 
```
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    //...
    provideHttpClient(),  
  ],
};
```
- `update client/src/app/app.component.ts`

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
- /* implementing observable app.components.ts*/

```
ngOnInit():void {
  this.http.get(this.baseUrl + 'products').subscribe({
    next: data => console.log(data),
    error: error => console.log(error),
    complete: () => console.log('complete')
  })
}
```
- ` to test go broswer console `
- ` encounter error - Cross Origin Request Blocked: on laptop`
- `update note: working sa desktop node --version 22.13.1` 

- ` /* Display actual product from API to UI(client - angular project) */`
- `update app.component.ts`
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

- ` update client/src/app/app.component.html ` 
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
- ` update client/src/styles.scss ` 
```
//...
.container {
  @apply mx-auto max-w-screen-2xl
}
``` 

###### 68. Introduction to observables
- `Note:`
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
- ` Observable `
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
- ` HTTP, Observables and RxJS `
```
1. HTTP Get request from ShopService
2. Receive the Observable and cast it into a Products Array
3. Subscribe to the Obeservable from the component
4. Assign the Products array to a local variable for use in the components template
```
- ` RxJS `
```
    -Reactive Extensions for JavaScript
    -Utility library for working with observables, similar to lodash or
     underscore for javascript objects and arrays.
    -Uses the pipe() method to chain RxJS operators together
```
###### 69. Introduction to TypeScript
-   ` Typescript ` 
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

- `create client/src/demo.ts `
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

- ` creating Product models -> client/src/app/shared/models/products.ts `
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
- ` creating Paginantion models -> client/src/app/shared/models/pagination.ts `
```
//<T> is a generics the same with c# but it can also be used in Typescript
export type Pagination<T> = { 
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}
```
- ` update src/app/app.component.ts `
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
- ` update client/src/app/app.component.html `
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

- `Goal:` 
```
Goal:
To be able to use the http client to retrieve data from the API

To understand the basics of obeservables and Typescript
```

- ` FAQs `
```
    Question: 
    Can I use <CssFramework>
    instead of Angular Material and Tailwind?

    Answer: 
    Only with great difficulty. 
    This is heavily intergrated into the app.
```
- ` fixing Tailwind Compile issue at global style.scss`
```
/* Fixing tailwind import issue = FF Solution with chatgpt's help */

// @import "tailwindcss";
@use "tailwindcss" as *; 
```
<hr>

### Section 9: Building the User Interface for the shop
<hr>

- ` 73. Introduction : Client Building the UI`
```
    -Angular services (AS) - Singleton, for this project we don't redux (state management) to remember things utilize AS for this project but we can if we want to.
    -Building the UI for the shop
    -Material components
    -Pagination
    -Filtering, Sorting & Search
    -Input properties - child components

```

###### 74. Introduction to Angular services

- ` create angular service for reusable logic, ng service is a singleton, `
- ` ' ng g service core/services/shop.services.ts ' => go to core component <---/* working */ ` 
- ` ' ng g service core/services/shop.services.ts ' --dry-run <- this command will generate service at app/core/services/shop <---/* not working */`
- ` ' ng g service core/services/shop.services.ts '  --dry-run --skip-tests <---/* not working */ <- this command will generate service at app/core/services/shop.services.ts`

- ` create client/src/app/core/services/shop.services.ts`

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
- ` update app/app.component.ts`
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
- ` update client/src/app/core/services/shop.services.ts`
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

- ` create features/shop.component.ts`
- ` ng g component features/shop --dry-run / --skip-tests `
- ` shop.component.ts`
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
- ` update src/app/app.component.ts `
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
- ` update app.component.html `

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

- ` update src/app/app.component.ts `
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
- ` client/src/app/features/shop/shop.component.ts`
```

```
- ` client/src/app/features/shop/shop.component.html`
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

- ` update shop.component.ts`
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
- ` update client/src/app/core/services/shop.services.ts `
```
    getProducts() {

    new return this.http.get<Pagination<Product>>(this.baseUrl + 'products?pageSize=20')
    
    <!--old return this.http.get<Pagination<Product>>(this.baseUrl + 'products') -->

  }
```

###### 76. Adding a product item component

- `ng g c features/shop/product-item --skip-tests`

```
    - src/app/features/shop/product-item/product-item.component.ts
    - src/app/features/shop/product-item/product-item.component.scss
    - src/app/features/shop/product-item/product-item.component.html
```
- `src/app/features/shop/product-item/product-item.component.ts`
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
- `update product-item.component.html`
```
@if (product) {
  <mat-card appearance="raised">
    <img src="{{  product.pictureUrl }}" alt="alt imge of {{ product.name }}" class="w-full h-48 object-cover" />
  </mat-card>
}
```
- ` update import @ shop.component.ts`
```
@Component({
  selector: 'app-shop',
  imports: [
    MatCard,
    ProductItemComponent // <-- import to shop.component.ts
],
```
- ` update product-item.component.ts`
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
- `updae product-item.component.html`
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



- `API/Program.cs`
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
- ` update client/src/app/core/services/shop.services.ts `

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
- ` update client/src/app/features/shop/shop.components.ts` 
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
- `second update client/src/app/features/shop/shop.components.ts` 
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

- ` create dialog cd client - ng g c features/shop/filters-dialog --skip-tests `

- ` client/src/app/features/shop/filters-dialog/filters-dialog.components.ts `
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

- ` update filters-dialog.components.html  `
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

- ` update shop.components.ts `
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
- ` update shop.component.html `
- ` google search: Material Symbols and Icons ` 
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
- `update filters-dialog.component.ts`
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
- `update filters-dialog.components.html`
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
- ` update shop.service.ts `
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
- ` update shop.components.ts`
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

- ` update shop.components.ts `
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
- ` update shop.components.html `
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

- ` update shop.service.ts `
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
- [Angular Paginator](https://material.angular.dev/components/paginator/overview)

- ` create client/src/app/shared/models/shopParams.ts `

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

- ` update client/src/app/features/shop/shop.components.ts `
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


- ` update client/src/app/features/shop/shop.components.html `

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

- ` update client/src/app/core/services/shop.services.ts `
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
- ` update client/src/app/core/services/shop.serives.ts ` 
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
- `update client/src/app/features/shop/shop.component.ts `
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
- `update client/src/app/features/shop/shop.component.html `
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
- `update client/src/app/features/shop/shop.component.ts `
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
- `update client/src/app/features/shop/shop.component.html `
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
- ` style default bg color of mat-paginator client/tailwind.config.ts `
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
- `Update shop.component.ts  `
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
- `Update shop.component.html `
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
- `Update shop.services.ts`
```
    //...if (shopParams.sort) { params = params.append('sort', shopParams.sort);  }

    if(shopParams.search) {
      params = params.append('search', shopParams.search);
    }

    //... params = params.append('pageSize', shopParams.pageSize);
```
- `update styles.scss`
```
.text-primary{
  color: #7d00fa;
}

button.match-input-height {
  height: var(--mat-form-field-container-height) !important;
}

```
- ` update shop.component.html`
```
// 'match-input-height' = aligns the input and button for search in UI
<button class="match-input-height" mat-stroked-button (click)="openFiltersDialog()">...
</button>

<button class="match-input-height" mat-stroked-button [matMenuTriggerFor]="sortMenu">...
</button>

```

- `!Issue/Solution for not displaying all items in ui `
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
- Angular Services
- Building the UI for the loop
- Material components
- Pagination
- Filtering, Sorting & Search
- Input properties

<hr>

### Section 10: Angular Routing

<hr>

######  : 86. Introduction

- Adding new feature components
- Setting up routes
- Nav links 
- This way to the shop!

###### 87. Creating components and routes

- ` cd client create 'ng g c features/home --skip-tests' ` 
- ` cd client create 'ng g c features/shop/product-detail --skip-tests' ` 

- ` client/src/app/app.routes.ts `
```
export const routes: Routes = [
    { path: '', component: HomeCompnent },
    { path: 'shop', component: ShopCompnent },
    { path: 'shop/:id', component: ProductDetailsCompnent },
    { path: '**', redirectTo: '', pathMatch: 'full' },
]
```
- ` tracing client/src/app/app.component.ts `
```
// 
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ShopComponent],
  templateUrl: './app.component.html', // right click 'go to defination' in vs code
  styleUrl: './app.component.scss'
})
```
- ` tracing client/src/app/app.component.ts `
```
<app-header></app-header>
<div class="container mt-6">
  <router-outlet></router-outlet> // <!-- update here -->
</div>
```
- ` client/src/app/app.config.ts `
```
// nothing to change here but its a good way to understand the structure for routes in angular
```
- for testing the routes
- https://localhost:4200/
- https://localhost:4200/shop
- https://localhost:4200/shop/none

###### 88. Setting up the links in the app

- ` update client/src/app/layout/header/header.component.html ` 
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
- ` update client/src/app/layout/header/header.component.ts ` 
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
- ` update client/src/app/layout/header/header.component.scss ` 
```
a {
    &.active{
        color: #7d00fa;
    }
}
```
-` client/src/app/features/shop/shop.component.html flickering issue `
```
@if(products) {
    //... insert all entire component code here and remove ? from products?.count
}
```
###### 89. Getting an individual product using Route params
- ` update shop.service.ts `
```
//... getProducts(shopParams: ShopParams){}

getProduct(id: number){
    return this.http.get<Product>(this.baseUrl + 'products/' + id);
}

//... getBrands(){... }
``` 
- ` update product-details.components.ts `
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
- ` connection with app.routes.ts from produc-details.component.ts `
```
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'shop', component: ShopComponent },
  { path: 'shop/:id', component: ProductDetailsComponent }, // its refering to this id
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
```

- ` update product-details.components.html `
```
@if(product){
    <h1 class="text-2xl">{{ product?.name }}</h1>
}
    <h1 class="text-2xl">{{ product?.name }}</h1> // product?. is called optional chaining

```
- ` update product-item.component.ts `
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
- ` update product-item.component.html `
```
@if (product) {
  <mat-card appearance="raised" routerLink="/shop/{{ prdouct.id }}" class="product-card"> //  update code
  </mat-card>
  //... 
}
```
- ` update product-item.component.scss `
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
- ` update product-details.component.ts `
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
- ` Update product-details.component.ts `
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
- ` update styles.scss `
```
//... button.match-input-height{...}

.mdc-notched=outline__notch{
    border-right-style: none !important;
}
```
###### 91. Summary
- Adding new feature components
- Setting up routes 
- Nav links
- Q: What about lazy loading?
    - A: Optimization comes at the end, not the beginning

<hr>

### Section 11: Client Side error handling and loading
<hr>

###### 92. Introduction
- `Error handling in Angular`
- `Http interceptors`
- `Adding toast notifications`
- `Adding loading indicators`
- `Goal`
```
Goal: 

To handle errors we receive from the API 
centrally and handled by the Http interceptor.

To understand how to troubleshoot API Errors

```
###### 93. Creating a test error component

- ` @ client create new component = ng g c features/test-erorr --skip-tests ` 

- `update app.routes.ts`
```
//...
import { TestErrorComponent } from './features/test-error/test-error.component';

export const routes: Route =[
    {...},
    {path: 'test-error', component: TestErrorComponent},
    {...},
]
```
- ` update header.component.html`
```
    <a routerLink="/test-error" routerLinkActive="active">Errors</a> //for testing purposes only
    <!-- <a routerLink="/contact">Contact</a>// old -->
```
- ` update test-components.ts` 
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
- ` refer to BuggyControllers.cs API `

- ` update test-error.component.html `
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

- `ng g c shared/components/not-found --skip-tests `
```
<div class="container mt-5 ">
    <h1 class="text-3xl"> Not found </h1>
</div>
```
- `ng g c shared/components/server-error --skip-tests `
```
<div class="container mt-5 ">
    <h1 class="text-3xl"> Internal server error </h1>
</div>
```

- ` update app.routes.ts `
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

- ` cd /client folder  'ng g interceptor core/interceptors/error --dry-run' `
- ` ng g interceptor core/interceptors/error --skip-tests`
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
- ` update app.config.ts `
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

- ` create new service 'ng g s core/services/snackbar --skip-tests'`
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
- ` update style.scss` 
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
- ` update error.interceptor.ts `
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
- ` update error.interceptor.ts `
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
- ` update test-error.components.ts`
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
- ` update test-error.components.html template `
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

- ` update error-interceptor.ts `
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

- ` update server-error-components.ts `
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
- ` update server-error.component.html template`
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

- ` update not-found-component.html `
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
- ` update not-found-component.ts `
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
- ` update not-found-component.scss`
```
.icon-display{
  transform: scale(3);
}
```

###### 100. Adding an HTTP Interceptor for loading
- ` create "ng g interceptor core/interceptors/loading --skip-tests" `
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
- ` app.config.ts`
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
- `create "ng g s core/services/busy --skip-tests " `
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

- ` update header.component.ts `
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
- ` update header.component.html `
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
- ` update header.component.html`
```
<header class="border-b shadow-md p-3 w-full fixed top-0 z-50 bg-white">
</header>

@if(busyService.loading){
  <mat-progress-bar mode="indeterminate" class="fixed top-20 z-50"></mat-progress-bar>
}

```
- ` update app.component.html`
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
- Question: Would we create an errors component in a 'real' app?
- Answer: Probably not, but it is helpful

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

- ` update docker-compose.yml `
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
- ` terminal cmd: cd root folder `
- ` docker compose down ` // delete only the current container and doesn't include the volumes
- ` docker compose up -d ` // restarts container and volumes 
- ` docker compose down `
- ` docker compose down -v ` // delete the volumes

###### 106. Using Redis with .Net

- ` install redis via nuget -> Infrastructure.csproject [-]`
- ` update API Program.cs `
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

- ` in API folder install ' dotnet add package StackExchange.Redis ' `
- ` update Program.cs`
```
//... using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
```
- ` update appsettings.Development.json`
```
"ConnectionStrings": {
    "DefaultConnection":"...",
    "Redis": "localhost" // update
}

```
- ` restart api 'dotnet watch' `

###### 107. Creating the Cart classes

- ` Redis is not relational database and for this project app use as a key value and store `

- ` create new class API/Core/Entities/ShoppingCart.cs  `
```
namespace Core.Entities;

public class ShoppingCart
{
    public required string Id { get; set; }
    public List<CartItem> Items {get; set;} = [];
}
```

- ` create new class API/Core/Entities/CartItem.cs  `
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

- ` create service @ Core/Interfaces/create ICartService.cs `
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

- `create service @ Infrastructure/Services/CartService.cs `
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
- ` Update Program.cs`
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
- ` API/Controllers/CartController.cs`
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

- At postman

```
Get Cart = {{url}}/api/cart?id=cart1

Update Cart = {{url}}/api/cart 
     - think of this as a client side storage and focus on productId and quantity because we are going to validate this in our API, for checking 'Get Cart'

Delete Cart = 

```
- ` add redis extension in VSCode - Redis by Dunn` 
- ` redis explorer - click Add button`

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

- Goal
```
To add the cart feature to the
angular app. 
To understand the usage of signals in Angular
```

###### 113. Creating the cart components

- ` cd client 'ng g s core/services/cart --skip-tests `
```

```
- ` 'ng g c features/cart --skip-tests `
```

```

- ` update app.routes.ts `
```
import { cartComponent } from './features/cart/cart.component';

export const routes: Routes =[
    //... 'shop/:id'
    {path: 'cart', component: CartComponent}
    //...
]
```
- ` update header.component.html `
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
- ` client/src/app/shared/models/cart.ts `
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
- ` install a utility packages that generate a random id `
- ` npm install nanoid `

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

- ` update cart.service.ts `
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
- ` ng g --help `
- ` ng g environments `
- ` create client/src/environment.development.ts `

```
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/'
};
```
- ` create client/src/environment.ts `
```
export const environment = {
  production: true,
  apiUrl: 'api/'
};
```


###### 116. Adding an item to the cart

- ` update cart.service.ts `
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
- ` update product-item.component.ts `
```
import { Component, inject, Input } from '@angular/core';
import { CartService } from '../../../core/services/cart.service';

export class ProductItemComponent {
  @Input() product?: Product;
  cartService = inject(CartService);
}

```
- ` update product-item.component.html `

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
- Issue: About after clicking 'Add to cart' button in shop page
- ` update product-item.component.html `
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
- ` to check go to browser tools `
- ` storage-> Local Storage ` 
- ` app initializer `
- ` update app.config.ts `
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
- `create new server 'ng g s core/services/init --skip-tests' => init.service.ts ` 
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
- ` update cart.service.ts `
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
- ` update app.config.ts`
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

- ` update index.html`
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

- ` update cart.service.ts`
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
- ` update header.component.ts`
```
import { CartService } from '../../core/services/cart.service';

//... @Component({..})

export class HeaderComponent{
  //... busyService
  cartService = inject(CartService);
}
```
- ` update header.component.html`
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

- ` update cart.component.ts `
```
import { Component, inject } from '@angular/core';
import { CartService } from '../../core/services/cart.service';

export class CartComponent{
   cartService = inject(CartService);
}
```
- ` update cart.component.html `
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
- ` ng g c features/cart/cart-item --skip-tests `

- ` update cart-item.component.ts`
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
- ` update cart-item.component.html`
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
- ` update cart.component.html`

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

- ` update cart.component.ts`
```
@Component({
  //...
  imports: [CartItemComponent],
  //...
})
```

###### 121. Creating the order summary component
- ` create 'ng g c shared/components/order-summary --skip-tests `

- ` update order-summary.component.html`
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
- ` update cart.component.html `
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

- ` update order-summary.component.html `
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

- ` update order-summary.component.ts `
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

- ` update cart.service.ts `
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
- ` update order-summary.component.ts`
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

- ` update order-summary.component.html`
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

- !!Bug - cart-item.component.html issue - visual issue - minor = status: !resolve
```
Discovered: bug @ cart-item.component.html visual issue on visual minus sign in cart should be red and plus sign should be green, my theory is the tailwind is not detected.
```

- ` update cart.service.ts `
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
- `update cart-item.component.ts `

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
- ` update cart-item.component.html`
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
- ` check or test in https://localhost:4200/cart `

###### 126. Adding the update cart functionality to the product details

- ` update product-details.component.ts `

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
- ` update product-details.component.html `
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
- ` To test you go to e.g https://localhost:4200/shop/1 `


