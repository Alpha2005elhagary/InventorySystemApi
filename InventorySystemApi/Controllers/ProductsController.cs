using InventorySystemApi.Data;
using InventorySystemApi.DTOs;
using InventorySystemApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderBy(p => p.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinStock = p.MinStock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                IsLowStock = p.Quantity <= p.MinStock && p.Quantity > 0,
                IsOutOfStock = p.Quantity <= 0
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] bool? lowStock = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || (p.Description != null && p.Description.ToLower().Contains(s)));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(p => p.SupplierId == supplierId.Value);
        }

        if (lowStock.HasValue && lowStock.Value)
        {
            query = query.Where(p => p.Quantity <= p.MinStock);
        }

        var totalItems = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinStock = p.MinStock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                IsLowStock = p.Quantity <= p.MinStock && p.Quantity > 0,
                IsOutOfStock = p.Quantity <= 0
            })
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", totalItems.ToString());
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var p = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound(new { message = "Product not found." });

        return Ok(new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Quantity = p.Quantity,
            MinStock = p.MinStock,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : string.Empty,
            SupplierId = p.SupplierId,
            SupplierName = p.Supplier != null ? p.Supplier.Name : null,
            IsLowStock = p.Quantity <= p.MinStock && p.Quantity > 0,
            IsOutOfStock = p.Quantity <= 0
        });
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
    {
        var lowStockProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Quantity <= p.MinStock)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinStock = p.MinStock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                IsLowStock = p.Quantity <= p.MinStock && p.Quantity > 0,
                IsOutOfStock = p.Quantity <= 0
            })
            .ToListAsync();

        return Ok(lowStockProducts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "Invalid CategoryId. Category does not exist." });
        }

        if (dto.SupplierId.HasValue && dto.SupplierId.Value > 0)
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value);
            if (!supplierExists)
            {
                return BadRequest(new { message = "Invalid SupplierId. Supplier does not exist." });
            }
        }

        string? imageUrl = null;
        if (dto.Image != null && dto.Image.Length > 0)
        {
            imageUrl = await SaveProductImageAsync(dto.Image);
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            MinStock = dto.MinStock,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId > 0 ? dto.SupplierId : null,
            ImageUrl = imageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // If product started with initial quantity > 0, record initial stock transaction
        if (product.Quantity > 0)
        {
            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = "Stock In",
                Quantity = product.Quantity,
                Date = DateTime.UtcNow,
                Notes = "Initial inventory stock"
            });
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            MinStock = product.MinStock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = (await _context.Categories.FindAsync(product.CategoryId))?.Name ?? string.Empty,
            SupplierId = product.SupplierId,
            SupplierName = product.SupplierId.HasValue ? (await _context.Suppliers.FindAsync(product.SupplierId.Value))?.Name : null,
            IsLowStock = product.Quantity <= product.MinStock && product.Quantity > 0,
            IsOutOfStock = product.Quantity <= 0
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "Invalid CategoryId." });

        if (dto.SupplierId.HasValue && dto.SupplierId.Value > 0)
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value);
            if (!supplierExists) return BadRequest(new { message = "Invalid SupplierId." });
        }

        if (dto.Image != null && dto.Image.Length > 0)
        {
            // Delete previous image if exists
            DeleteProductImage(product.ImageUrl);
            product.ImageUrl = await SaveProductImageAsync(dto.Image);
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.MinStock = dto.MinStock;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId > 0 ? dto.SupplierId : null;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        DeleteProductImage(product.ImageUrl);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string> SaveProductImageAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "products");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/products/{fileName}";
    }

    private void DeleteProductImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;

        try
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch
        {
            // Ignore file deletion errors
        }
    }
}
