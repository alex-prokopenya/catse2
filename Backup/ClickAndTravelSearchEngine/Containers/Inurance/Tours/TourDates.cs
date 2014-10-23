using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Tours
{
    public class TourDates
    {
        private int _daysLong;
        [JsonMemberName("days_long")]
        public int DaysLong
        {
            get { return _daysLong; }
            set { _daysLong = value; }
        }

        private DateTime _startDate;
        [JsonIgnore]
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }
        [JsonMemberName("date")]
        public string StartDateString
        {
            get { return _startDate.ToString("yyyy-MM-dd"); }
            set {  }
        }

        private KeyValuePair<string, decimal>[] _minPrices;
        [JsonIgnore]
        public KeyValuePair<string, decimal>[] MinPrices
        {
            get { return _minPrices; }
            set { _minPrices = value; }
        }

        [JsonMemberName("min_price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _minPrices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set { }
        }
    }
}