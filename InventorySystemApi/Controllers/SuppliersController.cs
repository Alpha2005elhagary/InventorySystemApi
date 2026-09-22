using InventorySystemApi.Data;
using InventorySystemApi.DTOs;
using InventorySystemApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                ProductsCount = s.Products.Count
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .Where(s => s.Id == id)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                ProductsCount = s.Products.Count
            })
            .FirstOrDefaultAsync();

        if (supplier == null) return NotFound(new { message = "Supplier not found." });

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var supplier = new Supplier
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            ProductsCount = 0
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound(new { message = "Supplier not found." });

        supplier.Name = dto.Name;
        supplier.Email = dto.Email;
        supplier.Phone = dto.Phone;
        supplier.Address = dto.Address;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound(new { message = "Supplier not found." });

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
