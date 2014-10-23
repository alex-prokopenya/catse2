using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers
{
    public class AnyFile
    {
        private string _extension;

        public string Extension
        {
            get { return _extension; }
            set { _extension = value; }
        }
        private int _size;

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
        private string _folder;

        public string Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}