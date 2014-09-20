using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Web;

namespace GoPay.api {
    public class GopayHelper {

        /// <summary>
        /// Kody stavu platby 
        /// </summary>
        public enum SessionState {
	        CREATED,
	        PAYMENT_METHOD_CHOSEN,
	        PAID,
	        AUTHORIZED,
	        CANCELED,
	        TIMEOUTED,
	        REFUNDED,
	        FAILED
        }

        public static string CALL_COMPLETED = "CALL_COMPLETED";
        public static string CALL_FAILED = "CALL_FAILED";
    	
	    /**
	     * Konstanty pro opakovanou platbu
	     */
        public static string RECURRENCE_CYCLE_MONTH = "MONTH";
        public static string RECURRENCE_CYCLE_WEEK = "WEEK";
        public static string RECURRENCE_CYCLE_DAY = "DAY";
        public static string RECURRENCE_CYCLE_ON_DEMAND = "ON_DEMAND";
    	
	    /**
	     * Konstanty pro zruseni opakovani platby
	     */
        public static string CALL_RESULT_ACCEPTED = "ACCEPTED";
        public static string CALL_RESULT_FINISHED = "FINISHED";
        public static string CALL_RESULT_FAILED = "FAILED";

	    /**
	     * URL obrazku tlacitek pro platebni formulare a odkazy 
	     */
        public static string iconRychloplatba = "https://www.gopay.cz/download/PT_rychloplatba.png";
        public static string iconDaruj = "https://www.gopay.cz/download/PT_daruj.png";
        public static string iconBuynow = "https://www.gopay.cz/download/PT_buynow.png";
        public static string iconDonate = "https://www.gopay.cz/download/PT_donate.png";


        /**
         * Hlaseni o stavu platby
         */
        private static string PAID_MESSAGE = "Platba byla úspěšně provedena.\n Děkujeme Vám za využití našich služeb.";
        private static string CANCELED_MESSAGE = "Platba byla zrušena.\n Opakujte platbu znovu, prosím.";
        private static string AUTHORIZED_MESSAGE = "Platba byla autorizována, čeká se na dokončení. O provedení platby Vás budeme budeme neprodleně informovat pomocí emailu s potvrzením platby.";
        private static string REFUNDED_MESSAGE = "Platba byla vrácena.";
        private static string PAYMENT_METHOD_CHOSEN_ONLINE_MESSAGE = "Platba zatím nebyla provedena. O provedení platby Vás budeme neprodleně informovat pomocí emailu s potvrzením platby. Pokud neobdržíte do následujícího pracovního dne potvrzovací email o platbě, kontaktujte podporu GoPay na emailu podpora@gopay.cz.";
        private static string PAYMENT_METHOD_CHOSEN_OFFLINE_MESSAGE = "Platba zatím nebyla provedena. Na platební bráně GoPay jste získali platební údaje a na Váš email Vám byly zaslány informace k provedení platby. O provedení platby Vás budeme budeme neprodleně informovat pomocí emailu s potvrzením platby.";
        private static string PAYMENT_METHOD_CHOSEN_MESSAGE = "Platba zatím nebyla provedena. O provedení platby Vás budeme neprodleně informovat pomocí emailu s potvrzením platby.";
        private static string FAILED_MESSAGE = "V průběhu platby nastala chyba. Kontaktujte podporu GoPay na emailu podpora@gopay.cz.";

        public static string GetResultMessage(string sessionState, string sessionSubState) {

	        string result = "";
    		
	        if (sessionState == SessionState.PAID.ToString()) {
		        result = PAID_MESSAGE;

	        } else if (sessionState == SessionState.CANCELED.ToString()
		        || sessionState == SessionState.TIMEOUTED.ToString()
		        || sessionState == SessionState.CREATED.ToString()) {
		        result = CANCELED_MESSAGE;

	        } else if (sessionState == SessionState.AUTHORIZED.ToString()) {
		        result = AUTHORIZED_MESSAGE;

	        } else if (sessionState == SessionState.REFUNDED.ToString()) {
		        result = REFUNDED_MESSAGE;

	        } else if (sessionState == SessionState.PAYMENT_METHOD_CHOSEN.ToString()) {
		        if (sessionSubState == "101") {
			        result = PAYMENT_METHOD_CHOSEN_ONLINE_MESSAGE;				

		        } else if (sessionSubState == "102") {
			        result = PAYMENT_METHOD_CHOSEN_OFFLINE_MESSAGE;

		        } else {
			        result = PAYMENT_METHOD_CHOSEN_MESSAGE;
    				
		        }
    			
	        } else {
		        result = FAILED_MESSAGE;
	        }
    		
	        return result;

        }

