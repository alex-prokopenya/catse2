using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Tours
{
    public class TourHotelRoom
    {
        private int _hotelId;
        [JsonMemberName("hotel_id")]
        public int HotelId
        {
            get { return _hotelId; }
            set { _hotelId = value; }
        }

        private string _pansionTitle;
        [JsonMemberName("pansion_title")]
        public string PansionTitle
        {
            get { return _pansionTitle; }
            set { _pansionTitle = value; }
        }

        private string _roomCategory;
        [JsonMemberName("room_category")]
        public string RoomCategory
        {
            get { return _roomCategory; }
            set { _roomCategory = value; }
        }

        private string _roomType;
        [JsonMemberName("room_type")]
        public string RoomType
        {
            get { return _roomType; }
            set { _roomType = value; }
        }

        private int _pansionGroupId;
        [JsonMemberName("pansion_group_id")]
        public int PansionGroupId
        {
            get { return _pansionGroupId; }
            set { _pansionGroupId = value; }
        }

        private int _daysLong;
        [JsonMemberName("days_long")]
        public int DaysLong
        {
            get { return _daysLong; }
            set { _daysLong = value; }
        }

        private int _day;
        [JsonMemberName("day")]
        public int Day
        {
            get { return _day; }
            set { _day = value; }
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