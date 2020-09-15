using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCloudStorage
{
    public static class SetupSecurity
    {
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("NormalUser").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "NormalUser";
                IdentityResult roleResult = roleManager.CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("PremiumUser").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "PremiumUser";
                IdentityResult roleResult = roleManager.CreateAsync(role).Result;
            }
        }
    }
}
