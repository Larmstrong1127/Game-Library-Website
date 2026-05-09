using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CSC595_Week04.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CSC595_Week04.Models
{
    public class ProductDBContext : IdentityDbContext<ProductUser>
    {
        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed the library with realistic game data so the app looks complete on first run.
            // To apply schema changes (e.g. new Genre column), delete myProductData.db and restart.
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1,  Name = "The Legend of Zelda: Breath of the Wild", Genre = "Adventure",  Price = 59.99M },
                new Product { Id = 2,  Name = "God of War",                              Genre = "Action",     Price = 39.99M },
                new Product { Id = 3,  Name = "Red Dead Redemption 2",                  Genre = "Action",     Price = 49.99M },
                new Product { Id = 4,  Name = "The Witcher 3: Wild Hunt",               Genre = "RPG",        Price = 29.99M },
                new Product { Id = 5,  Name = "Elden Ring",                             Genre = "RPG",        Price = 59.99M },
                new Product { Id = 6,  Name = "Minecraft",                              Genre = "Simulation", Price = 26.99M },
                new Product { Id = 7,  Name = "Stardew Valley",                        Genre = "Simulation", Price = 14.99M },
                new Product { Id = 8,  Name = "Hollow Knight",                         Genre = "Platform",   Price = 14.99M },
                new Product { Id = 9,  Name = "Call of Duty: Modern Warfare II",       Genre = "Shooter",    Price = 59.99M },
                new Product { Id = 10, Name = "FIFA 24",                               Genre = "Sports",     Price = 69.99M }
            );
        }
    }
}
