using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.PlatformApi
{
    public class PlatformApiConfig
    {
        public PlatformApiConfig() {
            //default is text
            FieldType = FieldType.TEXT;
        }
        public string Name { get; set; }
        public bool IsUrl { get; set; }
        public string HtmlStyle { get; set; }
        public string HtmlCssClass { get; set; }
        public bool Optional { get; set; }
        public bool IsPerPlatformProductParam { get; set; }
        //If the type is a dropdown, then this will be its possible values.
        //The key is the actual value that will be set.
        public IDictionary<string, string> Values { get; set; }
        public FieldType FieldType { get; set; }
    }

    public enum ConfigDataType
    {
        String = 0,
        Number = 0,
    }

    public enum FieldType
    {
        TEXT = 0,
        DROPDOWN = 1, //HTML select element and the likes
        RADIO = 3, //Radio buttons or single option element fields
    }
}
