using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Excursions;
using Jayrock.Json;

using Jayrock.Json.Conversion;


namespace ClickAndTravelSearchEngine.Responses
{
    public class ExcursionDatesResult : _Response
    {
        [JsonMemberName("excursion_dates")]
        public JsonArray excursionDates;
    }
}