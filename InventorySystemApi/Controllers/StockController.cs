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
public class StockController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StockController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetTransactions(
        [FromQuery] int? productId = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 100)
    {
        var query = _context.StockTransactions
            .Include(t => t.Product)
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        var list = await query
            .OrderByDescending(t => t.Date)
            .Take(limit)
            .Select(t => new StockTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product != null ? t.Product.Name : string.Empty,
                Quantity = t.Quantity,
                Type = t.Type,
                Date = t.Date,
                Notes = t.Notes
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("in")]
    public async Task<ActionResult<StockTransactionDto>> StockIn([FromBody] StockMovementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Product not found." });

        product.Quantity += dto.Quantity;

        var transaction = new StockTransaction
        {
            ProductId = product.Id,
            Type = "Stock In",
            Quantity = dto.Quantity,
            Date = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? "Stock In receipt" : dto.Notes.Trim()
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new StockTransactionDto
        {
            Id = transaction.Id,
            ProductId = transaction.ProductId,
            ProductName = product.Name,
            Quantity = transaction.Quantity,
            Type = transaction.Type,
            Date = transaction.Date,
            Notes = transaction.Notes
        });
    }

    [HttpPost("out")]
    public async Task<ActionResult<StockTransactionDto>> StockOut([FromBody] StockMovementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Product not found." });

        if (product.Quantity < dto.Quantity)
        {
            return BadRequest(new
            {
                message = $"Insufficient stock for {product.Name}. Available: {product.Quantity}, requested: {dto.Quantity}."
            });
        }

        product.Quantity -= dto.Quantity;

        var transaction = new StockTransaction
        {
            ProductId = product.Id,
            Type = "Stock Out",
            Quantity = dto.Quantity,
            Date = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? "Stock Out dispatch" : dto.Notes.Trim()
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new StockTransactionDto
        {
            Id = transaction.Id,
            ProductId = transaction.ProductId,
            ProductName = product.Name,
            Quantity = transaction.Quantity,
            Type = transaction.Type,
            Date = transaction.Date,
            Notes = transaction.Notes
        });
    }
}
