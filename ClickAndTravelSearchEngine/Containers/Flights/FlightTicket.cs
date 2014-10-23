using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.SF_service;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Flights
{
    public class FlightTicket
    {
        public FlightTicket()
        { }

        private FlightTicket(SF_service.Flight inp)
        {
           // _ticketId = inp.Id;
            this._serviceClass = inp.Parts[0].Legs[0].ServiceClass;
            this._airlineCode = inp.AirlineCode;


            //!!!FAKE
            this._timeLimit = DateTime.Now.AddHours(3);
            this._prices = new KeyValuePair<string, decimal>[] {
                    new KeyValuePair<string, decimal>("RUB", inp.Price),
                    new KeyValuePair<string, decimal>("BYR", inp.Price * 275),
                    new KeyValuePair<string, decimal>("USD", inp.Price / 31),
                    new KeyValuePair<string, decimal>("EUR", inp.Price / 41),
            };

            this._routeItems = new RouteItem[inp.Parts.Length];

            for (int i = 0; i < this._routeItems.Length; i++)
            {
                this._routeItems[i] = new RouteItem(inp.Parts[i]);
            }


        }

        public FlightTicket(SF_service.Flight inp, string id, int route_items_cnt, int ru_price)
        {
            this._ticketId = id;
            this._serviceClass = inp.Parts[0].Legs[0].ServiceClass;
            this._airlineCode = inp.AirlineCode;


            //!!!FAKE
            this._timeLimit = DateTime.Now.AddHours(2);
           /* _prices = new KeyValuePair<string, decimal>[] {
                    new KeyValuePair<string, decimal>("RUB", inp.Price),
                    new KeyValuePair<string, decimal>("BYR", inp.Price * 275),
                    new KeyValuePair<string, decimal>("USD", inp.Price / 31),
                    new KeyValuePair<string, decimal>("EUR", inp.Price / 41),
            };

            */

            _routeItems = new RouteItem[route_items_cnt];

            for (int i = 0; i < Math.Min(_routeItems.Length, route_items_cnt); i++)
                _routeItems[i] = new RouteItem(inp.Parts[i]);

            this.ruPrice = ru_price;
        }

        //идентификатор билета
        private string _ticketId;

        [JsonMemberName("id")]
        public string TicketId
        {
            get { return _ticketId; }
            set { _ticketId = value; }
        }

        //полная стоимость билетов
        //private int _price;

        //public int Price
        //{
        //    get { return _price; }
        //    set { _price = value; }
        //}

        private int ruPrice;
        [JsonIgnore]
        public int RuPrice
        {
            get { return ruPrice; }
            set { ruPrice = value; }
        }


        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get 
            {
                JsonObject pr = new JsonObject();

                foreach(KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr; 
            }
            set {  }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }
             

        //класс обслуживания E (econom) или B (business)
        private string _serviceClass;

        [JsonMemberName("service_class")]
        public string ServiceClass
        {
            get { return _serviceClass; }
            set { _serviceClass = value; }
        }

        //код авиакомпании
        private string _airlineCode;

        [JsonMemberName("airline_code")]
        public string AirlineCode
        {
            get { return _airlineCode; }
            set { _airlineCode = value; }
        }

        //время на выписку билета
        private DateTime _timeLimit;

        [JsonMemberName("time_limit")]
        public string TimeLimitString
        {
            get { return _timeLimit.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { }
        }

        [JsonIgnore]
        public DateTime TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; }
        }

        //массив участков маршрута
        private RouteItem[] _routeItems;

        [JsonMemberName("route_items")]
        public RouteItem[] RouteItems
        {
            get { return _routeItems; }
            set { _routeItems = value; }
        }

        private string _ticketHashKey = "";

        [JsonIgnore]
        public string TicketHashKey
        {
            get {

                if (this._ticketHashKey == "")
                {
                    string temp_hash = "" + this.ruPrice + "_" + this._airlineCode;

                    foreach (RouteItem ri in this._routeItems)
                        foreach (Leg leg in ri.Legs)
                            temp_hash += "_" + leg.FlightNum +"_"+ leg.ArrivalTime.ToString("dd");

                    this._ticketHashKey = temp_hash;
                }

                return this._ticketHashKey;
            }
            set { _ticketHashKey = value; }
        }
    }
}