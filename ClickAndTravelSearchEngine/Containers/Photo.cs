using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers
{
    public class Photo : AnyFile
    {
        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        private int _width;

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }
}