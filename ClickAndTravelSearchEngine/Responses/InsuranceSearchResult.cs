using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Inurance;
using Jayrock.Json.Conversion;


namespace ClickAndTravelSearchEngine.Responses
{
    public class InsuranceSearchResult: _Response
    {
        private string _searchId;
        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }

        private int _purposeId;
        [JsonMemberName("purpose_id")]
        public int PurposeId
        {
            get { return _purposeId; }
            set { _purposeId = value; }
        }

        private InsuranceVariant[] _variants;
        [JsonMemberName("variants")]
        public InsuranceVariant[] Variants
        {
            get { return _variants; }
            set { _variants = value; }
        }

        //private string _currencyCode;

        //public string CurrencyCode
        //{
        //    get { return _currencyCode; }
        //    set { _currencyCode = value; }
        //}   
    }
}