using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.CarRent;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class CarRentSearchResult: _Response
    {
        //private string _currencyCode;

        //public string CurrencyCode
        //{
        //    get { return _currencyCode; }
        //    set { _currencyCode = value; }
        //}

        private string _searchId;
        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }

        private CarRentVariant[] _variants;
        [JsonMemberName("variants")]
        public CarRentVariant[] Variants
        {
            get { return _variants; }
            set { _variants = value; }
        }
    }
}