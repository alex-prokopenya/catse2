using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;
namespace ClickAndTravelSearchEngine.Responses
{
    public class CarRentVerifyCarResult: _Response
    {
        private KeyValuePair<string, decimal>[] _newPrices;

        [JsonMemberName("new_price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _newPrices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set { }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] NewPrices
        {
            get { return _newPrices; }
            set { _newPrices = value; }
        }
             

        private string _message;
        [JsonMemberName("message")]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        //private string _currencyCode;

        //public string CurrencyCode
        //{
        //    get { return _currencyCode; }
        //    set { _currencyCode = value; }
        //}   
    }
}