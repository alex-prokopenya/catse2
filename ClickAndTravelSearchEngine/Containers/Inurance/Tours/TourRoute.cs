using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Tours;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Tours
{
    public class TourRoute
    {
        private int _tourId;
        [JsonMemberName("tour_id")]
        public int TourId
        {
            get { return _tourId; }
            set { _tourId = value; }
        }

        private string _title;
         [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private int _tourType;
         [JsonMemberName("tour_type")]
        public int TourType
        {
            get { return _tourType; }
            set { _tourType = value; }
        }

        private string _operatorName;
         [JsonMemberName("operator")]
        public string OperatorName
        {
            get { return _operatorName; }
            set { _operatorName = value; }
        }

        private TourDates[] _dates;
         [JsonMemberName("tour_dates")]
        public TourDates[] Dates
        {
            get { return _dates; }
            set { _dates = value; }
        }
    }
}