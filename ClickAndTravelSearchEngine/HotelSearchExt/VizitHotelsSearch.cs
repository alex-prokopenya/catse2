using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.MasterTour;

using System.Data;
using System.Data.SqlClient;

using System.Configuration;

using Jayrock.Json.Conversion;
using Jayrock.Json;
using ClickAndTravelSearchEngine.VizitMaster;
using System.Collections.Specialized;


namespace ClickAndTravelSearchEngine.HotelSearchExt
{
    public class VizitHotelsSearch: IHotelExt
    {
        private string searchId = "";

        public static readonly string id_prefix = "ve_";

        private static Decimal VIZIT_COEF = Convert.ToDecimal(ConfigurationManager.AppSettings["VizitMargin"]);

        private Hotel[] FindedHotels = null;
        private Room FindedRoom = null;

        private int cityId          = 0;
        private DateTime startDate  = DateTime.MinValue;
        private DateTime endDate    = DateTime.MinValue;
        private int[] stars         = new int[0];
        private int[] pansions      = new int[0];

        private RequestRoom room  = null;
        private bool added = false;

        private Dictionary<int, int[]> pansionsGroups = new Dictionary<int, int[]>();
        private Dictionary<int, int> pansionsLib = new Dictionary<int, int>();

        private Dictionary<int, int> citiesLib = new Dictionary<int, int>();

        private long HOTELS_RESULTS_LIFETIME = 0;

        public static void ApplyConfig(NameValueCollection settings, string partnerCode = "")
        {
            VIZIT_COEF = Convert.ToDecimal(settings["VizitMargin"]);
            Logger.WriteToLog(partnerCode + " config applied to vizithotels");
        }

        private VizitHotelsSearch()
        {
        
        }

        public VizitHotelsSearch(int CityId, DateTime StartDate, DateTime EndDate, int[] Stars, int[] Pansions, RequestRoom Room, string SearchId, long RESULTS_LIFETIME)
        {
            //парсим группы по питанию из конфига
            for (int i = 0; i < 7; i++)
            {
                string ids = ConfigurationManager.AppSettings["VizitPansionGroup" + i];

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

            InitCityId(CityId);

            this.stars = Stars;
            if (stars.Length == 0)
                this.stars = new int[] { 1, 2, 3, 4, 5 };

            this.pansions = Pansions;

            List<int> pansionsList = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                if ((this.pansions.ToList().Contains(i)) || (this.pansions.Length == 0))
                    pansionsList.AddRange(pansionsGroups[i]);
            }

            this.pansions = pansionsList.ToArray();

            this.startDate = StartDate;
            this.endDate = EndDate;
            this.room = Room;
            this.searchId = SearchId;

            this.HOTELS_RESULTS_LIFETIME = RESULTS_LIFETIME;
        }

        //проставляем id города
        private void InitCityId(int CityId)
        {
            #region Города
            #region Чехия
            citiesLib.Add(274, 3);//прага
            citiesLib.Add(267, 157);//вары
            citiesLib.Add(270, 220);//м.лазне
            citiesLib.Add(279, 240);//ф.лазне
            citiesLib.Add(262, 231);//брно
            citiesLib.Add(981, 221);//подебр
            citiesLib.Add(979, 253);//тепл
            citiesLib.Add(268, 255);//либерец
            citiesLib.Add(281, 227);//ч. крумлов
            citiesLib.Add(980, 222);//яхимов
            citiesLib.Add(283, 594);//ш.млын
            #endregion
            #region Австрия
            citiesLib.Add(22, 35); //вена
            #endregion
            #region Германия
            citiesLib.Add(395, 207);  //берлин
            citiesLib.Add(683, 162);  //мюнхен
            citiesLib.Add(374, 295);  //баден
            #endregion
            #region Венгрия
            citiesLib.Add(1194, 568); //Будапешт
            #endregion
            #region Венгрия
            citiesLib.Add(1144, 445); //париж
            #endregion
            #endregion

            this.cityId = CityId;

            if (citiesLib.ContainsKey(CityId))
                this.cityId = citiesLib[CityId];
        }

        //СОЗДАЕМ ПУСТУЮ ТАБЛИЦУ ДЛЯ ЦЕН
        private DataTable createPriceTable()
        {
            DataTable  res = new DataTable();
            DataColumn pr = new DataColumn("price", Type.GetType("System.Decimal"));
            DataColumn ht = new DataColumn("hotel", Type.GetType("System.Int32"));
            DataColumn room = new DataColumn("room", Type.GetType("System.Int32"));
            DataColumn pansion = new DataColumn("pansion", Type.GetType("System.Int32"));
            DataColumn pansname = new DataColumn("pansname", Type.GetType("System.String"));
            DataColumn hotelname = new DataColumn("hotelname", Type.GetType("System.String"));
            DataColumn dateFrom = new DataColumn("dateFrom", Type.GetType("System.String"));
            DataColumn dateTo = new DataColumn("dateTo", Type.GetType("System.String"));
            DataColumn chDateIn = new DataColumn("chDateIn", Type.GetType("System.String"));
            DataColumn chDateOut = new DataColumn("chDateOut", Type.GetType("System.String"));
            DataColumn longmin = new DataColumn("longmin", Type.GetType("System.Int32"));
            DataColumn longmax = new DataColumn("longmax", Type.GetType("System.Int32"));
            DataColumn week = new DataColumn("week", Type.GetType("System.String"));
            DataColumn roomname = new DataColumn("roomname", Type.GetType("System.String"));
            DataColumn catname = new DataColumn("catname", Type.GetType("System.String"));
            res.Columns.AddRange(new DataColumn[] { pr, ht, room, pansion, pansname, hotelname, dateFrom, dateTo, roomname, catname, chDateIn, chDateOut, longmin, longmax, week });
            return res;
        }


