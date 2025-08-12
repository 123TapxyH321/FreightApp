using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreightApp.Domain.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public string StatName { get; set; } = string.Empty;
    }
}
