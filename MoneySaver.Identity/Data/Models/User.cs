using Microsoft.AspNetCore.Identity;
using MoneySaver.Identity.Enums;

namespace MoneySaver.Identity.Data.Models
{
    public class User : IdentityUser
    {
        public UserState State { get; set; }
    }
}
