using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // Cần cho GetRequiredService
using Microsoft.Extensions.Logging; // Cần cho ILogger

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

            // Kiểm tra xem user đã tồn tại bằng UserName chưa (an toàn hơn)
            var adminUser = await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                logger.LogInformation("Admin user '{AdminUserName}' not found, attempting to create.", adminUserName);
                var newAdminUser = new IdentityUser
                {
                    UserName = adminUserName, // Quan trọng: Đặt UserName
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Mật khẩu nên mạnh hơn, hoặc bạn cần cấu hình Identity để chấp nhận mật khẩu yếu hơn trong quá trình dev
                var result = await userManager.CreateAsync(newAdminUser, "Admin@12345"); // THỬ MỘT MẬT KHẨU MẠNH HƠN

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
                    // GHI LOG CHI TIẾT LỖI TẠO USER
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
                // Kiểm tra xem user đã có trong role Admin chưa
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