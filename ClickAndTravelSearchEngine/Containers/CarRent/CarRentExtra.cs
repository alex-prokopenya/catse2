using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentExtra
    {
        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _title;
        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public object Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set { }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

        //private int _price;
        //[JsonMemberName("price")]
        //public int Price
        //{
        //    get { return _price; }
        //    set { _price = value; }
        //}
        //private string _currencyCode;
        //[JsonMemberName("currency_code")]
        //public string CurrencyCode
        //{
        //    get { return _currencyCode; }
        //    set { _currencyCode = value; }
        //}
    }
}