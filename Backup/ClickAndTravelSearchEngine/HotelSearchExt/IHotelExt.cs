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

         Room[] GetRooms();

         bool GetAdded();

         void SetAdded();

         HotelPenalties GetHotelPenalties(int hotelId, string variantId);

         HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId);

         HotelBooking[] BookRooms(int hotelId, List<BookRoom> operatorRooms, List<List<int>> operatorTurists);
    }
}