using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class TourSearchState: _Response
    {
        private bool _isFinished;
        [JsonMemberName("completed")]
        public bool IsFinished
        {
            get { return _isFinished; }
            set { _isFinished = value; }
        }

        private int _resultsCount;
        [JsonMemberName("results_count")]
        public int ResultsCount
        {
            get { return _resultsCount; }
            set { _resultsCount = value; }
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