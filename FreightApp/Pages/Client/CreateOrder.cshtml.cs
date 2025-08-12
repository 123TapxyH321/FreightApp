using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.Domain.Models;
using FreightApp.DataAccess;
using System;
using System.Security.Claims;

public class CreateOrderModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public CreateOrderModel(FreightAppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public OrderList Order { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToPage("/Auth/Login");

        Order.IdUser = int.Parse(userIdClaim.Value);
        Order.DateOrder = DateTime.UtcNow;//DateTime.Now;

        _db.OrderList.Add(Order);
        _db.SaveChanges();

        return RedirectToPage("/Client/MyOrders");
    }
}
