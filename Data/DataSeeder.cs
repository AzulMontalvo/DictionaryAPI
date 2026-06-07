using Microsoft.AspNetCore.Identity;

namespace DictionaryAPI.Data
{
    public class DataSeeder
    {
        public static async Task SeedUsersAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            //Seed de roles
            string[] roleNames = { "Administrador", "Usuario" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }


            //Seed de users
            string adminEmail = "admin@admin.com";
            string adminPassword = "Password123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                IdentityResult createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Administrador");
                }
            }

            string userEmail = "user@user.com";
            string userPassword = "Password123!";


            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                IdentityResult createUserResult = await userManager.CreateAsync(newUser, userPassword);
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Usuario");
                }
            }
        }
    }
}
