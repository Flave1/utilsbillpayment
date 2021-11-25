using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class AgencyListingModel
    {
        public long AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string Admin { get; set; }
        public string AgentType { get; set; }
        public decimal Percentage { get; set; }
        public AgencyListingModel(Agency obj)
        {
            AgencyId = obj.AgencyId;
            AgencyName = obj.AgencyName;
            Admin = obj?.User?.Name + " " + obj?.User?.SurName;
            //AgentType = ((AgentTypeEnum)obj.AgentType).ToString();
            Percentage = obj?.Commission?.Percentage?? 0;
        }
    }

    public class AgentListingModel
    {
        public long POSID { get; set; }
        public string SerialNumber { get; set; }
        public string AgencyName { get; set; }
        public string CellPhone { get; set; }
        public string AgentName { get; set; }
        public bool Enabled { get; set; }
        public string TodaySales { get; set; }
        public string Balance { get; set; }  
        public string Vendor { get; set; }
        public AgentListingModel(POS obj)
        {
            var sale = obj.TransactionDetails.Where(f => f.CreatedAt.Date == DateTime.UtcNow.Date).Select(d => d.Amount)?.Sum() ?? 0;
            POSID = obj.POSId;
            SerialNumber = obj.SerialNumber;
            AgencyName = obj?.User?.Agency?.AgencyName;
            CellPhone = "+232" + obj.Phone;
            AgentName = $"{obj?.User?.Name} {obj?.User?.SurName}";
            Enabled = (bool)obj.Enabled;
            TodaySales = string.Format("{0:N0}", sale) ;
            Balance = string.Format("{0:N0}", obj?.Balance ?? 0);
            Vendor = obj?.User?.Vendor;
        }

    }
        public class AddAgentModel : SaveAgentModel
        {

            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public class SaveAgentModel
        {
            public string Company { get; set; } 
            [Required(ErrorMessage = "Required")]
            public string AgencyName { get; set; }
            [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Invalid Email")]
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string Phone { get; set; }
            public string CountryCode { get; set; }
            public int AgentType { get; set; }
            public int Percentage { get; set; }
        public long? Representative { get; set; }
        public long AgencyId { get;  set; }
    } 
}
