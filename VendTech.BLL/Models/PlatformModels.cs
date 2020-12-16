using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
  public  class PlatformModel
    {
        public int PlatformId { get; set; }
        public string ShortName { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
    }

  public class SavePlatformModel
  {
      public int? Id { get; set; }
      public string ShortName { get; set; }
      public string Title { get; set; }
  }
}
