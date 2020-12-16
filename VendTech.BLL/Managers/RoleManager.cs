using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Web;
using System.IO;

namespace VendTech.BLL.Managers
{
    public class RoleManager : BaseManager, IRoleManager
    {

        List<RoleModel> IRoleManager.GetRoles()
        {
            return Context.UserRoles.Where(p => !p.IsDeleted  && p.Role != UserRoles.AppUser).ToList().Select(p => new RoleModel
            {
                RoleId = p.RoleId,
                Value = p.Role
            }).ToList();
        }

        ActionOutput IRoleManager.SaveRole(SaveRoleModel model)
        {
            var dbRole = new UserRole();
            if (model.Id > 0)
            {
                dbRole = Context.UserRoles.FirstOrDefault(p => p.RoleId == model.Id);
                if (dbRole == null)
                    return ReturnError("Role not exist.");
            }
            dbRole.Role = model.Value;
            if(model.Id==null || model.Id==0)
            {
                dbRole.IsDeleted = false;
                Context.UserRoles.Add(dbRole);
            }
            Context.SaveChanges();
            return ReturnSuccess("Role saved successfully.");

        }

        ActionOutput IRoleManager.DeleteRole(int id)
        {
            var role = Context.UserRoles.Where(z => z.RoleId == id).FirstOrDefault();
            if (role == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Role Not Exist."
                };
            }
            else
            {
                role.IsDeleted = true;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Role Deleted Successfully."
                };
            }
        }

        //ActionOutput ICommissionManager.DeleteCommission(int commissionId)
        //{
        //    var commission = Context.Commissions.Where(z => z.CommissionId == commissionId).FirstOrDefault();
        //    if (commission == null)
        //    {
        //        return new ActionOutput
        //        {
        //            Status = ActionStatus.Error,
        //            Message = "Commission Not Exist."
        //        };
        //    }
        //    else
        //    {
        //        commission.IsDeleted = true;
        //        Context.SaveChanges();
        //        return new ActionOutput
        //        {
        //            Status = ActionStatus.Successfull,
        //            Message = "Commission Deleted Successfully."
        //        };
        //    }
        //}

      
    }


}
