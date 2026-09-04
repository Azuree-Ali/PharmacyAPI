using PharmacyAPI.Models;

namespace PharmacyAPI.JwtFeatures
{
    public interface IJwtHandler
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    }
}
