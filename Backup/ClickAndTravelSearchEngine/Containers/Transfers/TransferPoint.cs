using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Transfers
{
    public class TransferPoint
    {
        //идентификатор точки
        private int _id;

        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        //название
        private string _title;

        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        //тип точки
        private int _type;

        [JsonMemberName("type")]
        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
}