        private DataTable getTotalPriceTable(DateTime dateFrom, DateTime dateTo, int cityKey, 
                               int adlCount, int childCount, int age1, int age2, int[] pansions, int[] stars)
        {
            DataTable dt = createPriceTable();
            SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["VizitConnectionString"]);


            DateTime date1 = System.Convert.ToDateTime(dateFrom);
            DateTime date2 = System.Convert.ToDateTime(dateTo);

            conn.Open();
            SqlDataReader reader = null;

            SqlCommand com = new SqlCommand("pr_ePricesExt", conn);//отдает:  price | hotel | room(key) | pansion(key) | pansname | dateFrom | dateTo | roomname | catname

            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add(new SqlParameter("@cityKey", cityKey));
            com.Parameters.Add(new SqlParameter("@tourTip", 13));
            com.Parameters.Add(new SqlParameter("@adlCount", adlCount));
            com.Parameters.Add(new SqlParameter("@chCount", childCount));
            com.Parameters.Add(new SqlParameter("@chAge1", age1));
            com.Parameters.Add(new SqlParameter("@chAge2", age2));
            com.Parameters.Add(new SqlParameter("@dateFrom", dateFrom.ToString("yyyy-MM-dd")));
            com.Parameters.Add(new SqlParameter("@dateTo", dateTo.ToString("yyyy-MM-dd")));
            com.Parameters.Add(new SqlParameter("@days", ((TimeSpan)(date2 - date1)).Days));

            com.Parameters.Add(new SqlParameter("@pansions", string.Join(",", pansions)));
            com.Parameters.Add(new SqlParameter("@stars", string.Join("*,", stars) + "*"));
            com.CommandTimeout = 3000;
            reader = com.ExecuteReader();
            while (reader.Read())
            {
                decimal price = System.Convert.ToDecimal(reader["price"]);
                int hotel = System.Convert.ToInt32(reader["hotel"]);
                int room = System.Convert.ToInt32(reader["room"]);
                int pansion = System.Convert.ToInt32(reader["pansion"]);
                string pansname = System.Convert.ToString(reader["pansname"]);
                string hotelname = "";
                string strDateFrom = System.Convert.ToString(reader["dateFrom"]);
                string strDateTo = System.Convert.ToString(reader["dateTo"]);
                string roomname = System.Convert.ToString(reader["roomname"]);
                string catname = System.Convert.ToString(reader["catname"]);

                string ch_datefrom = System.Convert.ToString(reader["ch_datefrom"]);
                string ch_dateto = System.Convert.ToString(reader["ch_dateto"]);

                int longmin = System.Convert.ToInt32(reader["longmin"]);
                int longmax = System.Convert.ToInt32(reader["long"]);

                string weekdays = System.Convert.ToString(reader["weekdays"]);

                dt.Rows.Add(new object[] { price, hotel, room, pansion, pansname, hotelname, strDateFrom, strDateTo, roomname, catname, ch_datefrom, ch_dateto, longmin, longmax, weekdays });
            }
            return dt;
        }


        private DataTable getHotelPriceTable(DateTime dateFrom, DateTime dateTo,
                                           int adlCount, int childCount, int age1, 
                                           int age2, int[] pansions, int hotelId)
        {
            hotelId = ConvertHotelId(hotelId);

            DataTable dt = createPriceTable();
            SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["VizitConnectionString"]);

            DateTime date1 = System.Convert.ToDateTime(dateFrom);
            DateTime date2 = System.Convert.ToDateTime(dateTo);

            conn.Open();
            SqlDataReader reader = null;

            SqlCommand com = new SqlCommand("pr_hotelPricesExt", conn);//отдает:  price | hotel | room(key) | pansion(key) | pansname | dateFrom | dateTo | roomname | catname

            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add(new SqlParameter("@hotelKey", hotelId));
            com.Parameters.Add(new SqlParameter("@tourTip", 13));
            com.Parameters.Add(new SqlParameter("@adlCount", adlCount));
            com.Parameters.Add(new SqlParameter("@chCount", childCount));
            com.Parameters.Add(new SqlParameter("@chAge1", age1));
            com.Parameters.Add(new SqlParameter("@chAge2", age2));
            com.Parameters.Add(new SqlParameter("@dateFrom", dateFrom.ToString("yyyy-MM-dd")));
            com.Parameters.Add(new SqlParameter("@dateTo", dateTo.ToString("yyyy-MM-dd")));
            com.Parameters.Add(new SqlParameter("@days", ((TimeSpan)(date2 - date1)).Days));
            com.Parameters.Add(new SqlParameter("@pansions", string.Join(",", pansions)));
            
