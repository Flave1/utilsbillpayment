using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var platforms = Context.Platforms.Where(p => !p.IsDeleted).Select(p => new PlatformModel
            {
                PlatformId = p.PlatformId,
                ShortName = p.ShortName,
                Title = p.Title,
                Enabled = p.Enabled,
                MinimumAmount = p.MinimumAmount,
                DiabledPlaformMessage = p.DisabledPlatformMessage,
                DisablePlatform = p.DisablePlatform,
                Logo = string.IsNullOrEmpty(p.Logo) ? "" : Utilities.DomainUrl + p.Logo,
                PlatformType = p.PlatformType,
                PlatformApiConnName = p.PlatformApiConnId > 0 ? p.PlatformApiConnection.Name : null
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
                userAssignedPos = Context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).FirstOrDefault();

                if (userAssignedPos != null && userAssignedPos.POSId > 0)
                {
                    var res =  userAssignedPos.POSAssignedPlatforms.Where(p => !p.Platform.IsDeleted && p.Platform.Enabled)
                        .Select(p => new PlatformModel(p))
                    .OrderBy(p => p.Title)
                    .ToList();
                    return res;
                }
            }
            catch (Exception ex)
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
            dbPlatform.MinimumAmount = model.MinimumAmount;
            dbPlatform.DisabledPlatformMessage = (model.DiabledPlaformMessage != null) ? model.DiabledPlaformMessage.ToString() : "";
            dbPlatform.DisablePlatform = model.DisablePlatform;
            dbPlatform.PlatformType = model.PlatformType;
            dbPlatform.PlatformApiConnId = model.PlatformApiConnId;

            if (model.Id == null || model.Id == 0)
            {
                dbPlatform.CreatedAt = DateTime.UtcNow;
                dbPlatform.IsDeleted = false;
                Context.Platforms.Add(dbPlatform);
            }
            SaveChanges();
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

        PlatformModel IPlatformManager.GetPlatformById(long platformId)
        {
            try
            {
                return Context.Platforms.Where(d => d.PlatformId == platformId)
                    .Select(d => new PlatformModel
                    {
                        Enabled = d.Enabled,
                        MinimumAmount = d.MinimumAmount,
                        Title = d.Title,
                        DiabledPlaformMessage = d.DisabledPlatformMessage,
                        DisablePlatform = d.DisablePlatform,
                        PlatformType = d.PlatformType,
                        PlatformId = d.PlatformId,
                        PlatformApiConnId = d.PlatformApiConnId
                    })
                    .FirstOrDefault();

            }
            catch (Exception)
            {
                return new PlatformModel();
            }
        }

        public PlatformModel GetSinglePlatform(long platformId)
        {
            try
            { 
                return Context.Platforms.Where(d => d.PlatformId == platformId 
                && d.IsDeleted == false 
                && d.Enabled == true)
                    .Select(d => new PlatformModel { Enabled = d.Enabled, 
                        MinimumAmount = d.MinimumAmount, Title = d.Title,
                        DiabledPlaformMessage = d.DisabledPlatformMessage,
                        DisablePlatform = d.DisablePlatform,
                        PlatformType = d.PlatformType,
                        PlatformId = d.PlatformId,
                        PlatformApiConnId = d.PlatformApiConnId
                    })
                    .FirstOrDefault();
                 
            }
            catch (Exception)
            {
                return new PlatformModel();
            }  
        }

        public List<PlatformModel> GetPlatformsByTypeForRecharge(PlatformTypeEnum type)
        {
            try
            {
                return Context.Platforms.Where(d => d.PlatformType == (int) type
                && d.IsDeleted == false
                && d.Enabled == true)
                    .Select(d => new PlatformModel
                    {
                        Enabled = d.Enabled,
                        MinimumAmount = d.MinimumAmount,
                        Title = d.Title,
                        DiabledPlaformMessage = d.DisabledPlatformMessage,
                        DisablePlatform = d.DisablePlatform,
                        PlatformType = d.PlatformType,
                        PlatformId = d.PlatformId,
                        PlatformApiConnId = d.PlatformApiConnId
                    })
                    .ToList();

            }
            catch (Exception)
            {
                return new List<PlatformModel>();
            }
        }

    }


}
