
<%@ WebHandler Language="C#" Class="ClickAndTravelSearchEngine.json_handler" %>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using System.Collections;
using Jayrock.Services;
using ClickAndTravelSearchEngine.Containers.Transfers;
using ClickAndTravelSearchEngine.Containers.CarRent;

using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.Responses;

namespace ClickAndTravelSearchEngine
{
    /// <summary>
    /// Summary description for json_handler
    /// </summary>
    public class json_handler : JsonRpcHandler
    {
        private SearchEngine search_engine = new SearchEngine();

	    public json_handler()
	    {
            
            JsonRpcDispatcherFactory.Current = s => new ClickAndTravelSearchEngine.JsonRpcDispatcher(s);
	    }
        
        #region private
        private static DateTime ParseDateFromJson(JsonArray arr)
        {
            string str_date = "";
            DateTime arg = DateTime.MinValue;
            if(arr.Length == 3)
            {
                str_date = arr[0] + "-" + arr[1] + "-" + arr[2];

                arg = DateTime.ParseExact(str_date, "yyyy-M-d", null);
            }
            else if (arr.Length == 5)
            {
                str_date = arr[0] + "-" + arr[1] + "-" + arr[2] + " " + arr[3] + ":" + arr[4];
                arg = DateTime.ParseExact(str_date, "yyyy-M-d HH:mm", null);
            }

            return arg; 
        }
        #endregion

        #region Курсы валют
        [JsonRpcMethod("get_courses")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"get_courses\",\"params\":[\"2013-07-10\"],\"id\":0}")]
        public object get_courses(string date)
        {
            DateTime argDate = DateTime.ParseExact(date,"yyyy-MM-dd",null);

            return (this.search_engine.GetCourses(argDate));
        }
        #endregion

        #region авиа
        [JsonRpcMethod("flight_check_redis")]
        public object flight_check_redis()
        {
            return (this.search_engine.CheckRedis());
        }
        
        
        [JsonRpcMethod("flight_init_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_init_search\",\"params\":[1, 2, [11,12], \"A\", [{\"dep_code\":\"MSQ\",\"arr_code\":\"DME\",\"date\":\"2013-08-14\"}, {\"dep_code\":\"DME\",\"arr_code\":\"MSQ\",\"date\":\"2013-08-24\"} ]],\"id\":0}")]
	    /// <summary>
        /// Summary description for json_handler
        /// </summary>
        public object flight_init_search(int adults, int children, JsonArray children_ages, string service_class,
                                         JsonArray segments, params object[] args)
        {
            Segment[] _segments = new Segment[segments.Length];

            for (int i = 0; i < segments.Length; i++ )
            {
                JsonObject segment = segments[i] as JsonObject;

                DateTime segmentDate = DateTime.ParseExact(segment["date"].ToString(), "yyyy-MM-dd", null); //new DateTime((int)dateArray[0], (int)dateArray[1], (int)dateArray[2]);

                _segments[i] = new Segment() {  ArrCode = segment["arr_code"].ToString(),
                                                DepCode = segment["dep_code"].ToString(),
                                                Date = segmentDate};
            }
            int[] child_ages = new int[children_ages.Length];

            for (int i = 0; i < children_ages.Length; i++)
                child_ages[i] = Convert.ToInt32(children_ages[i]);

          
            Responses._Response res = (this.search_engine.FlightInitSearch(adults, children, service_class, child_ages, _segments));

            if (res.ErrorCode > 0) throw new CatseException(res.ErrorMessage, res.ErrorCode);

            return (res as Responses.FlightInitSearchResult).SearchId;
        }

