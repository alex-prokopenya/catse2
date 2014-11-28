using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.Containers.Excursions
{
    public class ExcursionBooking
    {
        private ExcursionVariant _excVariant;

        [JsonMemberName("variant")]
        public ExcursionVariant ExcVariant
        {
            get { return _excVariant; }
            set { _excVariant = value; }
        }

        private string _selectedDate;

        [JsonMemberName("date")]
        public string SelectedDate
        {
            get { return _selectedDate; }
            set { _selectedDate = value; }
        }

        private string[] _turists;
        [JsonMemberName("turists")]
        public string[] Turists
        {
            get { return _turists; }
            set { _turists = value; }
        }

        private string search_id;
        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return search_id; }
            set { search_id = value; }
        }
    }
}