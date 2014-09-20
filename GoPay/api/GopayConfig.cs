using System;
using System.Text;
using System.IO;
using System.Web;

namespace GoPay.api
{
    public class GopayConfig
    {
        // Pracovni prostredi
        public enum Environment
        {
            PRODUCTION,
            TESTING
        }

        private static Environment prostredi = Environment.TESTING;

        private static string baseIntegrationUrl_Test = "https://testgw.gopay.cz/gw/pay-base-v2";
        private static string fullIntegrationURL_Test = "https://testgw.gopay.cz/gw/pay-full-v2";
        private static string wsWsdl_Test = "https://testgw.gopay.cz/axis/EPaymentServiceV2?wsdl";
        private static string ws_Test = "https://testgw.gopay.cz/axis/EPaymentServiceV2";

        private static string baseIntegrationUrl = "https://gate.gopay.cz/gw/pay-base-v2";
        private static string fullIntegrationURL = "https://gate.gopay.cz/gw/pay-full-v2";
        private static string wsWsdl = "https://gate.gopay.cz/axis/EPaymentServiceV2?wsdl";
        private static string ws = "https://gate.gopay.cz/axis/EPaymentServiceV2";
        

        /// <summary>
        /// Nastaveni pracovniho prostredi
        /// </summary>
        public static Environment Prostredi
        {
            set
            {
                prostredi = value;
            }

            get
            {
                return prostredi;
            }
        }

        /// <summary>
        /// URL platebni brany pro uplnou integraci
        /// </summary>
        public static string FullIntegrationUrl
        {
            get
            {
                if (prostredi == Environment.PRODUCTION)
                    return GopayConfig.fullIntegrationURL;
                else
                    return GopayConfig.fullIntegrationURL_Test;
            }

        }


        /// <summary>
        /// URL platebni brany pro zakladni integraci
        /// </summary>
        public static string BaseIntegrationUrl
        {
            get
            {
                if (prostredi == Environment.PRODUCTION)
                    return GopayConfig.baseIntegrationUrl;
                else
                    return GopayConfig.baseIntegrationUrl_Test;

            }
        }


        /// <summary>
        /// URL WSDL webservice
        /// </summary>
        public static string WsWsdl
        {
            get
            {
                if (prostredi == Environment.PRODUCTION)
                    return GopayConfig.wsWsdl;
                else
                    return GopayConfig.wsWsdl_Test;
            }
        }


        /// <summary>
        /// URL webservice
        /// </summary>
        public static string Ws
        {
            get
            {
                if (prostredi == Environment.PRODUCTION)
                    return GopayConfig.ws;
                else
                    return GopayConfig.ws_Test;
            }
        }

    }
}
