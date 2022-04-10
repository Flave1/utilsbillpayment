using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class RoleModel
    {
        public int RoleId { get; set; }
        public string Value { get; set; }
    }

    public class SaveRoleModel
    {
        public int? Id { get; set; }
        public string Value { get; set; }
    }
}
