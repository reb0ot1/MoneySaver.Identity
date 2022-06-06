using MoneySaver.Identity.Data.Models;
using MoneySaver.System.Services;
using MoneySaver.Identity.Models.Identity;

namespace MoneySaver.Identity.Services.Identity
{
    public interface IIdentityService
    {
        Task<Result<User>> Register(UserInputModel userInput);

        Task<Result<UserOutputModel>> Login(UserInputModel userInput);

        Task<Result> ChangePassword(string userId, ChangePasswordInputModel changePasswordInput);
    }
}
