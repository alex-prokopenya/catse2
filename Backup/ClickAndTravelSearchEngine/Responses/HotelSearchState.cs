using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class HotelSearchState: _Response
    {
        private bool _isFinished;
        [JsonMemberName("completed")]
        public bool IsFinished
        {
            get { return _isFinished; }
            set { _isFinished = value; }
        }

        private int _hotelsCount;
        [JsonMemberName("hotels_count")]
        public int HotelsCount
        {
            get { return _hotelsCount; }
            set { _hotelsCount = value; }
        }

        private string _hash;
        [JsonMemberName("hash")]
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        

        //представление объекта в виде строки
        public string ToJsonString()
        {
            return JsonConvert.ExportToString(this);
        }
    }
}