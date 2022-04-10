using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class BankAccountModel
    {
        [Required]
        public string BankName { get; set; }
        [Required]
        public string AccountName { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string BBAN { get; set; }
        public int BankAccountId { get; set; }
    }
    public  class CheqbankList
    {
    
        public int Id { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
    
    }

    public class ChequeBankModel
    {
        public int ChequeBanktId { get; set; }
        [Required]
        public string BankName { get; set; } 
        [Required]
        public string BBAN { get; set; } 
        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; } 
    }
}
