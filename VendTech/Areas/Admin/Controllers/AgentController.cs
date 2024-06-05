using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.Areas.Admin.Controllers
{
    public class AgentController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly IAgencyManager _agencyManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IVendorManager _vendorManager;
        private readonly IPOSManager _posManager;
        private readonly IBankAccountManager _bankAccountManager;
        private readonly IPaymentTypeManager _paymentTypeManager;
        private readonly IDepositManager _depositManager;
        private readonly ISMSManager _smsManager;
        #endregion

        public AgentController(IAgencyManager agencyManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, ICommissionManager commissionManager, IPaymentTypeManager paymentTypeManager, IBankAccountManager bankAccountManager, IPOSManager posManager, IVendorManager vendorManager, IDepositManager depositManager, ISMSManager smsManager)
            : base(errorLogManager)
        {
            _agencyManager = agencyManager;
            _templateManager = templateManager;
            _commissionManager = commissionManager;
            _paymentTypeManager = paymentTypeManager;
            _bankAccountManager = bankAccountManager;
            _posManager = posManager;
            _vendorManager = vendorManager;
            _depositManager = depositManager;
            _smsManager = smsManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageAgents()
        {
            try
            {
                ViewBag.SelectedTab = SelectedAdminTab.Agents;
                ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelectList();
                var vendors = _vendorManager.GetVendorsSelectList();
                ViewBag.PosId = new SelectList(_vendorManager.GetPosSelectList(), "Value", "Text");

                ViewBag.ChkBankName = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");

                var bankAccounts = _bankAccountManager.GetBankAccounts();
                ViewBag.bankAccounts = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName.ToUpper(), Value = p.BankAccountId.ToString() }).ToList();
                ViewBag.vendors = vendors;

                var users = _agencyManager.GetAgenciesPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"));
                return View("ManageAgentsV2", users);
            }
            catch (Exception)
            {
                return View("ManageAgentsV2", new PagingResult<AgencyListingModel>());
            }
          
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var modal = _agencyManager.GetAgenciesPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_agencyListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
      
        [HttpPost]
        public ActionResult AddAgent(SaveAgentModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.AddAgent(model));
        }
        [HttpGet]
        public ActionResult AddAgent(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var model = new SaveAgentModel();
            ViewBag.Users = _userManager.GetAgentSelectList();
            
            ViewBag.Commisions = _commissionManager.GetCommissionSelectList();
            if (id.HasValue && id > 0)
            {
                model = _agencyManager.GetAgentDetail(id.Value);
                model.ModuleList = _userManager.GetAllModules(model.Representative.Value);
                model.WidgetList = _userManager.GetAllWidgets(model.Representative.Value);
                return View("AddAgentV2", model);
            }
            model.ModuleList = _userManager.GetAllModules(0);
            model.WidgetList = _userManager.GetAllWidgets(0);
            return View("AddAgentV2", model);
        }

        [AjaxOnly, HttpPost]
        public async Task<JsonResult> SendOTP()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.SendOTP();
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositOTP);
                if (emailTemplate.TemplateStatus)
                {
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%otp%", result.Object);
                    body = body.Replace("%USER%", LOGGEDIN_USER.FirstName);
                    var currentUser = LOGGEDIN_USER.UserID;
                    Utilities.SendEmail(User.Identity.Name, emailTemplate.EmailSubject, body);
                    Utilities.SendEmail("vblell@gmail.com", emailTemplate.EmailSubject, body);
                }

                var user = _userManager.GetAppUserProfile(LOGGEDIN_USER.UserID);
                if (user != null)
                {
                    var msg = new SendSMSRequest
                    {
                        Recipient = "232" + user.Phone,
                        Payload = $"Greetings {user.Name} \n" +
                          $"To Approve deposits, please use the following OTP (One Time Passcode). {result.Object}\n" +
                          "VENDTECH"
                    };
                    await _smsManager.SendSmsAsync(msg);
                }
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }

        [AjaxOnly, HttpPost]
        public JsonResult AddDeposit(DepositToAdmin request)
        {
            if (request.PosId == 0)
            {
                return JsonResult(new ActionOutput { Message = "POS Required", Status = ActionStatus.Error });
            }

            if (request.ValueDate == null)
                request.ValueDate = DateTime.UtcNow.ToString();
            else
                request.ValueDate = request.ValueDate;

            try
            {
                
                var depositCr = new Deposit
                {
                    Amount = request.Amount,
                    POSId = request.PosId,
                    BankAccountId = request.BankAccountId,
                    CreatedAt = DateTime.UtcNow,
                    PercentageAmount = request.Amount,
                    CheckNumberOrSlipId = Utilities.TrimLeadingZeros(request.ChkOrSlipNo),
                    ChequeBankName = request.Bank,
                    ValueDate = request.ValueDate,
                    NameOnCheque = request.NameOnCheque,
                    PaymentType = request.PaymentType,
                    ValueDateStamp = request.ValueDate == null ? DateTime.UtcNow : Convert.ToDateTime(request.ValueDate),
                };

               var result =  _depositManager.DepositToAgencyAdminAccount(depositCr, LOGGEDIN_USER.UserID, request.OTP);

               if(result.Status == ActionStatus.Successfull)
                {
                    var pos = _posManager.GetSinglePos(request.PosId);
                    if (pos != null && pos.EmailNotificationDeposit == true)
                    {
                        var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.TransferToNotification);
                        if (emailTemplate.TemplateStatus)
                        {
                            string body = emailTemplate.TemplateContent;
                            body = body.Replace("%USER%", pos.User.Name);
                            Utilities.SendEmail(pos.User.Email, emailTemplate.EmailSubject, body);
                        }
                    }
                }
                return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
            }
            catch (Exception ex)
            {
                return JsonResult(new ActionOutput { Message = $"Error occurred!!  {ex?.Message}", Status = ActionStatus.Error });
            }
        }
      
        
        public ActionResult GetAgentPercentage(long vendorId)
        {
            return Json(new { percentage = _agencyManager.GetAgentPercentage(vendorId) }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeleteAgent(long agentId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.DeleteAgency(agentId));
        }
        #endregion
    }
}