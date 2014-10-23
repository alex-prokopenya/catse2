using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Tours
{
    public class TourVariantsGroup //варианты тура по сочетанию отелей, продолжительности и дате тура
    {
        private int _variantsGroupId;
        [JsonMemberName("variants_group_id")]
        public int VariantsGroupId
        {
            get { return _variantsGroupId; }
            set { _variantsGroupId = value; }
        }

        private int _daysLong;
        [JsonMemberName("days_long")]
        public int DaysLong
        {
            get { return _daysLong; }
            set { _daysLong = value; }
        }

        private DateTime _turDate;
        [JsonIgnore]
        public DateTime TurDate
        {
            get { return _turDate; }
            set { _turDate = value; }
        }
        [JsonMemberName("tour_date")]
        public string TurDateString
        {
            get { return _turDate.ToString("yyyy-MM-dd"); }
            set {}
        }



        private int _tourId;
        [JsonMemberName("tour_id")]
        public int TourId
        {
            get { return _tourId; }
            set { _tourId = value; }
        }

        private int[] _hotelIds;//уникальный набор отелей в сочетании с датой тура и продолжительностью для результатов поиска
        [JsonMemberName("hotels_ids")]                         //либо один отель
        public int[] HotelIds
        {
            get { return _hotelIds; }
            set { _hotelIds = value; }
        }

        private TourVariant[] _variants;
        [JsonMemberName("variants")] 
        public TourVariant[] Variants
        {
            get { return _variants; }
            set { _variants = value; }
        }
    }
}