using System;

namespace FreightApp.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // (хеш)
        public string UserName { get; set; } = string.Empty;
        public int IdRole { get; set; }
        public bool UsStatus { get; set; } = false;    // Подтверждён
        public bool UsEnabled { get; set; } = true;    // Активен/заблокирован
    }
}
