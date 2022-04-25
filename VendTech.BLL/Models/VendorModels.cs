using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Common;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
 public   class VendorListingModel
    {
        public long VendorId { get; set; }
        public string VendorName { get; set; }
        public string AgencyName { get; set; }
        public string StringId { get; set; }
        public decimal Balance { get; set; }
        public string Name { get; set; } 
        public string SurName { get; set; } 
        public string Phone { get; set; } 
        public string Address { get; set; }
        public VendorListingModel(User obj)
        {
            VendorId = obj.UserId;
            VendorName = obj.Vendor;
            AgencyName = obj.Agency==null?"":obj.Agency.AgencyName;
            Name = obj.Name;
            SurName = obj.SurName;
            Phone = obj.Phone;
            Address = obj.Address;
            //Balance = obj.Balance==null?0:obj.Balance.Value;
            StringId = Utilities.Base64Encode(obj.UserId.ToString());
        }
    }

 public class SaveVendorModel
 { 
        public string Name { get; set; } 
        public string SurName { get; set; } 
        public string Vendor { get; set; }

        public string Address { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; } 
        [MaxLength(10)]
        public string Phone { get; set; }
        public string CountryCode { get; set; }
        public long AgencyId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public long VendorId { get; set; } 
        public string Password { get; set; }
        public decimal AgentPercentage { get; set; } 
        public string ConfirmPassword { get; set; }
        public int? VendorType { get; set; }
        public int? Percentage { get; set; }

        //public long? POSId { get; set; }
    }

    public class TransferFromVendors
    {
        public string VendorName { get; set; }
        public long PosId { get; set; }
        public string Serialnumber { get; set; }
        public string Balance { get; set; }
    }
}
