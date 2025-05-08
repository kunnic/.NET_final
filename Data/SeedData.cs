using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace news_project_mvc.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            string[] roleNames = { "Admin", "Editor", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                    }
                    else
                    {
                        logger.LogError("Error creating role '{RoleName}'.", roleName);
                        foreach (var error in roleResult.Errors)
                        {
                            logger.LogError("- Code: {ErrorCode}, Description: {ErrorDescription}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Role '{RoleName}' already exists.", roleName);
                }
            }

            var adminEmail = "admin@abc.com";
            var adminUserName = "admin";

            var adminUser = await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                logger.LogInformation("Admin user '{AdminUserName}' not found, attempting to create.", adminUserName);
                var newAdminUser = new IdentityUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdminUser, "Admin@12345");

                if (result.Succeeded)
                {
                    logger.LogInformation("User '{AdminUserName}' created successfully.", adminUserName);
                    var addToRoleResult = await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("User '{AdminUserName}' added to 'Admin' role successfully.", adminUserName);
                    }
                    else
                    {
                        logger.LogError("Error adding user '{AdminUserName}' to 'Admin' role.", adminUserName);
                        foreach (var error in addToRoleResult.Errors)
                        {
                            logger.LogError("- Code: {ErrorCode}, Description: {ErrorDescription}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    logger.LogError("Failed to create user '{AdminUserName}'.", adminUserName);
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("- Code: {ErrorCode}, Description: {ErrorDescription}", error.Code, error.Description);
                    }
                }
            }
            else
            {
                logger.LogInformation("Admin user '{AdminUserName}' already exists.", adminUserName);
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    logger.LogInformation("User '{AdminUserName}' exists but is not in 'Admin' role. Attempting to add.", adminUserName);
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("User '{AdminUserName}' added to 'Admin' role successfully.", adminUserName);
                    }
                    else
                    {
                        logger.LogError("Error adding existing user '{AdminUserName}' to 'Admin' role.", adminUserName);
                        foreach (var error in addToRoleResult.Errors)
                        {
                            logger.LogError("- Code: {ErrorCode}, Description: {ErrorDescription}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    logger.LogInformation("User '{AdminUserName}' already in 'Admin' role.", adminUserName);
                }
            }
        }
    }
}
