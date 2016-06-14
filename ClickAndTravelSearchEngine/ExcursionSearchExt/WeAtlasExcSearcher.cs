using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.MasterTour;
using ClickAndTravelSearchEngine.ParamsContainers;

using Jayrock.Json.Conversion;
using Jayrock.Json;
using ClickAndTravelSearchEngine.Containers.Excursions;
using System.Net;
using System.Net.Security;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;


namespace ClickAndTravelSearchEngine.ExcursionSearchExt
{
    public class WeAtlasExcSearcher
    {
        private static int idShift = 10000000;

        private static int cityIdShift = 2000000000;

        private static string serviceUrl = ConfigurationManager.AppSettings["WeatlasServiceUrl"];

        private static string AID = ConfigurationManager.AppSettings["WeatlasAid"];

        private static string KEY = ConfigurationManager.AppSettings["WeatlasKey"];
        private static string PHONE = ConfigurationManager.AppSettings["WeatlasPhone"];
        private static string EMAIL = ConfigurationManager.AppSettings["WeatlasEmail"];

        private static decimal WEATLAS_COEF = Convert.ToDecimal(ConfigurationManager.AppSettings["WeatlasMargin"]);

        public static void ApplyConfig(NameValueCollection settings, string partnerCode = "")
        {
            AID = settings["WeatlasAid"];
            KEY = settings["WeatlasKey"];
            serviceUrl = settings["WeatlasServiceUrl"];

            WEATLAS_COEF = Convert.ToDecimal(settings["WeatlasMargin"]);

            PHONE = settings["WeatlasPhone"];
            EMAIL = settings["WeatlasEmail"];

            Logger.WriteToLog(partnerCode + " config applied to weatlas");
        }

        private static string MakePost(string url, Dictionary<string, string> sendParams)
        {
            sendParams["aid"] = AID;
            sendParams["key"] = KEY;

            sendParams["Lang"] = "ru";
            sendParams["mode"] = "json";

            WebClient cl = new WebClient();
            cl.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string URI = serviceUrl + url;// "export/countrylist/";

            List<string> data = new List<string>();

            foreach (string key in sendParams.Keys)
                data.Add(key + "=" + sendParams[key]);

            var response = cl.UploadString(URI, string.Join("&", data.ToArray()));

            Console.WriteLine(response);
            if (response.Contains("error"))
            {
                Logger.WriteToErrorLog(URI + " " + string.Join("&", data.ToArray()));

                Logger.WriteToErrorLog(response);
            }

            return response;
        }

        private static int GetCityId(int CityId)
        {
            if (CityId > cityIdShift)
                return CityId - cityIdShift;

            var selectQuery = "select weatlas_key from cities where id = " + CityId;

            selectQuery.Split('\\').Last();

            //коннектимся к базе и выполняем запрос
            var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();

            var row = command.ExecuteScalar();

            connection.Close();

            if (row == null)
                return CityId;
            else
                return Convert.ToInt32(row) - cityIdShift;
        }

        //поиск экскурсий
        public ExcursionVariant[] SearchExcursions(int CityId, DateTime MinDate, DateTime MaxDate, int TuristsCount)
        {
            CityId = GetCityId(CityId);

            //заполняем параметры запроса
            Dictionary<string, string> reqParams = new Dictionary<string, string>();

            reqParams.Add("IDCity", CityId.ToString());                 //идент. экскурсии
            reqParams.Add("dateStart", MinDate.ToString("yyyy-MM-dd")); //дата начала
            reqParams.Add("dateEnd", MaxDate.ToString("yyyy-MM-dd"));   //дата окончания
            reqParams.Add("Adults", TuristsCount.ToString());           //кол-во туристов
            reqParams.Add("activityType", "Excursion");                 //ищем только экскурсии
            reqParams.Add("Currency", "RUB");                           //валюта

            Logger.WriteToLog("before search");

            var resp = WeAtlasExcSearcher.MakePost("export/pricesearch/", reqParams); //делаем запрос

            var searchResult = JsonConvert.Import<JsonObject>(resp);

            var response = new List<ExcursionVariant>();
            Logger.WriteToLog("after search");

            var _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);
            Logger.WriteToLog("after course");

