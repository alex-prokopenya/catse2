using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.SF_service;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Flights
{
    //класс для описания участка маршрута с учетом пересадок и стыковок
    public class RouteItem
    {
        public RouteItem()
        { }

        public RouteItem(SF_service.FlightPart inp)
        {
            _timeLong = inp.FlightLong;
            _legs = new Leg[inp.Legs.Length];

            for (int i = 0; i < _legs.Length; i++)
                if (i + 1 < _legs.Length)
                    _legs[i] = new Leg(inp.Legs[i], inp.Legs[i+1]);
                else
                    _legs[i] = new Leg(inp.Legs[i], null);
        }

        //продолжительность перелетов с учетом ожидания
        private int _timeLong;

        [JsonMemberName("time_long")]
        public int TimeLong
        {
            get { return _timeLong; }
            set { _timeLong = value; }
        }

        //все перелеты при движении по участку маршрута
        private Leg[] _legs;

        [JsonMemberName("legs")]
        public Leg[] Legs
        {
            get { return _legs; }
            set { _legs = value; }
        }
    }
}