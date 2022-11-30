using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static VendTech.BLL.Managers.POSManager;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IPOSManager
    {
        PagingResult<POSListingModel> GetPOSPagedList(PagingModel model, long agentId = 0, long vendorId = 0, bool callForGetVendorPos = false);
        KeyValuePair<string, string> GetVendorDetail(long posId);
        SavePosModel GetPosDetail(long posId);
        SavePosModel GetPosDetails(string passCode);
        UserModel GetUserPosDetails(string posSerialNumber);
        ActionOutput SavePos(SavePosModel model);
        ActionOutput SavePasscodePos(SavePassCodeModel savePassCodeModel);
        ActionOutput SavePasscodePosApi(SavePassCodeModel savePassCodeModel);
        IList<PlatformCheckbox> GetAllPlatforms(long posId);
        ActionOutput DeletePos(long posId);
        ActionOutput ChangePOSStatus(int posId, bool value);
        decimal GetPosCommissionPercentage(long posId);
        decimal GetPosBalance(long posId);
        decimal GetPosCommissionPercentageByUserId(long userId); 
        List<SelectListItem> GetPOSSelectList(long userId = 0, long agentId = 0);
        List<PosSelectItem> GetVendorPos(long userId);
        List<PosAPiListingModel> GetPOSSelectListForApi(long userId = 0);
        PagingResult<POSListingModel> GetUserPosPagingListForApp(int pageNo, int pageSize, long userId);
        UserModel GetUserPosDetailApi(string posSerialNumber);
        decimal GetPosPercentage(long posId);
        POS GetSinglePos(long pos);
        List<PosSelectItem> GetAgencyPos(long userId);
        POS ReturnAgencyAdminPOS(long userId);
        void DeductFromVendorPOSBalance(long userId, decimal amount);
        void keepTransanctiondetails(long userId, decimal amount, long meterId, long number);
    }

}
