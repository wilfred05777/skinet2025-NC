using Core.Entities;
using Infrastructure.Config;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// namespace Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
    }
}