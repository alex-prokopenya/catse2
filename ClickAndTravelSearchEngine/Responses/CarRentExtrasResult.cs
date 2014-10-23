using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.CarRent;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class CarRentExtrasResult: _Response
    {
        private CarRentExtra[] _extras;
        [JsonMemberName("extras")]
        public CarRentExtra[] Extras
        {
          get { return _extras; }
          set { _extras = value; }
        }
    }
}