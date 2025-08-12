using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using FreightApp.Domain.Models;

public class NewOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public NewOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderViewModel> Orders { get; set; }

    public void OnGet()
    {
        LoadOrders();
    }

    public IActionResult OnPost(int orderId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToPage("/Auth/Login");

        var head = new OrderHead
        {
            IdOrder = orderId,
            IdDispecher = int.Parse(userIdClaim.Value),
            IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Принят диспетчером").Id,
            DateAssigned = DateTime.UtcNow,
            DateStatusChange = null
        };


        _db.OrderHead.Add(head);
        _db.SaveChanges();

        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var assignedOrderIds = _db.OrderHead.Select(h => h.IdOrder).ToList();

        Orders = (from o in _db.OrderList
                  join u in _db.Userlist on o.IdUser equals u.Id
                  where !assignedOrderIds.Contains(o.Id)
                  select new OrderViewModel
                  {
                      OrderId = o.Id,
                      ClientName = u.UserName,
                      StoreOut = o.StoreOut,
                      StoreIn = o.StoreIn,
                      Note = o.Note,
                      DateOrder = o.DateOrder
                  }).ToList();
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; }
        public string StoreOut { get; set; }
        public string StoreIn { get; set; }
        public string Note { get; set; }
        public DateTime DateOrder { get; set; }
    }
}
