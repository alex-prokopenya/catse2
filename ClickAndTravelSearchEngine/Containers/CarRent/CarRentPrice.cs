using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentPrice
    {
        private string _name;
        [JsonMemberName("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        //private int _price;

        //public int Price
        //{
        //    get { return _price; }
        //    set { _price = value; }
        //}

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

        private string[] _incl;

        [JsonMemberName("incl")]
        public string[] Incl
        {
            get { return _incl; }
            set { _incl = value; }
        }
    }
}