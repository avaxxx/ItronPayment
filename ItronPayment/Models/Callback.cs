using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoPay.api;

namespace ItronPayment.Models
{
    public class Callback
    {

        /// <summary>
        /// Obsluha zpetneho volani po potvrzeni, zruseni platby
        /// </summary>
        /// 
        /// <returns>URL</returns>
        public string Call(
            long returnedGoId,
            long returnedPaymentSessionId,
            string returnedOrderNumber,
            string returnedEncryptedSignature)
        {

            //
            // Nacist data objednavky dle prichoziho paymentSessionId, zde z testovacich duvodu vse primo v testovaci tride objednavka
            // Upravte dle ulozeni vasich objednavek
            //
            //eshop.Order objednavka = new eshop.Order();
            //objednavka.loadByPaymentSessionId(returnedPaymentSessionId);

            var objednavka = new Order();
            objednavka.OrderNumber = "123";
            objednavka.ProductName = "itron";
            objednavka.Currency = "EUR";

            string location;

            try
            {
                GopayHelper.CheckPaymentIdentity(
                    returnedGoId,
                    returnedPaymentSessionId,
                    null,
                    returnedOrderNumber,
                    returnedEncryptedSignature,
                    Config.GOID,
                    objednavka.OrderNumber,
                    Config.SECURE_KEY);

            }
            catch (GopayException ex)
            {
                // Nevalidni informace z redirectu - fraud	
                // Zalogovani dat, chyby...
                // Presmerovani na error page
                return Config.FAILED_URL + "?sessionState=" + GopayHelper.SessionState.FAILED;
            }

            // Kontrola zaplacenosti objednavky na serveru GoPay
            CallbackResult callbackResult = GopayHelperWS.IsPaymentDone(
                                                                returnedPaymentSessionId,
                                                                returnedGoId,
                                                                returnedOrderNumber,
                                                                objednavka.TotalPrice,
                                                                objednavka.Currency,
                                                                objednavka.ProductName,
                                                                Config.SECURE_KEY);

            // Zpracovani objednavky, prezentace uspesne platby
            // ...
            // ...
            // ...
            if (callbackResult.sessionState == GopayHelper.SessionState.PAID.ToString())
            {
                //
                // Zpracovat pouze objednavku, ktera jeste nebyla zaplacena 
                //
                if (objednavka.State != GopayHelper.SessionState.PAID.ToString())
                {

                    //
                    //  Zpracovani objednavky  ! UPRAVTE !
                    //
                    //objednavka.ProcessPayment();
                }

                //
                // Presmerovani na prezentaci uspesne platby
                //
                location = Config.SUCCESS_URL;

            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.PAYMENT_METHOD_CHOSEN.ToString())
            {
                // Platba ceka na zaplaceni
                location = Config.SUCCESS_URL;


            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.CREATED.ToString())
            {
                // Platba nebyla zaplacena
                location = Config.FAILED_URL;

            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.CANCELED.ToString())
            {
                // Platba byla zrusena objednavajicim
                //objednavka.CancelPayment();
                location = Config.FAILED_URL;

            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.TIMEOUTED.ToString())
            {
                // Platnost platby vyprsela
                //objednavka.TimeoutPayment();
                location = Config.FAILED_URL;

            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.AUTHORIZED.ToString())
            {
                // Platba byla autorizovana, ceka se na dokonceni
                //objednavka.AutorizePayment();
                location = Config.SUCCESS_URL;

            }
            else if (callbackResult.sessionState == GopayHelper.SessionState.REFUNDED.ToString())
            {
                // Platba byla vracena - refundovana
                //objednavka.RefundePayment();
                location = Config.SUCCESS_URL;

            }
            else
            {
                // Chyba ve stavu platby
                location = Config.FAILED_URL;
                callbackResult.sessionState = GopayHelper.SessionState.FAILED.ToString();

            }

            return location + "?sessionState=" + callbackResult.sessionState + "&sessionSubState=" + callbackResult.sessionSubState;

        }

    }
}