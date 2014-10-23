using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;



namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentStationDetails
    {
        /*
            $street = $drop_st_doc->getElementsByTagName("PickUpStreet")->item(0)->nodeValue;
	        $city = $drop_st_doc->getElementsByTagName("PickUpCity")->item(0)->nodeValue;
	        $country = $drop_st_doc->getElementsByTagName("PickUpCountry")->item(0)->nodeValue;
	        $transfer = $drop_st_doc->getElementsByTagName("PickUpTransfer")->item(0)->nodeValue;  
	        $zip_code = $drop_st_doc->getElementsByTagName("PickUpZipCode")->item(0)->nodeValue;
	        $phone = $drop_st_doc->getElementsByTagName("PickUpPhone")->item(0)->nodeValue;
	        $fax = $drop_st_doc->getElementsByTagName("PickUpFax")->item(0)->nodeValue;
	        $pick_time = $drop_st_doc->getElementsByTagName("PickUpOpeningHours")->item(0)->nodeValue;
	        $drop_time = $drop_st_doc->getElementsByTagName("DropOffOpeningHours")->item(0)->nodeValue;
         */

        private string _street;
        [JsonMemberName("street")]
        public string Street
        {
            get { return _street; }
            set { _street = value; }
        }

        private string _cityName;
        [JsonMemberName("city_name")]
        public string CityName
        {
            get { return _cityName; }
            set { _cityName = value; }
        }

        private string _country;
        [JsonMemberName("country")]
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        private string _zipCode;
        [JsonMemberName("zip_code")]
        public string ZipCode
        {
            get { return _zipCode; }
            set { _zipCode = value; }
        }

        private string _phone;
        [JsonMemberName("phone")]
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        private string _fax;
         [JsonMemberName("fax")]
        public string Fax
        {
            get { return _fax; }
            set { _fax = value; }
        }

        private string _pickTime;
         [JsonMemberName("pick_time")]
        public string PickTime
        {
            get { return _pickTime; }
            set { _pickTime = value; }
        }

        private string _dropTime;
         [JsonMemberName("drop_time")]
        public string DropTime
        {
            get { return _dropTime; }
            set { _dropTime = value; }
        }
    }
}