        /// <summary>
        /// Sestaveni retezce pro podpis platebniho prikazu.
        /// </summary>
        ///
        /// <param name="gopayId">identifikator prijemce prideleny GoPay</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">identifikator meny platby</param>
        /// <param name="OrderNumber">identifikator objednavky u prijemce</param>
        /// <param name="failedURL">URL stranky, kam je zakaznik presmerovan po zruseni platby / neuspesnem zaplaceni</param>
        /// <param name="successURL">URL stranky, kam je zakaznik presmerovan po uspesnem zaplaceni</param>
        /// <param name="preAuthorization">jedna-li se o predautorizovanou platbu true => 1, false => 0, null=>""</param>
	    /// <param name="recurrentPayment">jedna-li se o opakovanou platbu true => 1, false => 0, null=>""</param>
	    /// <param name="recurrenceDateTo">do kdy se ma opakovana platba provadet</param>
	    /// <param name="recurrenceCycle">frekvence opakovane platby - mesic/tyden/den</param>
	    /// <param name="recurrencePeriod">pocet jednotek opakovani ($recurrencePeriod=3 ~ opakování jednou za tři jednotky (mesic/tyden/den))</param>
	    /// <param name="paymentChannel">platebni kanaly</param>
	    /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        ///
        /// <returns>retezec pro podpis</returns>
        public static string ConcatPaymentCommand(
            long gopayId,
            string productName,
            long totalPriceInCents,
		    string currency,
            string orderNumber,
            string failedURL,
            string successURL,
  		    System.Nullable<bool> preAuthorization,
 		    System.Nullable<bool> recurrentPayment,
  		    string recurrenceDateTo,
  		    string recurrenceCycle,
 		    System.Nullable<long> recurrencePeriod,
  		    string paymentChannels,
            string secureKey) {

            return gopayId.ToString() + "|" +
                        productName + "|" +
                        totalPriceInCents.ToString() + "|" +
                        currency.ToString() + "|" +
                        orderNumber + "|" +
                        failedURL + "|" +
                        successURL + "|" +
                        CastBooleanForWS(preAuthorization) + "|" +
                        CastBooleanForWS(recurrentPayment) + "|" +
                        ((recurrenceDateTo != null) ? recurrenceDateTo : "") + "|" +
  		                ((recurrenceCycle != null) ? recurrenceCycle : "") + "|" +
 		                ((recurrencePeriod != null) ? recurrencePeriod.ToString() : "") + "|" +
  		                paymentChannels + "|" +
                        secureKey;
        }


        /// <summary>
        /// Sestaveni retezce pro podpis vysledku stav platby.
        /// </summary>
        /// 
        /// <param name="gopayId">identifikator prijemce prideleny GoPay</param>
        /// <param name="productName">popis objednavky zobrazujici se na platebni brane</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">identifikator meny platby</param>
        /// <param name="OrderNumber">identifikator objednavky u prijemce</param>
        /// <param name="parentPaymentSessionId">id puvodni platby pri opakovane platbe</param>
        /// <param name="preAuthorization">jedna-li se o predautorizovanou platbu true => 1, false => 0, null=>""</param>
        /// <param name="recurrentPayment">jedna-li se o opakovanou platbu true => 1, false => 0, null=>""</param>
        /// <param name="result">vysledek volani (CALL_COMPLETED / CALL_FAILED)</param>
        /// <param name="sessionState">stav platby - viz GopayHelper</param>
        /// <param name="sessionSubState">podstav platby - detailnejsi popis stavu platby</param>
        /// <param name="paymentChannel">pouzita platebni metoda</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// <returns>retezec pro podpis</returns>
        public static string ConcatPaymentStatus(
            long gopayId,
            string productName,
            long totalPriceInCents,
	        string currency,
            string orderNumber,
            System.Nullable<bool> recurrentPayment,
            System.Nullable<long> parentPaymentSessionId,
            System.Nullable<bool> preAuthorization,
            string result,
            string sessionState,
	        string sessionSubState,
            string paymentChannel,
            string secureKey) {

            return gopayId.ToString() + "|" +
                    productName + "|" +
                    totalPriceInCents.ToString() + "|" +
                    currency + "|" +
                    orderNumber + "|" +
		            CastBooleanForWS(recurrentPayment) + "|" +
		            parentPaymentSessionId + "|" +
                    CastBooleanForWS(preAuthorization) + "|" +
                    result + "|" +
                    sessionState + "|" +
                    sessionSubState + "|" +
                    paymentChannel + "|" +
                    secureKey;
        }

