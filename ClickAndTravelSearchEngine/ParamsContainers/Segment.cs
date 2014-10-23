using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class Segment
    {
        private string _depCode;

        public string DepCode
        {
            get { return _depCode; }
            set { _depCode = value; }
        }
        private string _arrCode;

        public string ArrCode
        {
            get { return _arrCode; }
            set { _arrCode = value; }
        }
        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
    }
}