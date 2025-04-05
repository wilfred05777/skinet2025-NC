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
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type)
    {
      // return a product list  
    return Ok(await repo.GetProductsAsync(brand,type));

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

    private bool ProductExists(int id)
    {
        return repo.ProductExists(id);
        // return context.Products.Any(x => x.Id == id);
    }
}
