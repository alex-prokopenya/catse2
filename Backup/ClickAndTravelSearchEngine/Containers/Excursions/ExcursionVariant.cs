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

        private int _excursionDetails;
        [JsonMemberName("id")]
        public int ExcursionDetails
        {
            get { return _excursionDetails; }
            set { _excursionDetails = value; }
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
            set { }
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
            set {  }
        }
    }
}