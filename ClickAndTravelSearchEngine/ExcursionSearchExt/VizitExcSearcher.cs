using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using ClickAndTravelSearchEngine.Containers.Excursions;
using ClickAndTravelSearchEngine.MasterTour;

namespace ClickAndTravelSearchEngine.ExcursionSearchExt
{
    public class VizitExcSearcher
    {
        private int sourcePack = Convert.ToInt32(ConfigurationManager.AppSettings["VizitExcursionSourcePack"]);

        private SqlConnection _myConnection = new SqlConnection(ConfigurationManager.AppSettings["VizitConnectionString"]);

        private Dictionary<int, int> citiesLib = new Dictionary<int, int>();

        public VizitExcSearcher() 
        {
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

            citiesLib.Add(1194, 568);//будапешт
            citiesLib.Add(1463, 739);//хевиз

            citiesLib.Add(1169, 841);//лондон
            citiesLib.Add(683, 162);//мюнхен
            citiesLib.Add(1106, 726);//мадрид
            citiesLib.Add(1095, 653);//барса

            #endregion
        }

        private SqlConnection GetConnection()
        {
            if (_myConnection.State != ConnectionState.Open)
                _myConnection.Open();

            return _myConnection;
        }

        public ExcursionVariant[] SearchExcursions(int cityId, DateTime beginDate, DateTime endDate, int nMen)
        {
            if (!citiesLib.ContainsKey(cityId))
                return new ExcursionVariant[0];

            cityId = citiesLib[cityId];

            string selectQuery = " select CS_DATE, CS_DATEEND,CS_RATE, isnull(cs_type,0) as cs_type, CS_COSTNETTO, CS_COST, CS_WEEK, ED_NAME, ED_StdKey " +
                                 " FROM [dbo].tbl_Costs, dbo.ExcurDictionary, dbo.[Transport] " +
                                 " where CS_SVKEY = 4 and " +
                                 " ((CS_DATE between '{0:yyyy-MM-dd}' and '{1:yyyy-MM-dd}') or (CS_DATEend between '{0:yyyy-MM-dd}' and '{1:yyyy-MM-dd}') or ('{0:yyyy-MM-dd}' between CS_DATE and CS_DATEEND)) and "  +
                                 " CS_CODE = ED_KEY and ED_CTKEY = {2} and CS_PKKEY = {3} and not ED_StdKey = '' " +
                                 (nMen > 1? " and not ED_NAME like '%sgl%'":"") +
                                 " and cs_subcode1 = tr_key and isnull(tr_nmen,100) >= " + nMen +
                                 " order by CS_COST asc";


            selectQuery = String.Format(selectQuery, beginDate, endDate, cityId, sourcePack);


         //   Helpers.Logger.WriteToLog(selectQuery);

            var conn = GetConnection();
            var com = conn.CreateCommand();
            com.CommandText = selectQuery;

            //stdKey + price  =>  DateTime[]
            var dictionary = new Dictionary<string, List<DateTime>>();

            //
            var reader = com.ExecuteReader();

            while (reader.Read())
            {
                decimal price = Convert.ToDecimal(reader["CS_COST"]);
                string rateCode = reader["CS_RATE"].ToString();

                bool isGroup = Convert.ToInt32(reader["cs_type"]) > 0;

                if(!isGroup) 
                    price *= nMen;

                string stdKey = reader["ED_StdKey"].ToString();

                //для всех дат проверить, есть ли экскурсия
                string key = string.Format("{0}*{1}*{2}", stdKey, price, rateCode);

                //если в результатах еще нет такой экскурсии по такой цене
                if (!dictionary.ContainsKey(key)) 
                    dictionary[key] = new List<DateTime>(); //добавляем для нее пустой список дат

                //добавляем даты по найденному косту
                dictionary[key].AddRange(GetDates(beginDate,
                                                    endDate,
                                                    Convert.ToDateTime(reader["CS_DATE"]),
                                                    Convert.ToDateTime(reader["CS_DATEEnd"]),
                                                    reader["CS_WEEK"].ToString()));
            }
          //  Helpers.Logger.WriteToLog("cnt = " + dictionary.Count);

            if(dictionary.Count == 0) return new ExcursionVariant[0];

            return ConvertToExcVariants(dictionary);
        }

        private ExcursionVariant[] ConvertToExcVariants(Dictionary<string, List<DateTime>> dictionary)
        {
            //получаем курсы валют
            KeyValuePair<string, decimal>[] _courses = null;

            string lastRate = "";

            var respList = new List<ExcursionVariant>();
            //проходимся по списку и формируем ответ
            foreach (string key in dictionary.Keys)
            {
                //делим ключ, чтобы получить инфу об экскурсии
                var part = key.Split('*');
                if (part[2] == "E") part[2] = "EUR";
                //валюта стоимости поменялась, что маловероятно
                if (part[2] != lastRate)
                {
                    //запоминаем валюту, для которой выгружаем курсы
                    lastRate = part[2];
                    
                    //применяем курсы
                    _courses = MtHelper.GetCourses(MtHelper.rate_codes, lastRate, DateTime.Today);
                }

                respList.Add(
                                new ExcursionVariant()
                                {
                                    Id = Convert.ToInt32(part[0]),
                                    Prices = MtHelper.ApplyCourses(Convert.ToDecimal(part[1]), _courses),
                                    Dates = dictionary[key].ToArray()
                                }
                );
            }

        //    Helpers.Logger.WriteToLog("cnt2 = " + respList.Count);
            return respList.ToArray();
        }

        //проверяем даты экскурсии
        private DateTime[] GetDates(DateTime excDateBegin, DateTime excDateEnd, DateTime costDate, DateTime costDateEnd, string costWeek)
        {
            var dates = new List<DateTime>();

         //   Helpers.Logger.WriteToLog(excDateBegin.ToString() + excDateEnd.ToString());
            while (excDateBegin <= excDateEnd)
            {
                if (CheckDate(excDateBegin, costDate, costDateEnd, costWeek))
                    dates.Add(excDateBegin);

                excDateBegin = excDateBegin.AddDays(1);
            }

            return dates.ToArray();
        }

        //проверяем конкретную дату
        private bool CheckDate(DateTime date, DateTime costDate, DateTime costDateEnd, string costWeek) 
        {
            int day = ((int)date.DayOfWeek == 0) ? 7 : (int)date.DayOfWeek;

         //   Helpers.Logger.WriteToLog("" + day + "  - " + costWeek + date.ToString() + costDate.ToString() + costDateEnd.ToString());

            return (date >= costDate && date <= costDateEnd) && (costWeek == "" || costWeek.Contains("" + day));
        }
    }
}