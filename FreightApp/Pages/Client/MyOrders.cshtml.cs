using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;

public class ClientMyOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public ClientMyOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderVm> MyOrders { get; set; } = new();

    public void OnGet()
    {
        LoadOrders();
    }

    // «Снять заказ»
    [ValidateAntiForgeryToken]
    public IActionResult OnPostCancel(int orderId, int? headId)
    {
        var clientId = GetClientId();

        var order = _db.OrderList.FirstOrDefault(o => o.Id == orderId && o.IdUser == clientId);
        if (order == null) return NotFound();

        // Если головы нет — это «Создан клиентом» ? можно удалить сам заказ
        if (headId == null)
        {
            _db.OrderList.Remove(order);
            _db.SaveChanges();
            return RedirectToPage();
        }

        // Работать строго с указанной головой
        var head = _db.OrderHead.FirstOrDefault(h => h.Id == headId.Value && h.IdOrder == orderId);
        if (head == null) return NotFound();

        var currName = CurrName(head.IdStatusOrder);

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Отклонён водителем",
            "Принят диспетчером",
            "Создан клиентом",
            "Назначен водителю"
        };
        if (!allowed.Contains(currName))
            return BadRequest("Заказ нельзя снять на текущем статусе.");

        var cancelledId = StatusId("Аннулирован");
        if (cancelledId == 0) return BadRequest("Статус 'Аннулирован' не найден.");

        head.IdStatusOrder = cancelledId;
        head.DateStatusChange = DateTime.UtcNow;
        _db.SaveChanges();

        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var clientId = GetClientId();

        MyOrders = (from o in _db.OrderList
                    join h in _db.OrderHead on o.Id equals h.IdOrder into hj
                    from head in hj.DefaultIfEmpty()
                    join s in _db.Status_Order on (head != null ? head.IdStatusOrder : 0) equals s.Id into sj
                    from status in sj.DefaultIfEmpty()
                    where o.IdUser == clientId
                    orderby o.DateOrder descending
                    select new OrderVm
                    {
                        HeadId = head != null ? (int?)head.Id : null,
                        OrderId = o.Id,
                        StoreOut = o.StoreOut,
                        StoreIn = o.StoreIn,
                        Note = o.Note,
                        Status = status != null ? status.StatName : "Создан клиентом"
                    }).ToList();

        foreach (var x in MyOrders)
        {
            x.CanCancel = x.Status == "Отклонён водителем"
                       || x.Status == "Принят диспетчером"
                       || x.Status == "Создан клиентом"
                       || x.Status == "Назначен водителю";
        }
    }

    private int GetClientId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private int StatusId(string name) =>
        _db.Status_Order.AsNoTracking().Where(s => s.StatName == name).Select(s => s.Id).FirstOrDefault();

    private string CurrName(int statusId) =>
        _db.Status_Order.AsNoTracking().Where(s => s.Id == statusId).Select(s => s.StatName).FirstOrDefault() ?? "";

    public class OrderVm
    {
        public int? HeadId { get; set; }
        public int OrderId { get; set; }
        public string StoreOut { get; set; } = "";
        public string StoreIn { get; set; } = "";
        public string? Note { get; set; }
        public string Status { get; set; } = "";
        public bool CanCancel { get; set; }
    }
}
