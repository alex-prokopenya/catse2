using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Responses
{
    public class BookResult : _Response
    {
        //номер заказа в системе поставщика
        private int _bookingNumber;

        [JsonMemberName("book_number")]
        public int BookingNumber
        {
            get { return _bookingNumber; }
            set { _bookingNumber = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject PriceObj
        {
            get { 
                    JsonObject jObj = new JsonObject();
                    foreach (KeyValuePair<string, decimal> pr in _prices)
                        jObj.Add(pr.Key, pr.Value);
                    
                    return jObj;
            }
            set { }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

    }
}