using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Hotels
{
    public class RoomVariant
    {
        private string _variantId;
        [JsonMemberName("variant_id")]
        public string VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private string _pansionTitle;
        [JsonMemberName("pansion_title")]
        public string PansionTitle
        {
            get { return _pansionTitle; }
            set { _pansionTitle = value; }
        }

        private int _pansionGroupId;
        [JsonMemberName("pansion_group_id")]
        public int PansionGroupId
        {
            get { return _pansionGroupId; }
            set { _pansionGroupId = value; }
        }

        private string _roomTitle;
        [JsonMemberName("room_title")]
        public string RoomTitle
        {
            get { return _roomTitle; }
            set { _roomTitle = value; }
        }

        private string roomCategory;
        [JsonMemberName("room_category")]
        public string RoomCategory
        {
            get { return roomCategory; }
            set { roomCategory = value; }
        }

        private string roomInfo;
        [JsonMemberName("room_info")]
        public string RoomInfo
        {
            get { return roomInfo; }
            set { roomInfo = value; }
        }

        private KeyValuePair<string, decimal>[] _prices = new KeyValuePair<string,decimal>[0];

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

            set 
            {
                List< KeyValuePair<string, decimal>> temp = new List<KeyValuePair<string,decimal>>();//[value.Count];

                JsonObject val = value as JsonObject;

                foreach (string key in val.Names)
                { 
                    temp.Add(new KeyValuePair<string,decimal>(key, Convert.ToDecimal(val[key])));
                }

                this._prices = temp.ToArray();
            }
        }


        private JsonObject _penalties;
        [JsonIgnore]
        public object Penalties
        {
            get { return _penalties; }
            set { _penalties = value as JsonObject; }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }
    }
}