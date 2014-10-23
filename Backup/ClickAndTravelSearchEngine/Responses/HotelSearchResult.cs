using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Hotels;
using Jayrock.Json;
using Jayrock.Json.Conversion;


namespace ClickAndTravelSearchEngine.Responses
{
    public class HotelSearchResult: _Response
    {
        private HotelSearchState _searchState;
        [JsonMemberName("search_state")]
        public HotelSearchState SearchState
        {
            get { return _searchState; }
            set { _searchState = value; }
        }

        private Hotel[] _foundedHotels;
        [JsonMemberName("hotels")]
        public Hotel[] FoundedHotels
        {
            get { return _foundedHotels; }
            set { _foundedHotels = value; }
        }

    }
}