using Microsoft.AspNetCore.Identity;

namespace InventorySystemApi.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Role { get; set; }
}
