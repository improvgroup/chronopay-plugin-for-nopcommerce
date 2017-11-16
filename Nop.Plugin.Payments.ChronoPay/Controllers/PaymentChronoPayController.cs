using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.ChronoPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.ChronoPay.Controllers
{
    public class PaymentChronoPayController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ChronoPayPaymentSettings _chronoPayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public PaymentChronoPayController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ChronoPayPaymentSettings chronoPayPaymentSettings,
            PaymentSettings paymentSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._chronoPayPaymentSettings = chronoPayPaymentSettings;
            this._paymentSettings = paymentSettings;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                GatewayUrl = _chronoPayPaymentSettings.GatewayUrl,
                ProductId = _chronoPayPaymentSettings.ProductId,
                ProductName = _chronoPayPaymentSettings.ProductName,
                SharedSecrect = _chronoPayPaymentSettings.SharedSecrect,
                AdditionalFee = _chronoPayPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.ChronoPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _chronoPayPaymentSettings.GatewayUrl = model.GatewayUrl;
            _chronoPayPaymentSettings.ProductId = model.ProductId;
            _chronoPayPaymentSettings.ProductName = model.ProductName;
            _chronoPayPaymentSettings.SharedSecrect = model.SharedSecrect;
            _chronoPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_chronoPayPaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public ActionResult IPNHandler()
        {
            var form = Request.Form;
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.ChronoPay") as ChronoPayPaymentProcessor;
            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("ChronoPay module cannot be loaded");

            if (HostedPaymentHelper.ValidateResponseSign(form, _chronoPayPaymentSettings.SharedSecrect) && int.TryParse(form["cs1"], out int orderId))
            {
                var order = _orderService.GetOrderById(orderId);
                if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}