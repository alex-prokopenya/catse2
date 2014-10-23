using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentLocation
    {
        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _cityId;
        [JsonMemberName("city_id")]
        public int CityId
        {
            get { return _cityId; }
            set { _cityId = value; }
        }

        private string _title;
        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }
}