using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;
namespace ClickAndTravelSearchEngine.Containers.Visa
{
    public class VisaDetails
    {
        //идентификатор
        private int _id;

        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        //название
        private string _visaName;

        [JsonMemberName("visa_name")]
        public string VisaName
        {
            get { return _visaName; }
            set { _visaName = value; }
        }

        //код страны назначения
        private string _countryCode;

        [JsonMemberName("country_code")]
        public string CountryCode
        {
            get { return _countryCode; }
            set { _countryCode = value; }
        }

        //код гражданства
        private string _citizenshipCode;

        [JsonMemberName("citizenship_code")]
        public string CitizenshipCode
        {
            get { return _citizenshipCode; }
            set { _citizenshipCode = value; }
        }

        //стоимость
        private string _titleCurrency;


        [JsonIgnore]
        public string TitleCurrency
        {
            get { return _titleCurrency; }
            set { _titleCurrency = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set { }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

        //ограничение по возрасту
        private int _ageLimit;

        [JsonMemberName("age_limit")]
        public int AgeLimit
        {
            get { return _ageLimit; }
            set { _ageLimit = value; }
        }

        //город отправления
        private int _cityId;

        [JsonMemberName("city_id")]
        public int CityId
        {
            get { return _cityId; }
            set { _cityId = value; }
        }
    }
}