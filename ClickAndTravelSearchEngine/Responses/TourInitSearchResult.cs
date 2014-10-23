using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Responses
{
    public class TourInitSearchResult: _Response
    {
        private string _searchId;

        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }
    }
}