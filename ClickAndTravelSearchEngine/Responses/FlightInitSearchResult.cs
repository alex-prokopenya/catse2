using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    [XmlRoot]
    public class FlightInitSearchResult : _Response
    {
        //идентификатор поискового запроса
        private string _searchId;

        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }
    }
}