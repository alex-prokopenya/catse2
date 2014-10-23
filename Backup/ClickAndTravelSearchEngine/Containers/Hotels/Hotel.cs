using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Hotels
{
    public class Hotel
    {
        private int _hotelId;
        [JsonMemberName("id")]
        public int HotelId
        {
            get { return _hotelId; }
            set { _hotelId = value; }
        }

        private Room[] _rooms;
        [JsonMemberName("rooms")]
        public Room[] Rooms
        {
            get { return _rooms; }
            set { _rooms = value; }
        }
    }
}