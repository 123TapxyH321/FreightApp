using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FreightApp.DataAccess;
using FreightApp.Domain.Models;
using System.Collections.Generic;
using System.Linq;

public class CreateUserModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public CreateUserModel(FreightAppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public User User { get; set; }

    public List<SelectListItem> RoleOptions { get; set; }

    public void OnGet()
    {
        LoadRoles();
    }

    public IActionResult OnPost()
    {
        // �������� ������������ ������
        if (_db.Userlist.Any(u => u.Login == User.Login))
        {
            ModelState.AddModelError("User.Login", "����� ����� ��� ����������.");
        }

        if (!ModelState.IsValid)
        {
            LoadRoles();
            return Page();
        }

        User.UsStatus = true;     // ������� ����������
        User.UsEnabled = true;    // ������� �������
        _db.Userlist.Add(User);
        _db.SaveChanges();

        return RedirectToPage("/Admin/Users");
    }

    private void LoadRoles()
    {
        RoleOptions = _db.UserRole
            .Select(r => new SelectListItem
            {
                Value = r.id.ToString(),
                Text = r.roleName
            })
            .ToList();
    }
}
