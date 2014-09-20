using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoPay.api;

namespace ItronPayment.Models
{
    public class Payment
    {

        /*
         * Pole kodu platebnich metod, ktere se zobrazi na brane. 
         * Hodnoty viz vzorovy e-shop, zalozka "Prehled aktivnich platebnich metod"
         * Zde nastaveno zobrazovani platebnich metod GoPay penezenka, platebni karty VISA, MasterCard, MojePlatba a SuperCASH 
	     * Pro zobrazovani vsech platebnich metod ponechte prazdne pole
         */
        // private string[] paymentChannels = { "eu_gp_w", "eu_gp_u", "cz_kb", "SUPERCASH" };
        private string[] paymentChannels = { };

        /*
         * Platebni metoda, ktere je prvotne vybrana na brane. 
         * Zde nastaveno prvotni vybrani platebni metody platebni karty VISA, MasterCard
         */
        string defaultPaymentChannel = "eu_gp_u";


        private string p1 = null;
        private string p2 = null;
        private string p3 = null;
        private string p4 = null;

        /// <summary>
        /// Kontruktor tridy
        /// </summary>
        public Payment()
        {
            // Nastavime pracovni prostredi PRODUCTION / TESTING
            GopayConfig.Prostredi = GopayConfig.Environment.TESTING;
        }


        /// <summary>
        /// Zalozeni platby pomoci WS - na strane gopay
        /// </summary>
        /// 
        /// <returns>URL pro presmerovani</returns>
        public string Pay()
        {
            long paymentSessionId;

            //
            // Nacist data objednavky, zde z testovacich duvodu vse primo v testovaci tride objednavka
            // Upravte dle ulozeni vasich objednavek
            //

            var objednavka = new Order();
            objednavka.ProductName = "itron";
            objednavka.Currency = "EUR";
            objednavka.OrderNumber = "123";

            objednavka.FirstName = "Pavol";
            objednavka.LastName = "Decky";
            objednavka.City = "Zilina";
            objednavka.Street = "Pod Vinicou";
            objednavka.PostalCode = "01004";
            objednavka.CountryCode = "SVK";
            objednavka.Email = "avaxxx@gmail.com";
            objednavka.PhoneNumber = "0949554535";

            try
            {
                //
                // Vytvoreni platby na strane GoPay prostrednictvim API funkce
                // Pokud vytvoreni probehne korektne, je navracen identifikator paymentSessionId
                //
                paymentSessionId = GopayHelperWS.CreatePayment(
                                    Config.GOID,
                                    objednavka.ProductName,
                                    objednavka.TotalPrice,
                                    objednavka.Currency,
                                    objednavka.OrderNumber,
                                    Config.CALLBACK_URL,
                                    Config.CALLBACK_URL,
                                    this.paymentChannels,
                                    defaultPaymentChannel,
                                    Config.SECURE_KEY,
                                    objednavka.FirstName,
                                    objednavka.LastName,
                                    objednavka.City,
                                    objednavka.Street,
                                    objednavka.PostalCode,
                                    objednavka.CountryCode,
                                    objednavka.Email,
                                    objednavka.PhoneNumber,
                                    p1,
                                    p2,
                                    p3,
                                    p4,
                                    Config.LANG);

            }
            catch (GopayException ex)
            {
                // Zachyceni chyby, zalogovani chyby
                // ...

                return Config.FAILED_URL + "?sessionState=" + GopayHelper.SessionState.FAILED;
            }

            // Platba na strane GoPay uspesne vytvorena
            // Ulozeni paymentSessionId k objednavce. Slouzi pro komunikaci s GoPay
            // ...
            objednavka.PaymentSessionId = paymentSessionId;


            string ecryptedSignature = GopayHelper.Encrypt(
                    GopayHelper.Hash(
                        GopayHelper.ConcatPaymentSession(
                            Config.GOID,
                            paymentSessionId,
                            Config.SECURE_KEY)
                    ),
            Config.SECURE_KEY);

            // Presmerovani na platebni branu
            return GopayConfig.FullIntegrationUrl +
                   "?sessionInfo.targetGoId=" + Config.GOID +
                   "&sessionInfo.paymentSessionId=" + paymentSessionId +
                   "&sessionInfo.encryptedSignature=" + ecryptedSignature;

        }
    }
}