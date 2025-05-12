using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Data;
using news_project_mvc.Models;
using System.Security.Claims;
using NLog;
using NLog.Web;

namespace news_project_mvc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup NLog
            var logger = NLog.LogManager.Setup()
                .LoadConfigurationFromAppSettings()
                .GetCurrentClassLogger();
            
            try
            {
                logger.Debug("Application Starting Up");
                var builder = WebApplication.CreateBuilder(args);
                
                // Add NLog services
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));            // Đăng ký ASP.NET Core Identity với đầy đủ các dịch vụ
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                
                // Cấu hình thêm chính sách cookies
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();  // Thêm UI mặc định            // Cấu hình xác thực đa cấp: Identity + JWT
            builder.Services.AddAuthentication(options =>
            {
                // Đặt Identity làm scheme mặc định
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                
                // Thêm scheme dành riêng cho JWT để sử dụng khi cần
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            // Thêm JWT Bearer với schema name rõ ràng
            .AddJwtBearer("JWT", options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                
                // Đảm bảo các giá trị từ appsettings.json được thiết lập đúng
                var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key must be configured in appsettings.json");
                var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer must be configured in appsettings.json");
                var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience must be configured in appsettings.json");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero // Loại bỏ khoảng trễ mặc định 5p
                };                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Cố gắng lấy token từ cookie "AuthToken"
                        context.Token = context.Request.Cookies["AuthToken"];
                        if (!string.IsNullOrEmpty(context.Token))
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                            if (logger != null)
                            {
                                logger.LogInformation("JWT Token found in cookie 'AuthToken'.");
                            }
                            Console.WriteLine("Token found in cookie 'AuthToken'. Attempting to use.");
                        }
                        else
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                            if (logger != null)
                            {
                                logger.LogWarning("JWT Token NOT found in cookie 'AuthToken'.");
                            }
                            Console.WriteLine("Token NOT found in cookie 'AuthToken'.");
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log lỗi chi tiết để biết tại sao token validation thất bại
                        var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                        if (logger != null)
                        {
                            logger.LogError(context.Exception, "JWT Authentication failed.");
                        }
                        Console.WriteLine("Authentication failed: " + context.Exception.ToString());
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        // Token đã được validate thành công, Principal đã được tạo
                        var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                        if (logger != null && context.Principal?.Identity?.Name != null)
                        {
                            logger.LogInformation("JWT Token validated for: {UserName}", context.Principal.Identity.Name);
                        }
                        Console.WriteLine("Token validated for: " + context.Principal?.Identity?.Name);
                        
                        // Đồng bộ thông tin người dùng với ASP.NET Core Identity nếu chưa đăng nhập
                        try {
                            var userManager = context.HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                            var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<IdentityUser>>();
                            
                            if (userManager != null && signInManager != null && 
                                context.Principal?.Identity?.Name != null && 
                                !signInManager.IsSignedIn(context.HttpContext.User))
                            {
                                // Tìm user theo email và đăng nhập với Identity
                                var user = await userManager.FindByEmailAsync(context.Principal.Identity.Name);
                                if (user != null)
                                {                                    // Sử dụng SignInManager thay vì gọi SignInAsync trực tiếp
                                    await signInManager.SignInAsync(user, isPersistent: false);
                                    
                                    if (logger != null)
                                    {
                                        logger.LogInformation("User authenticated with JWT and synchronized with Identity");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            if (logger != null)
                            {
                                logger.LogError(ex, "Error synchronizing JWT authentication with Identity");
                            }
                        }
                    }
                    // Bạn có thể thêm OnChallenge nếu cần theo dõi khi nào challenge được gọi
                    // OnChallenge = context =>
                    // {
                    //     Console.WriteLine("OnChallenge invoked: " + context.ErrorDescription);
                    //     return Task.CompletedTask;
                    // }
                };
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Cấu hình route cho Area Admin
            app.MapControllerRoute(
                name: "AdminArea", // Đặt tên cho route Area
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"); // {area:exists} đảm bảo chỉ khớp khi có Area

            // Route mặc định phải đặt sau route Area
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await SeedData.Initialize(services);
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            await app.RunAsync();
        }
    }
}
