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
    public class PlatformManager : BaseManager, IPlatformManager
    {

        List<PlatformModel> IPlatformManager.GetPlatforms()
        {
            return Context.Platforms.Where(p => !p.IsDeleted).ToList().Select(p => new PlatformModel
            {
                PlatformId = p.PlatformId,
                ShortName = p.ShortName,
                Title = p.Title,
                Enabled = p.Enabled
            }).ToList();
        }
        List<PlatformModel> IPlatformManager.GetUserAssignedPlatforms(long userId)
        {
            var user = Context.Users.SingleOrDefault(p => p.UserId == userId);
            var userAssignedPos = new POS();
            //if (user.UserRole.Role == UserRoles.Vendor)
            //    userAssignedPos = user.POS.FirstOrDefault();
            //else if (user.UserRole.Role == UserRoles.AppUser && user.User1 != null)
            //    userAssignedPos = user.User1.POS.FirstOrDefault();
            userAssignedPos = Context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).FirstOrDefault();

            if (userAssignedPos != null && userAssignedPos.POSId > 0)
            {
                return userAssignedPos.POSAssignedPlatforms.Where(p => !p.Platform.IsDeleted && p.Platform.Enabled).ToList().Select(p => new PlatformModel
                {
                    PlatformId = p.Platform.PlatformId,
                    Title = p.Platform.Title
                }).ToList();
            }
            return new List<PlatformModel>();

        }
        ActionOutput IPlatformManager.SavePlateform(SavePlatformModel model)
        {
            var dbPlatform = new Platform();
            if (model.Id > 0)
            {
                dbPlatform = Context.Platforms.FirstOrDefault(p => p.PlatformId == model.Id);
                if (dbPlatform == null)
                    return ReturnError("Platform not exist.");
            }
            dbPlatform.Title = model.Title;
            dbPlatform.ShortName = model.ShortName;
            if (model.Id == null || model.Id == 0)
            {
                dbPlatform.CreatedAt = DateTime.UtcNow;
                dbPlatform.IsDeleted = false;
                Context.Platforms.Add(dbPlatform);
            }
            Context.SaveChanges();
            return ReturnSuccess("Platform detail saved successfully.");

        }

        ActionOutput IPlatformManager.DeletePlatform(int platformId)
        {
            var platform = Context.Platforms.Where(z => z.PlatformId == platformId).FirstOrDefault();
            if (platform == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Platform Not Exist."
                };
            }
            else
            {
                platform.IsDeleted = true;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Platform Deleted Successfully."
                };
            }
        }

        ActionOutput IPlatformManager.ChangePlatformStatus(int platformId, bool value)
        {
            var platform = Context.Platforms.Where(z => z.PlatformId == platformId).FirstOrDefault();
            if (platform == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Platform Not Exist."
                };
            }
            else
            {
                platform.Enabled = value;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Platform status changed Successfully."
                };
            }
        }
    }


}