        /// <summary>
        /// Sestaveni retezce pro podpis platební session pro přesměrování na platební bránu GoPay 
  	    /// nebo volání GoPay služby stav platby
        /// </summary>
        /// 
        /// <param name="gopayId">identifikator prijemce prideleny GoPay</param>
        /// <param name="paymentSessionId">identifikator platby na GoPay</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
        public static string ConcatPaymentSession(
            long gopayId,
            long paymentSessionId,
            string secureKey) {

            return gopayId.ToString() + "|" +
                   paymentSessionId.ToString() + "|" +
                   secureKey;
        }


        /// <summary>
        /// Sestaveni retezce pro podpis parametru platby (paymentIdentity)
        /// </summary>
        /// 
        /// <param name="gopayId">identifikator prijemce prideleny GoPay</param>
        /// <param name="paymentSessionId">identifikator platby na GoPay</param>
        /// <param name="parentPaymentSessionId">id puvodni platby pri opakovane platbe</param>
        /// <param name="OrderNumber">identifikator platby u prijemce</param>
        /// <param name="secureKey">kryptovaci heslo pridelene prijemci, urcene k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
        public static string ConcatPaymentIdentity(
            long gopayId,
            long paymentSessionId,
            System.Nullable<long> parentPaymentSessionId,
            string orderNumber,
            string secureKey) {

            return gopayId.ToString() + "|" +
                   paymentSessionId.ToString() + "|" +
                   parentPaymentSessionId.ToString() + "|" +
                   orderNumber + "|" +
                   secureKey;
        }


        /// <summary>
        /// Sestaveni retezce pro podpis.
        /// </summary>
        /// 
        /// <param name="paymentSessionId">identifikator platby na GoPay</param>
        /// <param name="result">vysledek volani</param>
        /// <param name="secureKey">kryptovaci heslo pridelene prijemci, urcene k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
        public static string ConcatPaymentResult(
            long paymentSessionId,
            string result,
            string secureKey) {

            return paymentSessionId.ToString() + "|" +
                   result + "|" +
                   secureKey;
        }

  	    /// <summary>
        /// Sestaveni retezce pro stazeni vypisu plateb uzivatele
        /// </summary>
        /// 
        /// <param name="dateFrom">datum (vcetne), od ktereho se generuje vypis</param>
        /// <param name="dateTo">datum (vcetne), do ktereho se generuje vypis</param>
        /// <param name="targetGoId">identifikator uzivatele prideleny GoPay</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
	    public static string ConcatStatementRequest(
		    string dateFrom,
		    string dateTo,
		    long targetGoId,
		    string secureKey) {

		    return dateFrom + "|" + 
			    dateTo + "|" + 
			    targetGoId.ToString() + "|" + 
			    secureKey; 
  	    }

        /// <summary>
        /// Sestaveni retezce pro podpis pozadavku opakovane platby.
        /// </summary>
        /// 
        /// <param name="targetGoId">identifikator prijemce prideleny GoPay</param>
        /// <param name="paymentSessionId">identifikator platby na GoPay</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="currency">identifikator meny platby</param>
        /// <param name="description">popis refundace</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
        public static string ConcatRefundRequest(
            long targetGoId,
            long paymentSessionId,
            long totalPriceInCents,
            string currency,
            string description,
            string secureKey)
        {

            return targetGoId + "|" +
            paymentSessionId + "|" +
            totalPriceInCents + "|" +
            currency + "|" +
            description + "|" +
            secureKey;
        }

