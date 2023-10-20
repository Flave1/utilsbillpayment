using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using PagedList;
using System.Linq.Dynamic;
using System.Data.Entity;
using VendTech.DAL;
using Newtonsoft.Json;
using VendTech.BLL.PlatformApi;
using VendTech.BLL.Common;
using System.Data.Entity.Core;

namespace VendTech.BLL.Managers
{
    public class PlatformTransactionManager : BaseManager, IPlatformTransactionManager
    {
        private IPlatformApiManager _platformApiManager;
        private IPlatformManager _platformManager;
        private IErrorLogManager _errorLog;
        public PlatformTransactionManager(IPlatformApiManager platformApiManager,
            IPlatformManager platformManager,
            IErrorLogManager errorLog)
        {
            _platformApiManager = platformApiManager;
            _platformManager = platformManager;
            _errorLog = errorLog;
        }

        public PlatformTransactionModel New(long userId, int platformId, long posId, decimal amount, string beneficiary, string currency, int? apiConnId)
        {
            //TODO - validate input


            VendTech.DAL.PlatformTransaction platformTransaction = new VendTech.DAL.PlatformTransaction
            {
                UserId = userId,
                PlatformId = platformId,
                Amount = amount,
                Beneficiary = beneficiary,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PosId = posId,
                Status = (int) TransactionStatus.InProgress,
                ApiConnectionId = apiConnId,
                LastPendingCheck = 0,
            };

            Context.PlatformTransactions.Add(platformTransaction);
            Context.SaveChanges();

            return Context.PlatformTransactions.Where(x => x.Id == platformTransaction.Id).Select(PlatformTransactionModel.Projection).FirstOrDefault();
        }

