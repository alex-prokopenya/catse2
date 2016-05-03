using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelSearchEngine.Helpers;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using Jayrock.JsonRpc;
using ClickAndTravelSearchEngine.Containers.Transfers;
using ClickAndTravelSearchEngine.MasterTour;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.ParamsContainers;

namespace ClickAndTravelSearchEngine.TransferSearchExt
{
    public class IWaySearcher
    {
        private static string url = "https://iwayex.com/rpc";
        private static int userId = 2464;
        private static string lang = "ru";
        private static JsonRpcClient jsonClient = null;

        private static JsonRpcClient GetClient()
        {
            if(jsonClient == null)
            {
                jsonClient = new JsonRpcClient();
                jsonClient.Url = url;
            }

            return jsonClient;
        }

        public static int[] GetPoints(int startId)
        {
            string redisKey = "iway_GetPlaceFromPlace_" + startId;

            var redisCache = RedisHelper.GetString(redisKey);

            //если есть в кэше, выводим из кэша
            if (!string.IsNullOrEmpty(redisCache))
                return redisCache.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            var client = GetClient();
            //делаем запрос в iWay
            var pointsArray = (JsonArray)(client.Invoke("GetPlaceFromPlace", new { userID = userId, startID = startId, lang = lang }));

            List<int> pointIds = new List<int>();
            //выбираем айдишники точек
            foreach (JsonObject pointObj in pointsArray)
                pointIds.Add(Convert.ToInt32(pointObj["AuditID"]));

            //кэшируем на 30 минут
            RedisHelper.SetString(redisKey, string.Join(",", pointIds), new TimeSpan(0,0,60*30));

            return pointIds.ToArray();
        }

        /// <summary>
        /// ищет цены на трансфер у iWay
        /// </summary>
        /// <param name="startPoint">точка начала поездки</param>
        /// <param name="endPoint">точка окончания</param>
        /// <param name="turistCnt">количество туристов</param>
        /// <param name="isRoundTrip">признак "в обе стороны"</param>
        /// <returns></returns>
        public static TransferVariant[] GetPriceVariants(int startPoint, int endPoint, int turistCnt, bool isRoundTrip)
        {
            string redisKey = "iway_GetPriceVariants_"+startPoint +"_"+endPoint+"_"+turistCnt+"_"+isRoundTrip ;

            //проверяем кэш
            var redisCache = RedisHelper.GetString(redisKey);

            //если что-то есть, пробуем вернуть из кэша
            if (!string.IsNullOrEmpty(redisCache))
                try
                {
                    return JsonConvert.Import<TransferVariant[]>(redisCache);
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + " " + ex.StackTrace );
                }
            

            var _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);

            //делаем запрос в iway
            var client = GetClient();

            var response = (JsonObject)client.Invoke("GetPrice", new{
                                                                    userID = userId,
                                                                    currency = "RUB", 
                                                                    lang = lang,
                                                                    startPlaceAuditID = startPoint,
                                                                    finishPlaceAuditID = endPoint
                                                                });

            var variants = new List<TransferVariant>();

            foreach (JsonArray jGroup in response["Result"] as JsonArray)
            {
                foreach (JsonObject jObj in jGroup)
                {
                    int capacity = Convert.ToInt32(jObj["Capacity"]);
                    short carsCount = Convert.ToInt16(Math.Ceiling(turistCnt*1.00000000/capacity));

                    //берем id, считаем стоимость
                    string priceId = jObj["AuditID"].ToString();
                    decimal price = Convert.ToDecimal(jObj["TotalCost"]) * carsCount;

                    //если трансфер в обе стороны, прибавляем id и стоимость
                    if(isRoundTrip)
                    {
                        priceId += "_" + jObj["ReverseID"] ;
                        price += Convert.ToDecimal(jObj["ReversePrice"]) * carsCount;
                    }

                    priceId += "#" + carsCount;

                    variants.Add(new TransferVariant(){
                        PriceId = priceId,
                        CarsInfo   = jObj["Models"].ToString(),
                        DetailsId  = Convert.ToInt32(jObj["CarClassID"]),
                        CarsCount  = carsCount,
                        Prices = MtHelper.ApplyCourses(price, _courses)
                     });
                }
            }

            RedisHelper.SetString(redisKey, JsonConvert.ExportToString(variants.ToArray()), new TimeSpan(0,30,0));

