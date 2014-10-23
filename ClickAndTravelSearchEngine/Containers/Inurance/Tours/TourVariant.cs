using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Tours
{
    public class TourVariant
    {
        private KeyValuePair<string, decimal>[] _prices;
        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

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


        private int _variantId;
         [JsonMemberName("variant_id")]
        public int VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private TourHotelRoom[] _hotelRooms;
         [JsonMemberName("hotel_rooms")]
        public TourHotelRoom[] HotelRooms
        {
            get { return _hotelRooms; }
            set { _hotelRooms = value; }
        }
    }
}