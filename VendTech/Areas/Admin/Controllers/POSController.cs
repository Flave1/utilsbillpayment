using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using static VendTech.Controllers.MeterController;

namespace VendTech.Areas.Admin.Controllers
{
    public class POSController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IPOSManager _posManager;
        private readonly IVendorManager _vendorManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IUserManager _userManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IMeterManager _meterManager;
        #endregion

        public POSController(IPOSManager posManager,
            IErrorLogManager errorLogManager,
            IEmailTemplateManager templateManager,
            IVendorManager vendorManager,
            IUserManager userManager,
            ICommissionManager commissionManager,
            IMeterManager meterManager)
            : base(errorLogManager)
        {
            _posManager = posManager;
            _templateManager = templateManager;
            _vendorManager = vendorManager;
            _userManager = userManager;
            _commissionManager = commissionManager;
            _meterManager = meterManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManagePOS()
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var users = _posManager.GetPOSPagedList(PagingModel.DefaultModel("VendorId", "Desc"));
            return View(users);
        }

       
        [AjaxOnly, HttpPost, Public]
        public JsonResult GetUserMeters(RequestObject tokenobject)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var result = _meterManager.GetMeters(Convert.ToInt64(tokenobject.token_string), 0, 10);    
            return Json(result);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var modal = _posManager.GetPOSPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_posListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [HttpPost]
        public ActionResult AddEditPos(SavePosModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            
            return JsonResult(_posManager.SavePos(model));
        }

        [AjaxOnly, HttpPost]
        public ActionResult SavePos(SavePassCodeModel savePassCodeModel)
        {
            var isEmailed = false;
            if (string.IsNullOrEmpty(savePassCodeModel.Email))
            {
                isEmailed = true;
            }
            if (!string.IsNullOrEmpty(savePassCodeModel.PassCode))
            {
                savePassCodeModel.VendorId = _posManager.GetPosDetail(savePassCodeModel.POSId).VendorId;
                var name = _vendorManager.GetVendorDetailApi(Convert.ToInt64(savePassCodeModel.VendorId)).Name;
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.GeneratePasscode);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%UserName%", name);
                body = body.Replace("%passcode%", savePassCodeModel.PassCode);
                if (!string.IsNullOrEmpty(savePassCodeModel.Email))
                {
                    isEmailed = Utilities.SendEmail(savePassCodeModel.Email, emailTemplate.EmailSubject, body);
                }
            }
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            if (isEmailed)
            {
                return Json(_posManager.SavePasscodePos(savePassCodeModel));
            }
            return Json(new ActionOutput
            {
                Status = ActionStatus.Error,
                Message = "Something went wrong!!"
            });
        }

        [HttpGet]
        public ActionResult AddEditPos(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var model = new SavePosModel();
            var readonlyStr = "readonly";
            ViewBag.read = "";

            if (id > 0) ViewBag.read = readonlyStr;

            ViewBag.PosTypes = Utilities.EnumToList(typeof(PosTypeEnum));
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            var commissions = _commissionManager.GetCommissions();
            var drpCommissions = new List<SelectListItem>();
            foreach (var item in commissions)
            {
                drpCommissions.Add(new SelectListItem { Text = item.Value.ToString(), Value = item.CommissionId.ToString() });
            }
            ViewBag.commissions = drpCommissions;
            if (id.HasValue && id > 0)
            {
                model = _posManager.GetPosDetail(id.Value);
                model.PlatformList = _posManager.GetAllPlatforms(id.Value);

            }
            else
                model.PlatformList = _posManager.GetAllPlatforms(0);
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeletePos(long posId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_posManager.DeletePos(posId));
        }

        [AjaxOnly, HttpPost]
        public JsonResult EnablePOS(int id)
        {
            return JsonResult(_posManager.ChangePOSStatus(id, true));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DisablePOS(int id)
        {
            return JsonResult(_posManager.ChangePOSStatus(id, false));
        }

        #endregion
    }
}