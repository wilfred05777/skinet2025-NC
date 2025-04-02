using System;
namespace Infrastructure.Data;
using Core.Entities; // Ensure the Core project is referenced in the Infrastructure project
using Microsoft.EntityFrameworkCore;


public class StoreContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}


/*
refactoring to use constructor injection for DbContext
public class StoreContext : DbContext
{
    public StoreContext(DbContextOptions options) : base(options)
    {
    }

    protected StoreContext()
    {
    }
}
*/