using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers
{
    public class Vehicle
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private Photo _photo;

        public Photo Photo
        {
            get { return _photo; }
            set { _photo = value; }
        }

        private short _nmen;

        public short Nmen
        {
            get { return _nmen; }
            set { _nmen = value; }
        }

        private int _carClass;

        public int CarClass
        {
            get { return _carClass; }
            set { _carClass = value; }
        }

    }
}