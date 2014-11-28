using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Excursions
{
    public class ExcursionVariant
    {
        //private ExcursionDetails _details;

        //public ExcursionDetails Details
        //{
        //    get { return _details; }
        //    set { _details = value; }
        //}

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

        private DateTime[] _dates;
        [JsonIgnore]

        public DateTime[] Dates
        {
            get { return _dates; }
            set { _dates = value; }
        }

        [JsonMemberName("dates")]
        public JsonArray jDates
        {
            get { JsonArray jAr = new JsonArray();

            for (int i = 0; i < _dates.Length; i++)
                jAr.Add( _dates[i].ToString("yyyy-MM-dd"));

                return jAr;
            }
            set {
                List<DateTime> dates = new List<DateTime>();

                foreach (string date in value)
                    dates.Add(Convert.ToDateTime(date));

                _dates = dates.ToArray();
            }
        }
    }
}