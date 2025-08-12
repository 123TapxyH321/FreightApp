using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreightApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string? FullName { get; set; }
        public string? UserRole { get; set; }

        public IndexModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult OnGet()
        {
            var context = _httpContextAccessor.HttpContext;
            FullName = context?.Session.GetString("FullName");
            UserRole = context?.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(UserRole))
            {
                return UserRole switch
                {
                    "Клиент" => RedirectToPage("/Client/MyOrders"),
                    "Диспетчер" => RedirectToPage("/Dispatcher/NewOrders"),
                    "Водитель" => RedirectToPage("/Driver/Orders"),
                    "Администратор" => RedirectToPage("/Admin/Users"),
                    _ => Page()
                };
            }

            return Page();
        }
    }
}
