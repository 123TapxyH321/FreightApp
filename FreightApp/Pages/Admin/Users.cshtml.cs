using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using FreightApp.Domain.Models;
using System.Linq;

public class AdminUsersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public AdminUsersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<UserViewModel> Users { get; set; }

    public void OnGet()
    {
        LoadUsers();
    }

    public IActionResult OnPost(int userId, string action)
    {
        var user = _db.Userlist.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            switch (action)
            {
                case "approve":
                    user.UsStatus = true;
                    break;
                case "block":
                    user.UsEnabled = false;
                    break;
                case "unblock":
                    user.UsEnabled = true;
                    break;
            }
            _db.SaveChanges();
        }
        LoadUsers();
        return Page();
    }

    private void LoadUsers()
    {
        Users = (from u in _db.Userlist
                 join r in _db.UserRole on u.IdRole equals r.id
                 select new UserViewModel
                 {
                     Id = u.Id,
                     Login = u.Login,
                     UserName = u.UserName,
                     RoleName = r.roleName,
                     UsStatus = u.UsStatus,
                     UsEnabled = u.UsEnabled
                 }).ToList();
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public bool UsStatus { get; set; }
        public bool UsEnabled { get; set; }
    }
}
