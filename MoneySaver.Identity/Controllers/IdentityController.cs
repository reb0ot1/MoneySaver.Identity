using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneySaver.System.Services.Identity;
using MoneySaver.Identity.Models.Identity;
using MoneySaver.Identity.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MoneySaver.Identity.Controllers
{
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUser;
        private readonly ITokenGeneratorService _tokenGeneratorService;

        public IdentityController(
            IIdentityService identityService,
            ICurrentUserService currentUserService,
            ITokenGeneratorService tokenGeneratorService
            )
        {
            this._identityService = identityService;
            this._currentUser = currentUserService;
            this._tokenGeneratorService = tokenGeneratorService;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] UserRegistrationModel input)
        {
            var result = await this._identityService.RegisterAsync(input);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] UserInputModel input)
        {
            var result = await this._identityService.LoginAsync(input);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }
        
        [HttpPost]
        [Route("validateToken")]
        public async Task<IActionResult> ValidateToken()
        {
            var token = this.HttpContext.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            if (token.Contains(JwtBearerDefaults.AuthenticationScheme))
            {
                token = token.Replace($"{JwtBearerDefaults.AuthenticationScheme} ", "");
            }

            var isValid = await this._tokenGeneratorService.ValidateTokenAsync(token);

            if (isValid)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPut]
        [Authorize]
        [Route(nameof(ChangePassword))]
        public async Task<IActionResult> ChangePassword(ChangePasswordInputModel input)
        { 
            var result = await this._identityService.ChangePasswordAsync(this._currentUser.UserId, new ChangePasswordInputModel
            {
                CurrentPassword = input.CurrentPassword,
                NewPassword = input.NewPassword
            });

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return this.Ok(result);
        }
    }
}
