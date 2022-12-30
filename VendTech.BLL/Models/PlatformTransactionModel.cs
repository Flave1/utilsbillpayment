using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class PlatformTransactionModel : LongIdentifierModelBase
    {
        [Required]
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

        public string CreatedAtStr { 
            get { return CreatedAt.ToString(ModelUtils.DISPLAY_DATE_FORMAT); } 
        }

        public static PlatformTransactionModel From(VendTech.DAL.PlatformTransaction x)
        {
            return new PlatformTransactionModel {
                Id = x.Id,
                ApiConnectionId = x.ApiConnectionId,
                ApiConnectionName = x.PlatformApiConnection.Name,
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
               
            };
        }

        public static Expression<Func<VendTech.DAL.PlatformTransaction, PlatformTransactionModel>> Projection
        {
            get
            {
                return x => new PlatformTransactionModel()
                {
                    Id = x.Id,
                    ApiConnectionId = x.ApiConnectionId,
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
                    PlatformApiName = x.PlatformApiConnection.PlatformApi.Name,
                    PlatformApiId = x.PlatformApiConnection.PlatformApi.Id
                };
            }
        }
    }
}