        /// <summary>
        /// Sestaveni retezce pro podpis pozadavku refundace platby.
        /// </summary>
        /// 
        /// <param name="parentPaymentSessionId">id puvodni platby pri opakovane platbe</param>
        /// <param name="targetGoId">identifikator prijemce prideleny GoPay</param>
        /// <param name="OrderNumber">identifikator platby u prijemce</param>
        /// <param name="totalPriceInCents">celkova cena objednavky v halerich</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>retezec pro podpis</returns>
        public static string ConcatRecurrenceRequest(
            long parentPaymentSessionId,
            string orderNumber,
            long totalPriceInCents,
            long targetGoId,
            string secureKey)
        {

            return parentPaymentSessionId + "|" +
            targetGoId + "|" +
            orderNumber + "|" +
            totalPriceInCents + "|" +
            secureKey;
        }

        /// <summary>
        /// SHA1 hash dat
        /// </summary>
        /// 
        /// <param name="data">Data</param>
        /// 
        /// <returns>Hash</returns>
        public static string Hash(string data) {
            byte[] dataToHash = ASCIIEncoding.UTF8.GetBytes(data);
            byte[] hashValue = new SHA1Managed().ComputeHash(dataToHash);

            StringBuilder hashData = new StringBuilder();
            foreach (byte b in hashValue) {
                hashData.Append(String.Format("{0:x2}", b));
            }

            return hashData.ToString();
        }

        /// <summary>
        /// Sifrovani dat 3DES
        /// - v pripade, ze vstupni data nemaji validni format vyhazuje GopayException
        /// </summary>
        /// 
        /// <param name="data">data</param>
        /// <param name="secureKey">secureKey</param>
        /// 
        /// <returns>zasifrovana data</returns>
        public static string Encrypt(string data, string secureKey) {
            // Vytvorime instanci 3DES algoritmu, nastavime parametry
            TripleDESCryptoServiceProvider alg = new TripleDESCryptoServiceProvider();
            alg.Padding = PaddingMode.Zeros;
            alg.Mode = CipherMode.ECB;
            alg.GenerateIV();
            try {
                alg.Key = new ASCIIEncoding().GetBytes(secureKey);

            } catch (Exception ex) {
                throw new GopayException(ex.ToString());
            }

            // Vytvorime encryptor
            ICryptoTransform encryptor = alg.CreateEncryptor(alg.Key, alg.IV);

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cs);
            byte[] encryptedByte;

            try {
                sw.Write(data);
                sw.Flush();
                cs.FlushFinalBlock();

                encryptedByte = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(encryptedByte, 0, (int)ms.Length);

            } catch (Exception ex) {
                throw new GopayException(ex.ToString());

            } finally {
                ms.Close();
                ms.Dispose();

                cs.Close();
                cs.Dispose();

                sw.Close();
                sw.Dispose();
            }

            StringBuilder encryptedData = new StringBuilder();
            foreach (byte b in encryptedByte) {
                try {
                    encryptedData.Append(String.Format("{0:x2}", b));

                } catch (Exception ex) {
                    throw new GopayException(ex.ToString());
                }
            }

            return encryptedData.ToString();
        }


        /// <summary>
        /// Desifrovani dat (3DES alg.)
        /// - v pripade, ze vstupni data nemaji validni format vyhazuje GopayException
        /// </summary>
        /// 
        /// <param name="data">data</param>
        /// <param name="secureKey">secureKey</param>
        /// 
        /// <returns>desifrovany retezec</returns>
        public static string Decrypt(string data, string secureKey) {
            byte[] cData = Hex2Byte(data);

            // Vytvorime instanci 3DES algoritmu, nastavime parametry
            TripleDESCryptoServiceProvider alg = new TripleDESCryptoServiceProvider();
            alg.Mode = CipherMode.ECB;
            alg.GenerateIV();
            alg.Key = Encoding.Default.GetBytes(secureKey);
            alg.Padding = PaddingMode.None;

            // Vytvorime decryptor
            ICryptoTransform decryptor = alg.CreateDecryptor(alg.Key, alg.IV);

            MemoryStream ms;
            CryptoStream cs;
            String decryptedData = "";

            try {
                ms = new MemoryStream(cData.Length);
                cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
                cs.Write(cData, 0, cData.Length);
                decryptedData = Encoding.UTF8.GetString(ms.ToArray());

            } catch (Exception ex) {
                throw new GopayException(ex.ToString());
            }

            return decryptedData.TrimEnd('\0');
        }


