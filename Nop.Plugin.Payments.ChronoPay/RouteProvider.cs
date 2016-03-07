using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.ChronoPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //IPNHandler
            routes.MapRoute("Plugin.Payments.ChronoPay.IPNHandler",
                 "Plugins/PaymentChronoPay/IPNHandler",
                 new { controller = "PaymentChronoPay", action = "IPNHandler" },
                 new[] { "Nop.Plugin.Payments.ChronoPay.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
