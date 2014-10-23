using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Flights
{
    //перелет
    public class Leg
    {
        public Leg()
        { }

        public Leg(SF_service.Leg inp, SF_service.Leg next_leg)
        {
            _airlineCode = inp.Airline;
            _arrivalCode = inp.LocationEnd;
            _arrivalTime = inp.DateEnd;
            _departureCode = inp.LocationBegin;
            _departureTime = inp.DateBegin;
            _flightNum = inp.FlightNumber;
            _planeCode = inp.Board;
            _timeLong = inp.Duration;

            

            _waitTime = 0;

            if (next_leg != null)
                _waitTime = Convert.ToInt32( (next_leg.DateBegin - inp.DateEnd).TotalMinutes);
        }

        //дата и время вылета
        private DateTime _departureTime;

        [JsonIgnore]
        public DateTime DepartureTime
        {
            get { return _departureTime; }
            set { _departureTime = value; }
        }

        [JsonMemberName("departure_time")]
        public string DepartureTimeJson
        {
            get { return _departureTime.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { }
        }

        //дата и время прилета
        private DateTime _arrivalTime;

        [JsonIgnore]
        public DateTime ArrivalTime
        {
            get { return _arrivalTime; }
            set { _arrivalTime = value; }
        }

        [JsonMemberName("arrival_time")]
        public string ArrivalTimeJson
        {
            get { return _arrivalTime.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { }
        }


        //продолжительность полета
        private int _timeLong;

        [JsonMemberName("time_long")]
        public int TimeLong
        {
            get { return _timeLong; }
            set { _timeLong = value; }
        }


        //продолжительность ожидания следующего рейса
        private int _waitTime;

        [JsonMemberName("wait_time")]
        public int WaitTime
        {
            get { return _waitTime; }
            set { _waitTime = value; }
        }

        //код авиакомпании
        private string _airlineCode;

        [JsonMemberName("airline_code")]
        public string AirlineCode
        {
            get { return _airlineCode; }
            set { _airlineCode = value; }
        }
        //код самолета
        private string _planeCode;

        [JsonMemberName("plane_code")]
        public string PlaneCode
        {
            get { return _planeCode; }
            set { _planeCode = value; }
        }

        //код аэропорта вылета
        private string _departureCode;

        [JsonMemberName("departure_code")]
        public string DepartureCode
        {
            get { return  _departureCode;
            }
            set { _departureCode = value; }
        }

        //код аэропорта прилета
        private string _arrivalCode;

        [JsonMemberName("arrival_code")]
        public string ArrivalCode
        {
            get { return _arrivalCode; }
            set { _arrivalCode = value; }
        }
        //номер рейса
        private string _flightNum;

        [JsonMemberName("flight_num")]
        public string FlightNum
        {
            get { return _flightNum; }
            set { _flightNum = value; }
        }
    }
}