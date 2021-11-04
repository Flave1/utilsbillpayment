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
    public interface IVendorManager
    {
        PagingResult<VendorListingModel> GetVendorsPagedList(PagingModel model, long agentId = 0);
        ActionOutput SaveVendor(SaveVendorModel model);
        ActionOutput DeleteVendor(long vendorId);
        SaveVendorModel GetVendorDetail(long vendorId);
        List<POSListingModel> GetVendorPOS(long vendorId);
        SaveVendorModel GetVendorDetailByPosId(long posId);
        decimal GetVendorPendingDepositTotal(long vendorId);
        List<SelectListItem> GetVendorsSelectList(long agentId = 0);
        List<SelectListItem> GetPosSelectList();
        decimal GetVendorPercentage(long userId);
        long GetVendorIdByAppUserId(long userId);
        SaveVendorModel GetVendorDetailApi(long vendorId);
        List<SelectListItem> GetVendorsForPOSPageSelectList(long agentId = 0);
    }

}
