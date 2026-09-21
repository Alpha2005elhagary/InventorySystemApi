using InventorySystemApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemApi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Admin and Employee Users
        var adminEmail = "admin@inventory.local";
        var defaultAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (defaultAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                Role = "Admin",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var employeeEmail = "employee@inventory.local";
        var defaultEmployee = await userManager.FindByEmailAsync(employeeEmail);
        if (defaultEmployee == null)
        {
            var empUser = new ApplicationUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                FullName = "Staff Employee",
                Role = "Employee",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(empUser, "Employee123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(empUser, "Employee");
            }
        }

        // 3. Seed Inventory Data if empty
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new Category[]
        {
            new() { Name = "Phones", Description = "Smartphones, flagship and mid-range devices" },
            new() { Name = "Laptops", Description = "Ultrabooks, gaming and productivity laptops" },
            new() { Name = "Accessories", Description = "Cables, chargers, earphones and peripherals" },
            new() { Name = "Tablets", Description = "Tablets, iPads and styluses" },
            new() { Name = "Monitors", Description = "4K displays, gaming and curved screens" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var suppliers = new Supplier[]
        {
            new() { Name = "ABC Electronics", Phone = "01012345678", Email = "contact@abcelectronics.com", Address = "Cairo, Egypt" },
            new() { Name = "Tech Supplies Co", Phone = "01187654321", Email = "sales@techsupplies.com", Address = "Giza, Egypt" },
            new() { Name = "Delta Distributions", Phone = "01233445566", Email = "info@deltadist.com", Address = "Alexandria, Egypt" }
        };
        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        var products = new Product[]
        {
            new() { Name = "iPhone 15", Price = 40000, Quantity = 10, MinStock = 5, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
            new() { Name = "Samsung A55", Price = 18000, Quantity = 20, MinStock = 8, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
            new() { Name = "AirPods", Price = 7000, Quantity = 5, MinStock = 6, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
            new() { Name = "Logitech Mouse", Price = 1200, Quantity = 30, MinStock = 10, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
            new() { Name = "Dell UltraSharp 27\" 4K", Price = 16500, Quantity = 2, MinStock = 4, CategoryId = categories[4].Id, SupplierId = suppliers[2].Id },
            new() { Name = "MacBook Air M3", Price = 54000, Quantity = 0, MinStock = 3, CategoryId = categories[1].Id, SupplierId = suppliers[0].Id }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var transactions = new StockTransaction[]
        {
            new() { ProductId = products[0].Id, Type = "Stock Out", Quantity = 2, Date = DateTime.UtcNow.AddDays(-2), Notes = "Retail Sale" },
            new() { ProductId = products[1].Id, Type = "Stock In", Quantity = 10, Date = DateTime.UtcNow.AddDays(-1), Notes = "Warehouse restock shipment" },
            new() { ProductId = products[2].Id, Type = "Stock Out", Quantity = 1, Date = DateTime.UtcNow.AddHours(-5), Notes = "Customer invoice #104" },
            new() { ProductId = products[3].Id, Type = "Stock In", Quantity = 15, Date = DateTime.UtcNow.AddHours(-2), Notes = "Supplier delivery batch A" }
        };
        context.StockTransactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
}
