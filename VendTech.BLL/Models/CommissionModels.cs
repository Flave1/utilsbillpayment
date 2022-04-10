using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class CommissionModel
    {
        public int CommissionId { get; set; }
        public decimal Value { get; set; }
    }

    public class SaveCommissionModel
    {
        public int? Id { get; set; }
        public decimal Value { get; set; }
    }
}
