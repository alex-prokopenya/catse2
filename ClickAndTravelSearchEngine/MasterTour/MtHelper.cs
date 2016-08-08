using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Megatec.MasterTour.BusinessRules;
using Megatec.MasterTour.DataAccess;

using System.Data.Sql;
using System.Data.SqlClient;

using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.ParamsContainers;
using System.Text.RegularExpressions;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Containers.Excursions;
using ClickAndTravelSearchEngine.Containers.Transfers;
using System.Configuration;


using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.MasterTour
{
    public class MtHelper
    {
        public static string[] rate_codes = new string[] { "RUB", "USD", "EUR", "UAH", "KZT" };

        public static KeyValuePair<string, decimal>[] ApplyCourses(decimal price, KeyValuePair<string, decimal>[] courses)
        {
            price = Math.Round(price);

            KeyValuePair<string, decimal>[] res = new KeyValuePair<string, decimal>[courses.Length];

            for (int i = 0; i < courses.Length; i++)
            {
                res[i] = new KeyValuePair<string, decimal>(courses[i].Key, Math.Round(price / courses[i].Value));
                
            }

            return res;
        }

        public static KeyValuePair<string, decimal>[] GetCourses(string[] iso_codes, string base_rate, DateTime date)
        {
            //check redis cache
            string key_for_redis = "courses_"+base_rate+"b"+iso_codes.Aggregate((a,b)=> a+","+b) + "d"+date.ToString();

            KeyValuePair<string, decimal>[] res = new KeyValuePair<string, decimal>[iso_codes.Length];

            string cache = RedisHelper.GetString(key_for_redis);

            if ((cache != null)&&(cache.Length > 0))
            {
                try
                {
                    var pairs = cache.Split(';');

                    var kvps = pairs.Select<string, KeyValuePair<string, decimal>>(x => {
                       string[] arr= x.Split('=');
                       return new KeyValuePair<string, decimal>(arr[0], Convert.ToDecimal(arr[1]));
                    }).ToArray();

                    if (kvps.Length == res.Length)
                    {
                        Logger.WriteToLog("from redis" + kvps[0].Value);
                        return kvps;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + ex.StackTrace);
                }
            }

            //get data from MasterTour
            int cnt = 0;
            foreach (string code in iso_codes)
            {
                if (code != base_rate)
                    res[cnt++] = new KeyValuePair<string, decimal>(code, getCourse(code, base_rate, date));
                else
                    res[cnt++] = new KeyValuePair<string, decimal>(code, 1M);
            }
            Logger.WriteToLog("from mt");
            //конвертируем массив в строку
            var str = res.Select(kvp => String.Format("{0}={1}", kvp.Key, kvp.Value));
            string value_for_redis = string.Join(";", str);
            //save_to redis
            RedisHelper.SetString(key_for_redis, value_for_redis);

            return res;
        }

        private static decimal getCourse(string rate1, string rate2, DateTime date)
        {
            Manager.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];

            Console.WriteLine(Manager.ConnectionString);
            try
            {
                RealCourses rcs = new RealCourses(new DataCache());

                rcs.RowFilter = String.Format("RC_RCOD2 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{0}') and " +
                                              "RC_RCOD1 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{1}') and " +
                                              "RC_DATEBEG <='{2}' and RC_DATEBEG > '{3}'", rate1, rate2, date.ToString("yyyy-MM-dd"), date.AddDays(-14).ToString("yyyy-MM-dd"));

                rcs.Sort = "RC_DATEBEG desc";
                
                rcs.Fill();

                
                if (rcs.Count > 0)
                   return Convert.ToDecimal(rcs[0].Course);


                rcs.RowFilter = String.Format("RC_RCOD2 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{0}') and " +
                                              "RC_RCOD1 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{1}') and " +
                                              "RC_DATEBEG <='{2}' and RC_DATEBEG > '{3}'", rate2, rate1, date.ToString("yyyy-MM-dd"), date.AddDays(-14).ToString("yyyy-MM-dd"));

                rcs.Sort = "RC_DATEBEG desc";

                rcs.Fill();

                if (rcs.Count > 0)
                    return 1/Convert.ToDecimal(rcs[0].Course);

            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }
           
            throw new CatseException("Course not founded for date and rates " + rate1 + " " + rate2 + " " + date.ToString());
        }

        public static int SaveTuristToCache(TuristContainer tst)
        {
            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();

            SqlCommand del_com = new SqlCommand(String.Format("delete from [CATSE_Turists] where [ts_id] =" + tst.Id), con);
            del_com.ExecuteNonQuery();

            SqlCommand com = new SqlCommand(String.Format("insert into [CATSE_Turists]  ([ts_name] ,[ts_fname] ,[ts_gender] ,[ts_id] ,[ts_passport] ,[ts_passportdate] ,[ts_birthdate]  ,[ts_citizenship]) "+
                                                    "VALUES ('{0}','{1}','{2}',{3},'{4}','{5}', '{6}', '{7}')", AntiInject(tst.Name), AntiInject(tst.FirstName), (tst.Sex - 1), tst.Id,   tst.PassportNum, tst.PassportDate.ToString("yyyy-MM-dd"), tst.BirthDate.ToString("yyyy-MM-dd"), tst.Citizenship), con);
            try
            {
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                con.Close();
                return 0;
            }

            con.Close();
            return 1;
        }

        public static TuristContainer[] GetTuristsFromCache(int[] tstId)
        {
            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();

            SqlCommand com = new SqlCommand("select * from  [CATSE_Turists] where [ts_id] in (" + tstId.Aggregate("-1", (sum, value) => sum += "," + value) + ") order by ts_birthdate asc", con);

            SqlDataReader reader = com.ExecuteReader();

            List<TuristContainer> listOfTurists = new List<TuristContainer>();

            while(reader.Read())
            {
                listOfTurists.Add( new TuristContainer()
                {
                    BirthDate =  Convert.ToDateTime(reader["ts_birthdate"]),
                    Citizenship = reader["ts_citizenship"].ToString(),
                    FirstName = reader["ts_fname"].ToString(),
                    Id = Convert.ToInt32( reader["ts_id"]),
                    Name = reader["ts_name"].ToString(),
                    PassportDate = Convert.ToDateTime(reader["ts_passportdate"]),
                    PassportNum = reader["ts_passport"].ToString(),
                    Sex = Convert.ToInt32(reader["ts_gender"]) +1
                });
            }

            return tstId.Length == listOfTurists.Count ? listOfTurists.ToArray() : null; 
        }


        public static int SaveFlightToCache(string[] turists_ids, SF_service.Flight ticket, string ticket_id, string searchId) 
        {
            Random rnd = new Random();

            string tsts = turists_ids.Aggregate( (a,b)=> a.ToString()+ "," + b );

            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();
            try
            {
                
                SqlCommand com = new SqlCommand(String.Format("insert into [CATSE_Flights] ([ft_ticketid],[ft_route],[ft_date],[ft_price],[ft_turists],[ft_lastdate]) OUTPUT INSERTED.ft_id " +
                                                        "VALUES ('{0}','{1}','{2}',{3},'{4}','{5}')", AntiInject(ticket_id),
                                                                                                searchId,//   ticket.RouteItems[0].Legs[0].DepartureCode + " - " + ticket.RouteItems[0].Legs[0].ArrivalCode,
                                                                                                ticket.Parts[0].Legs[0].DateBegin.ToString("yyyy-MM-dd"), //дата ПЕРВОГО вылета
                                                                                                ticket.Price,
                                                                                                tsts,
                                                                                                ticket.Parts.Last().Legs[0].DateBegin.ToString("yyyy-MM-dd")//дата ПОСЛЕДНЕГО вылета
                                                                                                ), con);
          
                Int32 newId = (Int32)com.ExecuteScalar();

                com = new SqlCommand(String.Format("insert into [CATSE_book_id] ([service_type],[service_id]) OUTPUT INSERTED.book_id " +
                                                        "VALUES ('{0}',{1})", "CATSE_Flights", newId), con);

                Logger.WriteToLog("log before inp" + com.CommandText);
                object jnewId = com.ExecuteScalar();
                Logger.WriteToLog("log after inp" + jnewId);

                con.Close();
                return Convert.ToInt32(jnewId);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                con.Close();
                return 0;
            }
        }

        public static int SaveHotelBookingToCache(List<HotelBooking> hotelBookings)
        {

            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();

            try
            {
                SqlCommand com = new SqlCommand(String.Format("insert into [CATSE_Hotels](ht_hash) OUTPUT INSERTED.ht_id  values('{0}')", Jayrock.Json.Conversion.JsonConvert.ExportToString(hotelBookings)), con);

                Int32 newId = (Int32)com.ExecuteScalar();

                com = new SqlCommand(String.Format("insert into [CATSE_book_id] ([service_type],[service_id]) OUTPUT INSERTED.book_id " +
                                                        "VALUES ('{0}','{1}')", "CATSE_Hotels", newId), con);

                newId = Convert.ToInt32(com.ExecuteScalar());

                con.Close();
                return newId;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                con.Close();
                return 0;
            }
        }


        public static HotelBooking[] GetHotelBookingFromCache(int bookId)
        {
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();

            SqlCommand com = new SqlCommand(String.Format("select book_id, ht_hash from [CATSE_hotels], [CATSE_book_id] where [ht_id] = service_id and book_id in(" + bookId + ") and service_type='CATSE_hotels'"), con);

            SqlDataReader reader = com.ExecuteReader();

            if (reader.Read())
            {
                string hash = reader["ht_hash"].ToString();
                HotelBooking[] htlBooks = JsonConvert.Import<HotelBooking[]>(hash);

                return htlBooks;

            }

            return null;
        }
          

        public static int SaveExcursionBookingToCache(ExcursionBooking excb)
        {
            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();
            
            try
            {
                SqlCommand com = new SqlCommand(String.Format("insert into [CATSE_excursions](ex_hash) OUTPUT INSERTED.ex_id  values('{0}')", Jayrock.Json.Conversion.JsonConvert.ExportToString(excb)), con);

                Int32 newId = (Int32)com.ExecuteScalar();

                com = new SqlCommand(String.Format("insert into [CATSE_book_id] ([service_type],[service_id]) OUTPUT INSERTED.book_id " +
                                                        "VALUES ('{0}','{1}')", "CATSE_excursions", newId), con);

                newId = Convert.ToInt32(com.ExecuteScalar());

                con.Close();
                return newId;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                con.Close();
                return 0;
            }
        }

        public static int SaveTransferBookingToCache(TransferBooking trfb)
        {
            SqlConnection con = new SqlConnection();

            if (Manager.ConnectionString == null)
                con.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
            else
                con.ConnectionString = Manager.ConnectionString;

            con.Open();

            try
            {
                var str = Jayrock.Json.Conversion.JsonConvert.ExportToString(trfb);

                SqlCommand com = new SqlCommand(String.Format("insert into [CATSE_transfers](tr_hash) OUTPUT INSERTED.tr_id  values('{0}')", Jayrock.Json.Conversion.JsonConvert.ExportToString(trfb)), con);

                Int32 newId = (Int32)com.ExecuteScalar();

                com = new SqlCommand(String.Format("insert into [CATSE_book_id] ([service_type],[service_id]) OUTPUT INSERTED.book_id " +
                                                        "VALUES ('{0}','{1}')", "CATSE_transfers", newId), con);

                newId = Convert.ToInt32(com.ExecuteScalar());

                con.Close();

                return newId;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                con.Close();
                return 0;
            }
        }

        private static string AntiInject(string inp)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("'", "`");
            pairs.Add("--", "- -");
            pairs.Add(" drop ", " dr op ");
            pairs.Add("insert", "inse rt");
            pairs.Add("select", "sel ect");
            pairs.Add("delete", "dele te");
            pairs.Add("update", "up date");

            foreach (string key in pairs.Keys)
                inp = Regex.Replace(inp, key, pairs[key], RegexOptions.IgnoreCase);

            return inp;
        }
    }
}