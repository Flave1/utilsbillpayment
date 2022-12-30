using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using PagedList;
using System.Linq.Dynamic;
using System.Data.Entity;
using VendTech.DAL;
using Newtonsoft.Json;
using VendTech.BLL.PlatformApi;
using System.Web.Util;
using System.Runtime.Remoting.Contexts;
using System.Data.SqlClient;
using System.Diagnostics;
using VendTech.BLL.Common;

namespace VendTech.BLL.Managers
{
    public class PlatformTransactionManager : BaseManager, IPlatformTransactionManager
    {
        private IPlatformApiManager _platformApiManager;
        private IPlatformManager _platformManager;

        public PlatformTransactionManager(IPlatformApiManager platformApiManager,
            IPlatformManager platformManager)
        {
            _platformApiManager = platformApiManager;
            _platformManager = platformManager;
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

            return PlatformTransactionModel.From(platformTransaction);
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

                var pendingTranx = DbCtx.PlatformTransactions
                    .Where(t => t.Status == (int)TransactionStatus.Pending)
                    .Where(t => t.LastPendingCheck < lastPendingCheck)
                    .FirstOrDefault();

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
                    VendTech.DAL.PlatformApiLog log = new VendTech.DAL.PlatformApiLog
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
                    if (execResponse.Status != (int) TransactionStatus.Pending)
                    {
                        pendingTranx.Status = execResponse.Status;
                        pendingTranx.OperatorReference = execResponse.OperatorReference;
                        pendingTranx.PinNumber = execResponse.PinNumber;
                        pendingTranx.PinSerial = execResponse.PinSerial;
                        pendingTranx.PinInstructions = execResponse.PinInstructions;
                        pendingTranx.ApiTransactionId = execResponse.ApiTransactionId;
                        pendingTranx.UpdatedAt = DateTime.UtcNow;

                        DbCtx.SaveChanges();
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
    }
}
