using Microsoft.AspNetCore.Identity;
using FitnessCenterWebApplication.Models.Entities;
using FitnessCenterWebApplication.Data;

namespace FitnessCenterWebApplication.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                //Ensure the database is ready
                logger.LogInformation("Ensuring the database is created");
                await context.Database.EnsureCreatedAsync();

                //Add roles
                logger.LogInformation("Seeding roles");
                await AddRolesAsync(roleManager, "Admin");
                await AddRolesAsync(roleManager, "User");

                //Add Admin users
                logger.LogInformation("Seeding admin user");
                var adminEmail = "b231210050@sakarya.edu.tr";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Assigning admin role to the admin user");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static async Task AddRolesAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