        [JsonRpcMethod("flight_get_search_state")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_get_search_state\",\"params\":[\"1232323453\"],\"id\":0}")]
        public object flight_get_search_state(string search_id, params object[] args)
        {
            Responses._Response res = (this.search_engine.FlightGetSearchState(search_id));

            if (res.ErrorCode > 0) throw new CatseException(res.ErrorMessage, res.ErrorCode);
            
            return res;
        }

        [JsonRpcMethod("flight_get_current_tickets")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_get_current_tickets\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object flight_get_current_tickets(string search_id, params object[] args)
        {
            Responses._Response res = (this.search_engine.FlightGetCurrentTickets(search_id));

            if (res.ErrorCode > 0)
            {
                throw new CatseException(res.ErrorMessage, res.ErrorCode);
            }
            
            
            return res;
        }

        [JsonRpcMethod("flight_wait_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_wait_search\",\"params\":[\"1408MSQDME2408DMEMSQ-1-2-0-A\"],\"id\":0}")]
        public object flight_wait_search(string search_id, params object[] args)
        {
            Responses._Response res = (this.search_engine.FlightWaitSearch(search_id));

            if (res.ErrorCode > 0) throw new CatseException(res.ErrorMessage, res.ErrorCode);

            return (res as FlightSearchResult).FlightTickets;
        }

        [JsonRpcMethod("flight_get_ticket_rules")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_get_ticket_rules\",\"params\":[\"srch_id\", 1],\"id\":0}")]
        public object flight_get_ticket_rules(string search_id, string ticket_id, string book_id, params object[] args)
        {
            SegmentRule res = this.search_engine.FlightGetTicketRules(search_id, ticket_id);

            if (res.ErrorCode == 0)
                return (res);
            else
                throw new CatseException(res.ErrorMessage, res.ErrorCode);
        }

        [JsonRpcMethod("flight_get_ticket_info")]
        [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_get_ticket_info\",\"params\":[\"srch_id\", 1],\"id\":0}")]
        public object flight_get_ticket_info(string search_id, string ticket_id, params object[] args)
        {
            var res = this.search_engine.FlightGetTicketInfo(search_id, ticket_id);

            //if (res.ErrorCode == 0)
                return res;
           // else
           //     throw new CatseException(res.ErrorMessage, res.ErrorCode);
        }
        
        
        [JsonRpcMethod("flight_check_ticket")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_check_ticket\",\"params\":[\"srch_id\",1],\"id\":0}")]
        public object flight_check_ticket(string search_id, string ticket_id, params object[] args)
        {
            FlightCheckTicketResult res = this.search_engine.FlightCheckTicket(search_id, ticket_id);

            if (res.ErrorCode == 0)
                return (res);
            else
                throw new CatseException(res.ErrorMessage, res.ErrorCode);
        }

        [JsonRpcMethod("flight_book_ticket")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"flight_book_ticket\",\"params\":[\"srch_id\",1,{\"email\":\"user@test.com\",\"phone\":\"+375 29 111 22 33\"}, [{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]],\"id\":0}")]
        public object flight_book_ticket(string search_id, string ticket_id, JsonObject user_info, JsonArray tourists, params object[] args)
        {
            //!!!!!!!!!!!!!!!!!!!!!
            //ErrorCode 17 -- не найден SearchId
            //ErrorCode 18 -- не найден TicketId
            //TODO: проверить состав туристов и запрос init search на соответствие количества детей и взрослых, возраста
            
            
            //ErrorCode 90: неправильный состав туристов (неверное количество, состав взрослые/дети)
            //ErrorCode 91: ошибка в дате рождения туриста
            //ErrorCode 92: ошибка в имени/фамилии туриста
            //ErrorCode 93: ошибка в гражданстве
            //ErrorCode 94: ошибка в номере паспорта
            //ErrorCode 95: ошибка в сроке действия паспорта
            //ErrorCode 96: дублируется турист
            //ErrorCode 97: ошибка в бонусной карте а/к
            
            
            //ErrorCode 98: невалидный и-мэйл пользователя
            //ErrorCode 99: невалидный телефон пользователя
            
            
            //конвертировать объект
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
          
                TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

                for (int i = 0; i < turistsCont.Length; i++)
                {
                    turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);
                }

                BookResult res = this.search_engine.FlightBookTicket(search_id, ticket_id, userInfo, turistsCont);

                if (res.ErrorCode == 0)
                    return (res);
                else
                    throw new CatseException(res.ErrorMessage, res.ErrorCode);
            }
            catch (CatseException ex)
            {
                Logger.WriteToLog("flight_book_ticket mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("flight_book_ticket mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);

                if (ex is CatseException)
                    throw ex;
                else
                    throw new CatseException(ex.Message, 0);
            }  
        }
        #endregion
        

        #region экскурсии
        [JsonRpcMethod("excursion_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"excursion_search\",\"params\":[1,\"2013-09-10\", \"2013-10-10\", 2],\"id\":0}")]
        //string CityCode, DateTime MinDate, DateTime MaxDate, int TuristsCount
        public object excursion_search(int city_id, string min_date, string max_date, int tourists_count)
        {
          	DateTime minDate = DateTime.ParseExact(min_date, "yyyy-MM-dd", null);
          	DateTime maxDate = DateTime.ParseExact(max_date, "yyyy-MM-dd", null);

            return (this.search_engine.ExcursionSearch(city_id, minDate, maxDate, tourists_count));
        }

        [JsonRpcMethod("excursion_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"excursion_book\",\"params\":[\"srch_id\", 1, \"2013-09-10\", {\"email\":\"user@test.com\",\"phone\":\"+375 29 111 22 33\"}, [{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]],\"id\":0}")]
        //string SearchId, int ExcursionId, DateTime ExcursionDate, UserInfo Info, TuristContainer[] turists
        public object excursion_book(string seach_id, int excursion_id, string excurion_date, JsonObject user_info, JsonArray tourists)
        {
            //конвертировать объект
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("excursion_book mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);
                throw new Exception("cann't parse user_info");
            }
            //   return user_info;

            //   if((turists==null)|| (turists.Length == 0)) throw new Exception("cann't parse turists");

            TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

            for(int i=0; i< turistsCont.Length; i++)
            {
                turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);
            }
		
	        DateTime dExc = DateTime.ParseExact(excurion_date, "yyyy-MM-dd", null);


            return this.search_engine.ExcursionBook(seach_id, excursion_id, dExc, userInfo , turistsCont);
        }
        #endregion

         #region Визы
        [JsonRpcMethod("visa_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"visa_search\",\"params\":[\"CZ\", 1, \"2013-08-08\", \"2013-08-09\", \"BY\", 25],\"id\":0}")]
        public object visa_h(  string country_code, int city_from_id,  string date_from, string date_to,
                                    string citizenship_code, int age,  params object[] args )
        {
            if (country_code.Length != 2) throw new Exception("Invalid country_code");
            if (citizenship_code.Length != 2) throw new Exception("Invalid citizenship_code");

            DateTime dFrom = DateTime.ParseExact(date_from, "yyyy-MM-dd", null);

            DateTime dTo = DateTime.ParseExact(date_to, "yyyy-MM-dd", null);

            if (dTo <= dFrom) throw new Exception("Invalid dates");


            return (this.search_engine.VisaSearch(country_code, dFrom, dTo, citizenship_code, age, city_from_id));
        }

        [JsonRpcMethod("visa_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"visa_book\",\"params\":[\"srch_id\", 1, {\"email\":\"user@test.com\", \"phone\":\"+375 29 111 22 33\"}, {\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}],\"id\":0}")]
        public object visa_book(string search_id, int visa_id, JsonObject user_info, JsonObject tourist, params object[] args)
        {
            //конвертировать объект
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("visa_book mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);
                throw new Exception("cann't parse user_info");
            }
            //   return user_info;

            if ((tourist == null)) throw new Exception("cann't parse turists");

            TuristContainer turistsCont = new TuristContainer(tourist as JsonObject);

         
            return (this.search_engine.VisaBook(search_id, visa_id, userInfo, turistsCont ));
        }
        #endregion

        #region трансфер

        [JsonRpcMethod("transfer_get_points")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"transfer_get_points\",\"params\":[1],\"id\":0}")]
        public object transfer_get_points(int start_point_id)
        {
            return this.search_engine.TransferGetPoints(start_point_id);
        }

        [JsonRpcMethod("transfer_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"transfer_search\",\"params\":[1, 2, \"2013-08-09\", \"2013-08-09\", 1],\"id\":0}")]
        public object transfer_search(  int start_point_id, int end_point_id,
                                        string transfer_date,
                                        string return_date,
                                        int tourists_count)
        {
            DateTime transferDate = DateTime.ParseExact(transfer_date, "yyyy-MM-dd", null);
            DateTime returnDate = DateTime.ParseExact(return_date, "yyyy-MM-dd", null);

            return (this.search_engine.TransferSearch(start_point_id, end_point_id, transferDate, returnDate, tourists_count));
        }

        [JsonRpcMethod("transfer_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"transfer_book\",\"params\":[\"srch_id\", 1, [{\"departure_info\":\"fdss\",\"arrival_info\":\"string\",\"time\":\"string\"}], {\"email\":\"user@test.com\", \"phone\":\"+375 29 111 22 33\"},[{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]],\"id\":0}")]
        public object transfer_book(string search_id, int transfer_id, JsonArray transfer_info,
                                    JsonObject user_info, JsonArray tourists)
        {
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex )// ex)
            {
                Logger.WriteToLog("transfer_book mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);
                throw new Exception("cann't parse user_info");
            }

            TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

            for (int i = 0; i < turistsCont.Length; i++)
                turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);


            TransferInfo[] tInfos = new TransferInfo[transfer_info.Length];

            for (int i = 0; i < transfer_info.Length; i++)
                tInfos[i] = new TransferInfo(transfer_info[i] as JsonObject);
            

            return (this.search_engine.TransferBook(search_id, transfer_id, tInfos, userInfo, turistsCont));
        }

        #endregion

        #region страховка

        [JsonRpcMethod("insurance_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"insurance_search\",\"params\":[10, 1, false, 1],\"id\":0}")]
        public object insurance_search(int days_long, int tourists_count, bool is_extra_country, int purpose_of_trip)
        {
            return (this.search_engine.InsuranceSearch(days_long, tourists_count, is_extra_country, purpose_of_trip));
        }

        [JsonRpcMethod("insurance_search_by_country")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"insurance_search_by_country\",\"params\":[\"2013-08-10\",\"2013-08-20\", \"CZ\", [{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]],\"id\":0}")]
        public object insurance_search_by_country(string start_date, string end_date, string country_code, JsonArray tourists)
        {
            //дата и время
            DateTime startDate = DateTime.ParseExact(start_date, "yyyy-MM-dd", null);
            DateTime endDate = DateTime.ParseExact(end_date, "yyyy-MM-dd", null);

            //туристы
            if ((tourists == null) || (tourists.Length == 0)) throw new Exception("cann't parse tourists");

            TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

            for (int i = 0; i < turistsCont.Length; i++)
            {
                turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);
            }


            return (this.search_engine.InsuranceSearchByCountry(startDate, endDate, country_code, turistsCont));
        }

        [JsonRpcMethod("insurance_calculate")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"insurance_calculate\",\"params\":[\"2013-08-10\", \"2013-08-20\", [\"CZ\",\"IT\"], [{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}],1,1,1],\"id\":0}")]
        public object insurance_calculate(string start_date, string end_date, JsonArray country_codes, JsonArray tourists, int purpose_of_trip, int insurance_program, int coverage)
        {
            //дата и время
            DateTime startDate = DateTime.ParseExact(start_date, "yyyy-MM-dd", null);
            DateTime endDate = DateTime.ParseExact(end_date, "yyyy-MM-dd", null);

            //страны
            string[] cnt_codes = new string[country_codes.Length];

            for (int i = 0; i < country_codes.Length; i++)
                if (country_codes[i] is JsonString)
                    cnt_codes[i] = country_codes[i].ToString();


                //туристы
            if ((tourists == null) || (tourists.Length == 0)) throw new Exception("cann't parse tourists");

            TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

            for (int i = 0; i < turistsCont.Length; i++)
            {
                turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);
            }


            //результат
            KeyValuePair<string, decimal>[] prices = (this.search_engine.InsuranceCalculate(startDate, endDate, cnt_codes, turistsCont, purpose_of_trip, insurance_program, coverage));

            JsonObject pr = new JsonObject();

            foreach (KeyValuePair<string, decimal> val in prices)
                pr.Add(val.Key, val.Value);

            return pr;
        }

        [JsonRpcMethod("insurance_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"insurance_book\",\"params\":[\"2013-08-10\", \"2013-08-20\", [\"CZ\",\"IT\"], [{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}],1,1,1],\"id\":0}")]
        public object insurance_book(string start_date, string end_date, JsonArray country_codes, JsonArray tourists, int purpose_of_trip, int insurance_program, int coverage, params object[] args)
        {
            //дата и время
            DateTime startDate = DateTime.ParseExact(start_date, "yyyy-MM-dd", null);
            DateTime endDate = DateTime.ParseExact(end_date, "yyyy-MM-dd", null);

            //страны
            string[] cnt_codes = new string[country_codes.Length];

            for (int i = 0; i < country_codes.Length; i++)
                if (country_codes[i] is JsonString)
                    cnt_codes[i] = country_codes[i].ToString();

            //туристы
            if ((tourists == null) || (tourists.Length == 0)) throw new Exception("cann't parse turists");

            TuristContainer[] turistsCont = new TuristContainer[tourists.Length];

            for (int i = 0; i < turistsCont.Length; i++)
            {
                turistsCont[i] = new TuristContainer(tourists[i] as JsonObject);
            }

            return this.search_engine.InsuranceBook(startDate, endDate, cnt_codes, turistsCont, purpose_of_trip, insurance_program, coverage);
        }
        #endregion


        #region аренда авто
        [JsonRpcMethod("car_rent_get_pick_up_locations")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_pick_up_locations\",\"params\":[1],\"id\":0}")]
        public object car_rent_get_pick_up_locations(int city_id)
        {
            return (this.search_engine.CarRentGetPickUpLocations(city_id));
        }

        [JsonRpcMethod("car_rent_get_drop_off_locations")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_drop_off_locations\",\"params\":[1],\"id\":0}")]
        public object car_rent_get_drop_off_locations(int pick_up_location_id)
        {
            return (this.search_engine.CarRentGetDropOffLocations(pick_up_location_id));
        }

        [JsonRpcMethod("car_rent_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_search\",\"params\":[1,1, \"2013-09-10 10:10\", \"2013-10-10 22:00\"],\"id\":0}")]
        public object car_rent_search(int pick_up_location_id, int drop_off_location_id, string pick_up_datetime, string drop_off_datetime)
        {
            DateTime pickDateTime = DateTime.ParseExact(pick_up_datetime, "yyyy-MM-dd HH:mm", null);
            DateTime dropDateTime = DateTime.ParseExact(drop_off_datetime, "yyyy-MM-dd HH:mm", null);

            return (this.search_engine.CarRentSearch(pick_up_location_id, drop_off_location_id, pickDateTime, dropDateTime));
        }

        [JsonRpcMethod("car_rent_get_pick_up_stations")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_pick_up_stations\",\"params\":[1,1],\"id\":0}")]
        public object car_rent_get_pick_up_stations(int location_id, int supplier_id)
        {
            return (this.search_engine.CarRentGetPickUpStations(location_id, supplier_id));
        }

        [JsonRpcMethod("car_rent_get_drop_off_stations")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_drop_off_stations\",\"params\":[1,1,1],\"id\":0}")]	
        public object car_rent_get_drop_off_stations(int location_id, int pick_up_station_id, int supplier_id)
        {
            return (this.search_engine.CarRentGetDropOffStations(location_id, pick_up_station_id, supplier_id));
        }

        [JsonRpcMethod("car_rent_get_station_details")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_station_details\",\"params\":[1,\"srch_id\"],\"id\":0}")]
        public object car_rent_get_station_details(int station_id, string search_id)
        {
            return (this.search_engine.CarRentGetStationDetails(station_id, search_id));
        }

        [JsonRpcMethod("car_rent_verify_car")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_verify_car\",\"params\":[\"srch_id\",1,1,\"2013-09-10 10:10\", \"2013-10-10 22:22\", 1,1],\"id\":0}")]
        public object car_rent_verify_car(string search_id, int pick_up_station_id, int drop_off_station_id,
                                                    string pick_up_datetime, string drop_off_datetime,
                                                    int variant_id, int program_id)
        {
            DateTime pickDateTime = DateTime.ParseExact(pick_up_datetime, "yyyy-MM-dd HH:mm", null);
            DateTime dropDateTime = DateTime.ParseExact(drop_off_datetime, "yyyy-MM-dd HH:mm", null);

            return (this.search_engine.CarRentVerifyCar(search_id, pick_up_station_id, drop_off_station_id,
                                                        pickDateTime, dropDateTime, variant_id, program_id));
        }

        [JsonRpcMethod("car_rent_get_extras")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_extras\",\"params\":[1,\"2013-07-11\"],\"id\":0}\r\n\r\nOR\n\n{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_get_extras\",\"params\":[1,\"2013-07-11 10:10\"],\"id\":0}")]
        public object car_rent_get_extras(int supplier_id, string pick_up_date)
        {
		    try{
	      		       DateTime pickDateTime = DateTime.ParseExact(pick_up_date, "yyyy-MM-dd HH:mm", null);
	
        		       return this.search_engine.CarRentGetExtras(supplier_id, pickDateTime );
		    }
		    catch(Exception)
		    {
			    DateTime pickDate = DateTime.ParseExact(pick_up_date, "yyyy-MM-dd", null);
	
        		    return this.search_engine.CarRentGetExtras(supplier_id, pickDate);
		    }
        }

        [JsonRpcMethod("car_rent_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"car_rent_book\",\"params\":[\"srch_id\", 1, 1, {\"email\":\"user@test.com\",\"phone\":\"+375 29 111 22 33\"}, {\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}, 1, 1, [{\"id\":1, \"count\":1},{\"id\":2, \"count\":2}]],\"id\":0}")]
        public object car_rent_book(string search_id, int pick_up_station_id, int drop_off_station_id,
                                    JsonObject user_info, JsonObject tourist, int variant_id, int program_id,
                                    JsonArray extras)
        {

            //extras для аренды
            CarRentBookExtra[] extrasCont = new CarRentBookExtra[extras.Length];

            for (int i = 0; i < extras.Length; i++)
                extrasCont[i] = new CarRentBookExtra(extras[i] as JsonObject);


            //туристы
            if ((tourist == null)) throw new Exception("cann't parse turists");


            TuristContainer turistsCont = new TuristContainer(tourist);



            //конвертировать объект инфо о пользователе
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("car_rent_book mehod exception while parse user info " + ex.Message + "\n" + ex.StackTrace);
                throw new Exception("cann't parse user_info");
            }

            return (this.search_engine.CarRentBook(search_id, pick_up_station_id,
                                                   drop_off_station_id, userInfo, turistsCont, variant_id, program_id,
                                                   extrasCont));

        }
        #endregion

        #region отели
     
        //public static HotelInitSearchResult HotelInitSearh(string CityCode, DateTime StartDate, DateTime EndDate,
        //                                                   int[] Stars, int[] Pansions, RequestRoom[] Rooms)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_init_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_init_search\",\"params\":[1,\"2013-09-10\", \"2013-09-15\", [1,2,3],[1,2,3],[{\"adults\":1,\"children\":2, \"children_ages\":[11,12]}]],\"id\":0}")]
        public object hotel_init_search(int city_id, string start_date, string end_date, 
                                               JsonArray stars, JsonArray pansions, JsonArray rooms)
        {
            //даты
            DateTime startDate = DateTime.ParseExact(start_date, "yyyy-MM-dd", null);
            DateTime endDate = DateTime.ParseExact(end_date, "yyyy-MM-dd", null);

            //звездность
            int[] stars_array = jsonArrayToIntArray(stars);//.ToArray(typeof(int));
            //питание
            int[] pansions_array = jsonArrayToIntArray(pansions);//.ToArray(typeof(int));
            //комнаты
            RequestRoom[] roomsCont = new RequestRoom[rooms.Length];

            for (int i = 0; i < rooms.Length; i++)
            {
                roomsCont[i] = new RequestRoom(rooms[i] as JsonObject);
            }

            return (this.search_engine.HotelInitSearh(city_id, startDate, endDate, stars_array, pansions_array, roomsCont).SearchId);
        }

        //[WebMethod]
        //private static HotelSearchState HotelGetSearchState(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_get_search_state")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_get_search_state\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object hotel_get_search_state(string search_id)
        {

            return (this.search_engine.HotelGetSearchState(search_id));
        }

        //[WebMethod]
        //private static HotelSearchResult HotelGetCurrentHotels(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_get_current_hotels")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_get_current_hotels\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object hotel_get_current_hotels(string search_id)
        {
            return (this.search_engine.HotelGetCurrentHotels(search_id));
        }

        //[WebMethod]
        //private static HotelSearchResult HotelWaitSearch(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_wait_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_wait_search\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object hotel_wait_search(string search_id)
        {
            return (this.search_engine.HotelWaitSearch(search_id));
        }

        ////варианты номеров по отелю
        //[WebMethod]
        //private static HotelSearchResult HotelGetRooms(string SearchId, int HotelId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_get_rooms")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_get_rooms\",\"params\":[\"srch_id\",1],\"id\":0}")]
        public object hotel_get_rooms(string search_id, int hotel_id)
        {
            return (this.search_engine.HotelGetRooms(search_id, hotel_id));
        }

        ////информация о штрафах по номеру в отеле
        //[WebMethod]
        //private static HotelPenalties[] HotelGetPenalties(string SearchId, int HotelId, int[] VariantsId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_get_room_info")]
        [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_get_room_info\",\"params\":[\"srch_id\",1,1],\"id\":0}")]
        public object hotel_get_room_info(string search_id, int hotel_id, string variant_id)
        {
            try
            {
                return (this.search_engine.HotelGetRoomInfo(search_id, hotel_id, variant_id));
            }
            catch (Exception ex)
            {
                Helpers.Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }

            return null;
        }

        [JsonRpcMethod("hotel_get_penalties")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_get_penalties\",\"params\":[\"srch_id\",1,[1,2,3]],\"id\":0}")]
        public object hotel_get_penalties(string search_id, int hotel_id, JsonArray variant_ids, string book_id)
        {
            try{
                return (this.search_engine.HotelGetPenalties(search_id, hotel_id, jsonArrayToStringArray(variant_ids)));
            }
            catch(Exception ex)
            {
	            Helpers.Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }
            
            return null;
        }

        ////проверить актуальность !!
        //[WebMethod]
        //private static HotelVerifyResult[] HotelVerify(string SearchId, int HotelId, int[] VariantsId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_verify")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_verify\",\"params\":[\"srch_id\", 1, [1,2,3,4]],\"id\":0}")]
        public object hotel_verify(string search_id, int hotel_id, JsonArray variant_ids)
        {
            return (this.search_engine.HotelVerify(search_id, hotel_id, jsonArrayToStringArray( variant_ids)));
        }

        ////бронировать отель
        //[WebMethod]
        //public static BookResult HotelBook(string SearchId, int HotelId, UserInfo Info, BookRoom Rooms)
        //{
        //    return null;
        //}

        [JsonRpcMethod("hotel_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"hotel_book\",\"params\":[\"srch_id\",1, {\"email\":\"user@test.com\", \"phone\":\"+375 29 111 22 33\"}, [{\"variant_id\":1, \"tourists\":[{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]}]],\"id\":0}")]
        public object hotel_book(string search_id, int hotel_id, JsonObject user_info, JsonArray rooms)
        {
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex)
            {
                Helpers.Logger.WriteToLog("hotel_book method exception: "+ex.Message);
                throw new Exception("cann't parse user_info");
            }

            BookRoom[] bookRooms = new BookRoom[rooms.Length];

            for (int i = 0; i < rooms.Length; i++ )
            {
                bookRooms[i] = new BookRoom(rooms[i] as JsonObject);
            }


            return (this.search_engine.HotelBook(search_id, hotel_id, userInfo, bookRooms));
        }

        #endregion

        #region тур
        ////поиск маршрутов
        //[WebMethod]
        //public static TourInitSearchResult TourRoutesInitSearch(string CityId, DateTime TourDate, int NightsMin, int NightsMax, bool IsRoughly, int[] Stars, int[] Pansions, RequestRoom Room)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_routes_init_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_routes_init_search\",\"params\":[1, \"2013-08-10\", 3, 12, true, [1,2,3,4], [1,2,3], {\"adults\":1, \"children\":2,\"children_ages\":[11,12]}],\"id\":0}")]
        public object tour_routes_init_search(int city_id, string tour_date, int nights_min, 
                                               int nights_max, bool is_roughly, JsonArray stars, JsonArray pansions, JsonObject room)
        {
            DateTime tourDate = DateTime.ParseExact(tour_date, "yyyy-MM-dd", null);

            RequestRoom roomCont = new RequestRoom(room);

            return (this.search_engine.TourRoutesInitSearch(city_id, tourDate, nights_min, nights_max,
                                                is_roughly, jsonArrayToIntArray(stars), jsonArrayToIntArray(pansions), roomCont)).SearchId;
        }

        ////поиск отелей
        //[WebMethod]
        //public static TourInitSearchResult TourHotelsInitSearch(string CityId, DateTime TourDate, int NightsMin, int NightsMax, bool IsRoughly, int[] Stars, int[] Pansions, RequestRoom Room, int TourId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_hotels_init_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_hotels_init_search\",\"params\":[1, \"2013-08-10\", 3, 12, true, [1,2,3,4], [1,2,3], {\"adults\":1, \"children\":2,\"children_ages\":[11,12]}, 10],\"id\":0}")]
        public object tour_hotels_init_search(int city_id, string tour_date, int nights_min,
                                               int nights_max, bool is_roughly, JsonArray stars, JsonArray pansions, JsonObject room, int tour_id)
        {
            DateTime tourDate = DateTime.ParseExact(tour_date, "yyyy-MM-dd", null);

            RequestRoom roomCont = new RequestRoom(room);

            return (this.search_engine.TourHotelsInitSearch(city_id, tourDate, nights_min, nights_max,
                                                is_roughly, jsonArrayToIntArray(stars), jsonArrayToIntArray(pansions), roomCont, tour_id)).SearchId;
        }


        //[WebMethod]
        //public static TourInitSearchResult TourVariantsInitSearch(string TourHotelsSearchId, int VariantsGroupId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_variants_init_search")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_variants_init_search\",\"params\":[\"tours_srch_id\", 12],\"id\":0}")]
        public object tour_variants_init_search(string tour_hotels_search_id, int variants_group_id)
        {
            // DateTime tourDate = ParseDateFromJson(tour_date);

            return (this.search_engine.TourVariantsInitSearch(tour_hotels_search_id, variants_group_id)).SearchId;
        }

        ////состояние поиска
        //[WebMethod]
        //public static TourSearchState TourGetSearchState(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_get_search_state")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_get_search_state\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object tour_get_search_state(string search_id)
        {
           // DateTime tourDate = ParseDateFromJson(tour_date);

            return (this.search_engine.TourGetSearchState(search_id));
        }

        ////результаты поиска
        //[WebMethod]
        //public static TourHotelsResult TourGetHotelsResult(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_get_hotels_result")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_get_hotels_result\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object tour_get_hotels_result(string search_id)
        {
            return (this.search_engine.TourGetHotelsResult(search_id));
        }

        //[WebMethod]
        //public static TourRoutesResult TourGetRoutesResult(string SearchId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_get_routes_result")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_get_routes_result\",\"params\":[\"srch_id\"],\"id\":0}")]
        public object tour_get_routes_result(string search_id)
        {
            return (this.search_engine.TourGetRoutesResult(search_id));
        }

        ////список услуг по туру
        //[WebMethod]
        //public static TourService[] TourGetTourServices(string SearchId, int TourId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_get_tour_services")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_get_tour_services\",\"params\":[\"srch_id\", 12],\"id\":0}")]
        public object tour_get_tour_services(string search_id, int tour_id)
        {
            return (this.search_engine.TourGetRoutesResult(search_id));
        }

        ////информация о штрафах по pricekey тура
        //[WebMethod]
        //public static TourPenalties TourGetTourVariantPenalties(string SearchId, int TourId, int VariantId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_get_tour_variant_penalties")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_get_tour_variant_penalties\",\"params\":[\"srch_id\", 12, \"34\"],\"id\":0}")]
        public object tour_get_tour_variant_penalties(string search_id, int tour_id, string variant_id)
        {
            return (this.search_engine.TourGetTourVariantPenalties(search_id, tour_id, variant_id));
        }

        ////проверить актуальность !!
        //[WebMethod]
        //public static TourVerifyResult TourVerifyVariant(string SearchId, int TourId, int VariantId)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_verify_variant")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_verify_variant\",\"params\":[\"srch_id\", 12, \"34\"],\"id\":0}")]
        public object tour_verify_variant(string search_id, int tour_id, string variant_id)
        {
             return (this.search_engine.TourVerifyVariant(search_id, tour_id, variant_id));
        }

        ////бронировать тур 
        //[WebMethod]
        //public static BookResult TourBook(string SearchId, int TourId, UserInfo Info, BookRoom Room)
        //{
        //    return null;
        //}

        [JsonRpcMethod("tour_book")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"tour_book\",\"params\":[\"srch_id\", 12,  {\"email\":\"user@test.com\",\"phone\":\"+375 29 111 22 33\"}, {\"variant_id\":1, \"tourists\":[{\"last_name\":\"Vasia\",\"first_name\":\"Pupkin\",\"birth_date\":\"2010-06-15\",\"citizenship\":\"BY\",\"passport_num\":\"MP12211221\",\"passport_date\":\"2014-06-15\",\"bonus_card\":{\"airline_code\":\"LH\",\"card_number\":\"24352200\"}}]}],\"id\":0}")]
        public object tour_book(string search_id, int tour_id, JsonObject user_info, JsonObject room)
        {
            UserInfo userInfo = null;
            try
            {
                userInfo = new UserInfo(user_info);
            }
            catch (Exception ex)
            {
                Helpers.Logger.WriteToLog("hotel_book method exception: " + ex.Message);
                throw new Exception("cann't parse user_info");
            }

            BookRoom bookRooms = new BookRoom(room);


            return (this.search_engine.TourBook(search_id, tour_id, userInfo, bookRooms));
        }

        #endregion


        #region private_methods
        
        private int[] jsonArrayToIntArray(JsonArray inp)
        {
            int[] res = new int[inp.Length];

            for (int i = 0; i < inp.Length; i++)
                res[i] = Convert.ToInt32(inp[i]);

            return res;
        }
        
   	private string[] jsonArrayToStringArray(JsonArray inp)
        {
            string[] res = new string[inp.Length];

            for (int i = 0; i < inp.Length; i++)
                res[i] = inp[i].ToString();

            return res;
        }
        
        #endregion
    }
}