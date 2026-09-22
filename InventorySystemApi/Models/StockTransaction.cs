using System.ComponentModel.DataAnnotations;

namespace InventorySystemApi.Models;

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [StringLength(250)]
    public string? Notes { get; set; }
}
