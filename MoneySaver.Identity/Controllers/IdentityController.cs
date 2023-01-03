using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneySaver.System.Services.Identity;
using MoneySaver.Identity.Models.Identity;
using MoneySaver.Identity.Services.Identity;

namespace MoneySaver.Identity.Controllers
{
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService identityService;
        private readonly ICurrentUserService currentUser;

        public IdentityController(
            IIdentityService identityService,
            ICurrentUserService currentUserService
            )
        {
            this.identityService = identityService;
            this.currentUser = currentUserService;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register(UserInputModel input)
        {
            var result = await this.identityService.Register(input);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return await Login(input);
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] UserInputModel input)
        {
            var result = await this.identityService.Login(input);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPut]
        [Authorize]
        [Route(nameof(ChangePassword))]
        public async Task<IActionResult> ChangePassword(ChangePasswordInputModel input)
        { 
            var result = await this.identityService.ChangePassword(this.currentUser.UserId, new ChangePasswordInputModel
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
