using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using FreightApp.Domain.Models;

public class DispatcherMyOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public DispatcherMyOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderVm> MyOrders { get; set; } = new();
    public List<DriverVm> Drivers { get; set; } = new();

    public void OnGet()
    {
        LoadOrders();
    }

    [ValidateAntiForgeryToken]
    public IActionResult OnPostComplete(int orderId, int headId)
    {
        var dispatcherId = GetDispatcherId();

        var head = _db.OrderHead
            .FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDispecher == dispatcherId);
        if (head == null) return NotFound();

        int deliveredId = StatusId("Доставлен");
        int completedId = StatusId("Завершён");
        if (deliveredId == 0 || completedId == 0)
            return BadRequest("Не найдены статусы 'Доставлен' или 'Завершён'.");

        if (head.IdStatusOrder != deliveredId)
        {
            var currName = CurrName(head.IdStatusOrder);
            return BadRequest($"Завершить можно только доставленный заказ. Текущий статус: '{currName}'.");
        }

        head.IdStatusOrder = completedId;
        head.DateStatusChange = DateTime.UtcNow;
        _db.SaveChanges();

        return RedirectToPage();
    }

    // см. последнюю корректировку: аннулировать старую запись — ТОЛЬКО если статус был "Отклонён водителем"
    [ValidateAntiForgeryToken]
    public IActionResult OnPostAssignDriver(int orderId, int headId, int driverId)
    {
        var dispatcherId = GetDispatcherId();

        using var tx = _db.Database.BeginTransaction();

        var head = _db.OrderHead
            .FirstOrDefault(h => h.Id == headId && h.IdOrder == orderId && h.IdDispecher == dispatcherId);
        if (head == null) return NotFound();

        var currName = CurrName(head.IdStatusOrder);
        int assignedId = StatusId("Назначен водителю");
        int cancelledId = StatusId("Аннулирован");
        if (assignedId == 0) return BadRequest("Статус 'Назначен водителю' не найден.");

        if (string.Equals(currName, "Отклонён водителем", StringComparison.OrdinalIgnoreCase))
        {
            if (cancelledId == 0) return BadRequest("Статус 'Аннулирован' не найден.");

            // 1) аннулируем ТЕКУЩУЮ запись
            head.IdStatusOrder = cancelledId;
            head.DateStatusChange = DateTime.UtcNow;
            _db.SaveChanges();

            // 2) создаём НОВУЮ запись с выбранным водителем
            var newHead = new OrderHead
            {
                IdOrder = head.IdOrder,
                IdDispecher = dispatcherId,
                IdDraiver = driverId,
                IdStatusOrder = assignedId,
                DateAssigned = DateTime.UtcNow,
                DateStatusChange = null
            };
            _db.OrderHead.Add(newHead);
            _db.SaveChanges();
        }
        else if (string.Equals(currName, "Принят диспетчером", StringComparison.OrdinalIgnoreCase))
        {
            // НЕ аннулируем: обновляем текущую запись
            head.IdDraiver = driverId;
            head.IdStatusOrder = assignedId;
            head.DateAssigned = DateTime.UtcNow;
            head.DateStatusChange = null;
            _db.SaveChanges();
        }
        else
        {
            return BadRequest("Назначать/заменять водителя можно только из статусов 'Принят диспетчером' или 'Отклонён водителем'.");
        }

        tx.Commit();
        return RedirectToPage();
    }

    private void LoadOrders()
    {
        var dispatcherId = GetDispatcherId();

        Drivers = (from u in _db.Userlist
                   join r in _db.UserRole on u.IdRole equals r.id
                   where r.roleName == "Водитель" && u.UsEnabled && u.UsStatus
                   orderby u.UserName
                   select new DriverVm { Id = u.Id, Name = u.UserName }).ToList();

        MyOrders = (from h in _db.OrderHead
                    join o in _db.OrderList on h.IdOrder equals o.Id
                    join s in _db.Status_Order on h.IdStatusOrder equals s.Id
                    where h.IdDispecher == dispatcherId
                    orderby o.DateOrder descending
                    select new OrderVm
                    {
                        HeadId = h.Id,
                        OrderId = o.Id,
                        StoreOut = o.StoreOut,
                        StoreIn = o.StoreIn,
                        Note = o.Note,
                        Status = s.StatName,
                        CurrentDriverId = h.IdDraiver
                    }).ToList();
    }

    private int GetDispatcherId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private int StatusId(string name) =>
        _db.Status_Order.AsNoTracking().Where(s => s.StatName == name).Select(s => s.Id).FirstOrDefault();

    private string CurrName(int statusId) =>
        _db.Status_Order.AsNoTracking().Where(s => s.Id == statusId).Select(s => s.StatName).FirstOrDefault() ?? "";

    public class OrderVm
    {
        public int HeadId { get; set; }
        public int OrderId { get; set; }
        public string StoreOut { get; set; } = "";
        public string StoreIn { get; set; } = "";
        public string? Note { get; set; }
        public string Status { get; set; } = "";
        public int? CurrentDriverId { get; set; }
    }

    public class DriverVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
