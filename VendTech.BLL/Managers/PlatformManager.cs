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
using System.Reflection;

namespace VendTech.BLL.Managers
{
    public class PlatformManager : BaseManager, IPlatformManager
    {

        List<PlatformModel> IPlatformManager.GetPlatforms()
        {
            var platforms = Context.Platforms.Where(p => !p.IsDeleted).ToList().Select(p => new PlatformModel
            {
                PlatformId = p.PlatformId,
                ShortName = p.ShortName,
                Title = p.Title,
                Enabled = p.Enabled,
                Logo = string.IsNullOrEmpty(p.Logo) ? "" : Utilities.DomainUrl + p.Logo
            }).ToList();
            return platforms;
        }
        List<PlatformModel> IPlatformManager.GetUserAssignedPlatforms(long userId)
        {
            try
            {
                if (userId == 0) return new List<PlatformModel>();
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
                        Title = p.Platform.Title,
                        Logo = string.IsNullOrEmpty(p.Platform.Logo) ? "" : Utilities.DomainUrl + p.Platform.Logo
                    }).ToList();
                }
            }
            catch (Exception)
            {
               return new List<PlatformModel>();
            } 
            return new List<PlatformModel>();

        }
        ActionOutput IPlatformManager.SavePlateform(SavePlatformModel model)
        {
            var dbPlatform = new Platform();
            var myfile = string.Empty;
            if (model.Id > 0)
            {
                dbPlatform = Context.Platforms.FirstOrDefault(p => p.PlatformId == model.Id);
                if (dbPlatform == null)
                    return ReturnError("Platform not exist.");
            }

            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo.Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            }

            if (model.Image != null)
            {
                var ext = Path.GetExtension(model.Image.FileName);   
                myfile = Guid.NewGuid().ToString() + ext;  
                var folderName = HttpContext.Current.Server.MapPath("~/Images/ProductImages");
                if (!Directory.Exists(folderName))
                    Directory.CreateDirectory(folderName);
                var path = Path.Combine(folderName, myfile);
                model.Image.SaveAs(path);
                if (!string.IsNullOrEmpty(dbPlatform.Logo))
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath("~" + dbPlatform.Logo)))
                        File.Delete(HttpContext.Current.Server.MapPath("~" + dbPlatform.Logo));
                }
                dbPlatform.Logo = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProductImages/" + myfile; 
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