        /// <summary>
        /// Prevod HEX retezce na pole bytu
        /// - v pripade, ze vstupni data nemaji validni format vyhazuje GopayException
        /// </summary>
        /// 
        /// <param name="hex">HEX string</param>
        /// 
        /// <returns>pole bytu</returns>
        private static byte[] Hex2Byte(string hex) {
            // Overeni, zda neni retezec prazdny
            if (hex == null) return null;

            // Delka retezce
            int delka = hex.Length;

            if (delka % 2 == 1) return null;
            int pulkaDelky = delka / 2;

            // Vytvorime pole bytu
            byte[] pole = new byte[pulkaDelky];

            try {
                for (int i = 0; i != pulkaDelky; i++) {
                    pole[i] = (byte)Int32.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }

            } catch (Exception ex) {
                throw new GopayException(ex.ToString());
            }

            return pole;
        }


        /// <summary>
        /// Kontrola stavu platby proti internim udajum objednavky
        /// 
        /// - verifikace podpisu
        /// - pokud nesouhlasi udaje, tak se vyvola GopayException
        /// </summary>
        /// 
        /// <param name="paymentStatus">vysledek volani paymentStatus</param>
        /// <param name="sessionState">ocekavany stav paymentSession (WAITING, PAYMENT_DONE)</param>
        /// <param name="gopayId">identifikator prijemce prideleny GoPay</param>
        /// <param name="OrderNumber">identifikace akt. objednavky u prijemce</param>
        /// <param name="totalPriceInCents">cena objednavky v halerich</param>
        /// <param name="currency">identifikator meny platby</param>
        /// <param name="productName">nazev objednavky / zbozi</param>
        /// <param name="secureKey">kryptovaci klic prideleny prijemci, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>True</returns>
        public static bool CheckPaymentStatus(
            EPaymentStatus paymentStatus,
            String sessionState,
            long gopayId,
            string orderNumber,
            long totalPriceInCents,
            string currency,
            string productName,
            string secureKey) {

	        if (paymentStatus != null) {

		        if (paymentStatus.result != GopayHelper.CALL_COMPLETED) {
			        throw new GopayException(GopayException.Reason.INVALID_CALL_STATE_STATE);
		        }

		        if ( paymentStatus.sessionState != sessionState) {
			        throw new GopayException(GopayException.Reason.INVALID_SESSION_STATE);
		        }

		        if (paymentStatus.orderNumber != orderNumber) {
			        throw new GopayException(GopayException.Reason.INVALID_ON);
		        }

		        if (paymentStatus.productName != productName) {
			        throw new GopayException(GopayException.Reason.INVALID_PN);
		        }

		        if (paymentStatus.targetGoId != gopayId) {
			        throw new GopayException(GopayException.Reason.INVALID_GOID);
		        }

		        if (paymentStatus.totalPrice != totalPriceInCents) {
			        throw new GopayException(GopayException.Reason.INVALID_PRICE);
		        }

		        if (paymentStatus.currency != currency) {
			        throw new GopayException(GopayException.Reason.INVALID_CURRENCY);
		        }

	        } else {
		        throw new GopayException(GopayException.Reason.NO_PAYMENT_STATUS);
	        }
	        /*
	         * Kontrola podpisu objednavky
	         */
            string hashedSignature = GopayHelper.Hash(
                GopayHelper.ConcatPaymentStatus(
                    (long)paymentStatus.targetGoId,
                    paymentStatus.productName,
                    (long)paymentStatus.totalPrice,
                    paymentStatus.currency,
                    paymentStatus.orderNumber,
                    paymentStatus.recurrentPayment,
                    paymentStatus.parentPaymentSessionId,
                    paymentStatus.preAuthorization,
                    paymentStatus.result,
                    paymentStatus.sessionState,
                    paymentStatus.sessionSubState,
                    paymentStatus.paymentChannel,
                    secureKey));

            string decryptedHash = GopayHelper.Decrypt(paymentStatus.encryptedSignature, secureKey);

            if (hashedSignature != decryptedHash) {
                throw new GopayException(GopayException.Reason.INVALID_STATUS_SIGNATURE);
            }

            return true;
        }


