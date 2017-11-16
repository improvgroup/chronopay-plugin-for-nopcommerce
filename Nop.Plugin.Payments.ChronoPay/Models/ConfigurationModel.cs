using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.ChronoPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.ChronoPay.GatewayUrl")]
        public string GatewayUrl { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChronoPay.ProductId")]
        public string ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChronoPay.ProductName")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChronoPay.SharedSecrect")]
        public string SharedSecrect { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChronoPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}