            return variants.ToArray();
        }

        public static JsonObject[] GetInfoMasks(string transferId)
        {
            var listOf = new List<JsonObject>();

            string[] parts = transferId.Split('#');
            string[] subparts = parts.First().Split('_');

            foreach (string part in subparts)
                listOf.Add(GetInfoMask(Convert.ToInt32(part)));

            return listOf.ToArray();
        }

        private static JsonObject GetInfoMask(int priceId)
        {
            string[] needKeys = new string[] { "DateArrival", "DateDeparture", "LocationAddress", "ArrivalNumber", "DestinationAddress" };

            var infoObj = new JsonObject();

            var client = GetClient();
            var response = (JsonObject)client.Invoke("GetMaskReservation", new { lang = lang, priceID = priceId });

            foreach (string name in needKeys)
                if(response.Contains(name))
                    infoObj[name] = (response[name] as JsonArray)[0].ToString().Replace(" не может быть пустым not_empty", "");

            return infoObj;
        }

        public static TransferBooking BookTransfer( DateTime startDate, DateTime endDate, string priceId,
                                                    TransferInfo[] transferInfo, UserInfo userInfo, 
                                                    TuristContainer[] turists, string searchId)
        {
            string[] parts = priceId.Split('#');

            string[] priceIds = parts[0].Split('_');

            int carsCount = Convert.ToInt32(parts[1]);
            int nmenLimit = Convert.ToInt32(Math.Ceiling(1.000 * turists.Length / carsCount));

            List<JsonObject> prices = new List<JsonObject>();

            int routeCnt = 0;

            foreach (string pId in priceIds) //для каждого направления
            {
                for (int i = 0; i < carsCount; i++) //для каждой машины
                {
                    JsonObject priceItem = new JsonObject();

                    priceItem["priceID"]  = Convert.ToInt32(pId);
                    priceItem["userID"]   = userId;
                    priceItem["lang"]     = "ru";
                    priceItem["currency"] = "RUB";

                    var passengers = new JsonArray();

                    for (int j = i * nmenLimit; j < Math.Min((nmenLimit * i + 1), turists.Length); j++)
                    {
                        var passenger = new JsonObject();

                        passenger["phone"] = userInfo.Phone;
                        passenger["email"] = "info@clickandtravel.ru";
                        passenger["name"] = turists[j].FirstName + " " + turists[j].Name;

                        passengers.Add(passenger);
                    }

                    priceItem["passengers"] = passengers;

                    //инфо о трансфере
                    if(!string.IsNullOrEmpty(transferInfo[routeCnt].ArrivalNumber))
                        priceItem["arrivalNumber"] = transferInfo[routeCnt].ArrivalNumber;

                    if (!string.IsNullOrEmpty(transferInfo[routeCnt].DateArrival))
                        priceItem["dateArrival"] = transferInfo[routeCnt].DateArrival;

                    if (!string.IsNullOrEmpty(transferInfo[routeCnt].DateDeparture))
                        priceItem["dateDeparture"] = transferInfo[routeCnt].DateDeparture;

                    if (!string.IsNullOrEmpty(transferInfo[routeCnt].DestinationAddress))
                        priceItem["destinationAddress"] = transferInfo[routeCnt].DestinationAddress;

                    if (!string.IsNullOrEmpty(transferInfo[routeCnt].LocationAddress))
                        priceItem["locationAddress"] = transferInfo[routeCnt].LocationAddress;

                    prices.Add(priceItem);
                }
                routeCnt++;
            }

            var client = GetClient();
            //делаем запрос в iway


                JsonArray response = null;
                try
                {
                    response = (JsonArray)client.Invoke("BatchReservations", new { data = prices.ToArray() });
                }
                catch (JsonException ex)
                {
                    JsonObject err = JsonConvert.Import<JsonObject>(ex.Message);

                    throw new Exceptions.CatseException(err["data"].ToString(), 29);
                }
                
                decimal totalPrice = 0;
                int transactionId = 0;
                int carClass = 0;

                foreach (JsonObject order in response)
                {
                    totalPrice += Convert.ToDecimal(order["Price"]);
                    transactionId = Convert.ToInt32(order["Transaction"]);
                    carClass = Convert.ToInt32(order["CarClassID"]);
                }

                TransferBooking resp = new TransferBooking();
                resp.SearchId = searchId;
                resp.StartDate = startDate.ToString("yyyy-MM-dd");
                resp.EndDate = endDate == DateTime.MinValue ? startDate.ToString("yyyy-MM-dd") : endDate.ToString("yyyy-MM-dd");
                resp.TransactionId = transactionId.ToString();

                //заполнить TransferVariant
                resp.TransferVariant = new TransferVariant()
                {
                    CarsCount = Convert.ToInt16(carsCount),
                    CarsInfo = "",
                    DetailsId = carClass,
                    PriceId = priceId,
                    Prices = MtHelper.ApplyCourses(totalPrice, MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today))
                };

                var turistsIds = new List<string>();
                //сохранить туристов
                for (int i = 0; i < turists.Length; i++)
                {
                    MtHelper.SaveTuristToCache(turists[i]);
                    turistsIds.Add(turists[i].Id.ToString());
                }

                resp.Turists = turistsIds.ToArray();

                return resp;
          
        }
    }
}