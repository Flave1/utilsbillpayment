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
    public interface IPlatformManager
    {
        List<PlatformModel> GetPlatforms();
        ActionOutput SavePlateform(SavePlatformModel model);
        ActionOutput DeletePlatform(int platformId);
        ActionOutput ChangePlatformStatus(int platformId,bool value);
        List<PlatformModel> GetUserAssignedPlatforms(long userId);
        PlatformModel GetSinglePlatform(long platformId);
        PlatformModel GetPlatformById(long platformId);
        List<PlatformModel> GetPlatformsByTypeForRecharge(PlatformTypeEnum type);
        List<SelectListItem> GetActivePlatformsSelectList();
        List<SelectListItem> GetOperatorType(PlatformTypeEnum type);
        ActionOutput EnableThisPlateform(EnableThisPlatform model);
    }
    
}
