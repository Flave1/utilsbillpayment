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
using System.Web.Mvc;

namespace VendTech.BLL.Managers
{
    public class CommissionManager : BaseManager, ICommissionManager
    {

        List<CommissionModel> ICommissionManager.GetCommissions()
        {
            try
            {
                return Context.Commissions.Where(p => !p.IsDeleted).ToList().Select(p => new CommissionModel
                {
                    CommissionId = p.CommissionId,
                    Value = p.Percentage
                }).ToList();
            }
            catch (Exception)
            {
                return new List<CommissionModel>();
            }
        }

        ActionOutput ICommissionManager.SaveCommission(SaveCommissionModel model)
        {
            var dbCommission = new Commission();
            if (model.Id > 0)
            {
                dbCommission = Context.Commissions.FirstOrDefault(p => p.CommissionId == model.Id);
                if (dbCommission == null)
                    return ReturnError("Commission not exist.");
            }
            dbCommission.Percentage = model.Value;
            if(model.Id==null || model.Id==0)
            {
                dbCommission.CreatedAt = DateTime.UtcNow;
                dbCommission.IsDeleted = false;
                Context.Commissions.Add(dbCommission);
            }
            Context.SaveChanges();
            return ReturnSuccess("Commission detail saved successfully.");

        }

        ActionOutput ICommissionManager.DeleteCommission(int commissionId)
        {
            var commission = Context.Commissions.Where(z => z.CommissionId == commissionId).FirstOrDefault();
            if (commission == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Commission Not Exist."
                };
            }
            else
            {
                commission.IsDeleted = true;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Commission Deleted Successfully."
                };
            }
        }


        List<SelectListItem> ICommissionManager.GetCommissionSelectList()
        {
            return Context.Commissions.Where(d => d.IsDeleted == false).ToList().Select(p => new SelectListItem
            {
                Text = p.Percentage.ToString().ToUpper(),
                Value = p.CommissionId.ToString().ToUpper()
            }).ToList();
        }

    }


}
