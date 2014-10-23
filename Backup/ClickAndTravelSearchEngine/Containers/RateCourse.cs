using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class RateCourse
    {
        private decimal _course;

        [JsonMemberName("course")]
        public decimal Course
        {
            get { return _course; }
            set { _course = value; }
        }
        private string _currencyFrom;

        [JsonMemberName("from")]
        public string CurrencyFrom
        {
            get { return _currencyFrom; }
            set { _currencyFrom = value; }
        }
        private string _currencyTo;

        [JsonMemberName("to")]
        public string CurrencyTo
        {
            get { return _currencyTo; }
            set { _currencyTo = value; }
        }

    }
}