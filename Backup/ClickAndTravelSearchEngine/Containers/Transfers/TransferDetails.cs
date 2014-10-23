using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers.Transfers
{
    public class TransferDetails
    {
        //идентификатор
        private int _id;

        public int Id
        {
          get { return _id; }
          set { _id = value; }
        }

        //название
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        //стоимость за машину
        private int _pricePerCar;

        public int PricePerCar
        {
            get { return _pricePerCar; }
            set { _pricePerCar = value; }
        }

        //точка начала трансфера
        private int _startPoint;

        public int StartPoint
        {
            get { return _startPoint; }
            set { _startPoint = value; }
        }

        //конечная точка
        private int _endPoint;

        public int EndPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; }
        }

        private Vehicle _Vehicle;

        public Vehicle Vehicle
        {
            get { return _Vehicle; }
            set { _Vehicle = value; }
        }
    }
}