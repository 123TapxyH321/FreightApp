using System;

namespace FreightApp.Domain.Models
{
    public class OrderList
    {
        public int Id { get; set; }
        public int IdUser { get; set; }         // Заказчик
        public string StoreOut { get; set; } = string.Empty; // Откуда
        public string StoreIn { get; set; } = string.Empty;  // Куда
        public string? Note { get; set; }
        public DateTime DateOrder { get; set; }
    }
}
