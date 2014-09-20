using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoPay.api;
using ItronPayment.Models;

namespace ItronPayment.Controllers
{
    public class GoPayController : Controller
    {

        public ActionResult CheckOut()
        {
            Payment payment = new Payment();
            var redirectUrl = payment.Pay();
            return Redirect(redirectUrl);
        }

        public ActionResult CallbackAction()
        {
            // Ziskani parametru z redirektu po potvrzeni / zruseni platby
            long returnedPaymentSessionId = 0;
            long returnedGoId = 0;
            string returnedOrderNumber = "";
            string returnedEncryptedSignature = "";

            if (null != Request.QueryString.Get("paymentSessionId"))
                returnedPaymentSessionId = long.Parse(Request.QueryString.Get("paymentSessionId"));

            if (null != Request.QueryString.Get("targetGoId"))
                returnedGoId = long.Parse(Request.QueryString.Get("targetGoId"));

            if (null != Request.QueryString.Get("orderNumber"))
                returnedOrderNumber = Request.QueryString.Get("orderNumber");

            if (null != Request.QueryString.Get("encryptedSignature"))
                returnedEncryptedSignature = Request.QueryString.Get("encryptedSignature");

            Callback callback = new Callback();

            string redirectUrl = callback.Call(
                    returnedGoId,
                    returnedPaymentSessionId,
                    returnedOrderNumber,
                    returnedEncryptedSignature
                );

            return Redirect(redirectUrl);
        }

        public ActionResult SuccessPage()
        {
            string sessionState = Request.QueryString.Get("sessionState");
            string sessionSubState = Request.QueryString.Get("sessionSubState");

            var message = GopayHelper.GetResultMessage(sessionState, sessionSubState);
            ViewBag.Message = message;
            return View();
        }

        public ActionResult FailedPage()
        {
            string sessionState = Request.QueryString.Get("sessionState");
            string sessionSubState = Request.QueryString.Get("sessionSubState");

            if (sessionState == null || sessionState == "")
            {
                sessionState = GopayHelper.SessionState.CANCELED.ToString();
            }

            ViewBag.Message = GopayHelper.GetResultMessage(sessionState, sessionSubState);

            return View();
        }
	}
}