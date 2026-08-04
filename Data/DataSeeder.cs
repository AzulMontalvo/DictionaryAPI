using DictionaryAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace DictionaryAPI.Data
{
    public class DataSeeder
    {
        public static async Task SeedUsersAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            //Seed de roles
            string[] roleNames = { "Admin", "User" };
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
                var newAdmin = new AppUser  
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                IdentityResult createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            string userEmail = "user@user.com";
            string userPassword = "Password123!";


            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                var newUser = new AppUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                IdentityResult createUserResult = await userManager.CreateAsync(newUser, userPassword);
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "User");
                }
            }

            //Seed de catálogo de statusId
            if (!context.SubmissionStatus.Any())
            {
                context.SubmissionStatus.AddRange(
                    new Models.Entities.SubmissionStatus { Name = "Pending", Description = "The submission is awaiting review." },
                    new Models.Entities.SubmissionStatus { Name = "Approved", Description = "The submission has been approved and is now part of the dictionary." },
                    new Models.Entities.SubmissionStatus { Name = "Rejected", Description = "The submission has been rejected and will not be added to the dictionary." }
                );
                try
                {
                    await context.SaveChangesAsync();
                } catch ( Exception ex )
                {
                    Console.WriteLine($"Error seeding submission status: {ex.Message}");
                }
            }
        }
    }
}
