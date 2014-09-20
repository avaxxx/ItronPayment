using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace GoPay.api {
    /// <summary>
    /// Trida pro komunikaci s Gopay pomoci WS
    /// </summary>
    public class GopayHelperWS
    {

        private static bool validateCustomerData(ECustomerData customerData)
        {
            if (
                customerData != null
                && customerData.countryCode != null
                && customerData.countryCode != ""
                && !Enum.IsDefined(typeof(CountryCode), customerData.countryCode)
                )
            {

                return false;
            }

            return true;
        }

        /// <summary>
        /// Vytvoreni opakovane platby
        /// 
        /// - pri chybe komunikace s WS vyhozeni GopayException
        /// - pokud nesouhlasi udaje pri kontrole platby vyhozeni GopayException
        /// </summary>
        /// 
        /// <param name="targetGoId">identifikator prijemce - GoId</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">mena, ve ktere platba probiha</param>
        /// <param name="orderNumber">identifikator objednavky</param>
        /// <param name="successUrl">URL stranky, kam je zakaznik presmerovan po uspesnem zaplaceni</param>
        /// <param name="failedUrl">URL stranky, kam je zakaznik presmerovan po zruseni platby / neuspesnem zaplaceni</param>
        /// <param name="recurrentPayment">jedna-li se o opakovanou platbu</param>
        /// <param name="recurrenceDateTo">datum, do nehoz budou provadeny opakovane platby. Jedna se textovy retezec ve formatu yyyy-MM-dd.</param>
        /// <param name="recurrenceCycle">zakladni casovou jednotku opakovani. Nabyva hodnot [DAY, WEEK, MONTH], pro opakování od CS a.s. lze pouzit pouze hodnotu DAY.</param>
        /// <param name="recurrencePeriod">definuje periodu opakovane platby. Napr. při konfiguraci DAY,5 bude platba provadena kazdy 5. den</param>
        /// <param name="paymentChannels">pole platebnich kanalu, ktere se zobrazi na platebni brane</param>
        /// <param name="defaultPaymentChannel">platebni kanal, ktery se zobrazi (predvybere) na platebni brane po presmerovani</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci</param>
        /// 
        /// Informace o zakaznikovi
        /// <param name="firstName">Jmeno</param>
        /// <param name="lastName">Prijmeno</param>
        /// 
        /// Adresa
        /// <param name="city">Mesto</param>
        /// <param name="street">Ulice</param>
        /// <param name="postalCode">PSC</param>
        /// <param name="countryCode">stat</param>
        /// <param name="email">Email</param>
        /// <param name="phoneNumber">Tel. cislo</param>
        /// 
        /// <param name="p1 - p4">volitelne parametry (max. 128 znaku).</param>
        /// <param name="lang">jazyk plat. brany</param>
        /// Parametry jsou vraceny v nezmenene podobe jako soucast volani dotazu na stav platby $paymentStatus (viz metoda isPaymentDone)
        /// 
        /// <returns>paymentSessionId</returns>
        public static long CreateRecurrentPayment(
            long targetGoId,
            string productName,
            long totalPriceInCents,
            string currency,
            string orderNumber,
            string successURL,
            string failedURL,
            string recurrenceDateTo,
            string recurrenceCycle,
            System.Nullable<int> recurrencePeriod,
            string[] paymentChannels,
            string defaultPaymentChannel,
            string secureKey,
            string firstName,
            string lastName,
            string city,
            string street,
            string postalCode,
            string countryCode,
            string email,
            string phoneNumber,
            string p1,
            string p2,
            string p3,
            string p4,
            string lang)
        {

            return GopayHelperWS.CreateBasePayment(
                    targetGoId,
                    productName,
                    totalPriceInCents,
                    currency,
                    orderNumber,
                    successURL,
                    failedURL,
                    false,
                    true,
                    recurrenceDateTo,
                    recurrenceCycle,
                    recurrencePeriod,
                    paymentChannels,
                    defaultPaymentChannel,
                    secureKey,
                    firstName,
                    lastName,
                    city,
                    street,
                    postalCode,
                    countryCode,
                    email,
                    phoneNumber,
                    p1,
                    p2,
                    p3,
                    p4,
                    lang);
        }

        /// <summary>
        /// Vytvoreni predautorizovane platby
        /// 
        /// - pri chybe komunikace s WS vyhozeni GopayException
        /// - pokud nesouhlasi udaje pri kontrole platby vyhozeni GopayException
        /// </summary>
        /// 
        /// <param name="targetGoId">identifikator prijemce - GoId</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">mena, ve ktere platba probiha</param>
        /// <param name="orderNumber">identifikator objednavky</param>
        /// <param name="successUrl">URL stranky, kam je zakaznik presmerovan po uspesnem zaplaceni</param>
        /// <param name="failedUrl">URL stranky, kam je zakaznik presmerovan po zruseni platby / neuspesnem zaplaceni</param>
        /// <param name="paymentChannels">pole platebnich kanalu, ktere se zobrazi na platebni brane</param>
        /// <param name="defaultPaymentChannel">platebni kanal, ktery se zobrazi (predvybere) na platebni brane po presmerovani</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci</param>
        /// 
        /// Informace o zakaznikovi
        /// <param name="firstName">Jmeno</param>
        /// <param name="lastName">Prijmeno</param>
        /// 
        /// Adresa
        /// <param name="city">Mesto</param>
        /// <param name="street">Ulice</param>
        /// <param name="postalCode">PSC</param>
        /// <param name="countryCode">stat</param>
        /// <param name="email">Email</param>
        /// <param name="phoneNumber">Tel. cislo</param>
        /// 
        /// <param name="p1 - p4">volitelne parametry (max. 128 znaku).</param>
        /// <param name="lang">jazyk plat. brany</param>
        /// Parametry jsou vraceny v nezmenene podobe jako soucast volani dotazu na stav platby $paymentStatus (viz metoda isPaymentDone)
        /// 
        /// <returns>paymentSessionId</returns>
        public static long CreatePreAutorizedPayment(
                long targetGoId,
                string productName,
                long totalPriceInCents,
                string currency,
                string orderNumber,
                string successURL,
                string failedURL,
                string[] paymentChannels,
                string defaultPaymentChannel,
                string secureKey,
                string firstName,
                string lastName,
                string city,
                string street,
                string postalCode,
                string countryCode,
                string email,
                string phoneNumber,
                string p1,
                string p2,
                string p3,
                string p4,
                string lang
            )
        {

            return GopayHelperWS.CreateBasePayment(
                    targetGoId,
                    productName,
                    totalPriceInCents,
                    currency,
                    orderNumber,
                    successURL,
                    failedURL,
                    true,
                    false,
                    null,
                    null,
                    null,
                    paymentChannels,
                    defaultPaymentChannel,
                    secureKey,
                    firstName,
                    lastName,
                    city,
                    street,
                    postalCode,
                    countryCode,
                    email,
                    phoneNumber,
                    p1,
                    p2,
                    p3,
                    p4,
                    lang
                    );
        }


        /// <summary>
        /// Vytvoreni standardni platby
        /// 
        /// - pri chybe komunikace s WS vyhozeni GopayException
        /// - pokud nesouhlasi udaje pri kontrole platby vyhozeni GopayException
        /// </summary>
        /// 
        /// <param name="targetGoId">identifikator prijemce - GoId</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">mena, ve ktere platba probiha</param>
        /// <param name="orderNumber">identifikator objednavky</param>
        /// <param name="successUrl">URL stranky, kam je zakaznik presmerovan po uspesnem zaplaceni</param>
        /// <param name="failedUrl">URL stranky, kam je zakaznik presmerovan po zruseni platby / neuspesnem zaplaceni</param>
        /// <param name="paymentChannels">pole platebnich kanalu, ktere se zobrazi na platebni brane</param>
        /// <param name="defaultPaymentChannel">platebni kanal, ktery se zobrazi (predvybere) na platebni brane po presmerovani</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci</param>
        /// 
        /// Informace o zakaznikovi
        /// <param name="firstName">Jmeno</param>
        /// <param name="lastName">Prijmeno</param>
        /// 
        /// Adresa
        /// <param name="city">Mesto</param>
        /// <param name="street">Ulice</param>
        /// <param name="postalCode">PSC</param>
        /// <param name="countryCode">stat</param>
        /// <param name="email">Email</param>
        /// <param name="phoneNumber">Tel. cislo</param>
        /// 
        /// <param name="p1 - p4">volitelne parametry (max. 128 znaku).</param>
        /// <param name="lang">jazyk plat. brany</param>
        /// Parametry jsou vraceny v nezmenene podobe jako soucast volani dotazu na stav platby $paymentStatus (viz metoda isPaymentDone)
        /// 
        /// <returns>paymentSessionId</returns>
        public static long CreatePayment(
                long targetGoId,
                string productName,
                long totalPriceInCents,
                string currency,
                string orderNumber,
                string successURL,
                string failedURL,
                string[] paymentChannels,
                string defaultPaymentChannel,
                string secureKey,
                string firstName,
                string lastName,
                string city,
                string street,
                string postalCode,
                string countryCode,
                string email,
                string phoneNumber,
                string p1,
                string p2,
                string p3,
                string p4,
                string lang
            )
        {

            return GopayHelperWS.CreateBasePayment(
                    targetGoId,
                    productName,
                    totalPriceInCents,
                    currency,
                    orderNumber,
                    successURL,
                    failedURL,
                    false,
                    false,
                    null,
                    null,
                    null,
                    paymentChannels,
                    defaultPaymentChannel,
                    secureKey,
                    firstName,
                    lastName,
                    city,
                    street,
                    postalCode,
                    countryCode,
                    email,
                    phoneNumber,
                    p1,
                    p2,
                    p3,
                    p4,
                    lang
                    );
        }

        /// <summary>
        /// Vytvoreni platby pomoci WS z eshopu
        /// 
        /// - pri chybe komunikace s WS vyhozeni GopayException
        /// - pokud nesouhlasi udaje pri kontrole platby vyhozeni GopayException
        /// </summary>
        /// 
        /// <param name="targetGoId">identifikator prijemce - GoId</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">mena, ve ktere platba probiha</param>
        /// <param name="orderNumber">identifikator objednavky</param>
        /// <param name="successUrl">URL stranky, kam je zakaznik presmerovan po uspesnem zaplaceni</param>
        /// <param name="failedUrl">URL stranky, kam je zakaznik presmerovan po zruseni platby / neuspesnem zaplaceni</param>
        /// <param name="preAuthorization">jedna-li se o predautorizovanou platbu</param>
        /// <param name="recurrentPayment">jedna-li se o opakovanou platbu</param>
        /// <param name="recurrenceDateTo">datum, do nehoz budou provadeny opakovane platby. Jedna se textovy retezec ve formatu yyyy-MM-dd.</param>
        /// <param name="recurrenceCycle">zakladni casovou jednotku opakovani. Nabyva hodnot [DAY, WEEK, MONTH], pro opakování od CS a.s. lze pouzit pouze hodnotu DAY.</param>
        /// <param name="recurrencePeriod">definuje periodu opakovane platby. Napr. při konfiguraci DAY,5 bude platba provadena kazdy 5. den</param>
        /// <param name="paymentChannels">pole platebnich kanalu, ktere se zobrazi na platebni brane</param>
        /// <param name="defaultPaymentChannel">platebni kanal, ktery se zobrazi (predvybere) na platebni brane po presmerovani</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci</param>
        /// 
        /// Informace o zakaznikovi
        /// <param name="firstName">Jmeno</param>
        /// <param name="lastName">Prijmeno</param>
        /// 
        /// Adresa
        /// <param name="city">Mesto</param>
        /// <param name="street">Ulice</param>
        /// <param name="postalCode">PSC</param>
        /// <param name="countryCode">stat</param>
        /// <param name="email">Email</param>
        /// <param name="phoneNumber">Tel. cislo</param>
        /// 
        /// <param name="p1 - p4">volitelne parametry (max. 128 znaku).</param>
        /// <param name="lang">jazyk plat. brany</param>
        /// Parametry jsou vraceny v nezmenene podobe jako soucast volani dotazu na stav platby $paymentStatus (viz metoda isPaymentDone)
        /// 
        /// <returns>paymentSessionId</returns>
        public static long CreateBasePayment(
                long targetGoId,
                string productName,
                long totalPriceInCents,
                string currency,
                string orderNumber,
                string successURL,
                string failedURL,
                System.Nullable<bool> preAuthorization,
                System.Nullable<bool> recurrentPayment,
                string recurrenceDateTo,
                string recurrenceCycle,
                System.Nullable<int> recurrencePeriod,
                string[] paymentChannels,
                string defaultPaymentChannel,
                string secureKey,
                string firstName,
                string lastName,
                string city,
                string street,
                string postalCode,
                string countryCode,
                string email,
                string phoneNumber,
                string p1,
                string p2,
                string p3,
                string p4,
                string lang
        )
        {

            String paymentChannelsString = (null == paymentChannels) ? "" : String.Join(",", paymentChannels);

            // Sestaveni pozadavku pro podpis platby
            string encryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatPaymentCommand(
                            targetGoId,
                            productName,
                            totalPriceInCents,
                            currency,
                            orderNumber,
                            failedURL,
                            successURL,
                            preAuthorization,
                            recurrentPayment,
                            recurrenceDateTo,
                            recurrenceCycle,
                            recurrencePeriod,
                            paymentChannelsString,
                            secureKey)
                        ),
                        secureKey);

            // Sestaveni pozadavku pro zalozeni platby

            ECustomerData customerData = new ECustomerData();
            customerData.firstName = firstName;
            customerData.lastName = lastName;
            customerData.city = city;
            customerData.street = street;
            customerData.postalCode = postalCode;
            customerData.countryCode = countryCode;
            customerData.email = email;
            customerData.phoneNumber = phoneNumber;

            if (!validateCustomerData(customerData))
            {
                throw new GopayException(GopayException.Reason.INVALID_COUNTRY_CODE);
            }

            EPaymentCommand customerPaymentCommand = new EPaymentCommand();
            customerPaymentCommand.targetGoId = targetGoId;
            customerPaymentCommand.productName = productName;
            customerPaymentCommand.totalPrice = totalPriceInCents;
            customerPaymentCommand.currency = currency;
            customerPaymentCommand.orderNumber = orderNumber;
            customerPaymentCommand.failedURL = failedURL;
            customerPaymentCommand.successURL = successURL;
            customerPaymentCommand.preAuthorization = preAuthorization;
            customerPaymentCommand.recurrentPayment = recurrentPayment;
            customerPaymentCommand.recurrenceDateTo = recurrenceDateTo;
            customerPaymentCommand.recurrenceCycle = recurrenceCycle;
            customerPaymentCommand.recurrencePeriod = recurrencePeriod;
            customerPaymentCommand.paymentChannels = paymentChannelsString;
            customerPaymentCommand.defaultPaymentChannel = defaultPaymentChannel;
            customerPaymentCommand.encryptedSignature = encryptedSignature;
            customerPaymentCommand.customerData = customerData;
            customerPaymentCommand.p1 = p1;
            customerPaymentCommand.p2 = p2;
            customerPaymentCommand.p3 = p3;
            customerPaymentCommand.p4 = p4;
            customerPaymentCommand.lang = lang;

            EPaymentStatus paymentStatus;

            try
            {
                // Vytvorime providera pro komunikaci s WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);

                /*
                * Vytvareni platby na strane GoPay prostrednictvim providera 
                */
                paymentStatus = provider.createPayment(customerPaymentCommand);

                /*
                 * Kontrola stavu platby - musi byt ve stavu CREATED, kontrola parametru platby 
                 */
                if (paymentStatus.result == GopayHelper.CALL_COMPLETED
                    && paymentStatus.sessionState == GopayHelper.SessionState.CREATED.ToString()
                    && paymentStatus.paymentSessionId > 0)
                {

                    return (long)paymentStatus.paymentSessionId;

                }
                else
                {
                    throw new GopayException("Create payment failed: " + paymentStatus.resultDescription);
                }

            }
            catch (Exception ex)
            {
                /*
                 * Chyba pri komunikaci s WS
                 */
                throw new GopayException(ex.ToString());
            }
        }

        /// <summary>
        /// Kontrola stavu platby eshopu
        /// - verifikace parametru z redirectu
        /// - kontrola stavu platby
        /// - pokud nesouhlasi udaje vyhazuje GopayException
        /// - pri chybe komunikace s WS vyhazuje GopayException
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby </param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="orderNumber">identifikace akt. objednavky</param>
        /// <param name="totalPriceInCents">celkova cena v halerich</param>
        /// <param name="currency">mena, ve ktere platba probiha</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="secureKey">kryptovaci klic pridelene GoPay</param>
        /// 	  
        /// <returns>callbackResult</returns>
        /// callbackResult.sessionState   - stav platby
        /// callbackResult.sessionSubState - detailnejsi popis stavu platby
        public static CallbackResult IsPaymentDone(
                long paymentSessionId,
                long targetGoId,
                string orderNumber,
                long totalPriceInCents,
                string currency,
                string productName,
                string secureKey)
        {

            // Inicializace providera pro WS
            AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
            EPaymentStatus status;

            // Sestaveni dotazu na stav platby
            string sessionEncryptedSignature = GopayHelper.Encrypt(
                    GopayHelper.Hash(
                        GopayHelper.ConcatPaymentSession(
                            targetGoId,
                            paymentSessionId,
                            secureKey)
                    ), secureKey);

            EPaymentSessionInfo paymentSessionInfo = new EPaymentSessionInfo();
            paymentSessionInfo.targetGoId = targetGoId;
            paymentSessionInfo.paymentSessionId = paymentSessionId;
            paymentSessionInfo.encryptedSignature = sessionEncryptedSignature;

            CallbackResult callbackResult = new CallbackResult();

            try
            {
                /*
                 * Kontrola stavu platby na strane GoPay prostrednictvim WS 
                 */
                status = provider.paymentStatus(paymentSessionInfo);

                callbackResult.sessionState = status.sessionState;
                callbackResult.sessionSubState = status.sessionSubState;

                /*
                 * Kontrola zaplacenosti objednavky, verifikace parametru objednavky
                 */
                if (status.result != GopayHelper.CALL_COMPLETED)
                {
                    throw new GopayException("Payment Status Call failed: " + status.resultDescription);
                }

                if (callbackResult.sessionState != GopayHelper.SessionState.PAYMENT_METHOD_CHOSEN.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.CREATED.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.PAID.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.AUTHORIZED.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.CANCELED.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.TIMEOUTED.ToString()
                        && callbackResult.sessionState != GopayHelper.SessionState.REFUNDED.ToString()
                    )
                {

                    throw new GopayException("Bad Payment Session State: " + callbackResult.sessionState);
                }

                GopayHelper.CheckPaymentStatus(
                                status,
                                callbackResult.sessionState,
                                targetGoId,
                                orderNumber,
                                totalPriceInCents,
                                currency,
                                productName,
                                secureKey);

                return callbackResult;

            }
            catch (Exception ex1)
            {

                callbackResult.sessionState = GopayHelper.SessionState.FAILED.ToString();

            }
            finally
            {
                provider.Dispose();
            }

            return callbackResult;
        }

        /// <summary>
        /// Seznam vsech aktivnich platebnich metod
        /// </summary>
        ///
        /// <returns>paymentMethodsArray</returns>
        public static EPaymentMethod[] PaymentMethodsList()
        {
            AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);

            object[] paymentMethods = provider.paymentMethodList();

            EPaymentMethod[] paymentMethodsArray = new EPaymentMethod[paymentMethods.Length];

            for (int i = 0; i < paymentMethods.Length; i++)
            {
                paymentMethodsArray[i] = (EPaymentMethod)paymentMethods[i];
            }

            return paymentMethodsArray;

        }

        /// <summary>
        /// Zruseni predautorizovani plateb
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby </param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        public static void VoidAuthorization(
                long paymentSessionId,
                long targetGoId,
                string secureKey
                )
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentResult paymentResult;

                // Sestaveni dotazu na stav platby
                string sessionEncryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatPaymentSession(
                                targetGoId,
                                paymentSessionId,
                                secureKey)),
                             secureKey);

                EPaymentSessionInfo paymentSessionInfo = new EPaymentSessionInfo();
                paymentSessionInfo.targetGoId = targetGoId;
                paymentSessionInfo.paymentSessionId = paymentSessionId;
                paymentSessionInfo.encryptedSignature = sessionEncryptedSignature;

                paymentResult = provider.voidAuthorization(paymentSessionInfo);

                if (paymentResult.result == GopayHelper.CALL_RESULT_FAILED)
                {
                    throw new GopayException("autorization not voided [" + paymentResult.resultDescription + "]");
                }

                //Overeni podpisu
                GopayHelper.CheckPaymentResult(
                        (long)paymentResult.paymentSessionId,
                        paymentResult.encryptedSignature,
                        paymentResult.result,
                        paymentSessionId,
                        secureKey);

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }
        }

        /// <summary>
        /// Zruseni opakovani plateb
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby </param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        public static void VoidRecurrentPayment(
                long paymentSessionId,
                long targetGoId,
                string secureKey
                )
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentResult paymentResult;

                // Sestaveni dotazu na stav platby
                string hash = GopayHelper.Hash(
                            GopayHelper.ConcatPaymentSession(
                                targetGoId,
                                paymentSessionId,
                                secureKey)
                    );

                string sessionEncryptedSignature = GopayHelper.Encrypt(hash, secureKey);

                EPaymentSessionInfo paymentSessionInfo = new EPaymentSessionInfo();
                paymentSessionInfo.targetGoId = targetGoId;
                paymentSessionInfo.paymentSessionId = paymentSessionId;
                paymentSessionInfo.encryptedSignature = sessionEncryptedSignature;

                paymentResult = provider.voidRecurrentPayment(paymentSessionInfo);

                string returnHash = GopayHelper.Decrypt(paymentResult.encryptedSignature, secureKey);

                if (hash != returnHash)
                {
                    throw new GopayException("Encrypted signature differ");
                }

                if (paymentResult.result == GopayHelper.CALL_RESULT_FAILED)
                {
                    throw new GopayException("autorization not voided [" + paymentResult.resultDescription + "]");

                }
                else if (paymentResult.result == GopayHelper.CALL_RESULT_ACCEPTED)
                {
                    //zruseni opakovani platby bylo zarazeno ke zpracovani
                    //po urcite dobe je nutne dotazat zruseni se shodnymi parametry zda je jiz $paymentResult->result == GopayHelper::CALL_RESULT_FINISHED

                }
                else if (paymentResult.result == GopayHelper.CALL_RESULT_FINISHED)
                {
                    //opakovani platby bylo zruseno
                    //oznacte platbu
                }

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }

        }

        /// <summary>
        /// Založení opakovane platby
        /// </summary>
        /// 
        /// <param name="parentPaymentSessionId">identifikator rodicovske platby</param>
        /// <param name="recurrentPaymentOrderNumber">identifikator objednavky</param>
        /// <param name="recurrentPaymentTotalPriceInCents">celkova cena v halerich</param>
        /// <param name="recurrentPaymentCurrency">mena, ve ktere platba probiha</param>
        /// <param name="recurrentPaymentProductName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        ///
        /// <returns>paymentSessionId</returns>
        public static long PerformRecurrence(
                long parentPaymentSessionId,
                string recurrentPaymentOrderNumber,
                long recurrentPaymentTotalPriceInCents,
                string recurrentPaymentCurrency,
                string recurrentPaymentProductName,
                long targetGoId,
                string secureKey)
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentStatus paymentStatus;

                string encryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatRecurrenceRequest(
                                parentPaymentSessionId,
                                recurrentPaymentOrderNumber,
                                recurrentPaymentTotalPriceInCents,
                                targetGoId,
                                secureKey)
                        ), secureKey);

                ERecurrenceRequest recurrenceRequest = new ERecurrenceRequest();
                recurrenceRequest.parentPaymentSessionId = parentPaymentSessionId;
                recurrenceRequest.orderNumber = recurrentPaymentOrderNumber;
                recurrenceRequest.totalPrice = recurrentPaymentTotalPriceInCents;
                recurrenceRequest.targetGoId = targetGoId;
                recurrenceRequest.encryptedSignature = encryptedSignature;

                paymentStatus = provider.createRecurrentPayment(recurrenceRequest);

                if (paymentStatus.result == GopayHelper.CALL_COMPLETED)
                {

                    GopayHelper.CheckPaymentStatus(
                            paymentStatus,
                            GopayHelper.SessionState.CREATED.ToString(),
                            targetGoId,
                            recurrentPaymentOrderNumber,
                            recurrentPaymentTotalPriceInCents,
                            recurrentPaymentCurrency,
                            recurrentPaymentProductName,
                            secureKey);

                    return (long)paymentStatus.paymentSessionId;

                }
                else
                {
                    throw new GopayException("Bad payment status");

                }

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }
        }

        /// <summary>
        /// Dokončení platby
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby</param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        ///
        /// <returns>paymentSessionId</returns>
        public static long CapturePayment(
                    long paymentSessionId,
                    long targetGoId,
                    string secureKey)
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentResult paymentResult;

                string encryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatPaymentSession(
                                targetGoId,
                                paymentSessionId,
                                secureKey)
                        ), secureKey);

                EPaymentSessionInfo paymentSessionInfo = new EPaymentSessionInfo();
                paymentSessionInfo.targetGoId = targetGoId;
                paymentSessionInfo.paymentSessionId = paymentSessionId;
                paymentSessionInfo.encryptedSignature = encryptedSignature;

                paymentResult = provider.capturePayment(paymentSessionInfo);

                if (paymentResult.result == GopayHelper.CALL_RESULT_FAILED)
                {
                    throw new GopayException("payment not captured [" + paymentResult.resultDescription + "]");

                }

                return (long)paymentResult.paymentSessionId;

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }
        }


        /// <summary>
        /// Zruseni platby
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby</param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        ///
        /// <returns>result/returns>
        public static string RefundPayment(
                    long paymentSessionId,
                    long targetGoId,
                    string secureKey)
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentResult paymentResult;

                string encryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatPaymentSession(
                                targetGoId,
                                paymentSessionId,
                                secureKey)
                        ), secureKey);

                EPaymentSessionInfo paymentSessionInfo = new EPaymentSessionInfo();
                paymentSessionInfo.targetGoId = targetGoId;
                paymentSessionInfo.paymentSessionId = paymentSessionId;
                paymentSessionInfo.encryptedSignature = encryptedSignature;

                paymentResult = provider.refundPayment(paymentSessionInfo);

                if (paymentResult.result == GopayHelper.CALL_RESULT_FAILED)
                {
                    throw new GopayException("payment not refunded [" + paymentResult.resultDescription + "]");

                }

                return paymentResult.result;

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }
        }


        /// <summary>
        /// Castecne zruseni platby
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby</param>
        /// <param name="targetGoId">identifikator prijemnce - GoId</param>
        /// <param name="secureKey">kryptovaci klic prideleny GoPay</param>
        ///
        /// <returns>result</returns>
        public static string RefundPaymentPartially(
                    long paymentSessionId,
                    long amount,
                    String currency,
                    String description,
                    long targetGoId,
                    string secureKey)
        {
            try
            {

                // Inicializace providera pro WS
                AxisEPaymentProviderV2Service provider = new AxisEPaymentProviderV2Service(GopayConfig.Ws);
                EPaymentResult paymentResult;

                string encryptedSignature = GopayHelper.Encrypt(
                        GopayHelper.Hash(
                            GopayHelper.ConcatRefundRequest(
                                targetGoId,
                                paymentSessionId,
                                amount,
                                currency,
                                description,
                                secureKey)
                        ), secureKey);

                ERefundRequest eRefundRequest = new ERefundRequest();
                eRefundRequest.targetGoId = targetGoId;
                eRefundRequest.paymentSessionId = paymentSessionId;
                eRefundRequest.amount = amount;
                eRefundRequest.currency = currency;
                eRefundRequest.description = description;
                eRefundRequest.encryptedSignature = encryptedSignature;

                paymentResult = provider.refundPayment(eRefundRequest);

                if (paymentResult.result == GopayHelper.CALL_RESULT_FAILED)
                {
                    throw new GopayException("payment not refunded [" + paymentResult.resultDescription + "]");

                }

                return paymentResult.result;

            }
            catch (Exception ex)
            {
                //
                // Chyba pri komunikaci s WS
                //
                throw new GopayException(ex.ToString());
            }
        }



        // Validni kody statu
        public enum CountryCode
        {
            BFA, //Burkina Faso
            BGD, //Bangladesh
            BGR, //Bulgaria
            BHR, //Bahrain
            BHS, //Bahamas
            BIH, //Bosnia and Herzegovina
            BLM, //Saint Barthélemy
            BLR, //Belarus
            BLZ, //Belize
            BMU, //Bermuda
            BOL, //Bolivia, Plurinational State of
            BRA, //Brazil
            BRB, //Barbados
            BRN, //Brunei Darussalam
            BTN, //Bhutan
            BVT, //Bouvet Island
            BWA, //Botswana
            CAF, //Central African Republic
            CAN, //Canada
            CCK, //Cocos (Keeling) Islands
            CHE, //Switzerland
            CHL, //Chile
            CHN, //China
            CIV, //Côte d'Ivoire
            CMR, //Cameroon
            COD, //Congo, the Democratic Republic of the
            COG, //Congo
            COK, //Cook Islands
            COL, //Colombia
            COM, //Comoros
            CPV, //Cape Verde
            CRI, //Costa Rica
            CUB, //Cuba
            CUW, //Curaçao
            CXR, //Christmas Island
            CYM, //Cayman Islands
            CYP, //Cyprus
            CZE, //Czech Republic
            DEU, //Germany
            DJI, //Djibouti
            DMA, //Dominica
            DNK, //Denmark
            DOM, //Dominican Republic
            DZA, //Algeria
            ECU, //Ecuador
            EGY, //Egypt
            ERI, //Eritrea
            ESH, //Western Sahara
            ESP, //Spain
            EST, //Estonia
            ETH, //Ethiopia
            FIN, //Finland
            FJI, //Fiji
            FLK, //Falkland Islands (Malvinas)
            FRA, //France
            FRO, //Faroe Islands
            FSM, //Micronesia, Federated States of
            GAB, //Gabon
            GBR, //United Kingdom
            GEO, //Georgia
            GGY, //Guernsey
            GHA, //Ghana
            GIB, //Gibraltar
            GIN, //Guinea
            GLP, //Guadeloupe
            GMB, //Gambia
            GNB, //Guinea-Bissau
            GNQ, //Equatorial Guinea
            GRC, //Greece
            GRD, //Grenada
            GRL, //Greenland
            GTM, //Guatemala
            GUF, //French Guiana
            GUM, //Guam
            GUY, //Guyana
            HKG, //Hong Kong
            HMD, //Heard Island and McDonald Islands
            HND, //Honduras
            HRV, //Croatia
            HTI, //Haiti
            HUN, //Hungary
            IDN, //Indonesia
            IMN, //Isle of Man
            IND, //India
            IOT, //British Indian Ocean Territory
            IRL, //Ireland
            IRN, //Iran, Islamic Republic of
            IRQ, //Iraq
            ISL, //Iceland
            ISR, //Israel
            ITA, //Italy
            JAM, //Jamaica
            JEY, //Jersey
            JOR, //Jordan
            JPN, //Japan
            KAZ, //Kazakhstan
            KEN, //Kenya
            KGZ, //Kyrgyzstan
            KHM, //Cambodia
            KIR, //Kiribati
            KNA, //Saint Kitts and Nevis
            KOR, //Korea, Republic of
            KWT, //Kuwait
            LAO, //Lao People's Democratic Republic
            LBN, //Lebanon
            LBR, //Liberia
            LBY, //Libyan Arab Jamahiriya
            LCA, //Saint Lucia
            LIE, //Liechtenstein
            LKA, //Sri Lanka
            LSO, //Lesotho
            LTU, //Lithuania
            LUX, //Luxembourg
            LVA, //Latvia
            MAC, //Macao
            MAF, //Saint Martin (French part)
            MAR, //Morocco
            MCO, //Monaco
            MDA, //Moldova, Republic of
            MDG, //Madagascar
            MDV, //Maldives
            MEX, //Mexico
            MHL, //Marshall Islands
            MKD, //Macedonia, the former Yugoslav Republic of
            MLI, //Mali
            MLT, //Malta
            MMR, //Myanmar
            MNE, //Montenegro
            MNG, //Mongolia
            MNP, //Northern Mariana Islands
            MOZ, //Mozambique
            MRT, //Mauritania
            MSR, //Montserrat
            MTQ, //Martinique
            MUS, //Mauritius
            MWI, //Malawi
            MYS, //Malaysia
            MYT, //Mayotte
            NAM, //Namibia
            NCL, //New Caledonia
            NER, //Niger
            NFK, //Norfolk Island
            NGA, //Nigeria
            NIC, //Nicaragua
            NIU, //Niue
            NLD, //Netherlands
            NOR, //Norway
            NPL, //Nepal
            NRU, //Nauru
            NZL, //New Zealand
            OMN, //Oman
            PAK, //Pakistan
            PAN, //Panama
            PCN, //Pitcairn
            PER, //Peru
            PHL, //Philippines
            PLW, //Palau
            PNG, //Papua New Guinea
            POL, //Poland
            PRI, //Puerto Rico
            PRK, //Korea, Democratic People's Republic of
            PRT, //Portugal
            PRY, //Paraguay
            PSE, //Palestinian Territory, Occupied
            PYF, //French Polynesia
            QAT, //Qatar
            REU, //Réunion
            ROU, //Romania
            RUS, //Russian Federation
            RWA, //Rwanda
            SAU, //Saudi Arabia
            SDN, //Sudan
            SEN, //Senegal
            SGP, //Singapore
            SGS, //South Georgia and the South Sandwich Islands
            SHN, //Saint Helena, Ascension and Tristan da Cunha
            SJM, //Svalbard and Jan Mayen
            SLB, //Solomon Islands
            SLE, //Sierra Leone
            SLV, //El Salvador
            SMR, //San Marino
            SOM, //Somalia
            SPM, //Saint Pierre and Miquelon
            SRB, //Serbia
            SSD, //South Sudan
            STP, //Sao Tome and Principe
            SUR, //Suriname
            SVK, //Slovakia
            SVN, //Slovenia
            SWE, //Sweden
            SWZ, //Swaziland
            SXM, //Sint Maarten (Dutch part)
            SYC, //Seychelles
            SYR, //Syrian Arab Republic
            TCA, //Turks and Caicos Islands
            TCD, //Chad
            TGO, //Togo
            THA, //Thailand
            TJK, //Tajikistan
            TKL, //Tokelau
            TKM, //Turkmenistan
            TLS, //Timor-Leste
            TON, //Tonga
            TTO, //Trinidad and Tobago
            TUN, //Tunisia
            TUR, //Turkey
            TUV, //Tuvalu
            TWN, //Taiwan, Province of China
            TZA, //Tanzania, United Republic of
            UGA, //Uganda
            UKR, //Ukraine
            UMI, //United States Minor Outlying Islands
            URY, //Uruguay
            USA, //United States
            UZB, //Uzbekistan
            VAT, //Holy See (Vatican City State)
            VCT, //Saint Vincent and the Grenadines
            VEN, //Venezuela, Bolivarian Republic of
            VGB, //Virgin Islands, British
            VIR, //Virgin Islands, U.S.
            VNM, //Viet Nam
            VUT, //Vanuatu
            WLF, //Wallis and Futuna
            WSM, //Samoa
            YEM, //Yemen
            ZAF, //South Africa
            ZMB, //Zambia
            ZWE //Zimbabwe 
        }
    }
}
