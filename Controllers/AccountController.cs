using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using news_project_mvc.Models;
using news_project_mvc.ViewModels;

namespace news_project_mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AdminLogin(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (passwordValid)
                    {
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (isAdmin)
                        {
                            _logger.LogInformation($"Admin user {user.Email} logged in successfully.");

                            var authClaims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id),
                                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new Claim(ClaimTypes.Role, "Admin"),
                                new Claim(ClaimTypes.Name, user.UserName)
                            };

                            var jwtKey = _configuration["Jwt:Key"];
                            var jwtIssuer = _configuration["Jwt:Issuer"];
                            var jwtAudience = _configuration["Jwt:Audience"];
                            var jwtDurationInMinutes = _configuration.GetValue<int>("Jwt:DurationInMinutes", 120);

                            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                            {
                                _logger.LogError("JWT Key, Issuer or Audience not configured in appsettings.json");
                                ModelState.AddModelError(string.Empty, "Lỗi cấu hình hệ thống. Vui lòng thử lại sau.");
                                return View(model);
                            }

                            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));                            var token = new JwtSecurityToken(
                                issuer: jwtIssuer,
                                audience: jwtAudience,
                                expires: DateTime.UtcNow.AddMinutes(jwtDurationInMinutes),
                                claims: authClaims,
                                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                            );

                            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                            
                            // Thêm JWT vào cookie
                            Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = Request.IsHttps,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddMinutes(jwtDurationInMinutes)
                            });
                            
                            // Sign in với ASP.NET Core Identity để cookie authentication hoạt động
                            await _signInManager.SignOutAsync(); // Logout trước để tránh xung đột
                            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                            _logger.LogInformation($"User {user.Email} logged in with identity.");

                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {
                            _logger.LogWarning($"User {user.Email} attempted to log in but is not in 'Admin' role.");
                            ModelState.AddModelError(string.Empty, "Tài khoản không có quyền Admin.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid password attempt for user {user.Email}.");
                        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    }
                }
                else
                {
                    _logger.LogWarning($"Login attempt for non-existent email: {model.Email}.");
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất khỏi ASP.NET Core Identity
            await _signInManager.SignOutAsync();

            // Xóa Cookie JWT
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict
                });
            }

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            _logger.LogInformation("User logged out completely.");

            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
        }
    }
}