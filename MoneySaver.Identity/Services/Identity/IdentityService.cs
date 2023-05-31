using Microsoft.AspNetCore.Identity;
using MoneySaver.Identity.Data.Models;
using MSSystem = MoneySaver.System.Services;
using MoneySaver.Identity.Models.Identity;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MoneySaver.Identity.Models.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MoneySaver.Identity.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private const string InvalidErrorMessage = "Invalid credentials.";

        private readonly UserManager<User> userManager;
        private readonly ITokenGeneratorService jwtTokenGenerator;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<IdentityService> logger;
        private readonly UrlRoutesConfiguration routesConfig;

        public IdentityService(
            UserManager<User> userManager, 
            ITokenGeneratorService jwtTokenGenerator,
            IHttpClientFactory httpClientFactory,
            ILogger<IdentityService> logger,
            IOptions<UrlRoutesConfiguration> urlRoutesConfig)
        {
            this.userManager = userManager;
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.routesConfig = urlRoutesConfig.Value;
        }

        public async Task<MSSystem.Result> ChangePassword(string userId, ChangePasswordInputModel changePasswordInput)
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
                    ? MSSystem.Result.Success
                    : MSSystem.Result.Failure(errors);
            }
            catch (Exception ex)
            {
                var messageTemplate = "Something when wrong while trying to change password for user {0}";
                this.logger.LogError(ex, messageTemplate, userId);
                return string.Format(messageTemplate, userId);
            }
        }

        public async Task<MSSystem.Result<UserOutputModel>> Login(UserInputModel userInput)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(userInput.Email);

                if (user == null)
                {
                    return InvalidErrorMessage;
                }

                if (user.State == Enums.UserState.Inactive)
                {
                    return "User is not active.";
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

        public async Task<MSSystem.Result<bool>> Register(UserRegistrationModel userInput)
        {
            try
            {
                if (userInput.Password != userInput.ConfirmPassword)
                {
                    return "The confirmed password is not the same.";
                }

                var userExists = await this.userManager.Users.AnyAsync(user => user.Email == userInput.Email);
                if (userExists)
                {
                    return "User with this email already exists.";
                }

                var user = new User
                {
                    Email = userInput.Email,
                    UserName = userInput.Email,
                };

                var identityResult = await this.userManager.CreateAsync(user, userInput.Password);
                var errors = identityResult.Errors.Select(e => e.Description);

                if (identityResult.Succeeded)
                {
                    user = await this.userManager.FindByEmailAsync(userInput.Email);
                    var roles = await this.userManager.GetRolesAsync(user);
                    var userToken = this.jwtTokenGenerator.GenerateToken(user, roles);

                    //TODO: Create system token which will take care for requests as the below
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{this.routesConfig.DataApiUrl}/api/appConfiguration/setuserconfig");
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

                    var httpClient = this.httpClientFactory.CreateClient();
                    var httpRequest = await httpClient.SendAsync(requestMessage);

                    if (!httpRequest.IsSuccessStatusCode)
                    { 
                        this.logger.LogError("Configuration was not created for user with id [{0}]", user.Id);
                    }

                    return MSSystem.Result<bool>.SuccessWith(true);
                }

                return MSSystem.Result<bool>.Failure(errors);
            }
            catch (Exception ex)
            {
                var messageTemplate = "Something when wrong while trying to register user with email {0}";
                this.logger.LogError(ex, messageTemplate, userInput.Email);
                return string.Format(messageTemplate, userInput.Email);
            }
            
        }

        public async Task<MSSystem.Result<bool>> Activate(string email)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    return "User with this email does not exist.";
                }

                user.State = Enums.UserState.Active;

                var updateStateResult = await this.userManager.UpdateAsync(user);
                var errors = updateStateResult.Errors.Select(e => e.Description);

                if (updateStateResult.Succeeded)
                {
                    var userConfigurationCreated = await this.CreateUserConfigurationAsync(user);

                    if (!userConfigurationCreated.Succeeded)
                    {
                        throw new Exception();
                    }

                    return MSSystem.Result<bool>.SuccessWith(true);
                }

                return MSSystem.Result<bool>.Failure(errors);
            }
            catch (Exception ex)
            {
                var messageTemplate = "Something when wrong while trying to activate user with email {0}";
                this.logger.LogError(ex, messageTemplate, email);
                return string.Format(messageTemplate, email);
            }
        }

        private async Task<MSSystem.Result<bool>> CreateUserConfigurationAsync(User user)
        {
            var roles = await this.userManager.GetRolesAsync(user);
            var userToken = this.jwtTokenGenerator.GenerateToken(user, roles);

            //TODO: Create system token which will take care for requests as the below
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{this.routesConfig.DataApiUrl}/api/appConfiguration/setuserconfig");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var httpClient = this.httpClientFactory.CreateClient();
            var httpRequest = await httpClient.SendAsync(requestMessage);

            if (!httpRequest.IsSuccessStatusCode)
            {
                this.logger.LogError("Configuration was not created for user with id [{0}]", user.Id);
                return false;
            }

            return true;
        }

    }
}
