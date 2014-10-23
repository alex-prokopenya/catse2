using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class FlightSearchState : _Response
    {
        private bool _isFinished;

        [JsonMemberName("completed")]
        public bool IsFinished
        {
            get { return _isFinished; }
            set { _isFinished = value; }
        }

        private int _flightsCount;

        [JsonMemberName("tickets_count")]
        public int FlightsCount
        {
            get { return _flightsCount; }
            set { _flightsCount = value; }
        }

        private string _hash;

        [JsonMemberName("hash")]
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }
    }
}