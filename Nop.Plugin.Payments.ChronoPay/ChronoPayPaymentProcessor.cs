using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.ChronoPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.ChronoPay
{
    /// <summary>
    /// ChronoPay payment processor
    /// </summary>
    public class ChronoPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ChronoPayPaymentSettings _chronoPayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ChronoPayPaymentProcessor(ChronoPayPaymentSettings chronoPayPaymentSettings,
            ICurrencyService currencyService, CurrencySettings currencySettings, 
            ISettingService settingService, IWebHelper webHelper,
            ILocalizationService localizationService)
        {
            this._chronoPayPaymentSettings = chronoPayPaymentSettings;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var gatewayUrl = new Uri(_chronoPayPaymentSettings.GatewayUrl);

            var post = new RemotePost
            {
                FormName = "ChronoPay",
                Url = gatewayUrl.ToString(),
                Method = "POST"
            };

            post.Add("product_id", _chronoPayPaymentSettings.ProductId);
            post.Add("product_name", _chronoPayPaymentSettings.ProductName);
            post.Add("product_price", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", postProcessPaymentRequest.Order.OrderTotal));
            post.Add("product_price_currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
            post.Add("cb_url", string.Format("{0}Plugins/PaymentChronoPay/IPNHandler", _webHelper.GetStoreLocation()));
            post.Add("cb_type", "P");
            post.Add("cs1", postProcessPaymentRequest.Order.Id.ToString());
            post.Add("f_name", postProcessPaymentRequest.Order.BillingAddress.FirstName);
            post.Add("s_name", postProcessPaymentRequest.Order.BillingAddress.LastName);
            post.Add("street", postProcessPaymentRequest.Order.BillingAddress.Address1);
            post.Add("city", postProcessPaymentRequest.Order.BillingAddress.City);
            post.Add("zip", postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode);
            post.Add("phone", postProcessPaymentRequest.Order.BillingAddress.PhoneNumber);
            post.Add("email", postProcessPaymentRequest.Order.BillingAddress.Email);

            var state = postProcessPaymentRequest.Order.BillingAddress.StateProvince;
            if (state != null)
            {
                post.Add("state", state.Abbreviation);
            }

            var country = postProcessPaymentRequest.Order.BillingAddress.Country;
            if (country != null)
            {
                post.Add("country", country.ThreeLetterIsoCode);
            }

            post.Add("sign", HostedPaymentHelper.CalcRequestSign(post.Params, _chronoPayPaymentSettings.SharedSecrect));

            post.Post();
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _chronoPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //ChronoPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentChronoPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.ChronoPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentChronoPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.ChronoPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentChronoPayController);
        }

        public override void Install()
        {
            var settings = new ChronoPayPaymentSettings
            {
                GatewayUrl = "https://secure.chronopay.com/index_shop.cgi",
                ProductId = "",
                ProductName = "",
                SharedSecrect = "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.RedirectionTip", "You will be redirected to ChronoPay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.GatewayUrl", "Gateway URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.GatewayUrl.Hint", "Enter gateway URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.ProductId", "Product ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.ProductId.Hint", "Enter product ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.ProductName", "Product Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.ProductName.Hint", "Enter product Name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.SharedSecrect", "Shared secret");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.SharedSecrect.Hint", "Enter shared secret.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ChronoPay.PaymentMethodDescription", "You will be redirected to ChronoPay site to complete the order.");
            
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.GatewayUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.GatewayUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.ProductId");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.ProductId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.ProductName");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.ProductName.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.SharedSecrect");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.SharedSecrect.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ChronoPay.PaymentMethodDescription");
            
            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            get { return _localizationService.GetResource("Plugins.Payments.ChronoPay.PaymentMethodDescription"); }
        }

        #endregion
    }
}