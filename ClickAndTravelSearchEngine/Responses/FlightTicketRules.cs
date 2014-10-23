using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class FlightTicketRules : _Response
    {
        //правила для каждого перелета в маршруте
        private SegmentRule _segementRules;

        [JsonMemberName("segment_rules")]
        public SegmentRule SegementRules
        {
            get { return _segementRules; }
            set { _segementRules = value; }
        }
    }
}