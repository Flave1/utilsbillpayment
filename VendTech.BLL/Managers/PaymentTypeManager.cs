using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using VendTech.DAL;
using System.Web.Mvc;

namespace VendTech.BLL.Managers
{
    public class PaymentTypeManager : BaseManager, IPaymentTypeManager
    {

        List<SelectListItem> IPaymentTypeManager.GetPaymentTypeSelectList()
        {
            try
            {
                IQueryable<PaymentType> query = Context.PaymentTypes.Where(d => d.IsDeleted == false && d.Active == true).OrderBy(s => s.PaymentTypeId);//  && p.Status == (int)UserStatusEnum.Active
             
                return query.ToList().Select(p => new SelectListItem
                {
                    Text = p.Name.ToUpper(),
                    Value = p.PaymentTypeId.ToString()
                }).ToList();
            }
            catch (Exception)
            {
                return new List<SelectListItem>();
            }
        }
        PagingResult<PaymentTypeModel> IPaymentTypeManager.GetPaymentTypes()
        {
            var result = new PagingResult<PaymentTypeModel>();
            var query = Context.PaymentTypes.Where(p => !p.IsDeleted);

            var list = query 
               .ToList().Select(x => new PaymentTypeModel
               {
                   Name = x.Name,
                   IsDeleted = x.IsDeleted,
                   Active = x.Active,
                   CreatedAt = x.CreatedAt,
                   UpdatedAt = x.UpdatedAt,
                   PaymentTypeId = x.PaymentTypeId,
               }).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Item List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<PaymentTypeModel> IPaymentTypeManager.GetPagedList(PagingModel model)
        {
            var result = new PagingResult<PaymentTypeModel>();
            var query = Context.PaymentTypes.Where(p => !p.IsDeleted).OrderBy(model.SortBy + " " + model.SortOrder);
           
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new PaymentTypeModel {
                    Name = x.Name,
                    IsDeleted = x.IsDeleted,
                    Active = x.Active,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    PaymentTypeId = x.PaymentTypeId,
        }).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Item List";
            result.TotalCount = query.Count();
            return result;
        }

        PaymentTypeModel IPaymentTypeManager.GetPaymentTypeDetail(int id)
        {
            var bank = Context.PaymentTypes.FirstOrDefault(p => p.PaymentTypeId == id);
            if (bank == null)
                return null;
            var result = new PaymentTypeModel();
            result.Name = bank.Name;
            result.IsDeleted = bank.IsDeleted;
            result.Active = bank.Active;
            result.CreatedAt = bank.CreatedAt;
            result.UpdatedAt = bank.UpdatedAt;
            result.PaymentTypeId = bank.PaymentTypeId;
            return result;
        }
 
        ActionOutput IPaymentTypeManager.Delete(int id)
        {
           var PaymentType = Context.PaymentTypes.FirstOrDefault(p => p.PaymentTypeId == id);
            if (PaymentType == null)
                return ReturnError("Payment type not exist");
            PaymentType.IsDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Payment type deleted successfully.");
        }

        ActionOutput IPaymentTypeManager.Deactivate(int id)
        {
            var PaymentType = Context.PaymentTypes.FirstOrDefault(p => p.PaymentTypeId == id);
            if (PaymentType == null)
                return ReturnError("Payment type not exist");
            PaymentType.Active = false;
            Context.SaveChanges();
            return ReturnSuccess("Payment type deactivated successfully.");
        }

        ActionOutput IPaymentTypeManager.Activate(int id)
        {
            var PaymentType = Context.PaymentTypes.FirstOrDefault(p => p.PaymentTypeId == id);
            if (PaymentType == null)
                return ReturnError("Payment type not exist");
            PaymentType.Active = true;
            Context.SaveChanges();
            return ReturnSuccess("Payment type Activated successfully.");
        }

        ActionOutput IPaymentTypeManager.SavePaymentType(PaymentTypeModel model)
        {
            var msg = "Payment type updated successfully.";
            var data = Context.PaymentTypes.FirstOrDefault(p => p.PaymentTypeId == model.PaymentTypeId);
            if (data == null) 
                data = new PaymentType(); 

            data.Name = model.Name;
            data.IsDeleted = false;
            data.Active = true; 
            if (model.PaymentTypeId == 0)
            {
                data.CreatedAt = DateTime.UtcNow; 
                Context.PaymentTypes.Add(data);
                msg = "Payment type added successfully.";
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
            }
            Context.SaveChanges();
            return ReturnSuccess(msg);
        }
    }


}
