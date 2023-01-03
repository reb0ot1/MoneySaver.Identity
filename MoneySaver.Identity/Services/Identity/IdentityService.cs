using Microsoft.AspNetCore.Identity;
using MoneySaver.Identity.Data.Models;
using MoneySaver.System.Services;
using MoneySaver.Identity.Models.Identity;

namespace MoneySaver.Identity.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private const string InvalidErrorMessage = "Invalid credentials.";

        private readonly UserManager<User> userManager;
        private readonly ITokenGeneratorService jwtTokenGenerator;
        private readonly ILogger<IdentityService> logger;

        public IdentityService(
            UserManager<User> userManager, 
            ITokenGeneratorService jwtTokenGenerator,
            ILogger<IdentityService> logger)
        {
            this.userManager = userManager;
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.logger = logger;
        }

        public async Task<Result> ChangePassword(string userId, ChangePasswordInputModel changePasswordInput)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return InvalidErrorMessage;
                }

                var identityResult = await this.userManager.ChangePasswordAsync(
                    user,
                    changePasswordInput.CurrentPassword,
                    changePasswordInput.NewPassword
                    );

                var errors = identityResult.Errors.Select(e => e.Description);

                return identityResult.Succeeded
                    ? Result.Success
                    : Result.Failure(errors);
            }
            catch (Exception ex)
            {
                var messageTemplate = "Something when wrong while trying to change password for user {0}";
                this.logger.LogError(ex, messageTemplate, userId);
                return string.Format(messageTemplate, userId);
            }
        }

        public async Task<Result<UserOutputModel>> Login(UserInputModel userInput)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(userInput.Email);

                if (user == null)
                {
                    return InvalidErrorMessage;
                }

                var passwordValid = await this.userManager.CheckPasswordAsync(user, userInput.Password);
                if (!passwordValid)
                {
                    return InvalidErrorMessage;
                }

                var roles = await this.userManager.GetRolesAsync(user);
                var token = this.jwtTokenGenerator.GenerateToken(user, roles);

                return new UserOutputModel(token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Something went wrong while login user {0}", userInput.Email);
                return "Something went wrong while trying to loggin.";
            }
        }

        public async Task<Result<User>> Register(UserInputModel userInput)
        {
            try
            {
                var user = new User
                {
                    Email = userInput.Email,
                    UserName = userInput.Email
                };

                var identityResult = await this.userManager.CreateAsync(user, userInput.Password);

                var errors = identityResult.Errors.Select(e => e.Description);

                return identityResult.Succeeded ?
                    Result<User>.SuccessWith(user) :
                    Result<User>.Failure(errors);
            }
            catch (Exception ex)
            {
                var messageTemplate = "Something when wrong while trying to register user with email {0}";
                this.logger.LogError(ex, messageTemplate, userInput.Email);
                return string.Format(messageTemplate, userInput.Email);
            }
            
        }


    }
}
