using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VendTech.BLL.Interfaces
{
    public interface IRoleManager
    {
        List<RoleModel> GetRoles();
        ActionOutput SaveRole(SaveRoleModel model);
        ActionOutput DeleteRole(int id);

    }
    
}
