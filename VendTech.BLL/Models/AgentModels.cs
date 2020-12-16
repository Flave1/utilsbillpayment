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
        public string Company { get; set; }
        public string AgencyName { get; set; }
        public string AgentType { get; set; }
        public decimal Percentage { get; set; }
        public AgencyListingModel(Agency obj)
        {
            AgencyId = obj.AgencyId;
            Company = obj.Company;
            AgencyName = obj.AgencyName;
            //AgentType = ((AgentTypeEnum)obj.AgentType).ToString();
            //Percentage = obj.Commission.Percentage;
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
        public long AgentId { get; set; }
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
    }
}
