using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Excursions;

using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Responses
{
    public class ExcursionSearchResultVizit: _Response
    {
        private string _searchId;
         [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }

        private VizitExcursionVariant[] _excursionVariants;
        [JsonMemberName("excursion_variants")]
        public VizitExcursionVariant[] ExcursionVariants
        {
            get { return _excursionVariants; }
            set { _excursionVariants = value; }
        }
    }
}