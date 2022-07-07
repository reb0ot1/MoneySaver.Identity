using Microsoft.AspNetCore.Http;
using MoneySaver.System.Infrastructure;
using System.Security.Claims;

namespace MoneySaver.System.Services.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ClaimsPrincipal user;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.user = httpContextAccessor.HttpContext.User;
            if (user == null)
            {
                throw new InvalidOperationException("This request does not have authenticated user.");
            }

            this.UserId = this.user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public string UserId { get; }

        public bool IsAdministrator => this.user.IsAdministrator();
    }
}
