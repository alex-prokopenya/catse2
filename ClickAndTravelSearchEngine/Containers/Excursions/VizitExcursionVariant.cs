using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Excursions
{
    public class VizitExcursionVariant
    {
        private int _desc_id;
        [JsonMemberName("did")]
        public int DescriptionId
        {
            get { return _desc_id; }
            set { _desc_id = value; }
        }

        private int _transport_id;
        [JsonMemberName("tid")]
        public int TransportId
        {
            get { return _transport_id; }
            set { _transport_id = value; }
        }

        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set {
                List<KeyValuePair<string, decimal>> prices = new List<KeyValuePair<string, decimal>>();
            
                JsonObject vl = value;
                foreach (string name in vl.Names)
                    prices.Add(new KeyValuePair<string,decimal>(name,Convert.ToDecimal(vl[name])));

                _prices = prices.ToArray();
            }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }
    }
}