        /// <summary>
        /// Kontrola parametru predavanych ve zpetnem volani po potvrzeni/zruseni platby
        /// 
        /// - verifikace podpisu
        /// - pokud nesouhlasi udaje, tak se vyvola GopayException
        /// </summary>
        /// 
        /// <param name="returnedGoId">goId vracene v redirectu</param>
        /// <param name="returnedPaymentSessionId">paymentSessionId vracene v redirectu</param>
        /// <param name="returnedParentPaymentSessionId">id puvodni platby pri opakovane platbe</param>
        /// <param name="returnedOrderNumber">identifikace objednavky vracena v redirectu - identifikator platby na eshopu</param>
        /// <param name="returnedEncryptedSignature">kontrolni podpis vraceny v redirectu</param>
        /// <param name="targetGoId">identifikace prijemce - GoId pridelene GoPay</param>
        /// <param name="OrderNumber">identifikace akt. objednavky</param>
        /// <param name="secureKey">kryptovaci klic prideleny eshopu / uzivateli, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>True</returns>
        public static bool CheckPaymentIdentity(
            long returnedGoId,
            long returnedPaymentSessionId,
            System.Nullable<long> returnedParentPaymentSessionId,
            string returnedOrderNumber,
            string returnedEncryptedSignature,
            long targetGoId,
            string orderNumber,
            string secureKey) {

            if (returnedOrderNumber != orderNumber) {
                throw new GopayException(GopayException.Reason.INVALID_ON);
            }

            if (returnedGoId != targetGoId) {
                throw new GopayException(GopayException.Reason.INVALID_GOID);
            }

            string hashedSignature = GopayHelper.Hash(
                    GopayHelper.ConcatPaymentIdentity(
                        returnedGoId,
                        returnedPaymentSessionId,
			            returnedParentPaymentSessionId,
                        returnedOrderNumber,
                        secureKey)
                );

            string decryptedHash = GopayHelper.Decrypt(returnedEncryptedSignature, secureKey).TrimEnd('\0');

            if (decryptedHash != hashedSignature) {
                throw new GopayException(GopayException.Reason.INVALID_SIGNATURE);
            }

            return true;
        }

        /// <summary>
        /// Kontrola parametru predavanych ve zpetnem volani po potvrzeni/zruseni platby
        /// 
        /// - verifikace podpisu
        /// - pokud nesouhlasi udaje, tak se vyvola GopayException
        /// </summary>
        /// <param name="returnedPaymentSessionId">paymentSessionId vracene v redirectu</param>
        /// <param name="returnedEncryptedSignature">id puvodni platby pri opakovane platbe</param>
        /// <param name="paymentResult">vysledek volani</param>
        /// <param name="paymentSessionId">identifikator platby na GoPay</param>
        /// <param name="secureKey">kryptovaci klic prideleny eshopu / uzivateli, urceny k podepisovani komunikace</param>
        /// 
        /// <returns>true</returns>
        public static bool CheckPaymentResult(
		    long returnedPaymentSessionId,
		    string returnedEncryptedSignature,
            string paymentResult,
            long paymentSessionId,
            string secureKey) {

    		if (returnedPaymentSessionId != paymentSessionId) {
               	throw new GopayException(GopayException.Reason.INVALID_PAYMENT_SESSION_ID);
		    }

            string hashedSignature = GopayHelper.Hash(
				            GopayHelper.ConcatPaymentResult(
				                (long)paymentSessionId,
				                paymentResult,
				                secureKey)
					);

            string decryptedHash = GopayHelper.Decrypt(returnedEncryptedSignature, secureKey);

            if (hashedSignature != decryptedHash) {
                throw new GopayException(GopayException.Reason.INVALID_STATUS_SIGNATURE);
            }

            return true;
        }

        public static string CastBooleanForWS(System.Nullable<bool> boolean)
        {
    		
		    if (boolean == false) {
			    return "0";
    	
		    } else 	if (boolean == true) {
			    return "1"; 

		    } else {
			    return "";
		    }
	    }

    }
}
