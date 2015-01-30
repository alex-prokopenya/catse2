using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.OktogoService;

using System.Data.Sql;
using System.Data.SqlClient;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.MasterTour;
using System.Configuration;

namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public class OktogoService : IHotelExt
    {
        public static readonly string id_prefix = "ok_";
        private static string oktogo_service_user = "Oktogo";
        private static string oktogo_service_pass = "Oktogo";
        private static string oktogo_affiliateId = "click.travel@oktogo.ru";
        private static string oktogo_password = "123";

        private bool added = false;
        private bool isFinished = false;

        private int cityId = 0;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MinValue;
        private int[] stars = new int[0];
        private int[] pansions = new int[0];
        private RequestRoom[] rooms = new RequestRoom[0];
        private string searchId = "";

        private long HOTELS_RESULTS_LIFETIME = 0;

        private int OktogoHotelIdShift = 20000000;

        private Hotel[] FindedHotels = null;
        private Containers.Hotels.Room[] FindedRooms = null;

        private Dictionary<string, string> pansionsNames = new Dictionary<string, string>();
        private Dictionary<string, int> pansionsGroups = new Dictionary<string, int>();

        //получаем курсы валют
        KeyValuePair<string, decimal>[] _courses = null;

        private void InitPansions()
        { 
            //  RO	    Room only
            //  AI	    All inclusive
            //  BB	    Bed and breakfast
            //  HB	    Halfboard
            //  FB	    Fullboard
            //  DI	    Deluxe
            //  SC	    Self catering
            //  RB	    Room and breakfast
            //  CB	    Continental breakfast
            //  HBP 	Halfboard plus
            //  UAI	    Ultra all inclusive
            //  FBP	    Fullboard plus
            //  Lunch	Lunch
            //  Dinner	Dinner
            //  BR	    Brunch
            //  AC	    A la carte

            pansionsNames.Add("RO", "Без питания");
            pansionsNames.Add("AI", "Все включено");
            pansionsNames.Add("BB", "Завтрак");
            pansionsNames.Add("HB", "Полупансион");
            pansionsNames.Add("FB", "Пансион");
            pansionsNames.Add("DI", "Делюкс");
            pansionsNames.Add("SC", "Самообслуживание");
            pansionsNames.Add("HBP", "Полупансион+");
            pansionsNames.Add("UAI", "Ультра все включено");
            pansionsNames.Add("FBP", "Пансион+");
            pansionsNames.Add("Lunch", "Обед");
            pansionsNames.Add("Dinner", "Ужин");
            pansionsNames.Add("BR", "Бранч");
            pansionsNames.Add("AC", "A-la carte");

            pansionsGroups.Add("RO", 0);
            pansionsGroups.Add("SC", 0);
            pansionsGroups.Add("AC", 0);

            pansionsGroups.Add("BB", 1);
            pansionsGroups.Add("RB", 1);
            pansionsGroups.Add("CB", 1);
            pansionsGroups.Add("Lunch",  1);
            pansionsGroups.Add("Dinner", 1);

            pansionsGroups.Add("HB", 2);
            pansionsGroups.Add("HBP", 2);

            pansionsGroups.Add("FB", 3);
            pansionsGroups.Add("FBP", 3);
            pansionsGroups.Add("BR", 3);

            pansionsGroups.Add("AI", 4);
            pansionsGroups.Add("DI", 4);
            pansionsGroups.Add("UAI", 4);
        }


        public OktogoService(int CityId, DateTime StartDate, DateTime EndDate, int[] Stars, int[] Pansions, RequestRoom[] Rooms, string SearchId, long RESULTS_LIFETIME)
        {
            this.stars = Stars;
            if (stars.Length == 0)
                this.stars = new int[] { 1, 2, 3, 4, 5 };

            //типы питания не поддерживаются системой oktogo
            this.pansions = Pansions;
            this.cityId = GetCityId(CityId); //получаем айдишник города в oktogo
            this.startDate = StartDate;
            this.endDate = EndDate;
            this.rooms = Rooms;
            this.searchId = SearchId;

            this.HOTELS_RESULTS_LIFETIME = RESULTS_LIFETIME;

            _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);
        }

        private static int GetCityId(int CityId)
        {
            #if DEBUG
                return CityId;
            #endif

            int okCityId = CityId;

            SqlConnection myCon = new SqlConnection(ConfigurationManager.AppSettings["MasterTourConnectionString"]);

            myCon.Open();

            SqlCommand myCom = new SqlCommand("select ok_ctkey from ok_cities where cl_ctkey = " + CityId, myCon);

            okCityId = Convert.ToInt32(myCom.ExecuteScalar());

            myCon.Close();

            return okCityId;
        }

        public void FindHotels()
        {
            //создаем апи-клиента
            var oktogoClient = new TravelApiServiceSoapClient("TravelApiServiceSoap");
            oktogoClient.ClientCredentials.UserName.UserName = oktogo_service_user;
            oktogoClient.ClientCredentials.UserName.Password = oktogo_service_pass;

            //формируем запрос
            var hotelRequest = new HotelRequest();
            hotelRequest.AffiliateId = oktogo_affiliateId; //логин
            hotelRequest.Password = oktogo_password;       //пароль

            hotelRequest.HotelRequestMethod = HotelRequestMethod.GetAvailabilityByDestinations; //тип запроса
            hotelRequest.HotelSearchParameters = new HotelSearchParameters()
            {
                DestinationId = this.cityId,
                CheckInDate = this.startDate,
                CheckOutDate = this.endDate,
                MaxStarRating = this.stars.Max(), //максимальная звездность
                MinStarRating = this.stars.Min(), //минимальная звездность
                Currency = Currency.RUB,
                Rooms = PrepareRoomsForRequest()
            };

            var response = oktogoClient.xmlRequest(hotelRequest);

            //парсим ответ, сохраняем результаты в редис
            this.FindedHotels = ComposeHotelsResults(response);
        }

        private Hotel[] ComposeHotelsResults(HotelResponse resp)
        {
            var result = new List<Hotel>();

            //идем по списку результатов
            foreach(var hotelRS  in resp.Products)
            {
                //парсим отели по одному
                var hotel = ComposeHotel(hotelRS, "av");

                //если удалось, добавляем в результат
                if (hotel != null)
                    result.Add(hotel);
            }

            return result.ToArray();
        }

        private Hotel ComposeHotel(HotelRS hotelRS, string availCode)
        {
            try
            {
                //создаем новый объект отеля 
                Hotel curHotel = new Hotel()
                {
                    HotelId = hotelRS.HotelId, // пишем айдишник
                };

                var rooms = new List<Containers.Hotels.Room>();
                
                //идем по комнатам, которые были указаны в запросе
                foreach(var reqRoom in this.rooms)
                {
                    Array.Sort(reqRoom.ChildrenAges);

                    //чтобы проверить к какой из запрошенных комнат относится комната из резульатов
                    //строка для сопоставления комнат
                    var checkStr = "ad=" + reqRoom.Adults + "ch=" + reqRoom.Children + "("+ string.Join(",",reqRoom.ChildrenAges)+")";

                    var tempRoom = new Containers.Hotels.Room()
                            {
                                Adults   = reqRoom.Adults,
                                Children = reqRoom.Children,
                                ChildrenAges = reqRoom.ChildrenAges
                            };

                    //варианты цен в комнате
                    var tmpVariants = new List<RoomVariant>();

                    foreach(var roomRS in hotelRS.Rooms)
                    {
                        var tmpChAges = roomRS.GuestAges.Where(a=> a < 18).ToArray();
                            Array.Sort(tmpChAges);

                        //формируем такую же строку из комнаты результатов
                        var chString = "ad=" + roomRS.GuestAges.Where(a=> a>= 18).Count();
                        
                        chString += "ch=" + tmpChAges.Length + "(" + string.Join(",", tmpChAges)+")";

                        //если они равны, добавляем вариант к команте
                        if(chString == checkStr)
                        {
                            foreach(var rateRS in roomRS.Rates)
                            {
                                
                                RoomVariant roomVariant = new RoomVariant()
                                {
                                    PansionGroupId = pansionsGroups[rateRS.MealType.ToString()],
                                    PansionTitle = pansionsNames[rateRS.MealType.ToString()],
                                    RoomCategory = roomRS.RoomType.ToString(),
                                    RoomTitle = rateRS.RoomName,
                                    RoomInfo = rateRS.RoomSubType,
                                    Prices = MtHelper.ApplyCourses(rateRS.TotalPrice, _courses),
                                    VariantId = "ok_" + checkStr +"_"+ rateRS.RateId + "_" + availCode + "_" + rateRS.TotalPrice //кодируем информацию о варианте, важно для бронирования варианта со страницы результатов
                                };
                                tmpVariants.Add(roomVariant);
                            }

                            if (availCode == "av") break;
                        }
                    }

                    tempRoom.Variants = tmpVariants.ToArray();
                    rooms.Add(tempRoom);
                }
                curHotel.Rooms = rooms.ToArray();

                return curHotel;
            }
            catch (Exception ex)
            {
                #if DEBUG
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                #endif
            }

            return null;
        }

        public void FindRooms(object hotelId)
        {
            //поиск комнат по отелю в согласии с параметрами
            int hotelKey = Convert.ToInt32(hotelId);

            //создаем апи-клиента
            var oktogoClient = new TravelApiServiceSoapClient("TravelApiServiceSoap");
            oktogoClient.ClientCredentials.UserName.UserName = oktogo_service_user;
            oktogoClient.ClientCredentials.UserName.Password = oktogo_service_pass;

            //формируем запрос
            var hotelRequest = new HotelRequest();
            hotelRequest.AffiliateId = oktogo_affiliateId; //логин
            hotelRequest.Password = oktogo_password;       //пароль

            hotelRequest.HotelRequestMethod = HotelRequestMethod.GetAvailabilityByHotels; //тип запроса

            hotelRequest.HotelSearchParameters = new HotelSearchParameters()
            {
                HotelId = hotelKey - OktogoHotelIdShift,
                CheckInDate = this.startDate,
                CheckOutDate = this.endDate,
                MaxStarRating = this.stars.Max(), //максимальная звездность
                MinStarRating = this.stars.Min(), //минимальная звездность
                Currency = Currency.RUB,
                Rooms = PrepareRoomsForRequest()
            };

            var response = oktogoClient.xmlRequest(hotelRequest);

            //парсим результаты
            this.FindedRooms = ComposeHotel(response.Products[0], response.AvailabilityCode).Rooms;
        }

        public Hotel[] GetHotels()
        {
            return this.FindedHotels;
        }

        public Containers.Hotels.Room[] GetRooms()
        {
            return this.FindedRooms;
        }

        public HotelPenalties GetHotelPenalties(int hotelId, string variantId)
        {
            //штрафы по отелю


            return null;
        }

        public HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId)
        {
            //проверить наличие номера и финальную цену
            string[] parts = variantId.Split('_');
            //availiable code
            string avCode = parts[parts.Length - 2];
            //стоимость
            decimal price = Convert.ToDecimal(parts.Last());
            //строка для проверки варианта
            string chString = "ok_" + parts[1] + "_";
            // идентификатор для уточнения цены
            var okRateId = parts[2]; 


            //если вариант добавлялся с главной страницы, нужно сделать поиск по отелю и найти соответствующу комнату
            if (avCode == "av")
            {
                //заменить avCode
                avCode = FindActualRateId(hotelId, avCode, price, chString, ref variantId);
                //заменить rateId

            }


            return null;
        }

        private string FindActualRateId(int hotelId, string avCode, decimal price, string chString, ref string variantId)
        {
            //проверить, есть ли в редисе
            var redisVariantId = RedisHelper.GetString("");

            //сделать поиск
            FindRooms(hotelId);

            var newVariantId = "";
            //проходимся по комнатам
            foreach (var room in this.FindedRooms)
            {
                //проходимся по вариантам
                foreach (var roomVar in room.Variants)
                {
                    //выдергиваем строку для проверки соответствиякомнат
                    var tmpCheckStr = "ok_" + roomVar.VariantId.Split('_')[1] + "_";

                    //если комната не та, уходим
                    if (tmpCheckStr != chString) break;

                    //сверяем цену
                    if (price == Convert.ToDecimal(roomVar.Price["RUB"]))
                    {
                        string[] tmpParts = roomVar.VariantId.Split('_');

                        avCode = tmpParts[tmpParts.Length - 2];
                        newVariantId = roomVar.VariantId;

                        break;
                    }
                }

                if (avCode != "av")
                {
                    //положить в редис
                    RedisHelper.SetString(variantId + "_true_code", newVariantId, new TimeSpan(HOTELS_RESULTS_LIFETIME * 10));
                    variantId = newVariantId;

                    return avCode;
                }
            }

            return avCode;
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
            return this.FindedHotels != null;
        }

        private RoomInfo[] PrepareRoomsForRequest()
        {
            var result = new List<RoomInfo>();

            foreach (var room in this.rooms)
            {
                //готовим список туристов
                var guests = new List<Guest>();

                int ads = room.Adults;
                //добавляем взрослых
                while (ads-- > 0)
                    guests.Add(new Guest() { Age = 20, IsChild = false });

                //добавляем детей
                foreach (int age in room.ChildrenAges)
                    guests.Add(new Guest() { Age = age, IsChild = true });
            }

            return result.ToArray();
        }
    }
}