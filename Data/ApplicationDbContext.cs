using InventorySystemApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Category Entity Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(300);
        });

        // 2. Supplier Entity Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Phone).HasMaxLength(30);
            entity.Property(s => s.Email).HasMaxLength(100);
            entity.Property(s => s.Address).HasMaxLength(250);
        });

        // 3. Product Entity Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Quantity).IsRequired();
            entity.Property(p => p.MinStock).IsRequired().HasDefaultValue(5);

            // 1 : M Category -> Product
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // 1 : M Supplier -> Product
            entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 4. StockTransaction Entity Configuration
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Type).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Quantity).IsRequired();
            entity.Property(t => t.Date).IsRequired();
            entity.Property(t => t.Notes).HasMaxLength(250);

            // 1 : M Product -> StockTransaction
            entity.HasOne(t => t.Product)
                  .WithMany(p => p.StockTransactions)
                  .HasForeignKey(t => t.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
