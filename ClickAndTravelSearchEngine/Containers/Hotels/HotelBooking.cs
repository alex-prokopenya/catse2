using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Hotels
{
    public class HotelBooking
    {
        private string title = "";
        [JsonMemberName("title")]
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string partner_book_id = "";
        [JsonMemberName("partner_book_id")]
        public string PartnerBookId
        {
            get { return partner_book_id; }
            set { partner_book_id = value; }
        }

        private string date_begin = "";
        [JsonMemberName("date_begin")]
        public string DateBegin
        {
            get { return date_begin; }
            set { date_begin = value; }
        }

        private int nights_cnt;
        [JsonMemberName("nights_cnt")]
        public int NightsCnt
        {
            get { return nights_cnt; }
            set { nights_cnt = value; }
        }

        private string search_id;
        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return search_id; }
            set { search_id = value; }
        }


        private int[] turists;
        [JsonMemberName("turists")]
        public int[] Turists
        {
            get { return turists; }
            set { turists = value; }
        }

        private string partner_prefix = "";
        [JsonMemberName("partner_prefix")]
        public string PartnerPrefix
        {
            get { return partner_prefix; }
            set { partner_prefix = value; }
        }

        private KeyValuePair<string, decimal>[] _prices = new KeyValuePair<string, decimal>[0];

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

            set
            {
                List<KeyValuePair<string, decimal>> temp = new List<KeyValuePair<string, decimal>>();//[value.Count];

                foreach (string key in value.Names)
                {
                    temp.Add(new KeyValuePair<string, decimal>(key, Convert.ToDecimal(value[key])));
                }

                this._prices = temp.ToArray();
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