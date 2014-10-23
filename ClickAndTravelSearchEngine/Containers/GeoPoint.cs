using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers
{
    public class GeoPoint
    {
        private decimal _longitude;

        public decimal Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
        private decimal _latitude;

        public decimal Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }
        private short _zoom;

        public short Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }
    }
}