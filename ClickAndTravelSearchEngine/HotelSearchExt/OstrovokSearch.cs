using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.MasterTour;
using ClickAndTravelSearchEngine.OktogoService;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Responses;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public class OstrovokSearch : IHotelExt
    {
        public static readonly string id_prefix = "os_";

        private static string OS_URL = ConfigurationManager.AppSettings["OstrovokServiceUrl"];
        private static string OS_APIKEY = ConfigurationManager.AppSettings["OstrovokApiKey"];
        private static string OS_AUTH = ConfigurationManager.AppSettings["OstrovokAuth"];
        private static decimal OSTROVOK_COEF = Convert.ToDecimal(ConfigurationManager.AppSettings["OstrovokMargin"]);

        private bool added = false;
        private int cityId = 0;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MinValue;
        private int[] stars = new int[0];
        private int[] pansions = new int[0];
        private RequestRoom room = null;
        private string searchId = "";

        private long HOTELS_RESULTS_LIFETIME = 0;

        private static readonly int OstrovokHotelIdShift = 30000000;

        private Hotel[] FoundHotels = null;
        private Containers.Hotels.Room FoundRoom = null;

        private Dictionary<string, int> hotelsDictionary = new Dictionary<string, int>();

        //получаем курсы валют
        KeyValuePair<string, decimal>[] _courses = null;

        public static void ApplyConfig(NameValueCollection settings, string partnerCode = "")
        {
            OS_URL = settings["OstrovokServiceUrl"];
            OS_APIKEY = settings["OstrovokApiKey"];
            OS_AUTH = settings["OstrovokAuth"];
            OSTROVOK_COEF = Convert.ToDecimal(settings["OstrovokMargin"]);

            Logger.WriteToLog(partnerCode+ " config applied to ostrovok");
        }

        public OstrovokSearch(int CityId, DateTime StartDate, DateTime EndDate, int[] Stars, int[] Pansions, RequestRoom Room, string SearchId, long RESULTS_LIFETIME)
        {
            this.stars = Stars;
            if (stars.Length == 0)
                this.stars = new int[] { 1, 2, 3, 4, 5 };

            //типы питания не поддерживаются системой ostrovok
            this.pansions = Pansions;

            this.cityId = GetCityId(CityId); //получаем айдишник города в oktogo

            this.startDate = StartDate;
            this.endDate = EndDate;
            this.room = Room;
            this.searchId = SearchId;

            this.HOTELS_RESULTS_LIFETIME = RESULTS_LIFETIME;

            _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);
        }

        private static int GetCityId(int CityId)
        {
            int cityIdShift = 1000000000;

            if (CityId > cityIdShift)
                return CityId - cityIdShift;

            int osCityId = CityId;

            var selectQuery = "select ostrovok_key from cities where id = " + CityId;

            //коннектимся к базе и выполняем запрос
            var cs = ConfigurationManager.AppSettings["MySqlConnectionString"];
            var connection = new MySqlConnection(cs);

            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();

            var row = command.ExecuteScalar();

            connection.Close();

            if (row == null)
                return CityId;
            else
                return Convert.ToInt32(row) - cityIdShift;
        }

        private int GetHotelId(string hotelSlug)
        {
            if (this.hotelsDictionary.ContainsKey(hotelSlug))
                return this.hotelsDictionary[hotelSlug];

            return 0; //по слагу найти описание отеля
        }

        private JsonObject GenerateRequestData(int cityId, int ads, int[] childAges,
                                                    string checkin, string checkout, int page)
        {
            JsonObject jObj = new JsonObject();

            if (cityId > 0)
                jObj["region_id"] = cityId;

            jObj["checkout"] = checkout;
            jObj["checkin"] = checkin;
            jObj["adults"] = ads;
            jObj["children"] = childAges;
            jObj["format"] = "json";
            jObj["lang"] = "ru";
            jObj["page"] = page;
            jObj["include"] = new string[] { "room_description", "value_adds", "rate_price", "room_name", "availability_hash", "cancellation_info" };

            return jObj;
        }

        //найти результаты первых страниц
        private Hotel[] GetHotelsByRoom(int cityId, int ads, int[] childAges, string checkin, string checkout)
        {
            try
            {
                //создаем анонимный объект для поискового запроса
                var dataForUrl = GenerateRequestData(cityId, ads, childAges, checkin, checkout, 1);

                var url = OS_URL + "/api/b2b/v2/hotel/rates?data=" + dataForUrl;

#if DEBUG
                var tmp = Guid.NewGuid();
                Logger.WriteToOstrovokLog("id " + tmp.ToString() + "request:" + url);
#endif

                var firstPageResponse = CreateOstrovokClient().DownloadString(url);//делаем запрос первой страницы

#if DEBUG
                Logger.WriteToOstrovokLog("id " + tmp.ToString() + "response_length:" + firstPageResponse.Length);
#endif

                JsonObject jObj = JsonConvert.Import<JsonObject>(firstPageResponse);

                //добавляем результаты в общий список
                List<Hotel> hotels = new List<Hotel>();

                hotels.AddRange(ComposeHotelsResults(jObj["result"] as JsonObject, ads, childAges));

                //сохраняем временный результат
                this.FoundHotels = hotels.ToArray();

                //кладем в редис информацию по аннуляциям и verify
                var redisCache = new JsonObject();
                var cancelationCache = new JsonObject();

                foreach (Hotel hotel in hotels)
                {
                    redisCache["hotel" + hotel.HotelId + "_" + hotel.Rooms[0].Variants[0].VariantId] = hotel.Rooms[0].Variants[0].Price;

                    cancelationCache["hotel" + hotel.HotelId + "_" + hotel.Rooms[0].Variants[0].VariantId] = hotel.Rooms[0].Variants[0].Penalties;
                }

                RedisHelper.SetString("hotel_verify_cache_" + searchId, JsonConvert.ExportToString(redisCache), new TimeSpan(0, 30, 0));
                RedisHelper.SetString("hotel_penalty_cache_" + searchId, JsonConvert.ExportToString(cancelationCache), new TimeSpan(0, 30, 0));

                //получаем количество страниц
                int pagesCount = Convert.ToInt32((jObj["result"] as JsonObject)["total_pages"]);

                int currentPage = 2;

                if (pagesCount > 20) pagesCount = 20;

                var nohotelsCount = 0;
                //синхронный поиск по страницам
                while (true)
                {
                    if (currentPage > pagesCount) break;

                    dataForUrl = GenerateRequestData(cityId, ads, childAges, checkin, checkout, currentPage++);

                    var itemUrl = OS_URL + "/api/b2b/v2/hotel/rates?data=" + dataForUrl;

#if DEBUG
                    tmp = Guid.NewGuid();
                    Logger.WriteToOstrovokLog("id " + tmp.ToString() + "request:" + itemUrl);
#endif

                    var response = CreateOstrovokClient().DownloadString(itemUrl);

#if DEBUG
                    Logger.WriteToOstrovokLog("id " + tmp.ToString() + "response_length:" + response.Length);

                    if (response.Length < 1000)
                        Logger.WriteToOstrovokLog("response: " + response);
#endif

                    JsonObject jsonRes = JsonConvert.Import<JsonObject>(response);

                    var hotelsTemp = ComposeHotelsResults(jsonRes["result"] as JsonObject, ads, childAges);

                    if (hotelsTemp.Length > 0)
                    {
                        nohotelsCount = 0;
#if DEBUG
                        Logger.WriteToOstrovokLog("id " + tmp.ToString() + "hotels.count " + hotelsTemp.Length);
#endif
                        hotels.AddRange(hotelsTemp);
                        this.FoundHotels = hotels.ToArray();
                        continue;
                    }
                    else if (nohotelsCount++ > 2)
                        break;
                }

                return hotels.ToArray();
            }
            catch (Exception ex)
            {
                Logger.WriteToOstrovokLog(ex.Message + " " + ex.StackTrace);
            }

            return null;
        }

        private Containers.Hotels.Room FindRoomVariants(int cityId, int ads, int[] childAges, string checkin, string checkout, string slug)
        {
            //готовим строку запроса
            var dataForUrl = GenerateRequestData(0, ads, childAges, checkin, checkout, 1);

            dataForUrl["ids"] = new string[] { slug };

            //готовим урл
            var url = OS_URL + "/api/b2b/v2/hotel/rates?data=" + dataForUrl;

#if DEBUG
            var tmp = Guid.NewGuid();
            Logger.WriteToOstrovokLog("id " + tmp.ToString() + "request:" + url);
#endif

            //получаем json с ответом
            var priceResponse = CreateOstrovokClient().DownloadString(url);

#if DEBUG
            Logger.WriteToOstrovokLog("id " + tmp.ToString() + "response_length:" + priceResponse);
#endif

            JsonObject jObj = JsonConvert.Import<JsonObject>(priceResponse);
            var jHotels = (jObj["result"] as JsonObject)["hotels"] as JsonArray;

            //формируем ответ
            Containers.Hotels.Room room = new Containers.Hotels.Room()
            {
                Adults = ads,
                ChildrenAges = childAges,
                Children = childAges.Length
            };

            var variants = new List<RoomVariant>();

            //получаем список вариантов комнаты
            foreach (JsonObject roomRS in (jHotels[0] as JsonObject)["rates"] as JsonArray)
            {
                var mealType = GetMealType(roomRS as JsonObject);

                RoomVariant roomVariant = new RoomVariant()
                {
                    //питание выбрать из опция
                    PansionGroupId = mealType == "RO" ? 0 : 1,

                    PansionTitle = mealType,

                    RoomCategory = "",

                    //имя комнаты
                    RoomTitle = roomRS["room_name"].ToString(),

                    Penalties = roomRS["cancellation_info"] as JsonObject,

                    //информация о номере
                    RoomInfo = GetRoomInfo(roomRS),
                    Prices = MtHelper.ApplyCourses(Convert.ToDecimal(roomRS["rate_price"].ToString().Replace(".", ",")) * OSTROVOK_COEF, _courses),
                    VariantId = id_prefix + roomRS["availability_hash"] //кодируем информацию о варианте, важно для бронирования варианта со страницы результатов
                };

                variants.Add(roomVariant);
            }
            room.Variants = variants.ToArray();

            return room;
        }

        private static WebClient CreateOstrovokClient()
        {
            WebClient wcl = new WebClient();
            wcl.Credentials = new NetworkCredential(OS_APIKEY, OS_AUTH);
            return wcl;
        }

        private string GetHotelSlug(int id)
        {
            try
            {
                //запрос на выбор слага
                var selectQuery = "select slug from hotels where id = " + id;

                //коннектимся к базе и выполняем запрос
                var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
                var command = new MySqlCommand(selectQuery, connection);
                connection.Open();

                var row = command.ExecuteScalar();

                var res = (row == null) ? "" : row.ToString();

                //закрываем соединение
                connection.Close();

                return res;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("ostrovok GetHotelSlug ex = " + ex.Message + " " + ex.StackTrace);
            }

            return "";
        }

        private void LoadAdditionalHotels(List<string> hotelIds)
        {
            try
            {
                var selectQuery = "select slug, id from hotels where slug in ('" + string.Join("','", hotelIds) + "')";

                //коннектимся к базе и выполняем запрос
                var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);

                var command = new MySqlCommand(selectQuery, connection);
                connection.Open();
                

                MySqlDataReader reader = command.ExecuteReader();
                int cnt = 0;
                while (reader.Read())
                    try
                    {
                        this.hotelsDictionary.Add(reader["slug"].ToString(), Convert.ToInt32(reader["id"]));
                        cnt++;
                    }
                    catch (Exception)
                    {
                        Logger.WriteToLog(reader["slug"].ToString() + Convert.ToInt32(reader["id"]));
                    }

                reader.Close();


                connection.Close();

                Logger.WriteToLog("additionally loaded "+ cnt);
            }
            catch (Exception ex)
            {
                Logger.WriteToOstrovokLog(ex.Message + " " + ex.StackTrace);
            }
        }

        private void FillHotelsDict(int id)
        {
            try
            {
                this.hotelsDictionary.Clear();

                var selectQuery = "select slug, id from hotels where id > " + OstrovokHotelIdShift + " and origin_id = " + id;
                
                //коннектимся к базе и выполняем запрос
                var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);

                var command = new MySqlCommand(selectQuery, connection);
                connection.Open();

                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    try
                    {
                        this.hotelsDictionary.Add(reader["slug"].ToString(), Convert.ToInt32(reader["id"]));
                    }
                    catch (Exception )
                    {
                        Logger.WriteToLog(reader["slug"].ToString() + Convert.ToInt32(reader["id"]));
                    }

                reader.Close();

                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteToOstrovokLog(ex.Message + " " + ex.StackTrace);
            }
        }

        public void FindHotels()
        {
            if (this.cityId == 0)
            {
                this.FoundHotels = new Hotel[0];
                this.isFinished = true;
                this.added = true;
                return;
            }

            try
            {
                //выгружаем справочник отелей по городу
                var fillHotelsLibTask = Task.Factory.StartNew(() => FillHotelsDict(this.cityId));

                //получить ищем отели по комнате
                //первые резульататы ищутся синхронно
                var hotels = GetHotelsByRoom(this.cityId, room.Adults, room.ChildrenAges,
                                               startDate.ToString("yyyy-MM-dd"),
                                               endDate.ToString("yyyy-MM-dd"));
                
                fillHotelsLibTask.Wait();

                //парсим ответ, сохраняем результаты в редис
                this.FoundHotels = MergeRoomsResults(hotels);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace );

                this.FoundHotels = new Hotel[0];
                this.added = true;
            }

            this.isFinished = true;
        }

        //объединяем результаты по разным комнатам в один массив отелей
        private Hotel[] MergeRoomsResults(Hotel[] roomHotels)
        {
            var redisCache = new JsonObject();

            var cancelationCache = new JsonObject();

            //если искали только одну комнату, объединять нечего
            if(roomHotels.Length ==0 || roomHotels[0] == null) return new Hotel[0];
                
            foreach (Hotel hotel in roomHotels)
            {
               redisCache["hotel" + hotel.HotelId + "_" + hotel.Rooms[0].Variants[0].VariantId] = hotel.Rooms[0].Variants[0].Price;

               cancelationCache["hotel" + hotel.HotelId + "_" + hotel.Rooms[0].Variants[0].VariantId] = hotel.Rooms[0].Variants[0].Penalties;
            }

            RedisHelper.SetString("hotel_verify_cache_" + searchId, JsonConvert.ExportToString( redisCache ), new TimeSpan(0, 30, 0));

            var penaltiesString = JsonConvert.ExportToString(cancelationCache);

            RedisHelper.SetString("hotel_penalty_cache_" + searchId, penaltiesString, new TimeSpan(0, 30, 0));

            return roomHotels;
        }

        private Hotel[] ComposeHotelsResults(JsonObject resp, int ads, int[] childAges)
        {
            var result = new List<Hotel>();

            if (resp["hotels"] != null)
            {
                Logger.WriteToInOutLog("found " + (resp["hotels"] as JsonArray).Count + " in ostrovok");
                //идем по списку результатов

                var notFoundIds = new List<string>();

                foreach (JsonObject hotelRS in resp["hotels"] as JsonArray)
                {
                    int hotelId = GetHotelId(hotelRS["id"].ToString() + "_ostrvk");

                    if (hotelId == 0)
                        notFoundIds.Add(hotelRS["id"].ToString() + "_ostrvk");
                }

                if (notFoundIds.Count > 0)
                    LoadAdditionalHotels(notFoundIds);

                foreach (JsonObject hotelRS in resp["hotels"] as JsonArray)
                {
                    //парсим отели по одному
                    var hotel = ComposeHotel(hotelRS, ads, childAges);

                    //если удалось, добавляем в результат
                    if (hotel != null)
                        result.Add(hotel);
                }
            }

            Console.WriteLine("composed: " + result.Count);

            return result.ToArray();
        }

        private string GetMealType(JsonObject room)
        {
            foreach (JsonObject amenity in room["value_adds"] as JsonArray)
            {
                if (amenity["code"].ToString() == "has_meal")
                    return amenity["description"].ToString();
            }

            return "RO";
        }

        private string GetRoomInfo(JsonObject room)
        {
            return room["room_description"].ToString();
        }

        //конвертируем json объект в топ предложение по отелю
        private Hotel ComposeHotel(JsonObject hotel, int ads, int[] childAges)
        {
            try
            {
                //создаем новый объект отеля 
                Hotel curHotel = new Hotel()
                {
                    HotelId = GetHotelId(hotel["id"].ToString()+"_ostrvk") // пишем айдишник
                };

                if (curHotel.HotelId == 0)
                {
                    Logger.WriteToLog("not found hotel key for " + hotel["id"].ToString());
                    return null;
                }

                var tempRoom = new Containers.Hotels.Room()
                {
                    Adults = ads,
                    Children = childAges.Length,
                    ChildrenAges = childAges
                };

                var roomRS = ((hotel["rates"] as JsonArray)[0]) as JsonObject;

                var mealType = GetMealType(roomRS as JsonObject);

                RoomVariant roomVariant = new RoomVariant()
                {
                    //питание выбрать из опция
                    PansionGroupId = mealType == "RO" ? 0 :1,

                    PansionTitle = mealType,

                    RoomCategory = "", //roomRS.RoomType.ToString(),

                    //имя комнаты
                    RoomTitle = roomRS["room_name"].ToString(),

                    Penalties = roomRS["cancellation_info"] as JsonObject,
                    //информация о номере
                    RoomInfo = GetRoomInfo(roomRS),
                    Prices = MtHelper.ApplyCourses(Convert.ToDecimal(roomRS["rate_price"].ToString().Replace(".",","))* OSTROVOK_COEF, _courses),
                    VariantId = id_prefix + roomRS["availability_hash"] //кодируем информацию о варианте, важно для бронирования варианта со страницы результатов
                };

                tempRoom.Variants = new RoomVariant[] { roomVariant };

                curHotel.Rooms = new Containers.Hotels.Room[] { tempRoom };

                return curHotel;
            }
            catch (Exception ex)
            {
                Logger.WriteToOstrovokLog(ex.Message + " " + ex.StackTrace);
            }

            return null;
        }

        public void FindRooms(object hotelId)
        {
            try
            {
                //поиск комнат по отелю в согласии с параметрами
                int hotelKey = Convert.ToInt32(hotelId);

                string hotelSlug = GetHotelSlug(hotelKey).Replace("_ostrvk", "");

                if (hotelSlug == "")
                {
                    this.FoundRoom = new Containers.Hotels.Room() { Variants = new RoomVariant[0]};
                    this.isFinished = true;

                    Logger.WriteToLog("exit from ostrovok find");
                    return;
                }

                //формируем запрос
                var roomRes = FindRoomVariants(cityId, room.Adults, room.ChildrenAges,
                                                       startDate.ToString("yyyy-MM-dd"),
                                                       endDate.ToString("yyyy-MM-dd"), hotelSlug);

                var roomVariants = new List<Containers.Hotels.Room>();

                JsonObject redisCache = new JsonObject();
                JsonObject cancelationCache = new JsonObject();

                foreach (RoomVariant variant in roomRes.Variants)
                {
                    redisCache["hotel" + hotelKey + "_" + variant.VariantId] = variant.Price;

                    cancelationCache["hotel" + hotelKey + "_" + variant.VariantId] = variant.Penalties;
                }

                RedisHelper.SetString("hotel_verify_cache_" + hotelKey + "_" + searchId, JsonConvert.ExportToString(redisCache), new TimeSpan(0, 60, 0));
                RedisHelper.SetString("hotel_penalty_cache_" + hotelKey + "_" + searchId, JsonConvert.ExportToString(cancelationCache), new TimeSpan(0, 60, 0));

                //сохраняем результаты результаты
                this.FoundRoom = roomRes;
            }
            catch (Exception ex)
            {
                Logger.WriteToOstrovokLog("ostrovok find rooms ex =" + ex.Message + " " + ex.StackTrace);

                //сохраняем пустые результаты
                this.FoundRoom = new Containers.Hotels.Room() { Variants = new RoomVariant[0] }; ;
            }

            this.isFinished = true;
        }

        private static HotelRequest CreateHotelRequest()
        {
            var hotelRequest = new HotelRequest();

            return hotelRequest;
        }

        public Hotel[] GetHotels()
        {
            return this.FoundHotels;
        }

        public Containers.Hotels.Room GetRoom()
        {
            return this.FoundRoom;
        }

        public HotelPenalties GetHotelPenalties(int hotelId, string variantId)
        {
            //пытаемся найти информацию в редисе, если нет, делаем повторный поиск
            //проверяем кэш по городу
            var cityCache = RedisHelper.GetString("hotel_penalty_cache_" + searchId);

            if (!string.IsNullOrEmpty(cityCache))
            {
                JsonObject cacheObject = JsonConvert.Import<JsonObject>(cityCache);
                if (cacheObject.Contains("hotel" + hotelId + "_" + variantId))
                {
                    var cancelPen = new List<KeyValuePair<DateTime, string>>();

                    var penaltiesArray = (cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject)["policies"] as JsonArray;
                    Logger.WriteToLog(penaltiesArray.ToString());
                    foreach (JsonObject policy in penaltiesArray)
                    {
                        if ((policy["penalty"] as JsonObject)["percent"] == null)
                        {
                            decimal course = Convert.ToDecimal((policy["penalty"] as JsonObject)["currency_rate_to_rub"].ToString().Replace(".", ","));
                            decimal amount = Convert.ToDecimal((policy["penalty"] as JsonObject)["amount"].ToString().Replace(".", ","));

                            amount = Math.Round(amount * course);

                            cancelPen.Add(new KeyValuePair<DateTime, string>(policy["start_at"] != null ? Convert.ToDateTime(policy["start_at"]) : DateTime.Today, amount + " RUB"));
                        }
                        else
                            cancelPen.Add(new KeyValuePair<DateTime, string>(policy["start_at"] != null ? Convert.ToDateTime(policy["start_at"]) : DateTime.Today, (policy["penalty"] as JsonObject)["percent"] + "%"));
                    }

                    if (cancelPen.Count == 0)
                        if (cacheObject["hotel" + hotelId + "_" + variantId] != null)
                        {
                            var jObj = cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject;

                            if(jObj["free_cancellation_before"] != null)
                                cancelPen.Add(new KeyValuePair<DateTime, string>(Convert.ToDateTime((cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject)["free_cancellation_before"]), "100 %"));
                        }

                    return new HotelPenalties()
                    {
                        CancelingPenalties = cancelPen.ToArray(),
                        ChangingPenalties = new KeyValuePair<DateTime, string>[0]
                    };
                }
            }
            //проверяем кэш по отелю
            var hotelCache = RedisHelper.GetString("hotel_penalty_cache_" + hotelId + "_" + searchId);
            //штрафы по отелю

            if (!string.IsNullOrEmpty(hotelCache))
            {
                JsonObject cacheObject = JsonConvert.Import<JsonObject>(hotelCache);

                if (cacheObject.Contains("hotel" + hotelId + "_" + variantId))
                {
                    var cancelPen = new List<KeyValuePair<DateTime, string>>();

                    var penaltiesArray = (cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject)["policies"] as JsonArray;

                    Logger.WriteToLog(penaltiesArray.ToString());

                    foreach (JsonObject policy in penaltiesArray)
                    {
                        if ((policy["penalty"] as JsonObject)["percent"] == null)
                        {
                            decimal course = Convert.ToDecimal((policy["penalty"] as JsonObject)["currency_rate_to_rub"].ToString().Replace(".",","));
                            decimal amount = Convert.ToDecimal((policy["penalty"] as JsonObject)["amount"].ToString().Replace(".", ","));

                            amount = Math.Round(amount * course);

                            cancelPen.Add(new KeyValuePair<DateTime, string>(policy["start_at"] != null ? Convert.ToDateTime(policy["start_at"]) : DateTime.Today, amount + " RUB"));

                        }
                        else
                            cancelPen.Add(new KeyValuePair<DateTime, string>(policy["start_at"] != null ? Convert.ToDateTime(policy["start_at"]) : DateTime.Today, (policy["penalty"] as JsonObject)["percent"] + "%"));
                    }

                    return new HotelPenalties()
                    {
                        CancelingPenalties = cancelPen.ToArray(),
                        ChangingPenalties = new KeyValuePair<DateTime, string>[0]
                    };
                }
            }

            return new HotelPenalties()
            {
                ErrorCode = 39, //неизвестный variant id
                ErrorMessage = "hotel room not found"
            };
        }


        public HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId)
        {
            try
            {
                //пытаемся поднять информацию из кэша
                //проверяем кэш по городу
                var cityCache = RedisHelper.GetString("hotel_verify_cache_" + searchId);

                if (!string.IsNullOrEmpty(cityCache))
                {
                    JsonObject cacheObject = JsonConvert.Import<JsonObject>(cityCache);

                    if (cacheObject.Contains("hotel" + hotelId + "_" + variantId))
                    {
                        return new HotelVerifyResult()
                        {
                            IsAvailable = true,
                            Prices = MtHelper.ApplyCourses(Convert.ToDecimal((cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject)["RUB"]), _courses),
                            VariantId = variantId
                        };
                    }
                }

                //проверяем кэш по отелю
                var hotelCache = RedisHelper.GetString("hotel_verify_cache_"+hotelId+"_" + searchId);

                if (!string.IsNullOrEmpty(hotelCache))
                {
                    JsonObject cacheObject = JsonConvert.Import<JsonObject>(hotelCache);

                    if (cacheObject.Contains("hotel" + hotelId + "_" + variantId))
                    {
                        return new HotelVerifyResult()
                        {
                            IsAvailable = true,
                            Prices = MtHelper.ApplyCourses(Convert.ToDecimal((cacheObject["hotel" + hotelId + "_" + variantId] as JsonObject)["RUB"]), _courses),
                            VariantId = variantId
                        };
                    }
                }

                //если не выходит, пробуем сделать повторный поиск комнат по отелю
                FindRooms(hotelId);
                //получаем информацию о номере             
                var room = this.FoundRoom;

                    foreach(RoomVariant variant in room.Variants)
                        if(variant.VariantId == variantId)
                            return new HotelVerifyResult()
                            {
                                IsAvailable = true,
                                Prices = variant.Prices,
                                VariantId = variantId
                            };
             
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }


            return new HotelVerifyResult()
            {
                IsAvailable = false,
                Prices = new KeyValuePair<string, decimal>[0],
                VariantId = variantId
            };
        }
    
        //сохраняем бронь для создания заказа в мастер-туре
        public HotelBooking BookRoom(string searchId, int hotelId, BookRoom operatorRooms, List<int> operatorTurists)
        {
                BookRoom room = operatorRooms;

                HotelVerifyResult hvr = this.VerifyHotelVariant(hotelId, room.VariantId);

                if (hvr == null) throw new Exceptions.CatseException("not verified", 0);

                var tempParts = hvr.VariantId.Split('_');

                if (tempParts[tempParts.Length - 2] == "av")
                {
                    var okRateId = tempParts[2];

                    var redisVariantId = RedisHelper.GetString(okRateId + "_true_code");

                    if (string.IsNullOrEmpty(redisVariantId))
                        throw new Exceptions.CatseException("not founded rateId", 0);

                    hvr.VariantId = redisVariantId;
                }

                return new HotelBooking()
                {
                    DateBegin = this.startDate.ToString("yyyy-MM-dd"),
                    NightsCnt = (this.endDate - this.startDate).Days,
                    PartnerBookId = hvr.VariantId + "_" + hotelId,
                    PartnerPrefix = id_prefix,
                    SearchId = searchId,
                    Prices = hvr.Prices,
                    Title = "some_title",
                    Turists = operatorTurists.ToArray()
                };

        }

        //делает реальное бронирование в системе "островок"
        public static string CreateBooking(int bookId, string dogovorCode)
        {
            string bookIds = "";

            try
            {
                var tmpBooking = MtHelper.GetHotelBookingFromCache(bookId);

                //проходимся по комнатам, бронируем их
                foreach (var room in tmpBooking)
                {
                    var turists = MtHelper.GetTuristsFromCache(room.Turists);

                    JsonArray arr = new JsonArray();

                    foreach (TuristContainer turist in turists)
                    {
                        var tmp = new JsonObject();

                        tmp["first_name"] = turist.FirstName;
                        tmp["last_name"] = turist.Name;

                        arr.Add(tmp);
                    }
                    if (room.PartnerBookId.IndexOf(id_prefix) == 0)
                    {
                        JsonObject bookObject = new JsonObject();
                        bookObject["email"] = "it@viziteurope.eu";
                        bookObject["phone"] = "9638219209";
                        bookObject["partner_order_id"] = dogovorCode;

                        bookObject["availability_hash"] = room.PartnerBookId.Split('_')[1];
                        bookObject["user_ip"] = "82.29.0.86";

                        bookObject["guests"] = arr;
                        bookObject["payment_type"] = "deposit";

                        Logger.WriteToLog(OS_URL + "/api/b2b/v2/order/reserve"  + JsonConvert.ExportToString(bookObject));
                        var bookResponse = CreateOstrovokClient().UploadString(OS_URL + "/api/b2b/v2/order/reserve", JsonConvert.ExportToString(bookObject));

                        JsonObject responseJson = JsonConvert.Import<JsonObject>(bookResponse);

                        string status = (responseJson["debug"] as JsonObject)["status"].ToString();

                        if (status == "200")
                            bookIds += (responseJson["result"] as JsonObject)["order_id"].ToString() + "_";
                        else
                            bookIds += "error:" + bookResponse + "_";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }

            return bookIds;
        }

        //ставим признак, что результаты обработаны
        public void SetAdded()
        {
            Logger.WriteToLog("ostrovok added");
            this.added = true;
        }

        //отдаем значение признака об обработке результатов
        public bool GetAdded()
        {
            return this.added;
        }

        private bool isFinished = false;
        //отдаем признак о завершении поиска
        public bool GetFinished()
        {
            return this.isFinished;
        }
        public string GetSearchId()
        {
            return this.searchId;
        }

        private RoomInfo PrepareRoomsForRequest()
        {
            var result = new List<RoomInfo>();

            //готовим список туристов
            var guests = new List<Guest>();

            int ads = room.Adults;
                //добавляем взрослых
            while (ads-- > 0)
               guests.Add(new Guest() { Age = 20, IsChild = false });

                //добавляем детей
            foreach (int age in room.ChildrenAges)
                guests.Add(new Guest() { Age = age, IsChild = true });

            return new RoomInfo() { Guests = guests.ToArray() };
        }

        public string GetPrefix()
        {
            return id_prefix;
        }
    }
}