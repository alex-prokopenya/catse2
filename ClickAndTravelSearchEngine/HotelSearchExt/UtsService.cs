using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.MasterTour;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Responses;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;

namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public class UtsService : IHotelExt
    {
        private static readonly string FINISHED_STATE = "finished";

        private static string Login = "clickandtravel";

        private static string Password = "iE9BAMaP";

        private static string Url = "http://hotelbook.pro/";

        private double lastTimeRequest = 0;

        private string lastTimeValue = "";

        private long HOTELS_RESULTS_LIFETIME = 0;

        private string searchId = "";

        public static readonly string id_prefix = "hb_";

        private Hotel[] FindedHotels = null;
        private Room[] FindedRooms = null;

        private int cityId = 0;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MinValue;
        private int[] stars = new int[0];
        private int[] pansions = new int[0];
        private RequestRoom[] rooms = new RequestRoom[0];

        private bool added = false;
        private bool isFinished = false;

        private Dictionary<int, int[]> pansionsGroups = new Dictionary<int, int[]>();
        private Dictionary<int, int> pansionsLib = new Dictionary<int, int>();

        private Decimal HOTELBOOK_COEF = Convert.ToDecimal(ConfigurationManager.AppSettings["HotelBookMargin"]);
        
        private string GetTime() //получаем время для авторизации
        {
            if ((lastTimeRequest + 50) < (DateTime.Now.TimeOfDay.TotalSeconds))
                lastTimeRequest = DateTime.Now.TimeOfDay.TotalSeconds;
            else
                return lastTimeValue;


            WebClient cl = new WebClient();

            lastTimeValue = cl.DownloadString(Url + "xml/unix_time");

            return lastTimeValue;
        }
        
        private string MakeUrlPostfix() //генерируем постфикс для авторизации
        {
            string time = GetTime();

            return String.Format("&time={0}&login={1}&checksum={2}", time, Login, HashSumm.CalculateMD5Hash(HashSumm.CalculateMD5Hash(Password) + time));
        }

        private string MakePostRequest(string url, string doc) // делаем запрос
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                return wc.UploadString(url, "&request=" + doc);
            }
        }

        public UtsService()
        { }

        public UtsService(int CityId, DateTime StartDate, DateTime EndDate, int[] Stars, int[] Pansions, RequestRoom[] Rooms, string SearchId, long RESULTS_LIFETIME)
        {
            Logger.WriteToLog("in UtsService");
            //парсим группы по питанию
            for (int i = 0; i < 7; i++)
            {
                string ids = ConfigurationManager.AppSettings["UtsPansionGroup" + i];

                Logger.WriteToLog(" group " + i + " " + ids);
                if ((ids != null) && (ids.Length > 0))
                {
                    string[] idsParts = ids.Split(',');

                    List<int> idsList = new List<int>();

                    foreach (string id in idsParts)
                    {
                        try
                        {
                            int idInt = Convert.ToInt32(id);
                            idsList.Add(idInt);
                            pansionsLib[idInt] = i;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToLog(" group " + i + " fail " + ex.Message);
                        }
                    }
                    pansionsGroups[i] = idsList.ToArray();
                }
                else
                    pansionsGroups[i] = new int[0];
            }

            List<int> selectedPansions = new List<int>();
            foreach (int pansGroupId in Pansions)
                selectedPansions.AddRange(pansionsGroups[pansGroupId]);

            this.pansions = selectedPansions.ToArray();

            //город
            this.cityId = GetCityId(CityId); //конвертируем кликовский id в id от uts
            
            //звездность
            Dictionary<int, int> clickToUtsStars = new Dictionary<int, int>();
            clickToUtsStars.Add(1,5);
            clickToUtsStars.Add(2,4);
            clickToUtsStars.Add(3,2);
            clickToUtsStars.Add(4,3);
            clickToUtsStars.Add(5,1);

            List<int> starsList = new List<int>();
            foreach (int starItem in Stars)
                starsList.Add(clickToUtsStars[starItem]);

            this.stars = starsList.ToArray();

            this.rooms = Rooms;

            this.searchId = SearchId;

            this.startDate = StartDate;
            this.endDate = EndDate;

            this.HOTELS_RESULTS_LIFETIME = RESULTS_LIFETIME;
        }

        //стартует поиск в системе uts
        private int StartSearch(RequestRoom room, int hotelId)  //если указан hotelId, ищем по конкретному отелю
        {
            XmlDocument xDoc = new XmlDocument();

            XmlNode docNode = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(docNode);

            //создаем элемент Request
            XmlNode hotelSearchRequestNode = xDoc.CreateElement("HotelSearchRequest");
            XmlNode requestNode = xDoc.CreateElement("Request");

            //создаем атрибуты
            XmlAttribute cityIdAttr = xDoc.CreateAttribute("cityId");
            cityIdAttr.Value = this.cityId.ToString();                              //город

            if (hotelId > 0) //если в параметрах получили id отеля
            {
                XmlAttribute hotelIdAttr = xDoc.CreateAttribute("hotelId");
                hotelIdAttr.Value = hotelId.ToString();
                requestNode.Attributes.Append(hotelIdAttr);
            }

            XmlAttribute checkInAttr = xDoc.CreateAttribute("checkIn");
            checkInAttr.Value = this.startDate.ToString("yyyy-MM-dd");             //дата заселения

            XmlAttribute durationAttr = xDoc.CreateAttribute("duration");
            durationAttr.Value = ((this.endDate - this.startDate).Days ).ToString(); //продолжительность

            requestNode.Attributes.Append(cityIdAttr);
            requestNode.Attributes.Append(checkInAttr);
            requestNode.Attributes.Append(durationAttr);

            xDoc.AppendChild(hotelSearchRequestNode);
            hotelSearchRequestNode.AppendChild(requestNode);

            //добавляем комнаты
            XmlNode roomsElement = xDoc.CreateElement("Rooms");
            hotelSearchRequestNode.AppendChild(roomsElement);

            XmlNode roomElement = xDoc.CreateElement("Room");

            XmlAttribute roomNumberAttr = xDoc.CreateAttribute("roomNumber");
            roomNumberAttr.Value =  "1";

            XmlAttribute adultsAttr = xDoc.CreateAttribute("adults");
            adultsAttr.Value = room.Adults.ToString();

            int[] ages = (room.ChildrenAges).Where(a => (a > 2) && a < 19).ToArray();

            XmlAttribute childrenAttr = xDoc.CreateAttribute("children");
            childrenAttr.Value = ages.Length.ToString();

            XmlAttribute cotsAttr = xDoc.CreateAttribute("cots");
            cotsAttr.Value = room.ChildrenAges.Where(a => (a < 3)).ToArray().Length.ToString();

            roomElement.Attributes.Append(roomNumberAttr);
            roomElement.Attributes.Append(adultsAttr);
            roomElement.Attributes.Append(childrenAttr);
            roomElement.Attributes.Append(cotsAttr);

            foreach (int age in ages)
            {
                XmlNode ageElement = xDoc.CreateElement("ChildAge");
                ageElement.InnerText = age.ToString();
                roomElement.AppendChild(ageElement);
            }

            roomsElement.AppendChild(roomElement);
         
            string xml_request = xDoc.InnerXml;

            string response = MakePostRequest(Url +"xml/hotel_search?async=1" + MakeUrlPostfix(), xDoc.InnerXml);

            Logger.WriteToHotelBookLog(xDoc.InnerXml);
            Logger.WriteToHotelBookLog(response);
            Logger.WriteToLog(xDoc.InnerXml);

            XmlDocument respDoc = new XmlDocument();
            respDoc.LoadXml(response);

            XmlNodeList searchIdList = respDoc.GetElementsByTagName("HotelSearchId");

            if (searchIdList.Count == 0)
                throw new Exception("init search fail. given response: " + response);

            return Convert.ToInt32((searchIdList[0] as XmlElement).InnerText);
        }

        //берем текущие результаты по отелю
        private Dictionary<int, Room> GetCurrentResults(int searchId, string lastResultId, RequestRoom inRoom, out string newLastResultId)
        {
        //    Logger.WriteToLog("in GetCurrentResults");

            newLastResultId = "";
            //забирает текущие результаты из ютс
            string url = string.Format(Url + "xml/hotel_search_async?login={0}&search_id={1}", Login, searchId);

            if (lastResultId != "")
                url += "&from_result_id=" + lastResultId;

            WebClient wcl = new WebClient();

            string response = wcl.DownloadString(url);

            //Logger.WriteToHotelBookLog(xDoc.InnerXml);
            Logger.WriteToHotelBookLog(response);
            XmlDocument respDoc = new XmlDocument();
            respDoc.LoadXml(response);

            Dictionary<int, Room> results = new Dictionary<int, Room>();

            List<int> starsFilter = new List<int>(this.stars);
            List<int> pansionsFilter = new List<int>(this.pansions);

            //проходимся по выборке отелей
            foreach (XmlElement el in respDoc.GetElementsByTagName("Hotel"))
            {
                int resultId = Convert.ToInt32(el.GetAttribute("resultId"));

                newLastResultId = "" + resultId;

                //читаем категорию отелей
                int hotelCatId = Convert.ToInt32(el.GetAttribute("hotelCatId"));

                //проверяем фильтр по категории
                if ((starsFilter.Count != 0) && (!starsFilter.Contains(hotelCatId)))
                    continue;

                //берем комнату в отеле
                XmlElement roomEl = el.GetElementsByTagName("Room")[0] as XmlElement;

                //читаем тип питания
                int mealId = Convert.ToInt32(roomEl.GetAttribute("mealId"));

                //проверяем фильтр по питанию
                if ((pansionsFilter.Count != 0) && (!pansionsFilter.Contains(mealId)))
                    continue;

                //читаем атрибуты результата
                int hotelId  = 10000000 + Convert.ToInt32(el.GetAttribute("hotelId"));
                
                string variantId = id_prefix + searchId + "_" + resultId;

                //создаем вариант комнаты
                RoomVariant roomVariant = new RoomVariant() { 
                    VariantId = variantId,
                    RoomTitle = roomEl.GetAttribute("roomSizeName"),
                    RoomCategory = roomEl.GetAttribute("roomTypeName")  + " " + roomEl.GetAttribute("roomViewName"),
                    PansionTitle = roomEl.GetAttribute("mealName") ,
                    PansionGroupId = pansionsLib[mealId],
                    RoomInfo = roomEl.GetAttribute("roomName"),
                    Prices = new KeyValuePair<string, decimal>[] {
                                    new KeyValuePair<string,decimal>( "RUB", Math.Round(HOTELBOOK_COEF * Convert.ToDecimal(el.GetAttribute("comparePrice").Replace(".",","))))
                    }
                };

                //если нет такого отеля в списке, добавляем
                if(!results.ContainsKey(hotelId))
                {
                    results.Add(
                                    hotelId,
                                    new Room()
                                    {
                                        Adults = inRoom.Adults,
                                        Children = inRoom.Children,
                                        ChildrenAges = inRoom.ChildrenAges,
                                        Variants = new RoomVariant[0]
                                    }
                            );
                }

                List<RoomVariant> vars = new List<RoomVariant>(results[hotelId].Variants);
                vars.Add(roomVariant);
                results[hotelId].Variants = vars.ToArray();
            }

            //если поиск закончился, отправляем статус выше
            #region CheckIfFinished
            XmlElement hotelsElement = respDoc.GetElementsByTagName("Hotels")[0] as XmlElement;

            if (hotelsElement.HasAttribute("finalResultId"))
                newLastResultId = FINISHED_STATE;
            #endregion
            return results;
        }

        //компонуем результаты по комнате
        private Dictionary<int, Room> ComposeRoomResults(Dictionary<int, Room> oldList, Dictionary<int, Room> newList)
        {
            //идем по новому списку номеров
            foreach (int hotelId in newList.Keys)
                //если в старом списке не было отеля с таки айди
                if(!oldList.ContainsKey(hotelId))
                    oldList.Add(hotelId, newList[hotelId]); //просто копируем из нового
                else
                    oldList[hotelId].Variants.ToList().AddRange(newList[hotelId].Variants); //добавляем к старому новые варианты

            return oldList;
        }

        private RoomVariant[] RoomApplyCourses(Room room, KeyValuePair<string, decimal>[] courses)
        {
            for (int i = 0; i < room.Variants.Length; i++)
            {
                room.Variants[i].Prices = MtHelper.ApplyCourses(room.Variants[i].Prices[0].Value, courses);
            }
            return room.Variants;
        }
        
        private Hotel[] ComposeHotelsResults(Dictionary<int,Dictionary<int, Room>> results)
        {
            List<Hotel> res = new List<Hotel>();

            Dictionary<int, Hotel> hotels = new Dictionary<int, Hotel>(); //справочник скомпанованных отелей

            int roomCnt = 0;

            //получаем курсы валют
            KeyValuePair<string, decimal>[] _courses = null;

            if (results.Count > 0)
                _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);


            foreach (int roomIndex in results.Keys)               //идем по всем комнатам
            {
                foreach (int hotelKey in results[roomIndex].Keys) //для каждой комнаты берем найденные отели
                {
                    //применить курсы
                    results[roomIndex][hotelKey].Variants = RoomApplyCourses(results[roomIndex][hotelKey], _courses);
                }
            }

            RedisHelper.SetString("uts_" + this.searchId + "_results", JsonConvert.ExportToString(results), new TimeSpan(HOTELS_RESULTS_LIFETIME));


            foreach (int roomIndex in results.Keys)               //идем по всем комнатам
            {
                foreach (int hotelKey in results[roomIndex].Keys) //для каждой комнаты берем найденные отели
                {
                    //применить курсы
                    //results[roomIndex][hotelKey].Variants = RoomApplyCourses(results[roomIndex][hotelKey], _courses);

                    if (roomCnt == 0)                             //если просматриваем первую комнату, добавляем отель в справочник
                    {
                        hotels.Add(hotelKey,
                                new Hotel()
                                {
                                    HotelId = hotelKey,
                                    Rooms = new Room[0]
                                }
                            );
                    }

                    if (hotels.ContainsKey(hotelKey))
                    {
                        if (hotels[hotelKey].Rooms.Length == roomCnt) //если отель содержит нужное количество комнат
                        {
                            Room addRoom = results[roomIndex][hotelKey]; //берем комнату -- кандидат на добавление

                            addRoom.Variants = GetTopVariant(addRoom.Variants); //оставляем самый дешевый вариант

                            List<Room> hotelRooms = hotels[hotelKey].Rooms.ToList(); //добавляем комнату отелю

                            hotelRooms.Add(addRoom);

                            hotels[hotelKey].Rooms = hotelRooms.ToArray();
                        }
                        else //если в отель не добавлялась предыдущая комната
                            hotels.Remove(hotelKey); //удаляем
                    }
                }
                roomCnt++;
            }

            foreach (int key in hotels.Keys)
                if (hotels[key].Rooms.Length == roomCnt)
                    res.Add(hotels[key]);

            hotelsReturned = false;

            return res.ToArray();
        }

        private RoomVariant[] GetTopVariant(RoomVariant[] vars)
        { 
            decimal minPrice = 10000000000;
            int minIndex = -1;
            
            for(int i=0; i<vars.Length; i++)
                if(vars[i].Prices[0].Value < minPrice)
                {
                    minPrice = vars[i].Prices[0].Value;
                    minIndex = i;
                }

            return new RoomVariant[] { vars[minIndex] };
        }

        public void FindHotels()
        {
            //Logger.WriteToLog("in find hotels");
            try
            {
                int globalTimer = 60;
                DateTime finishTime = DateTime.Now.AddSeconds(globalTimer);

                //сохраняем search_id
                var roomSearchId = new Dictionary<int,int>(); 
                //последний резалт айди для следующих запросов
                var roomLastResultId = new Dictionary<int,string>(); 
                //результаты поиска по комнатам
                var roomResults = new Dictionary<int,Dictionary<int, Room>>(); 

                //для всех комнат стартуем поиск и получаем searchId
                for(int i=0; i<this.rooms.Length; i++)
                {
                    roomLastResultId.Add(i,"");
                    roomResults.Add(i, new Dictionary<int, Room>());
                    roomSearchId[i] = StartSearch(this.rooms[i], 0); //стартуем поиск, сохраняем searchId
                }

                //для всех комнат дожидаемся первых результатов
                while(true)
                {
                    bool firstRes = true;

                    for(int i=0; i<this.rooms.Length; i++)
                    {
                        if(roomLastResultId[i] != FINISHED_STATE)
                        {
                            string lastId = "";

                            //получаем текущий результат
                            //варианты фильтруются по звездам и питанию
                            var results = GetCurrentResults(roomSearchId[i], roomLastResultId[i], rooms[i], out lastId); 

                            roomLastResultId[i] = lastId;

                            if (results.Count > 0)
                                //компонуем результат, в старую выборку добавляя новые варианты
                                roomResults[i] = ComposeRoomResults(roomResults[i], results); 
                            else
                                firstRes = firstRes && roomLastResultId[i] == FINISHED_STATE;
                        }
                    }

                    if(firstRes) break;

                    Thread.Sleep(100);
                }

                //компануем их и выставляем наружу
                // -- сортируем варианты по стоимости, оставляем по 1, самому дешевому
                // -- объединяем в пары, для поиска по нескольким комнатам
                this.FindedHotels = ComposeHotelsResults(roomResults);

                while (true) //ждем финиша всех потоков
                {
                    bool all_finished = true; //признак того, что поиск закончен

                    for (int i = 0; i < this.rooms.Length; i++) //проходимся по всем комнатам
                    {
                        if (roomLastResultId[i] != FINISHED_STATE) //если поиск по комнате еще не завершен
                        {
                            string lastId = "";
                            //получаем текущий результат
                            var results = GetCurrentResults(roomSearchId[i], roomLastResultId[i], rooms[i], out lastId); //фильтруем их по звездам и питанию

                            if (results.Count > 0) //если что-то нашлось
                            {
                                roomResults[i] = ComposeRoomResults(roomResults[i], results); //добавляем в выборку

                                this.FindedHotels = ComposeHotelsResults(roomResults); //изменяем общий массив с результатами
                            }

                            roomLastResultId[i] = lastId; //меняем lastId комнаты

                            all_finished = all_finished && roomLastResultId[i] == FINISHED_STATE;
                        }
                    }
                   
                    if ((all_finished)||(DateTime.Now > finishTime)) break;

                    Thread.Sleep(1000);
                }
                
                isFinished = true;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                this.FindedHotels = new Hotel[0];
                isFinished = true;
            }
        }

        public HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId)
        {
            #region результат проверки из кэша
            string redis_verify_key = this.searchId + "_" + hotelId + "_" + variantId + "_verify";

            string redis_hash = RedisHelper.GetString(redis_verify_key);

            if (!string.IsNullOrEmpty(redis_hash))
                return JsonConvert.Import<HotelVerifyResult>(redis_hash);
            #endregion

            string[] vIdParts = variantId.Split('_');

            XmlDocument xDoc = this.GetHotelSearchDetails(Convert.ToInt32( vIdParts[1]), Convert.ToInt32( vIdParts[2]));

            //пробуем найти информацию о цене
            XmlNodeList priceNodes = xDoc.GetElementsByTagName("ComparePrice");

            if (priceNodes.Count == 0) //если не получили информацию о цене, считаем, что вариант недоступен для бронирования
            {
                Logger.WriteToLog("verify error. response: " + xDoc.InnerXml);
                return new HotelVerifyResult()
                    {
                        IsAvailable = false,
                        VariantId = variantId,
                        Prices = new KeyValuePair<string, decimal>[0]
                    };
            }
            //если есть данные о цене, огругляем сумму в рублях, считаем по курсам
            //получаем курсы валют
            KeyValuePair<string, decimal>[] _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);

            decimal price = Math.Round(HOTELBOOK_COEF * Convert.ToDecimal(priceNodes[0].InnerText.Replace(".", ",")));

            var res = new HotelVerifyResult() { 
                
                     IsAvailable = true,
                     VariantId = variantId,
                     Prices= MtHelper.ApplyCourses(price, _courses)
            };

            //получить информацию о варианте по вариант id
            RedisHelper.SetString(redis_verify_key, JsonConvert.ExportToString(res), new TimeSpan(HOTELS_RESULTS_LIFETIME / 10));

            return res;
        }

        private static XmlNode CreateXmlElement(ref XmlNode parent, string tag,  string innerText, KeyValuePair<string, string>[] attrs)
        {
            XmlNode newItem = parent.OwnerDocument.CreateElement(tag);
            newItem.InnerText = innerText;

            if(attrs != null)
                foreach (KeyValuePair<string, string> attrItem in attrs)
                {
                    var attr = parent.OwnerDocument.CreateAttribute(attrItem.Key);
                    attr.Value = attrItem.Value;

                    newItem.Attributes.Append(attr);
                }

            parent.AppendChild(newItem);

            return newItem;
        }

        private string SaveBooking(string variantId, List<int> turistsIds, string orderID)
        {
            //получить данные по туристам из кэша МТ
            TuristContainer[] turists = MtHelper.GetTuristsFromCache(turistsIds.ToArray());


            //создать бронь
            //формируем запрос
            XmlDocument requestDoc = new XmlDocument();

            XmlNode docNode = requestDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            requestDoc.AppendChild(docNode);

            //создаем элемент Request
            XmlNode requestNode = requestDoc.CreateElement("AddOrderRequest");

            requestDoc.AppendChild(requestNode);

            if (orderID == "") // если новый заказ, отдаем контактную информацию
            {
                var contactsNode = CreateXmlElement(ref requestNode, "ContactInfo", "", null);

                CreateXmlElement(ref contactsNode, "Name", "click.travel", null);
                CreateXmlElement(ref contactsNode, "Email", "info@clickandtravel.ru", null);
                CreateXmlElement(ref contactsNode, "Phone", "8-495-784-62-56", null);
                CreateXmlElement(ref contactsNode, "Comment", "", null);

                CreateXmlElement(ref requestNode, "Tag", DateTime.Now.ToString("yyMMddHHmmss"), null);
            }
            else //если уже есть, даем ссылку на существующий заказ
                CreateXmlElement(ref requestNode, "OrderId", orderID, null);

            string[] variantArr = variantId.Split('_');

            var itemsEl = CreateXmlElement(ref requestNode, "Items", "", null);

            var hotelItemEl = CreateXmlElement(ref itemsEl, "HotelItem", "", null);

            CreateXmlElement(ref hotelItemEl, "Search", "", 
                                        new KeyValuePair<string, string>[]{
                                            new KeyValuePair<string, string>("resultId", variantArr[2]),
                                            new KeyValuePair<string, string>("searchId", variantArr[1])
                                        });

            CreateXmlElement(ref hotelItemEl, "DuplicatesAllowed", "true", null);

            var roomsEl = CreateXmlElement(ref hotelItemEl, "Rooms", "", null);

            var roomEl = CreateXmlElement(ref roomsEl, "Room", "", null);

            foreach(TuristContainer tst in turists)
            {
                var attributes = new KeyValuePair<string, string>[0];

                string title = "Mrs";

                if (tst.Sex == 1) title = "Mr";

                DateTime zeroTime = new DateTime(1, 1, 1);

                int tstYears = (zeroTime + (DateTime.Today - tst.BirthDate)).Year -1;

                if (tstYears < 3) continue;

                if (tstYears < 18)
                {
                    attributes = new KeyValuePair<string, string>[] { 
                        new KeyValuePair<string, string>("child","true"),
                        new KeyValuePair<string, string>("age",tstYears.ToString()),
                    };
                    title = "Chld";
                }
                else
                    attributes = new KeyValuePair<string, string>[] { 
                        new KeyValuePair<string, string>("child","false")
                    };

                var paxEl = CreateXmlElement(ref roomEl, "RoomPax", "", attributes);

                CreateXmlElement(ref paxEl, "Title", title, null);
                CreateXmlElement(ref paxEl, "FirstName", tst.Name, null);
                CreateXmlElement(ref paxEl, "LastName", tst.FirstName, null);
            }

           
            var response = MakePostRequest(Url + "xml/add_order?" + MakeUrlPostfix(), requestDoc.InnerXml);
            Logger.WriteToHotelBookLog(requestDoc.InnerXml);
            Logger.WriteToHotelBookLog(response);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(response);

            if (orderID == "")
            {
                XmlNodeList nList = xDoc.GetElementsByTagName("OrderId");

                if (nList.Count == 0) throw new Exception("book failed. recieved response: " + response);

                return nList[0].InnerText;
            }
            else
            {
                XmlNodeList nList = xDoc.GetElementsByTagName("Id");

                if (nList.Count == 0) throw new Exception("book failed. recieved response: " + response);

                return orderID;
            }
        }

        public HotelBooking[] BookRooms(string searchId, int hotelId, List<BookRoom> operatorRooms, List<List<int>> operatorTurists)
        {
            try
            {
                List<HotelBooking> res_arr = new List<HotelBooking>();

                string orderID = "";
               

                for (int i = 0; i < operatorRooms.Count; i++)// BookRoom room in operatorRooms)
                {
                    BookRoom room = operatorRooms[i];

                    HotelVerifyResult hvr = this.VerifyHotelVariant(hotelId, room.VariantId);

                    if (hvr == null) throw new Exceptions.CatseException("not verified", 0);

                    orderID = SaveBooking(operatorRooms[i].VariantId, operatorTurists[i], orderID);

                    res_arr.Add(new HotelBooking() //результат бронирования
                    {
                        DateBegin = this.startDate.ToString("yyyy-MM-dd"),
                        NightsCnt = (this.endDate - this.startDate).Days,
                        PartnerBookId = orderID,
                        PartnerPrefix = id_prefix,
                        SearchId = searchId,
                        Prices = hvr.Prices,
                        Title = "some_title",
                        Turists = operatorTurists[i].ToArray()
                    });
                }
                //бронирование комнаты
                return res_arr.ToArray();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace + " " + ex.Source);
                throw ex;
            }
        }

        public void FindRooms(object hotelId)
        {
            //поиск комнат по отелю в согласии с параметрами
            int iHotelId = Convert.ToInt32(hotelId);
            int hotelKey = iHotelId - 10000000; //айдишник отеля в системе ютс
               //айдишник отеля в базе клика

            int counter = 0;
            try
            {
                //проверить, есть ли в редисе кэш на поиск по городу
                string cityRedisKey = "uts_" + this.searchId + "_results";
                string cachedString =  RedisHelper.GetString(cityRedisKey);

                if (cachedString != null) //если есть кэш, берем номера из кэша
                {
                    //берем кэш
                    var jsonRes = JsonConvert.Import(cachedString) as JsonObject;

                    List<Room> cachedRooms = new List<Room>();

                    foreach (string key in jsonRes.Names)
                    {
                        if ((jsonRes[key] as JsonObject).Contains(hotelId.ToString()))
                            cachedRooms.Add(JsonConvert.Import<Room>((jsonRes[key] as JsonObject)[hotelId.ToString()].ToString()));
                    }

                    //формируем результат
                  
                    /*
                    //проходимся по результатам сохраненным в кэше
                    foreach (int roomIndex in cachedResults.Keys)
                    {
                        if (cachedResults[roomIndex].ContainsKey(hotelKey))
                            cachedRooms.Add(cachedResults[roomIndex][hotelKey]);
                        else
                            break;
                    }
                     */
                    //если из кэша подняли достаточное количество комнат, сохраняем их
                    //если не достаточно, ставим пустую ссылку
                    this.FindedRooms = cachedRooms.Count == this.rooms.Length? cachedRooms.ToArray(): null;
                }

                if ((this.FindedRooms == null) || (this.FindedRooms.Length == 0))
                {   
                    //если нет, сделать поиск по отелю
                    int globalTimer = 60;
                    DateTime finishTime = DateTime.Now.AddSeconds(globalTimer); //предельное время ожидания результата

                    //сохраняем search_id
                    var roomSearchId = new Dictionary<int, int>();
                    //последний резалт айди для следующих запросов
                    var roomLastResultId = new Dictionary<int, string>();
                    //результаты поиска по комнатам
                    var roomResults = new Room[this.rooms.Length];

                    //для всех комнат стартуем поиск и получаем searchId
                    for (int i = 0; i < this.rooms.Length; i++)
                    {
                        roomLastResultId.Add(i, "");
                       // roomResults[i] new Dictionary<int, Room>());
                        roomSearchId[i] = StartSearch(this.rooms[i], hotelKey); //стартуем поиск, сохраняем searchId
                    }

                    while (true)
                    {
                        bool all_finished = true; //признак того, что поиск закончен

                        for (int i = 0; i < this.rooms.Length; i++) //проходимся по всем комнатам
                        {
                            if (roomLastResultId[i] != FINISHED_STATE) //если поиск по комнате еще не завершен
                            {
                                string lastId = "";
                                //получаем текущий результат
                                var results = GetCurrentResults(roomSearchId[i], roomLastResultId[i], rooms[i], out lastId); //фильтруем их по звездам и питанию

                                Dictionary<int, Room> tempDict = new Dictionary<int, Room>();

                                if (roomResults[i] != null)
                                    tempDict.Add(iHotelId, roomResults[i]);

                                if (results.Count > 0) //если что-то нашлось
                                    roomResults[i] = ComposeRoomResults(tempDict, results)[iHotelId]; //добавляем в выборку

                                roomLastResultId[i] = lastId; //меняем lastId комнаты

                                all_finished = all_finished && roomLastResultId[i] == FINISHED_STATE;
                            }
                        }

                        if ((all_finished) || (DateTime.Now > finishTime))
                            break;

                        Thread.Sleep(1000);
                    }

                    //скомпановать результаты
                    //Применить курсы!!!
                    //получаем курсы валют
                    KeyValuePair<string, decimal>[] _courses = null;

                    if (roomResults.Length > 0)
                        _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);

                    for (int i = 0; i < roomResults.Length; i++ )
                        roomResults[i].Variants = RoomApplyCourses(roomResults[i], _courses);

                    this.FindedRooms = roomResults;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source + " cnt " + counter);
                this.FindedRooms = new Room[0];
            }
        }

        private static int GetCityId(int clickCityId) //convert click cityId to uts city id
        {
            int utsCityId = clickCityId;

            SqlConnection myCon = new SqlConnection(ConfigurationManager.AppSettings["MasterTourConnectionString"]);
            
            myCon.Open();

            SqlCommand myCom = new SqlCommand("select uts_ctkey from hb_cities where cl_ctkey = " + clickCityId, myCon);

            utsCityId = Convert.ToInt32(myCom.ExecuteScalar());

            myCon.Close();

            return utsCityId;
        }

        private XmlDocument GetHotelSearchDetails(int searchId, int resultId)
        {
            string redisKey = "uts_hotel_details_" + searchId + "_" + resultId;
            string cacheRedis = RedisHelper.GetString(redisKey);

            if (cacheRedis != null)
            {
                XmlDocument cachedDoc = new XmlDocument();
                cachedDoc.LoadXml(cacheRedis);

                return cachedDoc;
            }

            XmlDocument xDoc = new XmlDocument();

            XmlNode docNode = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(docNode);

            XmlNode hotelSearchDetailsRequest = xDoc.CreateElement("HotelSearchDetailsRequest");
            XmlNode hotelSearches = xDoc.CreateElement("HotelSearches");
            XmlNode hotelSearch = xDoc.CreateElement("HotelSearch");

            XmlNode searchIdElement = xDoc.CreateElement("SearchId");
            XmlNode resultIdElement = xDoc.CreateElement("ResultId");

            resultIdElement.InnerText = resultId.ToString();
            searchIdElement.InnerText = searchId.ToString();

            xDoc.AppendChild(hotelSearchDetailsRequest);

            hotelSearchDetailsRequest.AppendChild(hotelSearches);
            hotelSearches.AppendChild(hotelSearch);

            hotelSearch.AppendChild(searchIdElement);
            hotelSearch.AppendChild(resultIdElement);

            string xml_request = xDoc.InnerXml;

            string response = MakePostRequest(Url + "xml/hotel_search_details?" + MakeUrlPostfix(), xDoc.InnerXml);

            XmlDocument respDoc = new XmlDocument();
            respDoc.LoadXml(response);

            if(!response.ToLower().Contains("error"))
                RedisHelper.SetString(redisKey, response, new TimeSpan(HOTELS_RESULTS_LIFETIME/5));

            return respDoc;
        }

        public HotelPenalties GetHotelPenalties(int hotelId, string variantId)
        {
            string redis_key = this.searchId + "_" + hotelId + "_" + variantId + "_penalties";

            string redis_hash = RedisHelper.GetString(redis_key);

            if ((redis_hash != null) && (redis_hash.Length > 0))
                return JsonConvert.Import<HotelPenalties>(redis_hash);

            string[] vIdParts = variantId.Split('_');

            //запрашиваем детали по варианту у хотлебука
            XmlDocument xDoc = this.GetHotelSearchDetails(Convert.ToInt32(vIdParts[1]), Convert.ToInt32(vIdParts[2]));

            //если ответ содержит инфо об ошибке -- даем пустой ответ
            if(xDoc.InnerXml.ToLower().Contains("error")) 
            {
                Logger.WriteToLog("penalties error. response: " + xDoc.InnerXml);
                return null;
            }


            XmlNodeList penalties = xDoc.GetElementsByTagName("ChargeConditions");
            if (penalties.Count == 0)
            {
                return
                    new HotelPenalties()
                    {
                        CancelingPenalties = new KeyValuePair<DateTime,string>[0],
                        ChangingPenalties = new KeyValuePair<DateTime,string>[0],
                        VariantId = variantId
                    };
            }

            ///else
            ///
            //узнаем код валюты, в которой будут штрафы
            string rateCode = (penalties[0] as XmlElement).GetElementsByTagName("Currency")[0].InnerText;

            //получаем все записи о штрафах
            var res = new HotelPenalties()
            {
                VariantId = variantId,
                CancelingPenalties = ParsePenalties((penalties[0] as XmlElement).GetElementsByTagName("Cancellation"), rateCode),
                ChangingPenalties = ParsePenalties((penalties[0] as XmlElement).GetElementsByTagName("Amendment"), rateCode)
            };

            RedisHelper.SetString(redis_key, JsonConvert.ExportToString(res), new TimeSpan(this.HOTELS_RESULTS_LIFETIME * 2));

            return res;
        }

        private static KeyValuePair<DateTime, string>[] ParsePenalties(XmlNodeList nList, string rateCode)
        {
            //создаем список штрафов
            List<KeyValuePair<DateTime, string>> chPen = new List<KeyValuePair<DateTime, string>>();

            //проходимся по полученному от ютс списку
            foreach (XmlElement penaltyElement in nList)
            {
                //если штрафа нет, пропускаем строку
                if (penaltyElement.GetAttribute("charge") == "false") continue;

                DateTime fromDate = DateTime.Today;

                //узнаем дату начала штрафа
                if (penaltyElement.HasAttribute("from"))
                    fromDate = Convert.ToDateTime(penaltyElement.GetAttribute("from"));

                string penaltyText = "";

                //если штраф описан текстом
                if (penaltyElement.HasAttribute("policy"))
                    penaltyText = penaltyElement.GetAttribute("policy");
                else
                    if (penaltyElement.HasAttribute("price"))
                        penaltyText = penaltyElement.GetAttribute("price") + " " + rateCode;

                chPen.Add(new KeyValuePair<DateTime, string>(fromDate, penaltyText));
            }

            return chPen.ToArray();
        }

        private  bool hotelsReturned = true;
        public Hotel[] GetHotels()
        {
            if (hotelsReturned)
                return new Hotel[0];
            else
                hotelsReturned = true;

            return this.FindedHotels;
        }

        public Room[] GetRooms()
        {
            return this.FindedRooms;
        }

        public void SetAdded()
        {
            this.added = true;
        }

        public bool GetAdded()
        {
            return this.added;
        }

        
        public bool GetFinished()
        {
            return isFinished;
        }
    }
}