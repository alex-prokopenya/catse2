using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentPoint
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _supplierId;

        public int SupplierId
        {
            get { return _supplierId; }
            set { _supplierId = value; }
        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private int _locationId;

        public int LocationId
        {
            get { return _locationId; }
            set { _locationId = value; }
        }
    }
}