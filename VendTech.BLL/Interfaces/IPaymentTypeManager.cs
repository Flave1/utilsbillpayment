using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace VendTech.BLL.Interfaces
{
    public interface IPaymentTypeManager
    {
        PagingResult<PaymentTypeModel> GetPaymentTypes();
        PaymentTypeModel GetPaymentTypeDetail(int id);
        ActionOutput Delete(int id);
        ActionOutput SavePaymentType(PaymentTypeModel model);
        ActionOutput Deactivate(int id);
        List<SelectListItem> GetPaymentTypeSelectList();
        ActionOutput Activate(int id);
        PagingResult<PaymentTypeModel> GetPagedList(PagingModel model);
    }
}