using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ClickAndTravelSearchEngine.Containers.Flights;

using Jayrock.Json.Conversion;
namespace ClickAndTravelSearchEngine.Responses
{
    public class FlightSearchResult : _Response
    {
        private FlightSearchState _searchState;

        [JsonMemberName("search_state")]
        public FlightSearchState SearchState
        {
            get { return _searchState; }
            set { _searchState = value; }
        }

        private FlightTicket[] _flightTickets;

        [JsonMemberName("tickets")]
        public FlightTicket[] FlightTickets
        {
            get { return _flightTickets; }
            set { _flightTickets = value; }
        }

        //private string _currencyCode;

        //public string CurrencyCode
        //{
        //    get { return _currencyCode; }
        //    set { _currencyCode = value; }
        //}    
    }
}