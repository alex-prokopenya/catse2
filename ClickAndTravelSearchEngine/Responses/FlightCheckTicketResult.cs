using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;

namespace ClickAndTravelSearchEngine.Responses
{
    public class FlightCheckTicketResult : _Response
    {
        //доступен ли перелет для бронирования
        private bool _isAvailable;

         [JsonMemberName("is_available")]
        public bool IsAvailable
        {
            get { return _isAvailable; }
            set { _isAvailable = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject PriceObj
        {
            get
            {
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