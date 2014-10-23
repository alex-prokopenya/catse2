using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.CarRent
{
    public class CarRentVariant
    {
        private int _supplierId;
        [JsonMemberName("supplier_id")]
        public int SupplierId
        {
            get { return _supplierId; }
            set { _supplierId = value; }
        }

        private string _supplierName;
        [JsonMemberName("supplier_name")]
        public string SupplierName
        {
            get { return _supplierName; }
            set { _supplierName = value; }
        }

        private int _variantId;
        [JsonMemberName("id")]
        public int VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private string _producer;
        [JsonMemberName("producer")]
        public string Producer
        {
            get { return _producer; }
            set { _producer = value; }
        }

        private string _typeName;
        [JsonMemberName("type_name")]
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        private string _image;
        [JsonMemberName("image")]
        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }

        private int _transmission;
        [JsonMemberName("transmission")]
        public int Transmission
        {
            get { return _transmission; }
            set { _transmission = value; }
        }

        private int _ac;
        [JsonMemberName("ac")]
        public int Ac
        {
            get { return _ac; }
            set { _ac = value; }
        }

        private int _doors;
         [JsonMemberName("doors")]
        public int Doors
        {
            get { return _doors; }
            set { _doors = value; }
        }

        private int _seats;
         [JsonMemberName("seats")]
        public int Seats
        {
            get { return _seats; }
            set { _seats = value; }
        }

         private CarRentPrice[] _programs;

         [JsonMemberName("programs")]
         public CarRentPrice[] Programs
         {
             get { return _programs; }
             set { _programs = value; }
         }



        /*{
            "prices":[
            {
                "name":"All Inclusive Plus",
                "id":"44_13",
                "price":"485.84"
              },
            {
            "name":"Standard",
            "id":"28",
            "status":"13",
            "price":"447.49",
            "currency":"EUR"},
            {
            "name":"All Inclusive",
            "id":"2",
            "status":"13",
            "price":"478.14",
            "currency":"EUR"}]
            },*/
    }
}