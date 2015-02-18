using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Transfers
{
    public class TransferVariant
    {
        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                if (_prices != null)
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

        private int _detailsId;
        [JsonMemberName("details_id")]
        public int DetailsId
        {
            get { return _detailsId; }
            set { _detailsId = value; }
        }

        private short _carsCount;
        [JsonMemberName("cars_count")]
        public short CarsCount
        {
            get { return _carsCount; }
            set { _carsCount = value; }
        }

        private string _priceId;
        [JsonMemberName("price_id")]
        public string PriceId
        {
            get { return _priceId; }
            set { _priceId = value; }
        }

        private string _carsInfo;
        [JsonMemberName("cars_info")]
        public string CarsInfo
        {
            get { return _carsInfo; }
            set { _carsInfo = value; }
        }
    }
}