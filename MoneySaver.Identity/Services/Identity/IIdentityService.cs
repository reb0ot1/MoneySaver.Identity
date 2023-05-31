using MSSystem = MoneySaver.System.Services;
using MoneySaver.Identity.Models.Identity;

namespace MoneySaver.Identity.Services.Identity
{
    public interface IIdentityService
    {
        Task<MSSystem.Result<bool>> Register(UserRegistrationModel userInput);

        Task<MSSystem.Result<UserOutputModel>> Login(UserInputModel userInput);

        Task<MSSystem.Result> ChangePassword(string userId, ChangePasswordInputModel changePasswordInput);
    }
}
