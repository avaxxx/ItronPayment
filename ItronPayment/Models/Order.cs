using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItronPayment.Models
{
    public class Order
    {
        public Order()
        {
            Products = new List<Product>();
        }

        public List<Product> Products { get; set; }

        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string ProductName { get; set; }
        public long PaymentSessionId { get; set; }
        public string Currency { get; set; }

        public bool IsPayed { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsTimeouted { get; set; }
        public bool IsAutorized { get; set; }
        public bool IsRefunded { get; set; }

        public bool PreAuthorization { get; set; }
        public bool RecurrentPayment { get; set; }
        public string RecurrenceDateTo { get; set; }
        public string RecurrenceCycle { get; set; }
        public long RecurrencePeriod { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string State { get; set; }

        public long TotalPrice
        {
            get
            {
                long price = 100;
                foreach (var item in Products)
                {
                    price += item.Quantity*item.Price;
                }
                return price;
            }

        }
    }
}