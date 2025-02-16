using MSSystem = MoneySaver.System.Services;
using MoneySaver.Identity.Models.Identity;

namespace MoneySaver.Identity.Services.Identity
{
    public interface IIdentityService
    {
        Task<MSSystem.Result<bool>> RegisterAsync(UserRegistrationModel userInput);

        Task<MSSystem.Result<UserOutputModel>> LoginAsync(UserInputModel userInput);

        Task<MSSystem.Result> ChangePasswordAsync(string userId, ChangePasswordInputModel changePasswordInput);
    }
}
