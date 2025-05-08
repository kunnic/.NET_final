using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Data;
using news_project_mvc.Models;

namespace news_project_mvc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            //builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireDigit = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //});

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Cố gắng lấy token từ cookie "AuthToken"
                        context.Token = context.Request.Cookies["AuthToken"];
                        if (!string.IsNullOrEmpty(context.Token))
                        {
                            // Bạn có thể log ở đây để biết token có được đọc không
                            // Ví dụ: var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            // logger.LogInformation("Token found in cookie 'AuthToken'.");
                            Console.WriteLine("Token found in cookie 'AuthToken'. Attempting to use.");
                        }
                        else
                        {
                            Console.WriteLine("Token NOT found in cookie 'AuthToken'.");
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log lỗi chi tiết để biết tại sao token validation thất bại
                        Console.WriteLine("Authentication failed: " + context.Exception.ToString());
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Token đã được validate thành công, Principal đã được tạo
                        Console.WriteLine("Token validated for: " + context.Principal.Identity.Name);
                        // Bạn có thể kiểm tra các claim ở đây:
                        // var claims = context.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                        // Console.WriteLine("Claims: " + string.Join(", ", claims));
                        return Task.CompletedTask;
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