            com.CommandTimeout = 3000;
            reader = com.ExecuteReader();
            while (reader.Read())
            {
                decimal price = System.Convert.ToDecimal(reader["price"]);
                int hotel = System.Convert.ToInt32(reader["hotel"]);
                int room = System.Convert.ToInt32(reader["room"]);
                int pansion = System.Convert.ToInt32(reader["pansion"]);
                string pansname = System.Convert.ToString(reader["pansname"]);
                string hotelname = "";
                string strDateFrom = System.Convert.ToString(reader["dateFrom"]);
                string strDateTo = System.Convert.ToString(reader["dateTo"]);
                string roomname = System.Convert.ToString(reader["roomname"]);
                string catname = System.Convert.ToString(reader["catname"]);

                string ch_datefrom = System.Convert.ToString(reader["ch_datefrom"]);
                string ch_dateto = System.Convert.ToString(reader["ch_dateto"]);

                int longmin = System.Convert.ToInt32(reader["longmin"]);
                int longmax = System.Convert.ToInt32(reader["long"]);

                string weekdays = System.Convert.ToString(reader["weekdays"]);

                dt.Rows.Add(new object[] { price, hotelId, room, pansion, pansname, hotelname, strDateFrom, strDateTo, roomname, catname, ch_datefrom, ch_dateto, longmin, longmax, weekdays });
            }
            return dt;
        }

        private static int ConvertHotelId(int hotelId)
        {

            Dictionary<int, int> hotelsLib = new Dictionary<int, int>();
            #region links

            //прага
            hotelsLib.Add(10006143, 1062);
            hotelsLib.Add(10006146, 1140);
            hotelsLib.Add(10006150, 1060);
            hotelsLib.Add(10006156, 1401);
            hotelsLib.Add(10006157, 1134);
            hotelsLib.Add(10006165, 1638);
            hotelsLib.Add(10006167, 1059);
            hotelsLib.Add(10006170, 1038);
            hotelsLib.Add(10006173, 1920);
            hotelsLib.Add(10006174, 1128);
            hotelsLib.Add(10006175, 2145);
            hotelsLib.Add(10006176, 1367);
            hotelsLib.Add(10006177, 1057);
            hotelsLib.Add(10006178, 1056);
            hotelsLib.Add(10006180, 1055);
            hotelsLib.Add(10006181, 1356);
            hotelsLib.Add(10006182, 1119);
            hotelsLib.Add(10006185, 1384);
            hotelsLib.Add(10006191, 1037);
            hotelsLib.Add(10006193, 1656);
            hotelsLib.Add(10006194, 1108);
            hotelsLib.Add(10006198, 1052);
            hotelsLib.Add(10006199, 1397);
            hotelsLib.Add(10006202, 1281);
            hotelsLib.Add(10006212, 1530);
            hotelsLib.Add(10006216, 1396);
            hotelsLib.Add(10006221, 2112);
            hotelsLib.Add(10006222, 1046);
            hotelsLib.Add(10006223, 1096);
            hotelsLib.Add(10006226, 3398);
            hotelsLib.Add(10006228, 1561);
            hotelsLib.Add(10006230, 1090);
            hotelsLib.Add(10006231, 3392);
            hotelsLib.Add(10006233, 1381);
            hotelsLib.Add(10006235, 1084);
            hotelsLib.Add(10006236, 1045);
            hotelsLib.Add(10006238, 1610);
            hotelsLib.Add(10006240, 1044);
            hotelsLib.Add(10006241, 1749);
            hotelsLib.Add(10024919, 2224);
            hotelsLib.Add(10024931, 2009);
            hotelsLib.Add(10026184, 1604);
            hotelsLib.Add(10026190, 1637);
            hotelsLib.Add(10026261, 1250);
            hotelsLib.Add(10026270, 4365);
            hotelsLib.Add(10026452, 1543);
            hotelsLib.Add(10026468, 1721);
            hotelsLib.Add(10026596, 1703);
            hotelsLib.Add(10027365, 1669);
            hotelsLib.Add(10029009, 1472);
            hotelsLib.Add(10029643, 1864);
            hotelsLib.Add(10029924, 1583);
            hotelsLib.Add(10030070, 1729);
            hotelsLib.Add(10031758, 4239);
            hotelsLib.Add(10031762, 1777);
            hotelsLib.Add(10031764, 1535);
            hotelsLib.Add(10031765, 1458);
            hotelsLib.Add(10031768, 1547);
            hotelsLib.Add(10031770, 1719);
            hotelsLib.Add(10033890, 2136);
            hotelsLib.Add(10033891, 3329);
            hotelsLib.Add(10036546, 1873);
            hotelsLib.Add(10037887, 1895);
            hotelsLib.Add(10039368, 1364);
            hotelsLib.Add(10040421, 4248);
            hotelsLib.Add(10040424, 2069);
            hotelsLib.Add(10040428, 4092);
            hotelsLib.Add(10041084, 1919);
            hotelsLib.Add(10041916, 1914);
            hotelsLib.Add(10041921, 2170);
            hotelsLib.Add(10045720, 2047);
            hotelsLib.Add(10045722, 2189);
            hotelsLib.Add(10047721, 3293);
            hotelsLib.Add(10047724, 2045);
            hotelsLib.Add(10047727, 1407);
            hotelsLib.Add(10047728, 1408);
            hotelsLib.Add(10047742, 1555);
            hotelsLib.Add(10048404, 2092);
            hotelsLib.Add(10052345, 2084);
            hotelsLib.Add(10065397, 1357);
            hotelsLib.Add(10065401, 1106);
            hotelsLib.Add(10065402, 1760);
            hotelsLib.Add(10065408, 3786);
            hotelsLib.Add(10065412, 1375);
            hotelsLib.Add(10065614, 1655);
            hotelsLib.Add(10068583, 2113);
            hotelsLib.Add(10074326, 1846);
            hotelsLib.Add(10074363, 2146);
            hotelsLib.Add(10077209, 3275);
            hotelsLib.Add(10083474, 1937);
            hotelsLib.Add(10089380, 1504);
            hotelsLib.Add(10113074, 1865);
            hotelsLib.Add(10113183, 1092);
            hotelsLib.Add(10115015, 1457);
            hotelsLib.Add(10133424, 1714);
            hotelsLib.Add(10133425, 1509);
            hotelsLib.Add(10133430, 3393);
            hotelsLib.Add(10133431, 3519);
            hotelsLib.Add(10143042, 4247);
            hotelsLib.Add(10171557, 2075);
            hotelsLib.Add(10171608, 1495);
            hotelsLib.Add(10196428, 1740);
            hotelsLib.Add(10197004, 1593);
            hotelsLib.Add(10210457, 1520);
            hotelsLib.Add(10266687, 2408);
            hotelsLib.Add(10266734, 1683);
            hotelsLib.Add(10285730, 1103);
            hotelsLib.Add(10285791, 1107);
            hotelsLib.Add(10285897, 3334);
            hotelsLib.Add(10285933, 3556);
            hotelsLib.Add(10305084, 1400);
            hotelsLib.Add(10438604, 4394);
            hotelsLib.Add(10006130, 1717);


            //карловы вары 
            hotelsLib.Add(10204308, 819);
            hotelsLib.Add(10041079, 821);
            hotelsLib.Add(10006079, 968);
            hotelsLib.Add(10126863, 981);
            hotelsLib.Add(10071525, 1016);
            hotelsLib.Add(10305053, 1028);
            hotelsLib.Add(10006083, 1029);
            hotelsLib.Add(10049813, 1031);
            hotelsLib.Add(10285799, 1183);
            hotelsLib.Add(10006082, 1204);
            hotelsLib.Add(10191929, 1211);
            hotelsLib.Add(10191930, 1263);
            hotelsLib.Add(10041080, 1432);
            hotelsLib.Add(10172287, 1438);
            hotelsLib.Add(10092776, 1505);
            hotelsLib.Add(10231483, 1518);
            hotelsLib.Add(10126859, 1523);
            hotelsLib.Add(10231277, 1556);
            hotelsLib.Add(10126861, 3516);
            hotelsLib.Add(10231580, 3616);

            //marianskie
            hotelsLib.Add(10006088, 1354);
            hotelsLib.Add(10285948, 1927);
            hotelsLib.Add(10006089, 2273);
            hotelsLib.Add(10231506, 3551);
            hotelsLib.Add(10191931, 4015);
            hotelsLib.Add(10359166, 4082);


            #endregion

            if (hotelsLib.ContainsKey(hotelId))
                hotelId = hotelsLib[hotelId];
            else
                hotelId = 0;

            return hotelId;
        }

        private Dictionary<int, RoomVariant> ComposePrices(DataTable tbl, DateTime dateFrom, DateTime dateTo, string id_start)
        {
            ///продолжительность проживания в ночах
            int nigthsLong = (dateTo - dateFrom).Days;

            //заготовка для результатов
            Dictionary<int, Dictionary<string, Dictionary<string, decimal>>> result = new Dictionary<int, Dictionary<string, Dictionary<string, decimal>>>();

            Dictionary<int, string  > min_price_variant = new Dictionary<int, string>();
            Dictionary<int, decimal > min_price_value   = new Dictionary<int, decimal>();

            Dictionary<string, RoomVariant> room_variant_dict = new Dictionary<string, RoomVariant>();

            //проходимся по всем записям в таблице
            foreach (DataRow row in tbl.Rows)
            {
                //берем айдишник отеля
                int _hotel = Convert.ToInt32(row["hotel"]);

                //если отель еще не просматривался
                if (!result.ContainsKey(_hotel))
                    result[_hotel] = new Dictionary<string, Dictionary<string, decimal>>(); //добавим его в справочник

                //берем информацию о комнате и питании
                int _roomkey = Convert.ToInt32(row["room"]);
                int _pnkey = Convert.ToInt32(row["pansion"]);
                string variant_code = _roomkey+"_" +_pnkey;

                //если такие еще на рассматривались
                if (!result[_hotel].ContainsKey(variant_code))
                    result[_hotel][variant_code] =  new Dictionary<string, decimal>(); //добавляем в справочник

                if (!room_variant_dict.ContainsKey(variant_code))// == null)
                {
                    room_variant_dict[variant_code] = new RoomVariant()
                    {
                        PansionGroupId = _pnkey,
                        PansionTitle = Convert.ToString(row["pansname"]),
                        RoomCategory = Convert.ToString(row["catname"]),
                        RoomTitle = Convert.ToString(row["roomname"])
                    };
                }

                //берем даты проживания
                DateTime _dateTo = Convert.ToDateTime(row["dateTo"]);
                DateTime _dateFrom = Convert.ToDateTime(row["dateFrom"]);

                //...ограничения по дате заселения
                DateTime _chFrom = Convert.ToDateTime(row["chDateIn"]);
                DateTime _chTo = Convert.ToDateTime(row["chDateOut"]);

                _chFrom = (new DateTime(2013, 1, 1) > _chFrom) ? new DateTime(2013, 1, 1) : _chFrom;
                _chTo = (new DateTime(2013, 1, 1) > _chTo) ? new DateTime(2113, 1, 1) : _chTo;

                int _longmin = Convert.ToInt32(row["longmin"]);
                int _longmax = Convert.ToInt32(row["longmax"]);
                string _week = Convert.ToString(row["week"]);
               // _week = (_week == "") ? "1,2,3,4,5,6,7," : _week;

                decimal price = Convert.ToDecimal(row["price"]);

                DateTime curDate = dateFrom > _dateFrom ? dateFrom : _dateFrom;//c первой даты проживания или действия цены

                //проверяем дату заселения
                if ((dateFrom < _chFrom) || (dateFrom > _chTo)) continue;

                //проверяем продолжительность
                if (((nigthsLong < _longmin) && (_longmin != 0)) || ((nigthsLong > _longmax) && (_longmax != 0))) continue;

                for (int i = 0; i < nigthsLong; i++)
                { 
                    //проверяем дату проживания
                    if((curDate > _dateTo) || (curDate > dateTo)) break;

                    //проверяем день недели
                    if ((_week == "") || (_week.Contains("" + ((int)curDate.DayOfWeek + 1))))
                        result[_hotel][variant_code][curDate.ToString()] = price;

                    curDate = curDate.AddDays(1);
                }
            }

            Logger.WriteToLog("results composed " + result.Count);
            //проверяем, все ли даты закрыты ценой. выбираем самый дешевый вариант

            Dictionary<int, RoomVariant> varsResult = new Dictionary<int, RoomVariant>();

            foreach (int hotelId in result.Keys)
            { 
                min_price_value[hotelId] = 1000000;
                min_price_variant[hotelId] = "no_code";

                foreach(string variant_code  in result[hotelId].Keys)
                {
                    if (result[hotelId][variant_code].Count == nigthsLong)
                    {
                        decimal tot_price = result[hotelId][variant_code].Values.Sum();

                        string id = id_start + variant_code;

                        if (min_price_value[hotelId] > tot_price)
                        {
                            min_price_value[hotelId] = tot_price;

                            varsResult[hotelId] = new RoomVariant()
                            {
                                PansionGroupId = pansionsLib[ room_variant_dict[variant_code].PansionGroupId ], //преобразуем полученный айдишник питания в код группы питания
                                PansionTitle = room_variant_dict[variant_code].PansionTitle,
                                RoomCategory = room_variant_dict[variant_code].RoomCategory,
                                RoomTitle = room_variant_dict[variant_code].RoomTitle,
                                Prices = new KeyValuePair<string,decimal>[]{new KeyValuePair<string,decimal>("EUR", tot_price*VIZIT_COEF)},
                                VariantId = id 
                            };
                        }
                    }
                }
            }

            result = null;
            min_price_value = null;
            room_variant_dict = null;
            GC.Collect();

            return varsResult;
        }

        private RoomVariant[] ComposeRoomPrices(DataTable tbl, DateTime dateFrom, DateTime dateTo, string id_start, int hotelId)
        {
            ///продолжительность проживания в ночах
            int nigthsLong = (dateTo - dateFrom).Days;

            //заготовка для результатов
            Dictionary<string, Dictionary<string, decimal>> result = new Dictionary<string, Dictionary<string, decimal>>();

            Dictionary<int, string> min_price_variant = new Dictionary<int, string>();
            Dictionary<int, decimal> min_price_value = new Dictionary<int, decimal>();

            Dictionary<string, RoomVariant> room_variant_dict = new Dictionary<string, RoomVariant>();

            //проходимся по всем записям в таблице
            foreach (DataRow row in tbl.Rows)
            {
                //берем информацию о комнате и питании
                int _roomkey = Convert.ToInt32(row["room"]);
                int _pnkey = Convert.ToInt32(row["pansion"]);
                string variant_code = _roomkey + "_" + _pnkey;

                //если такие еще на рассматривались
                if (!result.ContainsKey(variant_code))
                    result[variant_code] = new Dictionary<string, decimal>(); //добавляем в справочник

                if (!room_variant_dict.ContainsKey(variant_code))// == null)
                {
                    room_variant_dict[variant_code] = new RoomVariant()
                    {
                        PansionGroupId = _pnkey,
                        PansionTitle = Convert.ToString(row["pansname"]),
                        RoomCategory = Convert.ToString(row["catname"]),
                        RoomTitle = Convert.ToString(row["roomname"])
                    };
                }

                //берем даты проживания
                DateTime _dateTo = Convert.ToDateTime(row["dateTo"]);
                DateTime _dateFrom = Convert.ToDateTime(row["dateFrom"]);

                //...ограничения по дате заселения
                DateTime _chFrom = Convert.ToDateTime(row["chDateIn"]);
                DateTime _chTo = Convert.ToDateTime(row["chDateOut"]);

                _chFrom = (new DateTime(2013, 1, 1) > _chFrom) ? new DateTime(2013, 1, 1) : _chFrom;
                _chTo = (new DateTime(2013, 1, 1) > _chTo) ? new DateTime(2113, 1, 1) : _chTo;

                int _longmin = Convert.ToInt32(row["longmin"]);
                int _longmax = Convert.ToInt32(row["longmax"]);
                string _week = Convert.ToString(row["week"]);

                decimal price = Convert.ToDecimal(row["price"]);

                DateTime curDate = dateFrom > _dateFrom ? dateFrom : _dateFrom;//c первой даты проживания или действия цены

                //проверяем дату заселения
                if ((dateFrom < _chFrom) || (dateFrom > _chTo)) continue;

                //проверяем продолжительность
                if (((nigthsLong < _longmin) && (_longmin != 0)) || ((nigthsLong > _longmax) && (_longmax != 0))) continue;

                for (int i = 0; i < nigthsLong; i++)
                {
                    //проверяем дату проживания
                    if ((curDate > _dateTo) || (curDate > dateTo)) break;

                    //проверяем день недели
                    if ((_week == "") || (_week.Contains("" + ((int)curDate.DayOfWeek + 1))))
                        result[variant_code][curDate.ToString()] = price;

                    curDate = curDate.AddDays(1);
                }
            }

            List< RoomVariant> varsResult = new List< RoomVariant>();

            foreach (string variant_code in result.Keys)
            {
                    if (result[variant_code].Count == nigthsLong)
                    {
                        decimal tot_price = result[variant_code].Values.Sum();

                        string id =  id_start + variant_code;

                            varsResult.Add( new RoomVariant()
                            {
                                PansionGroupId = pansionsLib[room_variant_dict[variant_code].PansionGroupId], //преобразуем полученный айдишник питания в код группы питания
                                PansionTitle = room_variant_dict[variant_code].PansionTitle,
                                RoomCategory = room_variant_dict[variant_code].RoomCategory,
                                RoomTitle = room_variant_dict[variant_code].RoomTitle,
                                Prices = new KeyValuePair<string, decimal>[] { new KeyValuePair<string, decimal>("EUR", tot_price*VIZIT_COEF) },
                                VariantId = id
                            });
                    }
            }
            return varsResult.ToArray();
        }

        public HotelPenalties GetHotelPenalties(int hotelId, string variantId)
        {
            string redis_key = this.searchId + "_" + hotelId + "_" + variantId + "_penalties";

            string redis_hash = RedisHelper.GetString(redis_key);

            if ((redis_hash != null) && (redis_hash.Length > 0))
                return JsonConvert.Import<HotelPenalties>(redis_hash);

            Dictionary<int, string> change_penalties = new Dictionary<int, string>();
            change_penalties.Add(10,"10%");
            change_penalties.Add(5, "20%");

            Dictionary<int, string> cancel_penalties = new Dictionary<int, string>();
            cancel_penalties.Add(10, "10%");
            cancel_penalties.Add(5, "30%");
            cancel_penalties.Add(3, "50%");
            cancel_penalties.Add(1, "100%");

            List<KeyValuePair<DateTime, string>> chPen = new List<KeyValuePair<DateTime, string>>();
            List<KeyValuePair<DateTime, string>> cansPen = new List<KeyValuePair<DateTime, string>>();

            foreach (int days in change_penalties.Keys)
                chPen.Add(new KeyValuePair<DateTime, string>(this.startDate.AddDays(-1 * days), change_penalties[days]));

            foreach (int days in cancel_penalties.Keys)
                cansPen.Add(new KeyValuePair<DateTime, string>(this.startDate.AddDays(-1 * days), cancel_penalties[days]));

            HotelPenalties res = new HotelPenalties() { VariantId = variantId, CancelingPenalties = cansPen.ToArray(), ChangingPenalties = chPen.ToArray() };

            RedisHelper.SetString(redis_key, JsonConvert.ExportToString(res), new TimeSpan(this.HOTELS_RESULTS_LIFETIME * 2));

            return res;
        }

        public HotelVerifyResult VerifyHotelVariant(int hotelId, string variantId)
        {
            #region результат проверки из кэша
            string redis_verify_key = this.searchId + "_" + hotelId + "_" + variantId + "_verify";

            string redis_hash = RedisHelper.GetString(redis_verify_key);

            if ((redis_hash != null) && (redis_hash.Length > 0))
                return JsonConvert.Import<HotelVerifyResult>(redis_hash);
            #endregion


            string redis_key = this.searchId + "_" + hotelId + "_rooms_for_verify";
            redis_hash = RedisHelper.GetString(redis_key);

            Room room = null;

            //если есть в кэше, подымаем из кэша
            if ((redis_hash != null) && (redis_hash.Length > 0))
                room = JsonConvert.Import<Room>(redis_hash);
            else //иначе -- ищем заново
            {
                this.FindRooms(hotelId);

                room = this.GetRoom();
            }

           
            foreach (RoomVariant rv in room.Variants)
               if (rv.VariantId == variantId)
               {
                   HotelVerifyResult res = new HotelVerifyResult()
                   {
                            VariantId = variantId,
                            IsAvailable = true,
                            Prices = rv.Prices
                   };

                   RedisHelper.SetString(redis_verify_key, JsonConvert.ExportToString(res), new TimeSpan(HOTELS_RESULTS_LIFETIME / 10));

                   return res;
              }

            return null;
        }

        public void FindHotels()
        {
            try
            {
                //конвертируем даты
                DateTime date1 = this.startDate, date2 = this.endDate;

                //делаем поиск по первой комнате        
                DataTable dt1 = getTotalPriceTable(this.startDate, this.endDate, this.cityId,
                                                   this.room.Adults, this.room.Children,
                                                   this.room.Children > 0 ? this.room.ChildrenAges[0] : 0,
                                                   this.room.Children > 1 ? this.room.ChildrenAges[1] : 0,
                                                   this.pansions, this.stars);

                
                Dictionary<int, RoomVariant> variants1 = ComposePrices(dt1, this.startDate, this.endDate, id_prefix );

                List<Hotel> hotels = new List<Hotel>();

                //получаем курсы валют
                KeyValuePair<string, decimal>[] _courses = null;

                if (variants1.Count > 0)
                    _courses = MtHelper.GetCourses(MtHelper.rate_codes, variants1[variants1.Keys.ToArray<int>()[0]].Prices[0].Key, DateTime.Today);

                Dictionary<int, int> hotelsLib = new Dictionary<int, int>();
                #region Links
                //прага
                hotelsLib.Add(1717, 10006130);
                hotelsLib.Add(1062, 10006143);
                hotelsLib.Add(1140, 10006146);
                hotelsLib.Add(3289, 10006146);
                hotelsLib.Add(1060, 10006150);
                hotelsLib.Add(1401, 10006156);
                hotelsLib.Add(1134, 10006157);
                hotelsLib.Add(1638, 10006165);
                hotelsLib.Add(1059, 10006167);
                hotelsLib.Add(1038, 10006170);
                hotelsLib.Add(1920, 10006173);
                hotelsLib.Add(1128, 10006174);
                hotelsLib.Add(2145, 10006175);
                hotelsLib.Add(1367, 10006176);
                hotelsLib.Add(1057, 10006177);
                hotelsLib.Add(1056, 10006178);
                hotelsLib.Add(1055, 10006180);
                hotelsLib.Add(1356, 10006181);
                hotelsLib.Add(1119, 10006182);
                hotelsLib.Add(1384, 10006185);
                hotelsLib.Add(1037, 10006191);
                hotelsLib.Add(1656, 10006193);
                hotelsLib.Add(1108, 10006194);
                hotelsLib.Add(1052, 10006198);
                hotelsLib.Add(1397, 10006199);
                hotelsLib.Add(1281, 10006202);
                hotelsLib.Add(1530, 10006212);
                hotelsLib.Add(1396, 10006216);
                hotelsLib.Add(2112, 10006221);
                hotelsLib.Add(1046, 10006222);
                hotelsLib.Add(1096, 10006223);
                hotelsLib.Add(3285, 10006223);
                hotelsLib.Add(3398, 10006226);
                hotelsLib.Add(1561, 10006228);
                hotelsLib.Add(1090, 10006230);
                hotelsLib.Add(3392, 10006231);
                hotelsLib.Add(1381, 10006233);
                hotelsLib.Add(1084, 10006235);
                hotelsLib.Add(1045, 10006236);
                hotelsLib.Add(1610, 10006238);
                hotelsLib.Add(1044, 10006240);
                hotelsLib.Add(3294, 10006240);
                hotelsLib.Add(1749, 10006241);
                hotelsLib.Add(2224, 10024919);
                hotelsLib.Add(2009, 10024931);
                hotelsLib.Add(2008, 10024931);
                hotelsLib.Add(2007, 10024931);
                hotelsLib.Add(1138, 10024931);
                hotelsLib.Add(1604, 10026184);
                hotelsLib.Add(1637, 10026190);
                hotelsLib.Add(1250, 10026261);
                hotelsLib.Add(4365, 10026270);
                hotelsLib.Add(1543, 10026452);
                hotelsLib.Add(1721, 10026468);
                hotelsLib.Add(1703, 10026596);
                hotelsLib.Add(1669, 10027365);
                hotelsLib.Add(1472, 10029009);
                hotelsLib.Add(1864, 10029643);
                hotelsLib.Add(1583, 10029924);
                hotelsLib.Add(1729, 10030070);
                hotelsLib.Add(4239, 10031758);
                hotelsLib.Add(1777, 10031762);
                hotelsLib.Add(1535, 10031764);
                hotelsLib.Add(1458, 10031765);
                hotelsLib.Add(1547, 10031768);
                hotelsLib.Add(1719, 10031770);
                hotelsLib.Add(2136, 10033890);
                hotelsLib.Add(3286, 10033890);
                hotelsLib.Add(3329, 10033891);
                hotelsLib.Add(1873, 10036546);
                hotelsLib.Add(1895, 10037887);
                hotelsLib.Add(1364, 10039368);
                hotelsLib.Add(4248, 10040421);
                hotelsLib.Add(2069, 10040424);
                hotelsLib.Add(4092, 10040428);
                hotelsLib.Add(1919, 10041084);
                hotelsLib.Add(1914, 10041916);
                hotelsLib.Add(2170, 10041921);
                hotelsLib.Add(2047, 10045720);
                hotelsLib.Add(2189, 10045722);
                hotelsLib.Add(1680, 10047721);
                hotelsLib.Add(3293, 10047721);
                hotelsLib.Add(2045, 10047724);
                hotelsLib.Add(1407, 10047727);
                hotelsLib.Add(1408, 10047728);
                hotelsLib.Add(3301, 10047728);
                hotelsLib.Add(1555, 10047742);
                hotelsLib.Add(2092, 10048404);
                hotelsLib.Add(2084, 10052345);
                hotelsLib.Add(1357, 10065397);
                hotelsLib.Add(1106, 10065401);
                hotelsLib.Add(1760, 10065402);
                hotelsLib.Add(3786, 10065408);
                hotelsLib.Add(1375, 10065412);
                hotelsLib.Add(1655, 10065614);
                hotelsLib.Add(3283, 10065614);
                hotelsLib.Add(2113, 10068583);
                hotelsLib.Add(1846, 10074326);
                hotelsLib.Add(2146, 10074363);
                hotelsLib.Add(3275, 10077209);
                hotelsLib.Add(1937, 10083474);
                hotelsLib.Add(1504, 10089380);
                hotelsLib.Add(1597, 10089380);
                hotelsLib.Add(1865, 10113074);
                hotelsLib.Add(1092, 10113183);
                hotelsLib.Add(1457, 10115015);
                hotelsLib.Add(1714, 10133424);
                hotelsLib.Add(1509, 10133425);
                hotelsLib.Add(3393, 10133430);
                hotelsLib.Add(3519, 10133431);
                hotelsLib.Add(4247, 10143042);
                hotelsLib.Add(2075, 10171557);
                hotelsLib.Add(1495, 10171608);
                hotelsLib.Add(1740, 10196428);
                hotelsLib.Add(3300, 10196428);
                hotelsLib.Add(1593, 10197004);
                hotelsLib.Add(1520, 10210457);
                hotelsLib.Add(2408, 10266687);
                hotelsLib.Add(1683, 10266734);
                hotelsLib.Add(1103, 10285730);
                hotelsLib.Add(1107, 10285791);
                hotelsLib.Add(3284, 10285791);
                hotelsLib.Add(3334, 10285897);
                hotelsLib.Add(3556, 10285933);
                hotelsLib.Add(1400, 10305084);
                hotelsLib.Add(4394, 10438604);

                //карловы
                hotelsLib.Add(819, 10204308);
                hotelsLib.Add(821, 10041079);
                hotelsLib.Add(968, 10006079);
                hotelsLib.Add(981, 10126863);
                hotelsLib.Add(1016, 10071525);
                hotelsLib.Add(1028, 10305053);
                hotelsLib.Add(1029, 10006083);
                hotelsLib.Add(1031, 10049813);
                hotelsLib.Add(1183, 10285799);
                hotelsLib.Add(1204, 10006082);
                hotelsLib.Add(1211, 10191929);
                hotelsLib.Add(1263, 10191930);
                hotelsLib.Add(1432, 10041080);
                hotelsLib.Add(1438, 10172287);
                hotelsLib.Add(1505, 10092776);
                hotelsLib.Add(1518, 10231483);
                hotelsLib.Add(1523, 10126859);
                hotelsLib.Add(1556, 10231277);
                hotelsLib.Add(3516, 10126861);
                hotelsLib.Add(3616, 10231580);

                //marianskie
                hotelsLib.Add(1354, 10006088);
                hotelsLib.Add(1927, 10285948);
                hotelsLib.Add(2273, 10006089);
                hotelsLib.Add(3551, 10231506);
                hotelsLib.Add(4015, 10191931);
                hotelsLib.Add(4082, 10359166);

                #endregion

                foreach (int hotelId in variants1.Keys)
                {
                    Hotel curHotel = new Hotel()
                    {
                        HotelId = hotelsLib.ContainsKey(hotelId) ? hotelsLib[hotelId] : hotelId,
                        Rooms = new Room[1]
                    };

                    variants1[hotelId].Prices = MtHelper.ApplyCourses(variants1[hotelId].Prices[0].Value, _courses);

                    curHotel.Rooms[0] = new Room(this.room)
                    {
                        Variants = new RoomVariant[] { variants1[hotelId] }
                    };

                    hotels.Add(curHotel);
                }
                Logger.WriteToLog("hotels finded " + hotels.Count + " rows");

                this.FindedHotels = hotels.ToArray();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                this.FindedHotels = new Hotel[0]; 
            }
        }

        public void FindRooms(object hotelId)
        {
            //поиск комнат по отелю в согласии с параметрами
            int hotelKey = Convert.ToInt32(hotelId);

            if (ConvertHotelId(hotelKey) == 0)
            {

                Logger.WriteToLog("no vizit rooms");
                this.FindedRoom = new Room() { Variants = new RoomVariant[0]};
                return;
            }

            try
            {
                //проверяем данные по количеству человек и возрасту детей
                //конвертируем даты
                DateTime date1 = this.startDate, date2 = this.endDate;

                //делаем поиск по первой комнате        
                DataTable dt1 = getHotelPriceTable(this.startDate, this.endDate,
                                                   this.room.Adults, this.room.Children,
                                                   this.room.Children > 0 ? this.room.ChildrenAges[0] : 0,
                                                   this.room.Children > 1 ? this.room.ChildrenAges[1] : 0,
                                                   this.pansions, hotelKey);

                RoomVariant[] variants1 = ComposeRoomPrices(dt1, this.startDate, this.endDate, id_prefix, hotelKey);


                List<Hotel> hotels = new List<Hotel>();
                KeyValuePair<string, decimal>[] _courses = null;

                if (variants1.Length > 0)
                    _courses = MtHelper.GetCourses(MtHelper.rate_codes, variants1[0].Prices[0].Key, DateTime.Today);

                this.FindedRoom = null;

                foreach (RoomVariant rv in variants1)
                    rv.Prices = MtHelper.ApplyCourses(rv.Prices[0].Value, _courses);

                this.FindedRoom = new Room(this.room)
                {
                    Variants =  variants1
                };

               
                string redis_key = this.searchId + "_" + hotelId + "_rooms_for_verify";

                RedisHelper.SetString(redis_key, JsonConvert.ExportToString(this.FindedRoom), new TimeSpan(HOTELS_RESULTS_LIFETIME / 10)); //!!!!!!!
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source );
                this.FindedRoom = new Room() { Variants = new RoomVariant[0] };
            }
        }

        public HotelBooking BookRoom(string searchId, int hotelId, BookRoom operatorRoom, List<int> operatorTurists)
        {
            try
            {
                var tempRooms = new List<VizitMaster.MyService>();
                var tempTurists = new List<VizitMaster.MyTurist>();

                BookRoom room = operatorRoom;

                HotelVerifyResult hvr = this.VerifyHotelVariant(hotelId, room.VariantId);

                if (hvr == null) throw new Exceptions.CatseException("not verified", 0);

                var booking = new HotelBooking()
                {
                    DateBegin = this.startDate.ToString("yyyy-MM-dd"),
                    NightsCnt = (this.endDate - this.startDate).Days,
                    PartnerBookId = "",
                    PartnerPrefix = id_prefix,
                    SearchId = searchId,
                    Prices = hvr.Prices,
                    Title = "some_title",
                    Turists = operatorTurists.ToArray()
                };

                string[] parts = room.VariantId.Split('_');
                
                #region  //добавляем комнаты
                tempRooms.Add(new VizitMaster.MyService() { 
                        dateBegin = this.startDate.ToString("yyyy-MM-dd"),
                        dateEnd = this.endDate.ToString("yyyy-MM-dd"),
                        agent_brutto = 0,
                        priceBrutto = 0,
                        priceNetto = 0,
                        room = 1,
                        svkey = 3,
                        is_vizit = "true",
                        key = ConvertHotelId(hotelId), //!
                        dopKey1 = Convert.ToInt32(parts[1]),
                        dopKey2 = Convert.ToInt32(parts[2])
                        
                });
                #endregion

                #region //добавляем туристов
                var turists = MtHelper.GetTuristsFromCache(operatorTurists.ToArray());
                foreach(var tst in turists)
                        tempTurists.Add( new VizitMaster.MyTurist() {
                                birthDate = tst.BirthDate.ToString("yyyy-MM-dd"),
                                citizenship = "ST" + tst.Citizenship,
                                fName = tst.FirstName,
                                gender = tst.Sex == 1 ? "M" :"F",
                                name = tst.Name,
                                passport_exp = tst.PassportDate.ToString("yyyy-MM-dd"),
                                passport_num = tst.PassportNum.Substring(2),
                                passport_ser = tst.PassportNum.Substring(0,2),
                                room = 1,
                                sName = tst.MiddleName == null ? "":tst.MiddleName
                            }
                );
                #endregion

                //бронирование комнаты
                var cl = new VizitMaster.MasterWebServiceFormSoapClient();

                var resp = cl.CreateDogovor( 
                    new VizitMaster.DogovorInfo() { 
                        phone = "8 (800) 555-54-33",
                        mail = "info@clickandtravel.ru",
                        manager = "click travel",
                        priceBrutto=10.0,
                        code = "code",

                        //авиабилеты
                        flights = new MyFlight [0],
                        //трансферы
                        transfers = new MyTransfer [0],
                        //визы
                        visas = new MyVisa [0],
                        services = tempRooms.ToArray(),
                        turists = tempTurists.ToArray()
                    }, 
                    "CT01", 
                    "db671de2b1f864321f0815a7a2d635df", 1, 1);

                if (!resp.Contains("Error"))
                {
                    booking.PartnerBookId = resp;

                    return booking;
                }
                else
                {
                    Logger.WriteToLog(resp);
                    throw new Exceptions.CatseException("cann't book ", 0);
                }
               
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " +ex.StackTrace + " " + ex.Source );
                throw ex;
            }
        }

        public Hotel[] GetHotels()
        {
            return this.FindedHotels;
        }

        public Room GetRoom()
        {
            return this.FindedRoom;
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
            return this.FindedHotels != null;
        }

        public string GetPrefix()
        {
            return id_prefix;
        }
    }
}