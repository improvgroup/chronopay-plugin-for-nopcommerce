using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.ChronoPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //IPNHandler
            routeBuilder.MapRoute("Plugin.Payments.ChronoPay.IPNHandler",
                 "Plugins/PaymentChronoPay/IPNHandler",
                 new { controller = "PaymentChronoPay", action = "IPNHandler" });
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
