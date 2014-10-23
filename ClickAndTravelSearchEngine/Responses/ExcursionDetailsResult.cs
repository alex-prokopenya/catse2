using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Excursions;


namespace ClickAndTravelSearchEngine.Responses
{
    public class ExcursionDetailsResult:_Response
    {
        private ExcursionVariant _excursionVariant;

        public ExcursionVariant ExcursionVariant
        {
            get { return _excursionVariant; }
            set { _excursionVariant = value; }
        }

        private string _currencyCode;

        public string CurrencyCode
        {
            get { return _currencyCode; }
            set { _currencyCode = value; }
        }  
    }
}