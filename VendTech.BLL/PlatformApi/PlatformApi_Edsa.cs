using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.PlatformApi
{
    public class PlatformApi_Edsa : AbstractPlatformApiConnection
    {
        public override ExecutionResponse CheckStatus(ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }

        public override ExecutionResponse Execute(ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFields()
        {
            return GetApiConfig();
        }

        private static IDictionary<string, PlatformApiConfig> GetApiConfig()
        {
            Dictionary<string, PlatformApiConfig> configFields = new Dictionary<string, PlatformApiConfig>()
            {
                { "url", new PlatformApiConfig { Name = "URL", IsUrl = true, HtmlCssClass = "form-control"} },
                { "username", new PlatformApiConfig { Name = "Username", HtmlCssClass = "form-control" } },
                { "password", new PlatformApiConfig { Name = "Password", HtmlCssClass = "form-control" } },
                { "testField", new PlatformApiConfig {
                        Name = "Test Field",
                        HtmlCssClass = "form-control",
                        IsPerPlatformProductParam = true,
                        HtmlStyle = "width: 30%"
                    }
                }
            };

            return configFields;
        }
    }
}
