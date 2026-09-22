using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventorySystemApi.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 10000000.00)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 100000)]
    public int Quantity { get; set; }

    [Range(1, 1000)]
    public int MinStock { get; set; } = 5;

    [Required]
    public int CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public IFormFile? Image { get; set; }
}

public class UpdateProductDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 10000000.00)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 100000)]
    public int Quantity { get; set; }

    [Range(1, 1000)]
    public int MinStock { get; set; } = 5;

    [Required]
    public int CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public IFormFile? Image { get; set; }
}
