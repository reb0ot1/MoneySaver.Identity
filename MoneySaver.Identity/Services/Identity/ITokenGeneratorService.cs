using MoneySaver.Identity.Data.Models;

namespace MoneySaver.Identity.Services.Identity
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(User user, IEnumerable<string> roles = null);
        Task<bool> ValidateTokenAsync(string token);
    }
}
