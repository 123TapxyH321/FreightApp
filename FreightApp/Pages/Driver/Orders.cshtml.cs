using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;

public class DriverOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public DriverOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderViewModel> MyOrders { get; set; } = new();
    public List<OrderViewModel> OfferedOrders { get; set; } = new();

    public void OnGet()
    {
        LoadOrders();
    }

    [ValidateAntiForgeryToken]
    public IActionResult OnPostAcceptOffer(int orderId, int headId)
    {
        var driverId = GetCurrentUserId();
        if (driverId == 0) return RedirectToPage("/Auth/Login");

        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDraiver == driverId);
        if (head == null) return NotFound();

        head.IdStatusOrder = StatusId("Принят водителем");
        head.DateStatusChange = DateTime.UtcNow;
        _db.SaveChanges();
        return RedirectToPage();
    }

    [ValidateAntiForgeryToken]
    public IActionResult OnPostRejectOffer(int orderId, int headId)
    {
        var driverId = GetCurrentUserId();
        if (driverId == 0) return RedirectToPage("/Auth/Login");

        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDraiver == driverId);
        if (head == null) return NotFound();

        head.IdStatusOrder = StatusId("Отклонён водителем");
        head.DateStatusChange = DateTime.UtcNow;
        _db.SaveChanges();
        return RedirectToPage();
    }

    // Принят водителем → Отклонён водителем
    [ValidateAntiForgeryToken]
    public IActionResult OnPostRejectMy(int orderId, int headId)
    {
        var driverId = GetCurrentUserId();
        if (driverId == 0) return RedirectToPage("/Auth/Login");

        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDraiver == driverId);
        if (head == null) return NotFound();

        var curr = CurrName(head.IdStatusOrder);
        if (curr == "Принят водителем")
        {
            head.IdStatusOrder = StatusId("Отклонён водителем");
            head.DateStatusChange = DateTime.UtcNow;
            _db.SaveChanges();
        }
        return RedirectToPage();
    }

    // Принят водителем → В пути
    [ValidateAntiForgeryToken]
    public IActionResult OnPostStartMy(int orderId, int headId)
    {
        var driverId = GetCurrentUserId();
        if (driverId == 0) return RedirectToPage("/Auth/Login");

        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDraiver == driverId);
        if (head == null) return NotFound();

        var curr = CurrName(head.IdStatusOrder);
        if (curr == "Принят водителем")
        {
            head.IdStatusOrder = StatusId("В пути");
            head.DateStatusChange = DateTime.UtcNow;
            _db.SaveChanges();
        }
        return RedirectToPage();
    }

    // В пути → Доставлен
    [ValidateAntiForgeryToken]
    public IActionResult OnPostCompleteMy(int orderId, int headId)
    {
        var driverId = GetCurrentUserId();
        if (driverId == 0) return RedirectToPage("/Auth/Login");

        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDraiver == driverId);
        if (head == null) return NotFound();

        var curr = CurrName(head.IdStatusOrder);
        if (curr == "В пути")
        {
            head.IdStatusOrder = StatusId("Доставлен");
            head.DateStatusChange = DateTime.UtcNow;
            _db.SaveChanges();
        }
        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var driverId = GetCurrentUserId();

        var all = from h in _db.OrderHead.AsNoTracking()
                  join o in _db.OrderList.AsNoTracking() on h.IdOrder equals o.Id
                  join s in _db.Status_Order.AsNoTracking() on h.IdStatusOrder equals s.Id
                  where h.IdDraiver == driverId
                  orderby o.DateOrder descending
                  select new OrderViewModel
                  {
                      HeadId = h.Id,
                      OrderId = o.Id,
                      StoreOut = o.StoreOut,
                      StoreIn = o.StoreIn,
                      Note = o.Note,
                      Status = s.StatName,
                      DateOrder = o.DateOrder
                  };

        OfferedOrders = all.Where(x => x.Status == "Назначен водителю").ToList();
        MyOrders = all.Where(x => x.Status == "Принят водителем" || x.Status == "В пути" || x.Status == "Доставлен").ToList();
    }

    private int GetCurrentUserId()
    {
        return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }

    private int StatusId(string name) =>
        _db.Status_Order.AsNoTracking().Where(s => s.StatName == name).Select(s => s.Id).FirstOrDefault();

    private string CurrName(int statusId) =>
        _db.Status_Order.AsNoTracking().Where(s => s.Id == statusId).Select(s => s.StatName).FirstOrDefault() ?? "";

    public class OrderViewModel
    {
        public int HeadId { get; set; }
        public int OrderId { get; set; }
        public string StoreOut { get; set; } = "";
        public string StoreIn { get; set; } = "";
        public string? Note { get; set; }
        public string Status { get; set; } = "";
        public DateTime DateOrder { get; set; }
    }
}
