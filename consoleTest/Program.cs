using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickAndTravelSearchEngine;
using System.Configuration;
using ClickAndTravelSearchEngine.HotelSearchExt;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Containers.Transfers;
using ClickAndTravelSearchEngine.MasterTour;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.TransferSearchExt;
//using ClickAndTravelSearchEngine;


namespace consoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //////протестировать бронирование
            var sId = "t-1301-42-2202-2502-1";
            var pId = "35200_35201#1";

            var trInfo = new List<TransferInfo>();

            trInfo.Add(new TransferInfo()
            {
                DateArrival = "2015-02-22 15:25",
                ArrivalNumber = "пж124",
                DestinationAddress = "пл. победы"
            });

            trInfo.Add(new TransferInfo()
            {
                DateArrival = "2015-02-25 08:25",
                DateDeparture = "2015-02-25 15:25",
                LocationAddress = "пл. победы"
            });

            var uInfo = new UserInfo()
            {
                Email = "test@test.tt",
                Phone = "+12234534523"
            };

            var turists = new List<TuristContainer>();
            turists.Add(new TuristContainer()
            {
                BirthDate = new DateTime(1980, 12, 20),
                Citizenship = "RU",
                FirstName = "tesetet",
                Id = 15067,
                MiddleName = "ollool",
                Name = "oololoev",
                PassportDate = new DateTime(2022, 5, 5),
                PassportNum = "123235345",
                Sex = 1
            });

            SearchEngine sre = new SearchEngine();

            sre.TransferBook(sId, pId, trInfo.ToArray(), uInfo, turists.ToArray());

            return;
            var res = IWaySearcher.GetInfoMasks("35200_35201#1");

            Console.WriteLine("" + res.Length);
            return;

            int ad1 = 1;
            int ad2 = 2;
            var chAges1 = new int[]{5};
            var chAges2 = new int[0];

            OktogoService okService = null;

            //oktogo search hotels in city
            Console.Write("Hello!");

            Hotel[] hotelsArray = null;
            Room[] roomsArray = null;

            #region //city search
            while (true)
            { 
                Console.WriteLine(" enter city id to search!");

                var cityId = Convert.ToInt32(Console.ReadLine());

                okService = CreateOkSearcher(cityId, DateTime.Today.AddDays(115), DateTime.Today.AddDays(125),
                                             new int[] { 3, 4, 5 }, ad1, chAges1, ad2, chAges2);

                hotelsArray = SearchHotelsInCity(okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more city search? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }
            #endregion

            #region //oktogo search rooms in hotel
            var hotelId = 0;

            while (true)
            {
                Console.WriteLine(" enter hotel id to search!");

                hotelId = Convert.ToInt32(Console.ReadLine());

                roomsArray = SearchRoomsInHotel(hotelId, okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more hotel search? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }
           #endregion

            #region //oktogo verify hotelvariant
            while (true)
            {
                Console.WriteLine(" enter hotels variant id index to verify");
                Console.WriteLine(" enter hotel index from 0 to " + (hotelsArray.Length -1));

                var hotelIndex = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine(" enter room index from 0 to " + (hotelsArray[hotelIndex].Rooms.Length - 1));

                var roomIndex = Convert.ToInt32(Console.ReadLine());

                VerifyRoom(hotelsArray[hotelIndex].Rooms[roomIndex].Variants[0].VariantId,
                            hotelsArray[hotelIndex].HotelId, okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more hotels variant verify? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }
            #endregion

            #region //oktogo verify roomvariant
            while (true)
            {
                Console.WriteLine(" enter room variant id index to verify");
                Console.WriteLine(" enter room index from 0 to " + (roomsArray.Length - 1));

                var roomIndex = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine(" enter variant index from 0 to " + (roomsArray[roomIndex].Variants.Length - 1));

                var variantIndex = Convert.ToInt32(Console.ReadLine());

                VerifyRoom(roomsArray[roomIndex].Variants[variantIndex].VariantId,
                            hotelId, okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more rooms variant verify? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }
            #endregion

            string variantId = "";
            //oktogo hotelvariant get penalties
            while (true)
            {
                Console.WriteLine(" enter hotels variant id index to get penalties");
                Console.WriteLine(" enter hotel index from 0 to " + (hotelsArray.Length - 1));

                var hotelIndex = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine(" enter room index from 0 to " + (hotelsArray[hotelIndex].Rooms.Length - 1));

                var roomIndex = Convert.ToInt32(Console.ReadLine());

                variantId = hotelsArray[hotelIndex].Rooms[roomIndex].Variants[0].VariantId;

                GetPenalties(variantId, hotelsArray[hotelIndex].HotelId, okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more hotels variant get penalties? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }

            //oktogo roomvariant get penalties
            while (true)
            {
                Console.WriteLine(" enter room variant id index to get penalties");
                Console.WriteLine(" enter room index from 0 to " + (roomsArray.Length - 1));

                var roomIndex = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine(" enter variant index from 0 to " + (roomsArray[roomIndex].Variants.Length - 1));

                var variantIndex = Convert.ToInt32(Console.ReadLine());

                variantId = roomsArray[roomIndex].Variants[variantIndex].VariantId; 

                GetPenalties(variantId, hotelId, okService);

                Console.WriteLine("_________________________________");
                Console.WriteLine("Make once more rooms variant get penalties? Y-yes, N-no");

                if (Console.ReadLine().ToUpper() == "N")
                    break;
            }

            Console.WriteLine("please enter hotel index from 0 to " + (hotelsArray.Length - 1));

            var bookIndex = Convert.ToInt32(Console.ReadLine());

            //oktogo reserve
            var userInfo = new UserInfo() { 
                Email = "it@viziteurope.eu",
                Phone = "123234534534" 
            };
            
            List<BookRoom> bookRooms = new List<BookRoom>();

            if((ad1 > 0) && (hotelsArray[bookIndex].Rooms.Length > 0))
                bookRooms.Add(new BookRoom() {
                    VariantId = hotelsArray[bookIndex].Rooms[0].Variants[0].VariantId,
                    Turists = GenerateTurists(ad1, chAges1)
                });

            if ((ad2 > 0) && (hotelsArray[bookIndex].Rooms.Length > 1))
                bookRooms.Add(new BookRoom()
                {
                    VariantId = hotelsArray[bookIndex].Rooms[1].Variants[0].VariantId,
                    Turists = GenerateTurists(ad2, chAges2)
                });

            var hb = HotelBook(okService.GetSearchId(), hotelsArray[bookIndex].HotelId, userInfo, bookRooms.ToArray());

            Console.WriteLine("bk " + hb.Length);

            //oktogo book

            var resp = okService.CreateBooking(hb, "dg234234", DateTime.Today.AddDays(115));

            Console.WriteLine("resp=" + resp);

            Console.ReadKey();
        }

        private static TuristContainer[] GenerateTurists(int ads, int[] childAges)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            var turits = new List<TuristContainer>();

            for (int i = 0; i < ads; i++)
            {
                turits.Add(new TuristContainer() { 
                    BirthDate = DateTime.Today.AddDays(-(7300+ rnd.Next(1500,5000))),
                    Citizenship = "BY",
                    BonusCard = null,
                    FirstName = GetName(rnd.Next(0,10)),
                    Name = GetLastName(rnd.Next(0, 10)),
                    Id = rnd.Next(1500, 5000),
                    MiddleName = "",
                    PassportDate = DateTime.Today.AddDays(rnd.Next(1500,5000)),
                    PassportNum = "KM" + rnd.Next(1000000, 1999999),
                    Sex = rnd.Next(0,1)
                });
            }

            foreach (int age in childAges)
            {
                turits.Add(new TuristContainer()
                {
                    BirthDate = DateTime.Today.AddDays(-(365*age + rnd.Next(50, 150))),
                    Citizenship = "BY",
                    BonusCard = null,
                    FirstName = GetName(rnd.Next(0, 10)),
                    Name = GetLastName(rnd.Next(0, 10)),
                    Id = rnd.Next(1500, 5000),
                    MiddleName = "",
                    PassportDate = DateTime.Today.AddDays(rnd.Next(1500, 5000)),
                    PassportNum = "KM" + rnd.Next(1000000, 1999999),
                    Sex = rnd.Next(0, 1)
                });
            }

            return turits.ToArray();
        }

        private static string GetName(int rnd)
        {
            string[] names = new string[] { "Anya","Vasya","Petya","Kolya","Misha","Sasha","Fedya","Vova","Natasha","Serega","Egor"};

            return names[rnd];
        }

        private static string GetLastName(int rnd)
        {
            string[] names = new string[] { "Frolova", "Egorova", "Ivanov", "Nikitin", "Klez", "Kaz", "Sidorova", "Zaitseva", "Hitrik", "Blisch", "Rusova" };

            return names[rnd];
        }

        /// <summary>
        /// Makes hotels search by city
        /// </summary>
        /// <param name="okService">object of oktogo service client</param>
        /// <returns>search id for hotel rooms search</returns>
        private static Hotel[] SearchHotelsInCity(OktogoService okService)
        {
            okService.FindHotels();

            var result = okService.GetHotels( );

            if(result != null)
                foreach (var hotel in result)
                    Console.WriteLine(hotel.HotelId + " rooms " + hotel.Rooms.Length + " " + hotel.Rooms[0].Variants[0].VariantId);

            return result;
        }

        /// <summary>
        /// create and initialize objects of oktogo searcher
        /// </summary>
        /// <param name="cityId">id of destination city</param>
        /// <param name="dateFrom">check in date</param>
        /// <param name="dateTo">check out date</param>
        /// <param name="stars">array of hotels' stars categories </param>
        /// <param name="ad1">adults count in room 1</param>
        /// <param name="chAges1">array of children ages in room 1</param>
        /// <param name="ad2">adults count in room 2</param>
        /// <param name="chAges2">array of children ages in room 2</param>
        /// <returns>object of oktogo service client</returns>
        private static OktogoService CreateOkSearcher(int cityId, DateTime dateFrom, DateTime dateTo, int[] stars, int ad1, int[] chAges1, int ad2, int[] chAges2)
        {
            var searchId = "";

            var rooms = new List<RequestRoom>();

            #region generate_search_id
            searchId = dateFrom.ToString("ddMM") + dateTo.ToString("ddMM") + cityId + "-";

            if (stars.Length == 0)
                searchId += "0";
            else
                foreach (int star in stars) searchId += star;

            searchId += "-";
            searchId += "0";

            if (ad1 > 0)
            {
                searchId += "-" + ad1 + "" + chAges1.Length;

                foreach (int age in chAges1)
                    searchId += (age < 10 ? "0" : "") + age;

                rooms.Add(new RequestRoom()
                {
                    Adults = ad1,
                    Children = chAges1.Length,
                    ChildrenAges = chAges1
                });
            }

            if (ad2 > 0)
            {
                searchId += "-" + ad2 + "" + chAges2.Length;

                foreach (int age in chAges2)
                    searchId += (age < 10 ? "0" : "") + age;

                rooms.Add(new RequestRoom()
                {
                    Adults = ad2,
                    Children = chAges2.Length,
                    ChildrenAges = chAges2
                });
            }
            #endregion

           return new OktogoService(
                cityId,
                dateFrom,
                dateTo,
                stars,
                new int[0],
                rooms.ToArray(),
                searchId,
                6000000000
            );
        }

        /// <summary>
        /// search rooms in selected hotel
        /// </summary>
        /// <param name="hotelId"></param>
        /// <param name="okService"></param>
        private static Room[] SearchRoomsInHotel(int hotelId, OktogoService okService)
        {
            okService.FindRooms(hotelId);

            if (okService.GetRooms() != null)
            {
                int roomIndex=0;
                foreach (var room in okService.GetRooms())
                {
                    Console.WriteLine(roomIndex++ + " room has " + room.Variants.Length + " vars");

                    foreach (var roomVariant in room.Variants)
                        Console.WriteLine("     " + roomVariant.RoomTitle + " " + roomVariant.VariantId );
                }
            }

            return okService.GetRooms();
        }

        private static void VerifyRoom(string roomId, int hotelId, OktogoService okService)
        {
            var resp = okService.VerifyHotelVariant(hotelId, roomId);

            if (resp.IsAvailable)
                Console.WriteLine(resp.IsAvailable + " " + resp.Prices[0].Value + " " + resp.Prices[0].Key);
            else
                Console.WriteLine(resp.IsAvailable);
        }

        private static void GetPenalties(string roomId, int hotelId, OktogoService okService)
        {
            var resp = okService.GetHotelPenalties(hotelId, roomId);

            Console.WriteLine(JsonConvert.ExportToString(resp.CancelingPenaltiesJson));
        }

        private static void ReserveRoom(string roomId, int hotelId, OktogoService okService)
        {
            //var resp = okService.BookRooms(hotelId, roomId);


        }

        private static void BookRoom()
        {

        }


        public static HotelBooking[] HotelBook(string SearchId, int HotelId, UserInfo Info, BookRoom[] bookRooms)
        {
            //ErrorCode 37: неизвестный searchId
            //ErrorCode 38: неизвестный hotelId
            //ErrorCode 39: не найден variantId

            //ErrorCode 90: неправильный состав туристов (неверное количество, состав взрослые/дети)
            //ErrorCode 91: ошибка в дате рождения туриста
            //ErrorCode 92: ошибка в имени/фамилии туриста
            //ErrorCode 93: ошибка в гражданстве
            //ErrorCode 94: ошибка в номере паспорта
            //ErrorCode 95: ошибка в сроке действия паспорта
            //ErrorCode 96: дублируется турист

            //ErrorCode 98: невалидный и-мэйл пользователя
            //ErrorCode 99: невалидный телефон пользователя

            //проверить поиск и варианты
            //var pHash = SearchEngine.ParseHotelSearchId(SearchId); //парсим серч айди
            //DateTime dateEnd = (DateTime)pHash["dateEnd"]; //берем дату окончания тура
            //RequestRoom[] requestRooms = pHash["rooms"] as RequestRoom[];

            ////создаем контрольные суммы для проверки состава туристов
            //string[] checkTuristString = new string[requestRooms.Length];

            //for (int i = 0; i < requestRooms.Length; i++)
            //    checkTuristString[i] = requestRooms[i].ToCompareString();

            ////проверяем совпадает ли количество комнат при поиске и при бронировании

            //List<int> turistsIds = new List<int>();
            //List<string> turistsHashes = new List<string>();

            //List<List<int>> roomTurists = new List<List<int>>();

            //#region//по каждой комнате сохраняем список туристов, проверяем паспорта и состав
            //foreach (BookRoom room in bookRooms)
            //{
            //    int adls = 0;
            //    int chd = 0;
            //    List<int> ages = new List<int>();

            //    //
            //    foreach (TuristContainer tsc in room.Turists)
            //    {
            //        int turistAge = tsc.GetAge(dateEnd);

            //        if (turistAge > 17)
            //            adls++;
            //        else
            //        {
            //            chd++;
            //            ages.Add(turistAge);
            //        }

            //        MtHelper.SaveTuristToCache(tsc);//туристы сохраняются в БД

            //        if ((tsc.PassportDate > new DateTime(1970, 1, 2)) && (tsc.PassportDate < dateEnd.AddDays(90)))
            //            throw new CatseException("pasport date limit", ErrorCodes.TuristCanntPaspDate);

            //        string turistHash = tsc.HashSumm;

            //        if ((!turistsIds.Contains(tsc.Id)) && (!turistsHashes.Contains(turistHash)))
            //        {
            //            turistsIds.Add(tsc.Id);
            //            turistsHashes.Add(turistHash);
            //        }
            //        else
            //            throw new CatseException("turist duplicate", ErrorCodes.TuristDuplicate);
            //    }

            //    string checkStr = new RequestRoom() { Adults = adls, Children = chd, ChildrenAges = ages.ToArray() }.ToCompareString();

            //    bool throwException = true;

            //    for (int i = 0; (i < checkTuristString.Length) && throwException; i++)
            //        if (checkTuristString[i] == checkStr)
            //        {
            //            checkTuristString[i] = "checked";
            //            throwException = false;
            //        }

            //    if (throwException)
            //        throw new CatseException("check rooms' turists" + checkTuristString[0] + " " + checkStr, ErrorCodes.HotelRoomsTurists);
            //    else
            //    {
            //        roomTurists.Add(turistsIds);
            //        turistsIds = new List<int>();
            //    }
            //}
            //#endregion

            //try
            //{
            //    Dictionary<string, IHotelExt> searchers = new Dictionary<string, IHotelExt>();
                
            //    if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
            //        searchers.Add(VizitHotelsSearch.id_prefix, new VizitHotelsSearch((int)pHash["cityId"],
            //                                                    (DateTime)pHash["dateStart"],
            //                                                    (DateTime)pHash["dateEnd"],
            //                                                    pHash["stars"] as int[],
            //                                                    pHash["pansions"] as int[],
            //                                                    pHash["rooms"] as RequestRoom[],
            //                                                    SearchId, Convert.ToInt64(7200000000)
            //            ));

            //    if (ConfigurationManager.AppSettings["SearchHotelsInUts"] == "true")
            //        searchers.Add(UtsService.id_prefix, new UtsService((int)pHash["cityId"],
            //                                                    (DateTime)pHash["dateStart"],
            //                                                    (DateTime)pHash["dateEnd"],
            //                                                    pHash["stars"] as int[],
            //                                                    pHash["pansions"] as int[],
            //                                                    pHash["rooms"] as RequestRoom[],
            //                                                    SearchId, Convert.ToInt64(7200000000)
            //            ));

            //    if (ConfigurationManager.AppSettings["SearchHotelsInOkToGo"] == "true")
            //        searchers.Add(OktogoService.id_prefix, new OktogoService((int)pHash["cityId"],
            //                                                    (DateTime)pHash["dateStart"],
            //                                                    (DateTime)pHash["dateEnd"],
            //                                                    pHash["stars"] as int[],
            //                                                    pHash["pansions"] as int[],
            //                                                    pHash["rooms"] as RequestRoom[],
            //                                                    SearchId, Convert.ToInt64(7200000000)
            //            ));

            //    Dictionary<string, List<BookRoom>> operatorRooms = new Dictionary<string, List<BookRoom>>();
            //    Dictionary<string, List<List<int>>> operatorTurists = new Dictionary<string, List<List<int>>>();

            //    //разбиваем туристов и комнаты на группы по поставщику
            //    for (int i = 0; i < bookRooms.Length; i++)
            //    {
            //        string prefix = bookRooms[i].VariantId.Split('_')[0] + "_";

            //        if (!operatorRooms.ContainsKey(prefix))
            //        {
            //            operatorRooms.Add(prefix, new List<BookRoom>());
            //            operatorTurists.Add(prefix, new List<List<int>>());
            //        }

            //        operatorRooms[prefix].Add(bookRooms[i]);
            //        operatorTurists[prefix].Add(roomTurists[i]);
            //    }

            //    List<HotelBooking> operatorBooking = new List<HotelBooking>();


            //    //бронируем
            //    foreach (string operatorPrefix in operatorRooms.Keys)
            //    {
            //        //бронирование
            //        operatorBooking.AddRange(searchers[operatorPrefix].BookRooms(SearchId, HotelId, operatorRooms[operatorPrefix], operatorTurists[operatorPrefix]));
            //    }

            //    //посчитали сумму
            //    Dictionary<string, decimal> totalPrice = new Dictionary<string, decimal>();
            //    foreach (HotelBooking htb in operatorBooking)
            //    {
            //        foreach (KeyValuePair<string, decimal> pricePart in htb.Prices)
            //        {
            //            if (!totalPrice.ContainsKey(pricePart.Key))
            //                totalPrice.Add(pricePart.Key, 0M);

            //            totalPrice[pricePart.Key] += pricePart.Value;
            //        }
            //    }

            //     MtHelper.SaveHotelBookingToCache(operatorBooking);

            //     return operatorBooking.ToArray();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message + " " + ex.StackTrace);
            //}
            return null;
        }
    }
}
