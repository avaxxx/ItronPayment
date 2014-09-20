using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItronPayment.Models
{
    public class Config
    {
        public static string SECURE_KEY = "ocxgXEL5psb7PAllKuCSblc9";
        public static long GOID = 8540279704;

        public static string LANG = "sk";

        public static string HTTP_SERVER = "http://localhost:32780/GoPay/";

        public static string SUCCESS_URL = HTTP_SERVER + "SuccessPage";
        public static string FAILED_URL = HTTP_SERVER + "FailedPage";

        public static string ACTION_URL = "PaymentAction.aspx?content=pay";

        public static string CALLBACK_URL = HTTP_SERVER + "CallbackAction";


    }
}