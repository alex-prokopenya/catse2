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
                this._arrivalInfo = arr["arrival_info"].ToString();
                this._departureInfo = arr["departure_info"].ToString();
                this._time = arr["time"].ToString();
            }
            catch (Exception)
            {
                throw new Exception("Cann't parse transfer info");
            }
        }
        
        private string _departureInfo;

        public string DepartureInfo
        {
            get { return _departureInfo; }
            set { _departureInfo = value; }
        }
        private string _arrivalInfo;

        public string ArrivalInfo
        {
            get { return _arrivalInfo; }
            set { _arrivalInfo = value; }
        }

        private string _time;

        public string Time
        {
            get { return _time; }
            set { _time = value; }
        }
    }
}