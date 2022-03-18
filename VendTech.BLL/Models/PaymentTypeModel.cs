using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class PaymentTypeModel
    {
        public int PaymentTypeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        
    }
}
