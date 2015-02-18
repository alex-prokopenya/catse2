using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Transfers
{
    public class TransferInfo
    {
        public TransferInfo()
        { 
        
        
        }


        public TransferInfo(JsonObject arr)
        {
            try
            {
                this._dateDeparture = arr["date_departure"].ToString();
                this._dateArrival = arr["date_arrival"].ToString();
                this._locationAddress = arr["location_address"].ToString();
                this._destinationAddress = arr["destination_address"].ToString();
                this._arrivalNumber = arr["arrival_number"].ToString();
            }
            catch (Exception)
            {
                throw new Exception("Cann't parse transfer info");
            }
        }


        private string _dateDeparture;
        public string DateDeparture
        {
            get { return _dateDeparture; }
            set { _dateDeparture = value; }
        }


        private string _dateArrival;
        public string DateArrival
        {
            get { return _dateArrival; }
            set { _dateArrival = value; }
        }


        private string _arrivalNumber;
        public string ArrivalNumber
        {
            get { return _arrivalNumber; }
            set { _arrivalNumber = value; }
        }


        private string _locationAddress;
        public string LocationAddress
        {
            get { return _locationAddress; }
            set { _locationAddress = value; }
        }

        private string _destinationAddress;
        public string DestinationAddress
        {
            get { return _destinationAddress; }
            set { _destinationAddress = value; }
        }
    }
}