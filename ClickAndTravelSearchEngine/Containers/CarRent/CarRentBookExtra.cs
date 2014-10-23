using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;


namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentBookExtra
    {
        public CarRentBookExtra() { }

        public CarRentBookExtra(JsonObject inp)
        {
            try
            {
                this._extraId =   Convert.ToInt32( inp["id"]);
                this._extraCount = Convert.ToInt32(inp["count"]);
            }
            catch (Exception ex)
            {
                Helpers.Logger.WriteToLog("cann't parse CarRentBookExtra " + inp.ToString() + ex.Message + " " + ex.StackTrace);
                throw new Exception("cann't parse CarRentBookExtra");
              
            }
        }

        private int _extraId;

        public int ExtraId
        {
            get { return _extraId; }
            set { _extraId = value; }
        }

        private int _extraCount;

        public int ExtraCount
        {
            get { return _extraCount; }
            set { _extraCount = value; }
        }
    }
}