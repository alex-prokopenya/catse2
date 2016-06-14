using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Inurance
{
    public class InsuranceVariant
    {
        //программа страхования
        private int _program;
        [JsonMemberName("program")]
        public int Program
        {
            get { return _program; }
            set { _program = value; }
        }

        //сумма покрытия
        private int _coverage;
        [JsonMemberName("coverage")]
        public int Coverage
        {
            get { return _coverage; }
            set { _coverage = value; }
        }

        //стоимость
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
    }
}