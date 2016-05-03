using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.ParamsContainers;

namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public interface IHotelExt
    {
         void FindHotels();

         void FindRooms(object hotelId);

         Hotel[] GetHotels();

         Room GetRoom();

         bool GetFinished();

         bool GetAdded();

         void SetAdded();

         string GetPrefix();

         HotelPenalties GetHotelPenalties(int hotelId, string variantId);

         HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId);

         HotelBooking BookRoom(string searchId, int hotelId, BookRoom operatorRoom, List<int> operatorTurists);
    }
}