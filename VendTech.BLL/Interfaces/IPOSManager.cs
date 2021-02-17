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
    public interface IPOSManager
    {
        PagingResult<POSListingModel> GetPOSPagedList(PagingModel model, long agentId = 0, long vendorId = 0, bool callForGetVendorPos = false);
        SavePosModel GetPosDetail(long posId);
        ActionOutput SavePos(SavePosModel model);
        ActionOutput SavePasscodePos(SavePassCodeModel savePassCodeModel);
        ActionOutput SavePasscodePosApi(SavePassCodeApiModel savePassCodeModel);
        IList<PlatformCheckbox> GetAllPlatforms(long posId);
        ActionOutput DeletePos(long posId);
        ActionOutput ChangePOSStatus(int posId, bool value);
        decimal GetPosCommissionPercentage(long posId);
        decimal GetPosBalance(long posId);
        decimal GetPosCommissionPercentageByUserId(long userId);
        List<SelectListItem> GetPOSSelectList(long userId = 0);
        List<SelectListItem> GetVendorPos(long userId);
        List<PosAPiListingModel> GetPOSSelectListForApi(long userId = 0);
        PagingResult<POSListingModel> GetUserPosPagingListForApp(int pageNo, int pageSize, long userId);

    }
    
}
