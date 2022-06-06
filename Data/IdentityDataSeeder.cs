using Microsoft.AspNetCore.Identity;
using MoneySaver.Identity.Data.Models;
using MoneySaver.System;
using MoneySaver.System.Services;

namespace MoneySaver.Identity.Data
{
    public class IdentityDataSeeder : IDataSeeder
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public IdentityDataSeeder(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void SeedData()
        {
            if (this.roleManager.Roles.Any())
            {
                return;
            }

            Task.Run(async () =>
            {
                var adminRole = new IdentityRole(Constants.AdministratorRoleName);
                await this.roleManager.CreateAsync(adminRole);

                var adminUser = new User
                {
                    UserName = "admin@moneysaver.eu",
                    Email = "admin@moneysaver.eu",
                    SecurityStamp = "RandomSecurityStamp"
                };

                await this.userManager.CreateAsync(adminUser, "adminpass12");

                await this.userManager.AddToRoleAsync(adminUser, Constants.AdministratorRoleName);
            })
            .GetAwaiter()
            .GetResult();

        }
    }
}
