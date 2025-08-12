using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FreightApp.Domain.Models;
using FreightApp.DataAccess;
using System.Linq;

public class RegisterModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public RegisterModel(FreightAppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public List<SelectListItem> RoleOptions { get; set; }

    public string Message { get; set; }

    public class InputModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
    }

    public void OnGet()
    {
        RoleOptions = _db.UserRole
            .Where(r => r.roleName != "Администратор")
            .Select(r => new SelectListItem
            {
                Value = r.id.ToString(),
                Text = r.roleName
            }).ToList();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (_db.Userlist.Any(u => u.Login == Input.Login))
        {
            Message = "Логин уже используется.";
            return Page();
        }

        var user = new User
        {
            Login = Input.Login,
            Password = Input.Password,
            UserName = Input.UserName,
            IdRole = Input.RoleId,
            UsStatus = false,
            UsEnabled = true
        };

        _db.Userlist.Add(user);
        _db.SaveChanges();

        Message = "Регистрация выполнена. Ожидайте подтверждения администратора.";
        return Page();
    }
}
