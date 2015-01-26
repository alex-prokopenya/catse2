using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.ParamsContainers;


namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public class OktogoService : IHotelExt
    {
        private bool added = false;
        private bool isFinished = false;

        private int cityId = 0;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MinValue;
        private int[] stars = new int[0];
        private int[] pansions = new int[0];
        private RequestRoom[] rooms = new RequestRoom[0];

        public OktogoService()
        { 
        
        }

        public void FindHotels()
        {
        }

        public void FindRooms(object hotelId)
        {

        }

        public Hotel[] GetHotels()
        {
            return null;
        }

        public Room[] GetRooms()
        {
            return null;
        }

        public HotelPenalties GetHotelPenalties(int hotelId, string variantId)
        {
            return null;
        }

        public HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId)
        {
            return null;
        }

        public HotelBooking[] BookRooms(string searchId, int hotelId, List<BookRoom> operatorRooms, List<List<int>> operatorTurists)
        {

            return null;
        }

        //ставим признак, что результаты обработаны
        public void SetAdded()
        {
            this.added = true;
        }

        //отдаем значение признака об обработке результатов
        public bool GetAdded()
        {
            return this.added;
        }

        //отдаем признак о завершении поиска
        public bool GetFinished()
        {
            return isFinished;
        }
    }
}