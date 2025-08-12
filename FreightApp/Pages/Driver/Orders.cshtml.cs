using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using System.Security.Claims;
using FreightApp.Domain.Models;
using System.Linq;
using System.Collections.Generic;
using System;

public class DriverOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public DriverOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderViewModel> MyOrders { get; set; }
    public List<OrderViewModel> OfferedOrders { get; set; }

    public void OnGet()
    {
        LoadOrders();
    }

    public IActionResult OnPost(int orderId, string action)
    {
        var driverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var order = _db.OrderHead.FirstOrDefault(h => h.IdOrder == orderId && h.IdDraiver == driverId);

        if (order != null)
        {
            if (action == "finish")
            {
                order.IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Отклонён водителем").Id;
                order.DateStatusChange = DateTime.UtcNow;
            }
            else if (action == "accept")
            {
                order.IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Принят водителем").Id;
                order.DateStatusChange = DateTime.UtcNow;
            }
            else if (action == "reject")
            {
                order.IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Доставлен").Id;
                order.DateStatusChange = DateTime.UtcNow;
            }


            _db.SaveChanges();
        }

        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var driverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var all = from h in _db.OrderHead
                  join o in _db.OrderList on h.IdOrder equals o.Id
                  join s in _db.Status_Order on h.IdStatusOrder equals s.Id into sj
                  from status in sj.DefaultIfEmpty()
                  where h.IdDraiver == driverId
                  select new OrderViewModel
                  {
                      OrderId = o.Id,
                      StoreOut = o.StoreOut,
                      StoreIn = o.StoreIn,
                      Note = o.Note,
                      Status = status != null ? status.StatName : "Назначен водителю"
                  };

        MyOrders = all.Where(x => x.Status == "В работе водителем" || x.Status == "Доставлено").ToList();
        OfferedOrders = all.Where(x => x.Status == "Ожидает ответа" || x.Status == "Отклонён водителем").ToList();
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string StoreOut { get; set; }
        public string StoreIn { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}
