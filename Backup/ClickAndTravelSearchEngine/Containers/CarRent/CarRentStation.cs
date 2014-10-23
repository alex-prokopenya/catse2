using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentStation
    {
        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
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