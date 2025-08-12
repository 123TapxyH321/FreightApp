using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FreightApp.DataAccess;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Generic;

public class MyDispatcherOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public MyDispatcherOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderViewModel> Orders { get; set; }
    public List<DriverOption> Drivers { get; set; }

    public void OnGet()
    {
        LoadOrders();
        LoadDrivers();
    }

    public IActionResult OnPost(int orderId, int? driverId, string action)
    {
        var head = _db.OrderHead.FirstOrDefault(h => h.IdOrder == orderId);
        if (head == null) return RedirectToPage();

        
        if (action == "assign" && driverId.HasValue)
        {
            head.IdDraiver = driverId;
            head.IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Принят диспетчером").Id;
        }
        else if (action == "complete")
        {
            head.IdStatusOrder = _db.Status_Order.First(s => s.StatName == "Назначен водителю").Id;
            head.DateStatusChange = DateTime.UtcNow;
        }
      


        _db.SaveChanges();
        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        Orders = (from h in _db.OrderHead
                  join o in _db.OrderList on h.IdOrder equals o.Id
                  join u in _db.Userlist on o.IdUser equals u.Id
                  join d in _db.Userlist on h.IdDraiver equals d.Id into dj
                  from driver in dj.DefaultIfEmpty()
                  join s in _db.Status_Order on h.IdStatusOrder equals s.Id into sj
                  from status in sj.DefaultIfEmpty()
                  where h.IdDispecher == userId
                  select new OrderViewModel
                  {
                      OrderId = o.Id,
                      ClientName = u.UserName,
                      StoreOut = o.StoreOut,
                      StoreIn = o.StoreIn,
                      Note = o.Note,
                      DateOrder = o.DateOrder,
                      DriverName = driver != null ? driver.UserName : null,
                      Status = status != null ? status.StatName : "Ожидает водителя"
                  }).ToList();
    }

    private void LoadDrivers()
    {
        var roleId = _db.UserRole.FirstOrDefault(r => r.roleName == "Водитель")?.id ?? 0;

        Drivers = _db.Userlist
            .Where(u => u.IdRole == roleId && u.UsEnabled && u.UsStatus)
            .Select(u => new DriverOption
            {
                Id = u.Id,
                Name = u.UserName
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
        public string DriverName { get; set; }
        public string Status { get; set; }
    }

    public class DriverOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
