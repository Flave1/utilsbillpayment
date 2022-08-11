using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VendTech.BLL.Models
{
  public  class PlatformModel
    {
        public int PlatformId { get; set; }
        public string ShortName { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
        public string Logo { get; set; }
        public decimal MinimumAmount { get; set; }
        public bool DisablePlatform { get; set; }
        public string DiabledPlaformMessage { get; set; }
    }

  public class SavePlatformModel
  {
      public int? Id { get; set; }
      public string ShortName { get; set; }
      public string Title { get; set; }
        public HttpPostedFile Image { get; set; }
        public HttpPostedFileBase ImagefromWeb { get; set; }
        public string Logo { get; internal set; }
        public decimal MinimumAmount { get; set; }
        public bool DisablePlatform { get; set; } = false;
        public string DiabledPlaformMessage { get; set; }
    }
}
