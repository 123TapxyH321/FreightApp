using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using FreightApp.DataAccess;

namespace FreightApp.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly FreightAppDbContext _db;

        public LoginModel(FreightAppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _db.Userlist.FirstOrDefault(u => u.Login == Username && u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль.";
                return Page();
            }

            if (!user.UsStatus)
            {
                ErrorMessage = "Профиль ещё не подтверждён администратором.";
                return Page();
            }

            if (!user.UsEnabled)
            {
                ErrorMessage = "Профиль заблокирован.";
                return Page();
            }

            var role = _db.UserRole.FirstOrDefault(r => r.id == user.IdRole)?.roleName ?? "Unknown";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.Session.SetString("FullName", user.UserName);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("Login", user.Login);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostQuickLoginAsync(string role)
        {
            switch (role)
            {
                case "admin":
                    Username = "admin";
                    Password = "admin123";
                    break;
                case "client":
                    Username = "client1";
                    Password = "client123";
                    break;
                case "dispatcher":
                    Username = "dispatcher1";
                    Password = "disp123";
                    break;
                case "driver":
                    Username = "driver123";
                    Password = "driver123";
                    break;
                default:
                    return Page();
            }

            return await OnPostAsync();
        }
    }
}
