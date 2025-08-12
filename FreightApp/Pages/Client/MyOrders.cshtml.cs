using Microsoft.AspNetCore.Mvc.RazorPages;
using FreightApp.DataAccess;
using FreightApp.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

public class MyOrdersModel : PageModel
{
    private readonly FreightAppDbContext _db;

    public MyOrdersModel(FreightAppDbContext db)
    {
        _db = db;
    }

    public List<OrderViewModel> Orders { get; set; }

    public void OnGet()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        int userId = int.Parse(userIdClaim.Value);

        Orders = (from o in _db.OrderList
                  join h in _db.OrderHead on o.Id equals h.IdOrder into joined
                  from oh in joined.DefaultIfEmpty()
                  join s in _db.Status_Order on oh.IdStatusOrder equals s.Id into statusJoined
                  from st in statusJoined.DefaultIfEmpty()
                  where o.IdUser == userId
                  select new OrderViewModel
                  {
                      StoreOut = o.StoreOut,
                      StoreIn = o.StoreIn,
                      Note = o.Note,
                      DateOrder = o.DateOrder,
                      Status = st != null ? st.StatName : "ќжидает обработки"
                  }).ToList();
    }

    public class OrderViewModel
    {
        public string StoreOut { get; set; }
        public string StoreIn { get; set; }
        public string Note { get; set; }
        public DateTime DateOrder { get; set; }
        public string Status { get; set; }
    }
}
