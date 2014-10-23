using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class HotelPenalties: _Response
    {
        private string  _variantId;
        [JsonMemberName("variant_id")]
        public string VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }


        private KeyValuePair<DateTime, string>[] _changingPenalties;
        [JsonIgnore]
        public KeyValuePair<DateTime, string>[] ChangingPenalties
        {
            get { return _changingPenalties; }
            set { _changingPenalties = value; }
        }

        [JsonMemberName("changing_penalties")]
        public JsonArray ChangingPenaltiesJson
        {
            get {
                    JsonArray res = new JsonArray();

                    foreach (KeyValuePair<DateTime, string> item in this._changingPenalties)
                    {
                        JsonObject obj = new JsonObject();
                        obj["date"] = item.Key.ToString("yyyy-MM-dd");
                        obj["info"] = item.Value;

                        res.Add(obj);
                    }

                    return res;
            }
            set {

                List<KeyValuePair<DateTime, string>> temp = new List<KeyValuePair<DateTime, string>>();

                foreach (JsonObject jObj in value)
                {
                    temp.Add(new KeyValuePair<DateTime, string>(DateTime.ParseExact(jObj["date"].ToString(), "yyyy-MM-dd", null), jObj["info"].ToString()));
                }

                this._changingPenalties = temp.ToArray();
            }
        }


        private KeyValuePair<DateTime, string>[] _cancelingPenalties;
        [JsonIgnore]
        public KeyValuePair<DateTime, string>[] CancelingPenalties
        {
            get { return _cancelingPenalties; }
            set { _cancelingPenalties = value; }
        }

        [JsonMemberName("canceling_penalties")]
        public JsonArray CancelingPenaltiesJson
        {
            get
            {
                JsonArray res = new JsonArray();

                foreach (KeyValuePair<DateTime, string> item in this._cancelingPenalties)
                {
                    JsonObject obj = new JsonObject();
                    obj["date"] = item.Key.ToString("yyyy-MM-dd");
                    obj["info"] = item.Value;

                    res.Add(obj);
                }

                return res;
            }
            set
            {
                List<KeyValuePair<DateTime, string>> temp = new List<KeyValuePair<DateTime, string>>();

                foreach (JsonObject jObj in value)
                {
                    temp.Add(new KeyValuePair<DateTime, string>(DateTime.ParseExact(jObj["date"].ToString(), "yyyy-MM-dd", null), jObj["info"].ToString()));
                }

                this._cancelingPenalties = temp.ToArray();
            }
        }
    } 
}