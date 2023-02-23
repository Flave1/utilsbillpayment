using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace VendTech.BLL.Models
{
    public class AirtimePurchaseModel
    {
        public int PlatformId { get; set; }
        public long Amount { get; set; }
        public long UserId { get; set; }
        public long PosId { get; set; }
        public string Phone { get; set; }
        public string Currency { get; set; }
    }
    public class PlatformTransactionModel : LongIdentifierModelBase
    {
        public int? ApiConnectionId { get; set; }
        [Required]
        public int PlatformId { get; set; }
        public long UserId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Beneficiary { get; set; }
        public string Currency { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        public decimal AmountPlatform { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }

        public string PlatformTypeName { get; set; }

        [Required]
        public long? PosId { get; set; }

        public string PlatformName { get; set; }
        public string OperatorReference { get; set; }
        public string PinNumber { get; set; }
        public string PinSerial { get; set; }
        public string PinInstructions { get; set; }
        public string ApiTransactionId { get; set; }
        public long LastPendingCheck { get; set; }
        public string UserReference { get; set; }
        public string ApiConnectionName { get; set; }
        public string PlatformApiName { get; set; }
        public int PlatformApiId { get; set; }
        public long? TransactionDetailId { get; set; }

        public string CreatedAtStr { 
            get { return CreatedAt.ToString(ModelUtils.DISPLAY_DATE_FORMAT); } 
        }

        public List<MeterRechargeApiListingModel> History { get; set; }

        public static PlatformTransactionModel From(VendTech.DAL.PlatformTransaction x)
        {
            var model = new PlatformTransactionModel();
            model.Id = x.Id;
            model.ApiConnectionId = x.ApiConnectionId;
            model.ApiConnectionName = x.PlatformApiConnection.Name;
            model.PlatformId = x.PlatformId;
            model.UserId = x.UserId;
            model.Beneficiary = x.Beneficiary;
            model.Amount = x.Amount;
            model.AmountPlatform = x.AmountPlatform;
            model.Currency = x.Currency;
            model.PosId = x.PosId;
            model.CreatedAt = x.CreatedAt;
            model.UpdatedAt = x.UpdatedAt;
            model.Status = x.Status;
            model.PlatformName = x.Platform.Title;
            model.OperatorReference = x.OperatorReference;
            model.PinNumber = x.PinNumber;
            model.PinSerial = x.PinSerial;
            model.PinInstructions = x.PinInstructions;
            model.ApiTransactionId = x.ApiTransactionId;
            model.StatusName = ((TransactionStatus)x.Status).ToString();
            model.PlatformTypeName = ((PlatformTypeEnum)x.Platform.PlatformType).ToString();
            model.LastPendingCheck = x.LastPendingCheck;
            model.UserReference = x.UserReference;
            model.TransactionDetailId = x.TransactionDetailId;

            return model;
        }

        public static Expression<Func<VendTech.DAL.PlatformTransaction, PlatformTransactionModel>> Projection
        {
            get
            {
                return x => new PlatformTransactionModel()
                {
                    Id = x.Id,
                    ApiConnectionId = x.ApiConnectionId,
                    ApiConnectionName = (x.PlatformApiConnection != null) ? x.PlatformApiConnection.Name : null,
                    PlatformId = x.PlatformId,
                    UserId = x.UserId,
                    Beneficiary = x.Beneficiary,
                    Amount = x.Amount,
                    AmountPlatform = x.AmountPlatform,
                    Currency = x.Currency,
                    PosId = x.PosId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Status = x.Status,
                    PlatformName = x.Platform.Title,
                    OperatorReference = x.OperatorReference,
                    PinNumber = x.PinNumber,
                    PinSerial = x.PinSerial,
                    PinInstructions = x.PinInstructions,
                    ApiTransactionId = x.ApiTransactionId,
                    StatusName = ((TransactionStatus)x.Status).ToString(),
                    PlatformTypeName = ((PlatformTypeEnum)x.Platform.PlatformType).ToString(),
                    LastPendingCheck = x.LastPendingCheck,
                    UserReference = x.UserReference,
                    PlatformApiName = (x.PlatformApiConnection != null) ? x.PlatformApiConnection.PlatformApi.Name : null,
                    PlatformApiId = (x.PlatformApiConnection != null) ? x.PlatformApiConnection.PlatformApi.Id : 0,
                    TransactionDetailId = x.TransactionDetailId
            };
            }
        }
    }
}
