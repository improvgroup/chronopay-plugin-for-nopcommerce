using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.ChronoPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

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

        public PaymentChronoPayController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ChronoPayPaymentSettings chronoPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._chronoPayPaymentSettings = chronoPayPaymentSettings;
            this._paymentSettings = paymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.GatewayUrl = _chronoPayPaymentSettings.GatewayUrl;
            model.ProductId = _chronoPayPaymentSettings.ProductId;
            model.ProductName = _chronoPayPaymentSettings.ProductName;
            model.SharedSecrect = _chronoPayPaymentSettings.SharedSecrect;
            model.AdditionalFee = _chronoPayPaymentSettings.AdditionalFee;

            return View("~/Plugins/Payments.ChronoPay/Views/PaymentChronoPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _chronoPayPaymentSettings.GatewayUrl = model.GatewayUrl;
            _chronoPayPaymentSettings.ProductId = model.ProductId;
            _chronoPayPaymentSettings.ProductName = model.ProductName;
            _chronoPayPaymentSettings.SharedSecrect = model.SharedSecrect;
            _chronoPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_chronoPayPaymentSettings);

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.ChronoPay/Views/PaymentChronoPay/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult IPNHandler(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.ChronoPay") as ChronoPayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("ChronoPay module cannot be loaded");

            if (HostedPaymentHelper.ValidateResponseSign(form, _chronoPayPaymentSettings.SharedSecrect))
            {
                int orderId;
                if (Int32.TryParse(form["cs1"], out orderId))
                {
                    Order order = _orderService.GetOrderById(orderId);
                    if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                }
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}