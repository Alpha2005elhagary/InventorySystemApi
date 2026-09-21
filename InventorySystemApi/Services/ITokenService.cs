using InventorySystemApi.Models;

namespace InventorySystemApi.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
