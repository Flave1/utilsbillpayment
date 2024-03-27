#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System.Web.Script.Serialization;
using VendTech.BLL.Common;
using Newtonsoft.Json;
using VendTech.BLL.PlatformApi;
using System.Threading.Tasks;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class MeterController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly ICMSManager _cmsManager;
        private readonly IMeterManager _meterManager;
        private readonly IPlatformManager _platformManager;
        private readonly IPOSManager _posManager;
        private readonly IPlatformTransactionManager _platformTransactionManager;


        #endregion

        public MeterController(IUserManager userManager, IPlatformManager platformManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IMeterManager meterManager, IPOSManager posManager, IPlatformTransactionManager platformTransactionManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;
            _meterManager = meterManager;
            _platformManager = platformManager;
            _posManager = posManager;
            _platformTransactionManager = platformTransactionManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.title = "Manage Meters";
            ViewBag.SelectedTab = SelectedAdminTab.Meters;
            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID);
            var meters = _meterManager.GetMeters(LOGGEDIN_USER.UserID, 1, 1000000000, true);
            ViewBag.IsDisable = _meterManager.IsModuleLocked(34, LOGGEDIN_USER.UserID);

            return View(meters);

        }
        [AjaxOnly, HttpPost]
        public JsonResult GetMetersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var pageNo = (model.PageNo / model.RecordsPerPage) + (model.PageNo % model.RecordsPerPage > 0 ? 1 : 0);
            var modal = _meterManager.GetMeters(LOGGEDIN_USER.UserID, pageNo, model.RecordsPerPage, model.IsActive);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_meterListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult Delete(long Id)
        {
            return JsonResult(_meterManager.DeleteMeter(Id, LOGGEDIN_USER.UserID));
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddEditMeter(string number="")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Meters;
            MeterModel model = new MeterModel();
            //if (meterId.HasValue && meterId > 0)
            //    model = _meterManager.GetMeterDetail(meterId.Value);
            var list = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "CONLOG",
                    Value = "CONLOG"
                },
                new SelectListItem
                {
                    Text = "HOLLEY",
                    Value = "HOLLEY"
                },
                new SelectListItem
                {
                    Text = "SAGEMCOM",
                    Value = "SAGEMCOM"
                },

                new SelectListItem
                {
                    Text = "APATOR",
                    Value = "APATOR"
                },
                new SelectListItem
                {
                    Text = "CLOU",
                    Value = "CLOU"
                }
            };

            ViewBag.meterMakes = list;
            model.Number = number;

            model.IsDisable = _meterManager.IsModuleLocked(34, LOGGEDIN_USER.UserID);

            return View(model);

        }
        public ActionResult EditMeter(long meterId)
        {
            MeterModel model = new MeterModel();
            if (meterId > 0)
                model = _meterManager.GetMeterDetail(meterId);
            var list = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "CONLOG",
                    Value = "CONLOG"
                },
                new SelectListItem
                {
                    Text = "HOLLEY",
                    Value = "HOLLEY"
                },
                new SelectListItem
                {
                    Text = "SAGEMCOM",
                    Value = "SAGEMCOM"
                },
                new SelectListItem
                {
                    Text = "APATOR",
                    Value = "APATOR"
                },
                new SelectListItem
                {
                    Text = "CLOU",
                    Value = "CLOU"
                }
            };
            model.IsDisable = _meterManager.IsModuleLocked(34, LOGGEDIN_USER.UserID);

            ViewBag.meterMakes = list;
            return View("AddEditMeter",model);

        }
        [AjaxOnly, HttpPost]

        public JsonResult AddEditMeter(MeterModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            model.IsSaved = true;
            return JsonResult(_meterManager.SaveMeter(model));
        }


        public ActionResult Utility()
        {
            ViewBag.UserId = LOGGEDIN_USER.UserID;
            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID);
            ViewBag.SelectedTab = SelectedAdminTab.BillPayment;
            ViewBag.Pos = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserID).POSNumber;
            var model = new List<PlatformModel>();
            model = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID);
            ViewBag.title = "Bill Payment";
            return View(model);
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Recharge(long? meterId)
        {

            ViewBag.UserId = LOGGEDIN_USER.UserID;
            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID);
            ViewBag.title = "EDSA Recharge";
            var platform = _platformManager.GetSinglePlatform(1); //1 is not to be changed
            ViewBag.IsDisable = platform.DisablePlatform;
            ViewBag.DisabledMessage = platform.DiabledPlaformMessage;
            ViewBag.MinumumVend = platform.MinimumAmount;
            ViewBag.SelectedTab = SelectedAdminTab.BillPayment;
            RechargeMeterModel model = new RechargeMeterModel();
            ViewBag.IsPlatformAssigned = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID).Count > 0;
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList; 
            ViewBag.meters = _meterManager.GetMetersDropDown(LOGGEDIN_USER.UserID);
            ViewBag.IsModuleDisable = _meterManager.IsModuleLocked(34, LOGGEDIN_USER.UserID);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var hostory_model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                PageNo = 1,
                VendorId = LOGGEDIN_USER.UserID
            };

            var deposits = _meterManager.GetUserMeterRechargesHistory(hostory_model, false, PlatformTypeEnum.ELECTRICITY);

            if (deposits.List.Any())
                model.History = deposits.List;
            if (meterId > 0) model.MeterId = meterId;
            if (posList.Count > 0)
                ViewBag.walletBalance = _posManager.GetPosBalance(Convert.ToInt64(posList[0].Value));
            else
                ViewBag.walletBalance = 0;
            return View(model);

        }

        //[HttpPost, AjaxOnly]
        //public JsonResult Recharge(RechargeMeterModel model)
        //{
        //    model.UserId = LOGGEDIN_USER.UserID;

        //    return JsonResult(_meterManager.RechargeMeter(model));
        //}


        [AjaxOnly, HttpPost, Public]
        public JsonResult ReturnVoucher(RequestObject tokenobject)
        { 

            var result = _meterManager.ReturnVoucherReceipt(tokenobject.token_string);
            if (result.ReceiptStatus.Status == "unsuccessful") 
                return Json(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message });
            return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = result });
        }

        [AjaxOnly, HttpPost, Public]
        public ActionResult GetUserMeters(RequestObject tokenobject)
        {
            bool status = true;
            if(tokenobject.active == "active")
            {
                status = true;
            }
            else
            {
                status = false;
            }
            var modal = _meterManager.GetMeters(Convert.ToInt64(tokenobject.token_string), 0, 1000000000, status); 
            return PartialView("Partials/_userMeterListing", modal);
        }

        [AjaxOnly, HttpPost]
        public ActionResult GetLatestRechargesAfterPurchase()
        {
            var hostory_model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                PageNo = 1,
                VendorId = LOGGEDIN_USER.UserID
            };

            var deposits = _meterManager.GetUserMeterRechargesHistory(hostory_model, false, PlatformTypeEnum.ELECTRICITY);
            var recharges = deposits.List;
            return PartialView("Partials/_salesListing", recharges);
        }


        [AjaxOnly, HttpPost, Public]
        public ActionResult GetPOSBalanceAfterPurchase()
        {
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID);
            if (posList.Count > 0)
                ViewBag.walletBalance = _posManager.GetPosBalance(Convert.ToInt64(posList[0].Value));
            else
                ViewBag.walletBalance = 0; 
            return Json(Utilities.FormatAmount(ViewBag.walletBalance));
        }


        [HttpPost, AjaxOnly]
        public async Task<JsonResult> RechargeReturn(RechargeMeterModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            try
            {
                var result = await _meterManager.RechargeMeterReturnIMPROVED(model);
                if (result.ReceiptStatus.Status == "unsuccessful")
                {
                    return Json(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message });
                }
                else if (result.ReceiptStatus.Status == "disabled")
                {
                    return Json(new { Success = false, Code = 403, Msg = result.ReceiptStatus.Message });
                }

                if (result != null)
                    return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = result });
                return Json(new { Success = false, Code = 302, Msg = "Meter recharged not successful.", Data = result });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Code = 302, Msg = "Meter recharge not successful." });
            }

        }

        [HttpPost, AjaxOnly, Public]
        public async Task<JsonResult> RechargeReturn2(RechargeMeterModel model)
        {
            //model.UserId = model.UserId;
            try
            {
                var result = await _meterManager.RechargeMeterReturnIMPROVED(model);
                if (result.ReceiptStatus.Status == "unsuccessful")
                {
                    return Json(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message });
                }

                if (result != null)
                    return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = result });
                return Json(new { Success = false, Code = 302, Msg = "Meter recharged not successful.", Data = result });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Code = 302, Msg = "Meter recharged not successful." });
            }

        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult ReturnRequestANDResponseJSON(RequestObject tokenobject)
        {
            var result = _meterManager.ReturnRequestANDResponseJSON(tokenobject.token_string.Trim());
            if (result.ReceiptStatus.Status == "unsuccessful")
                return Json(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message });
            return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = result });
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult GetAirtimeReceipt(RequestObject1 requestObject)
        {
            var result = _platformTransactionManager.GetAirtimeReceipt(requestObject.Id);
            if (result.ReceiptStatus.Status == "unsuccessful")
                return Json(JsonConvert.SerializeObject(new { Success = false, Code = 302, Msg = "Airtime recharged not successful.", Data = result }));
            return Json(JsonConvert.SerializeObject(new { Success = true, Code = 200, Msg = "Airtime recharged successfully.", Data = result }));
        }



    }
}