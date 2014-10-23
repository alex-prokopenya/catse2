using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class TourService: _Response
    {
        private int _day;
        [JsonMemberName("day")]
        public int Day
        {
            get { return _day; }
            set { _day = value; }
        }

        private int _daysLong;
        [JsonMemberName("days_long")]
        public int DaysLong
        {
            get { return _daysLong; }
            set { _daysLong = value; }
        }

        private int _serviceClass;
        [JsonMemberName("service_class")]
        public int ServiceClass
        {
            get { return _serviceClass; }
            set { _serviceClass = value; }
        }

        private string _title;
        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private int _cityId;
        [JsonMemberName("city_id")]
        public int CityId
        {
            get { return _cityId; }
            set { _cityId = value; }
        }
    }
}