using System.Web.Mvc;

namespace VendTech.Areas.Agent
{
    public class AgentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Agent";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Agent_default",
                "Agent/{controller}/{action}/{id}",
                new { action = "Index",controller="Home", id = UrlParameter.Optional }
            );
        }
    }
}