        public bool ProcessTransactionViaApi(long transactionId)
        {
            if (transactionId > 0)
            {
                VendTech.DAL.PlatformTransaction tranx = GetPendingTransactionById(transactionId);
                PlatformModel platform;
                if (tranx != null)
                {
                    if (tranx.ApiConnectionId < 1)
                    {
                        platform = _platformManager.GetPlatformById(tranx.PlatformId);
                        if (platform != null && platform.PlatformId > 0 && platform.PlatformApiConnId > 0)
                        {
                            //update the connection of the transaction
                            tranx.ApiConnectionId = platform.PlatformApiConnId;
                            tranx.UpdatedAt = DateTime.UtcNow;

                            Context.SaveChanges();
                        }
                    }
                    
                    if (tranx.ApiConnectionId < 1)
                    {
                        FlagTransactionWithStatus(tranx, TransactionStatus.Error);
                        return false;
                    }
                    
                    VendTech.DAL.PlatformApiConnection platformApiConnection = 
                            Context.PlatformApiConnections.Where(p => p.Id == tranx.ApiConnectionId).FirstOrDefault();

                    if (platformApiConnection == null)
                    {
                        FlagTransactionWithStatus(tranx, TransactionStatus.Error);
                        return false;
                    }
                    
                    ExecutionContext executionContext = new ExecutionContext();

                    //PlatformApi config
                    string config = platformApiConnection.PlatformApi.Config;
                    executionContext.PlatformApiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);

                    //Get the Per Platform API Conn Params
                    PlatformPacParams platformPacParams = _platformApiManager.GetPlatformPacParams(tranx.PlatformId, (int) tranx.ApiConnectionId);
                    if (platformPacParams != null && platformPacParams.ConfigDictionary != null)
                    {
                         executionContext.PerPlatformParams = platformPacParams.ConfigDictionary;
                    }

                    executionContext.Amount= tranx.Amount;

                    //For now we will configure both MSISDN and Account ID
                    //TODO: we should know by platform type what field we should populate
                    executionContext.Msisdn = tranx.Beneficiary;
                    executionContext.AccountId = tranx.Beneficiary;

                    IPlatformApi api = _platformApiManager.GetPlatformApiInstanceByTypeId(platformApiConnection.PlatformApi.ApiType);
                    ExecutionResponse execResponse = api.Execute(executionContext);

                    //Save the logs
                    string logJSON = JsonConvert.SerializeObject(execResponse);
                    VendTech.DAL.PlatformApiLog log = new VendTech.DAL.PlatformApiLog
                    {
                        TransactionId = tranx.Id,
                        LogType = (int)ApiLogType.InitialRequest,
                        ApiLog = logJSON,
                        LogDate = DateTime.UtcNow
                    };

                    Context.PlatformApiLogs.Add(log);
                    Context.SaveChanges();

                    //Fetch from DB
                    tranx = GetPendingTransactionById(transactionId);
                    tranx.Status = execResponse.Status;
                    tranx.UserReference = execResponse.UserReference;
                    tranx.OperatorReference = execResponse.OperatorReference;
                    tranx.PinNumber = execResponse.PinNumber;
                    tranx.PinSerial = execResponse.PinSerial;
                    tranx.PinInstructions = execResponse.PinInstructions;
                    tranx.ApiTransactionId = execResponse.ApiTransactionId;
                    tranx.UpdatedAt = DateTime.UtcNow;

                    Context.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public void CheckPendingTransaction()
        { 
            using (var DbCtx = new VendtechEntities())
            {
                //Last pending check done 1 min ago. This is to avoid checking the status within too short intervals
                long lastPendingCheck = Utilities.ToUnixTimestamp(DateTime.UtcNow) - 60;
                PlatformTransaction pendingTranx = null;
                if (lastPendingCheck != null)
                {
                    try
                    {
                        pendingTranx = DbCtx.PlatformTransactions
                                        .Where(t => t.Status == (int)TransactionStatus.Pending)
                                        .Where(t => t.LastPendingCheck < lastPendingCheck)
                                        .FirstOrDefault();
                    }
                    catch (EntityCommandExecutionException)
                    {
                        return;
                    }
                    
                    try
                    {
                        if (pendingTranx != null && pendingTranx.ApiConnectionId > 0)
                        {
                            pendingTranx.LastPendingCheck = Utilities.ToUnixTimestamp(DateTime.UtcNow);
                            DbCtx.SaveChanges();

                            ExecutionContext executionContext = new ExecutionContext
                            {
                                UserReference = pendingTranx.UserReference,
                                ApiTransactionId = pendingTranx.ApiTransactionId
                            };

                            VendTech.DAL.PlatformApiConnection platformApiConnection =
                                    DbCtx.PlatformApiConnections.Where(p => p.Id == pendingTranx.ApiConnectionId).FirstOrDefault();

                            //PlatformApi config
                            string config = platformApiConnection.PlatformApi.Config;
                            executionContext.PlatformApiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);

                            //Get the Per Platform API Conn Params
                            VendTech.DAL.PlatformPacParam platformPacParam = DbCtx.PlatformPacParams.FirstOrDefault(
                            p => p.PlatformId == pendingTranx.PlatformId && p.PlatformApiConnectionId == pendingTranx.ApiConnectionId);

                            PlatformPacParams platformPacParams = PlatformPacParams.From(platformPacParam);
                            if (platformPacParams != null && platformPacParams.ConfigDictionary != null)
                            {
                                executionContext.PerPlatformParams = platformPacParams.ConfigDictionary;
                            }

                            IPlatformApi api = _platformApiManager.GetPlatformApiInstanceByTypeId(platformApiConnection.PlatformApi.ApiType);
                            ExecutionResponse execResponse = api.CheckStatus(executionContext);

                            //Save the logs
                            string logJSON = JsonConvert.SerializeObject(execResponse);
                            PlatformApiLog log = new PlatformApiLog
                            {
                                TransactionId = pendingTranx.Id,
                                LogType = (int)ApiLogType.PendingCheckRequest,
                                ApiLog = logJSON,
                                LogDate = DateTime.UtcNow
                            };

                            DbCtx.PlatformApiLogs.Add(log);
                            DbCtx.SaveChanges();

                            //Fetch from DB
                            pendingTranx = DbCtx.PlatformTransactions.Where(t => t.Id == pendingTranx.Id).FirstOrDefault();
                            if (execResponse.Status != (int)TransactionStatus.Pending)
                            {
                                pendingTranx.Status = execResponse.Status;
                                pendingTranx.OperatorReference = execResponse.OperatorReference;
                                pendingTranx.PinNumber = execResponse.PinNumber;
                                pendingTranx.PinSerial = execResponse.PinSerial;
                                pendingTranx.PinInstructions = execResponse.PinInstructions;
                                pendingTranx.ApiTransactionId = execResponse.ApiTransactionId;
                                pendingTranx.UpdatedAt = DateTime.UtcNow;

                                DbCtx.SaveChanges();

                                if (pendingTranx.Status == (int)TransactionStatus.Successful)
                                {
                                    PlatformTransactionModel tranxModel = DbCtx.PlatformTransactions
                                                                            .Where(t => t.Id == pendingTranx.Id)
                                                                            .Select(PlatformTransactionModel.Projection)
                                                                            .FirstOrDefault();

                                    TransactionDetail transactionDetail = CreateTransactionDetail(tranxModel);
                                    List<PlatformApiLogModel> logs = DbCtx.PlatformApiLogs
                                                                            .Select(PlatformApiLogModel.Projection)
                                                                            .Where(l => l.TransactionId == pendingTranx.Id)
                                                                            .OrderBy(l => l.LogDate)
                                                                            .ToList();

                                    Logs tranxLogs = CreateLogs(logs);

                                    transactionDetail.Request = tranxLogs.Request.ToString();
                                    transactionDetail.Response = tranxLogs.Response.ToString();

                                    DbCtx.TransactionDetails.Add(transactionDetail);
                                    PlatformTransaction tranx = DbCtx.PlatformTransactions.Where(t => t.Id == tranxModel.Id).FirstOrDefault();
                                    tranx.TransactionDetailId = transactionDetail.TransactionDetailsId;
                                    DbCtx.SaveChanges();
                                }
                                //Transaction failed so reverse balance
                                else
                                {
                                    var pos = DbCtx.POS.FirstOrDefault(p => p.POSId == pendingTranx.PosId);
                                    ReverseBalanceDeduction(DbCtx, pos, pendingTranx.Amount);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                 

                
                
            }
            //SELECT DATEDIFF(SECOND,'1970-01-01', GETUTCDATE())
        }

        public List<PlatformApiLogModel> GetTransactionLogs(long transactionId)
        {
            return Context.PlatformApiLogs
                .Where(o => o.TransactionId == transactionId)
                .Select(PlatformApiLogModel.Projection)
                .OrderBy(o => o.LogDate)
                .ToList();
        }

        public PagingResult<MeterRechargeApiListingModel> GetUserAirtimeRechargeTransactionDetailsHistory(ReportSearchModel model, bool callFromAdmin)
        {
            if (model.RecordsPerPage != 20)
            {
                model.RecordsPerPage = 10;
            }
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = Context.TransactionDetails.OrderByDescending(d => d.CreatedAt)
                .Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null && p.Platform.PlatformType == (int) PlatformTypeEnum.AIRTIME && p.PlatFormId == model.PlatformId);

            if (model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }

            var list = query.Take(model.RecordsPerPage).OrderByDescending(x => x.CreatedAt).AsEnumerable().Select(x => new MeterRechargeApiListingModel(x)).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Airtime recharges fetched successfully.";
            return result;
        }

        public DataTableResultModel<PlatformTransactionModel> GetPlatformTransactionsForDataTable(DataQueryModel query)
        {
            var result = DataTableResultModel<PlatformTransactionModel>.NewResultModel();

            if (query != null)
            {
                result.PagedList = Context.PlatformTransactions
                    .Include(p => p.Platform)
                    .Where(p => query.IsAdmin || (p.UserId == query.UserId))
                    .Where(p => (query.PlatformId < 1) ?  true : p.PlatformId == query.PlatformId)
                    .Where(p => string.IsNullOrEmpty(query.Reference) ? true : p.OperatorReference.ToLower() == query.Reference.ToLower())
                    .Where(p => string.IsNullOrEmpty(query.Beneficiary) ? true : p.Beneficiary.ToLower() == query.Beneficiary.ToLower())
                    .Where(p => query.FromDate == null || p.CreatedAt >= query.FromDate)
                    .Where(p => query.ToDate == null || p.CreatedAt <= query.ToDate)
                    .Where(p => query.Status < 0 || p.Status == query.Status)
                    .Where(p => query.ApiConnId <= 0 || p.ApiConnectionId == query.ApiConnId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(PlatformTransactionModel.Projection).ToPagedList(query.Page, query.PageSize);   
            }

            return result;
        }
        private static void FlagTransactionWithStatus(VendTech.DAL.PlatformTransaction tranx, TransactionStatus status)
        {
            if (tranx != null)
            {
                tranx.Status = (int)status;
                tranx.UpdatedAt = DateTime.UtcNow;
                Context.SaveChanges();
            }
        }

        private static VendTech.DAL.PlatformTransaction GetPendingTransactionById(long id)
        {
            VendTech.DAL.PlatformTransaction tranx = Context.PlatformTransactions
                .Where(p => 
                    p.Id == id &&
                    (p.Status == (int)TransactionStatus.Pending || p.Status == (int)TransactionStatus.InProgress)
                 )
                .FirstOrDefault();
            return tranx;
        }

        public PlatformTransactionModel GetPlatformTransactionById(DataQueryModel query, long id)
        {
            if (id > 0 & query != null)
            {
                var tranx = Context.PlatformTransactions
                                    .Where(p => p.Id == id)
                                    .Where(p => query.IsAdmin ? true : p.UserId == query.UserId)
                                    .Select(PlatformTransactionModel.Projection)
                                    .FirstOrDefault();
                
                if (tranx != null)
                {
                    return tranx;
                }
            }

            return new PlatformTransactionModel();
        }

        public AirtimeReceiptModel RechargeAirtime(PlatformTransactionModel model)
        {
            var response = new AirtimeReceiptModel { ReceiptStatus = new ReceiptStatus() };
            var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            if (user == null)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "User not exist.";
                return response;
            }
            var posid = user.POS.FirstOrDefault().POSId;
            var pos = Context.POS.FirstOrDefault(p => p.POSId == posid);

            if (pos.Balance == null || pos.Balance.Value < model.Amount)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
                return response;
            }

            bool balanceDeducted = false;

            try
            {
                //Deduct the amount from the balance so the user does not go and initiate another transaction while this is still in progress
                pos.Balance = pos.Balance.Value - model.Amount;
                Context.SaveChanges();

                balanceDeducted = true;

                //Is the product configured with a Connection ID
                PlatformApiConnection apiConn = Context.PlatformApiConnections.Where(x => x.PlatformId == model.PlatformId).FirstOrDefault();

                PlatformTransactionModel tranxModel = this.New(model.UserId, model.PlatformId, pos.POSId, model.Amount,
                    model.Beneficiary, model.Currency, apiConn?.Id);

                //Process the transaction via the API and 
                this.ProcessTransactionViaApi(tranxModel.Id);

                int Status = Context.PlatformTransactions.Where(t => t.Id == tranxModel.Id).Select(t => t.Status).FirstOrDefault();

                //If it succeeds, then transfer to TransactionDetail 
                if (Status == (int)TransactionStatus.Successful)
                {
                    TransactionDetail transactionDetail = CreateTransactionDetail(tranxModel);
                    List<PlatformApiLogModel> logs = GetTransactionLogs(tranxModel.Id);
                    Logs tranxLogs = CreateLogs( logs );

                    transactionDetail.Request = tranxLogs.Request.ToString();
                    transactionDetail.Response = tranxLogs.Response.ToString();

                    Context.TransactionDetails.Add(transactionDetail);
                    PlatformTransaction tranx = Context.PlatformTransactions.FirstOrDefault(t => t.Id == tranxModel.Id);
                    tranx.TransactionDetailId = transactionDetail.TransactionDetailsId;

                    transactionDetail.TenderedAmount = model.Amount;
                    transactionDetail.Amount = model.Amount;
                    transactionDetail.CurrentVendorBalance = pos.Balance;
                    transactionDetail.BalanceBefore = (pos.Balance + model.Amount);
                    Context.SaveChanges();
                    response = GenerateReceipt(transactionDetail);
                    Push_notification_to_user(user, model, transactionDetail.TransactionDetailsId);
                    return response;
                }
                else if (Status == (int)TransactionStatus.Pending)
                {
                    response.ReceiptStatus.Status = "pending";
                    response.ReceiptStatus.Message = "Airtime recharge is pending";
                    return response;
                }else if(Status == (int)TransactionStatus.Failed)
                {
                    if (balanceDeducted)
                    {
                        ReverseBalanceDeduction(Context, pos, model.Amount);
                    }
                }
                else
                {
                    if (balanceDeducted)
                    {
                        ReverseBalanceDeduction(Context, pos, model.Amount);
                    }
                }

                response.ReceiptStatus.Status = "pending";
                response.ReceiptStatus.Message = "Airtime recharge failed.";
                return response;
            }
            catch(Exception ex)
            {
                _errorLog.LogExceptionToDatabase(ex);
                //If balance was deducted before exception then reverse
                if (balanceDeducted)
                {
                    ReverseBalanceDeduction(Context, pos, model.Amount);
                }
                response.ReceiptStatus.Status = "pending";
                response.ReceiptStatus.Message = "Airtime recharge failed due to an error. Please contact Administrator";
                return response;
            }
        }


        private void Push_notification_to_user(User user, PlatformTransactionModel model, long MeterRechargeId)
        {
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = MeterRechargeId;
            obj.Title = "Airtime recharged successfully";
            obj.Message = $"Your phone has successfully recharged with NLe {Utilities.FormatAmount(model.Amount)}";
            obj.NotificationType = NotificationTypeEnum.AirtimeRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                PushNotification.SendNotification(obj);
            }
        }

        private AirtimeReceiptModel GenerateReceipt(TransactionDetail trax)
        {
            var receipt = new AirtimeReceiptModel();
            receipt.Phone = trax?.MeterNumber1 ?? "";
            receipt.POS = trax?.POS?.SerialNumber ?? "";
            receipt.CustomerName = trax?.User.Vendor ?? "";
            receipt.ReceiptNo = trax?.ReceiptNumber ?? "";
            var amt = trax?.Amount.ToString("N");
            receipt.Amount = amt.Contains('.') ? amt.TrimEnd('0').TrimEnd('.') : amt;
            receipt.Charges = Utilities.FormatAmount(Convert.ToDecimal(trax.ServiceCharge));
            receipt.Commission = string.Format("{0:N0}", 0.00);
            receipt.Discount = string.Format("{0:N0}", 0);
            receipt.TransactionDate = trax.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            receipt.VendorId = trax.User.Vendor;
            receipt.EDSASerial = trax?.SerialNumber?? "";
            receipt.VTECHSerial = trax.TransactionId ?? "";
            receipt.mobileShowPrintButton = (bool)trax.POS.PosPrint;
            receipt.mobileShowSmsButton = (bool)trax.POS.PosSms;
            receipt.ShouldShowSmsButton = (bool)trax.POS.WebSms;
            receipt.ShouldShowPrintButton = (bool)trax.POS.WebPrint;

            if(trax.PlatFormId == 2)
                receipt.ReceiptTitle = "ORANGE";
            if (trax.PlatFormId == 3)
                receipt.ReceiptTitle = "AFRICELL";
            if (trax.PlatFormId == 4)
                receipt.ReceiptTitle = "QCELL";

            receipt.IsNewRecharge = true;
            return receipt;
        }
        private static void ReverseBalanceDeduction(VendtechEntities dbCtx, VendTech.DAL.POS pos, decimal amount)
        {
            pos.Balance = pos.Balance.Value + amount;
            dbCtx.SaveChanges();
        }

        private static TransactionDetail CreateTransactionDetail(PlatformTransactionModel tranxModel)
        {
            if (tranxModel == null)
            {
                throw new ArgumentNullException("PlatformTransaction to covert to TransactionDetail cannot be null");
            }

            var now = DateTime.UtcNow;

            var tranxDetail = new TransactionDetail
            {
                UserId = tranxModel.UserId,
                POSId = tranxModel.PosId,
                MeterNumber1 = tranxModel.Beneficiary,
                Amount = tranxModel.Amount,
                PlatFormId = tranxModel.PlatformId,
                TransactionId = Utilities.GetLastMeterRechardeId(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = now,
                RequestDate = now,
                Finalised = true,
                TaxCharge = "",
                Units = "",
                DebitRecovery = "",
                CostOfUnits = "",
            };

            return tranxDetail;
        }

        private static Logs CreateLogs(List<PlatformApiLogModel> logs)
        {
            StringBuilder request = new StringBuilder();
            StringBuilder response = new StringBuilder();

            if (logs != null && logs.Count > 0)
            {
                foreach (var log in logs)
                {
                    if (log.LogType == (int)ApiLogType.InitialRequest)
                    {
                        request.Append("Initial Request:\n");
                    }
                    else
                    {
                        request.Append("\n\nPending Request:\n");
                    }

                    ExecutionResponse execRes = log.ApiLogJson;
                    List<ApiRequestInfo> apiRequestInfos = execRes.ApiCalls;
                    if (apiRequestInfos.Count > 0)
                    {
                        foreach (ApiRequestInfo reqInfo in apiRequestInfos)
                        {
                            request.Append("Request Sent => ").Append(reqInfo.RequestSentStr).Append("\n")
                            .Append("Payload => ").Append(reqInfo.Request).Append("\n");

                            response.Append("Response Received => ").Append(reqInfo.ResponseReceivedStr).Append("\n")
                                .Append("Payload => ").Append(reqInfo.Response).Append("\n");
                        }
                    }
                }
            }

            return new Logs { Request = request, Response = response };
        }

        public AirtimeReceiptModel GetAirtimeReceipt(string traxId)
        {
            var trax = Context.TransactionDetails.Where(e => e.TransactionId == traxId).ToList().FirstOrDefault();
            if (trax != null)
            {
                var receipt = GenerateReceipt(trax);
                return receipt;
            }
            return new AirtimeReceiptModel { ReceiptStatus = new ReceiptStatus { Status = "unsuccessful", Message = "Unable to find voucher" } };
        }

        ReceiptModel IPlatformTransactionManager.ReturnAirtimeReceipt(string rechargeId)
        {
            var transaction_by_token = Context.TransactionDetails.Where(e => e.TransactionId == rechargeId).FirstOrDefault();
            if (transaction_by_token != null)
            {
                var receipt = Build_receipt_model_from_dbtransaction_detail(transaction_by_token);
                receipt.ShouldShowSmsButton = (bool)transaction_by_token.POS.WebSms;
                receipt.ShouldShowPrintButton = (bool)transaction_by_token.POS.WebPrint;
                receipt.mobileShowSmsButton = (bool)transaction_by_token.POS.PosSms;
                receipt.mobileShowPrintButton = (bool)transaction_by_token.POS.PosPrint;
                return receipt;
            }
            return new ReceiptModel { ReceiptStatus = new ReceiptStatus { Status = "unsuccessful", Message = "Unable to find voucher" } };
        }

        public ReceiptModel Build_receipt_model_from_dbtransaction_detail(TransactionDetail model)
        {
            if (model.POS == null) model.POS = new POS();
            var receipt = new ReceiptModel();
            receipt.AccountNo = model?.MeterNumber1;
            receipt.POS = model?.POS?.SerialNumber;
            receipt.CustomerName = model?.Customer;
            receipt.ReceiptNo = model?.ReceiptNumber;
            receipt.Amount = Utilities.FormatAmount(Convert.ToDecimal(model.Amount));
            receipt.TransactionDate = model.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            receipt.VendorId = model.User.Vendor;
            receipt.EDSASerial = model.SerialNumber;
            receipt.VTECHSerial = model.TransactionId;
            receipt.PlatformId = model.PlatFormId;
            return receipt;
        }
    }


    internal class Logs
    {
        public StringBuilder Request { get; set; }
        public StringBuilder Response { get; set; }
    }
    
}
