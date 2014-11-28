using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using ClickAndTravelSearchEngine.Exceptions;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class FlightBonusCard
    {

        public FlightBonusCard()
        { }

        public FlightBonusCard(JsonObject inp)
        {
            try
            {
                _airlineCode = (inp["airline_code"] is JsonNull) ? "" : inp["airline_code"].ToString().Replace("null", "");
                _cardNumber = (inp["card_number"] is JsonNull) ? "" : inp["card_number"].ToString().Replace("null","");
            }
            catch (Exception)
            { }

            if (_cardNumber.Trim().Length > 0)
                if (!Validator.CheckString(_airlineCode, @"[A-Z,0-9]{2}$")) throw new CatseException("Cannt parse c/n", ErrorCodes.TuristBonusCardError);
        }

        //код авиакомпании
        private string _airlineCode="";

        public string AirlineCodeinp
        {
            get { return _airlineCode; }
            set { _airlineCode = value; }
        }

        //номер карты
        private string _cardNumber="";

        public string CardNumber
        {
            get { return _cardNumber; }
            set { _cardNumber = value; }
        }
    }
}