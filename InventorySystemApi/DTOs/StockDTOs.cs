using System.ComponentModel.DataAnnotations;

namespace InventorySystemApi.DTOs;

public class StockTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}

public class StockMovementDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}
