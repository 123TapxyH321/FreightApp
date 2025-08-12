using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreightApp.Domain.Models
{
    public class OrderHead
    {
        public int Id { get; set; }
        public int IdOrder { get; set; }
        public int? IdDispecher { get; set; }
        public int? IdDraiver { get; set; }
        public int IdStatusOrder { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? DateStatusChange { get; set; }
    }
}
