using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.ChronoPay
{
    public class HostedPaymentHelper
    {
        #region Methods
        public static string CalcRequestSign(NameValueCollection reqParams, string sharedSecrect)
        {
            return CalcMd5Hash(String.Format("{0}-{1}-{2}", reqParams["product_id"], reqParams["product_price"], sharedSecrect));
        }

        public static bool ValidateResponseSign(NameValueCollection rspParams, string sharedSecrect)
        {
            string rspSign = rspParams["sign"];
            if (String.IsNullOrEmpty(rspSign))
            {
                return false;
            }
            return rspSign.Equals(CalcMd5Hash(String.Format("{0}{1}{2}{3}{4}", sharedSecrect, rspParams["customer_id"], rspParams["transaction_id"], rspParams["transaction_type"], rspParams["total"])));
        }
        #endregion

        #region Utilities
        private static string CalcMd5Hash(string s)
        {
            using (var cs = MD5.Create())
            {
                var sb = new StringBuilder();
                byte[] hash;

                hash = cs.ComputeHash(Encoding.UTF8.GetBytes(s));
                sb.Length = 0;
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
        #endregion
    }
}