            if (searchResult["error"] == null)
            {
                foreach (JsonObject activity in searchResult["activity"] as JsonArray)
                {
                    var variant = new ExcursionVariant();
                    variant.Id = Convert.ToInt32(activity["id"]) + idShift;
                    variant.Prices = MtHelper.ApplyCourses(Convert.ToDecimal(activity["minPrice"].ToString().Replace(".",",")), _courses);

                    response.Add(variant);
                }
            }

            Logger.WriteToLog("after convert");

            return response.ToArray();
        }

        //поиск дат по экскурсии
        public JsonArray GetExcursionCalendar(int excursionId, DateTime MinDate, DateTime MaxDate, int nMen)
        {
            //заполняем параметры запроса
            Dictionary<string, string> reqParams = new Dictionary<string, string>();

            reqParams.Add("activityId", (excursionId - idShift).ToString());    //идент. экскурсии
            reqParams.Add("dateStart", MinDate.ToString("yyyy-MM-dd"));         //дата начала
            reqParams.Add("dateEnd", MaxDate.ToString("yyyy-MM-dd"));           //дата окончаниа
            reqParams.Add("Currency", "RUB");                                   //валюта

            var resp = MakePost("export/activitycalendar/", reqParams);         //делаем запрос

            var searchResult = JsonConvert.Import<JsonObject>(resp);

            var response = new JsonArray();

            var listDates = new List<string>();

            foreach (JsonObject calendar in searchResult["calendar"] as JsonArray) //проходимся по результатам
            {
                var date = calendar["date"].ToString(); //забираем дату

                if (listDates.Contains(date)) //проверяем есть ли уже эта дата в результатах
                    continue;
                else
                    listDates.Add(date);

                var offer = ((calendar["offer"] as JsonArray)[0] as JsonObject);

                var id = offer["id"].ToString(); //забираем ид для бронирования

                var booktime = (offer["book_time"] as JsonArray)[0] as JsonObject;

                //время начала
                var timeArray = booktime["time"] as JsonArray;

                //цена
                decimal price = 0;
                foreach (JsonObject cost in (booktime["cost"] as JsonObject)["adult"] as JsonArray)
                {
                    if(Convert.ToInt32(cost["amount"]) == nMen)
                    {
                        price = Convert.ToDecimal(cost["price"].ToString().Replace(".",","));
                        id += "_"+cost["price"].ToString();
                        break;
                    }
                }

                var prices = new JsonObject();
                if (price > 0)
                {
                    var _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);

                    
                    prices = DictionaryToObject(MtHelper.ApplyCourses(Convert.ToDecimal(price), _courses));
                }

                response.Add(new { date = date, offer_id = id, time = timeArray, price = prices });
            }

            return response;
        }

        private static JsonObject DictionaryToObject(KeyValuePair<string, decimal>[] dict)
        {
            JsonObject jObj = new JsonObject();
            foreach (KeyValuePair<string, decimal> item in dict)
                jObj.Add(item.Key, item.Value);

            return jObj;
        }

        //делаем бронь экскурсии
        public string MakeOrder(string offerId, int activityId, DateTime startDate, string startTime,  TuristContainer[] turists)
        {
            var parts = offerId.Split('_');

            //заполняем параметры запроса
            Dictionary<string, string> reqParams = new Dictionary<string, string>();

            reqParams.Add("handler", "new");
            reqParams.Add("offer_id", parts[0]);
            reqParams.Add("activityId", (activityId - idShift).ToString());
            reqParams.Add("start", startDate.ToString("yyyy-MM-dd") + "T" + startTime); //дата начала
            reqParams.Add("Adults", turists.Length.ToString()); //дата начала
            reqParams.Add("Children", ""); //дата начала
            reqParams.Add("price", Math.Round(Convert.ToDecimal( parts[1].Replace(".",",") )).ToString()); 
            reqParams.Add("fio", turists[0].Name + " " + turists[0].FirstName); //дата начала

            reqParams.Add("Currency", "RUB");   //валюта
            reqParams.Add("phone", PHONE);   //телефон
            reqParams.Add("email", EMAIL);   //емэйл

            var resp = MakePost("order/create/", reqParams);//делаем запрос

            var order = JsonConvert.Import<JsonObject>(resp);

            return order["order"] +"_"+order["pin"];
        }

    }
}