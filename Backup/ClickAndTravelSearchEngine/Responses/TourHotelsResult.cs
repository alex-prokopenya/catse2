using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Tours;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class TourHotelsResult: _Response
    {
        private TourSearchState _searchState;

        [JsonMemberName("search_state")]
        public TourSearchState SearchState
        {
            get { return _searchState; }
            set { _searchState = value; }
        }

        private TourRoute[] _Routes;
         [JsonMemberName("routes")]
        public TourRoute[] Routes
        {
            get { return _Routes; }
            set { _Routes = value; }
        }

        private TourVariantsGroup[] _variantsGroups;
        [JsonMemberName("variants_groups")]
        public TourVariantsGroup[] VariantsGroups
        {
            get { return _variantsGroups; }
            set { _variantsGroups = value; }
        }
    }
}