//#define DEBUG
using ClickAndTravelSearchEngine.Containers.CarRent;
using ClickAndTravelSearchEngine.Containers.Excursions;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Containers.Inurance;
using ClickAndTravelSearchEngine.Containers.Transfers;
using ClickAndTravelSearchEngine.Containers.Visa;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.Helpers;
using ClickAndTravelSearchEngine.HotelSearchExt;
using ClickAndTravelSearchEngine.TransferSearchExt;
using ClickAndTravelSearchEngine.ExcursionSearchExt;
using ClickAndTravelSearchEngine.MasterTour;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Responses;
using ClickAndTravelSearchEngine.SF_service;
using Jayrock.Json.Conversion;
using Megatec.MasterTour.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web.Services;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine
{
    [ServiceContractAttribute(Namespace = "http://schemas.myservice.com")]

    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://clickandtravel.ru/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]

    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SearchEngine : System.Web.Services.WebService
    {
        private const int CHILD_AGE_LIMIT = 12;

        private const int PASSPORT_DAYS_LIMIT = 30;

        public static long HOTELS_RESULTS_LIFETIME = Convert.ToInt64(ConfigurationManager.AppSettings["HotelsResultsLifetime"]) * Convert.ToInt64(10000000);

        public SearchEngine()
            : base()
        {
            Manager.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
        }

        public static void ApplyConfig(string partnerCode) {

            if (!string.IsNullOrEmpty(partnerCode))
            {
                var settings = ConfigurationManager.GetSection(partnerCode + "Config") as NameValueCollection;

                if (settings != null)
                {
                    Logger.ApplyConfig(settings, partnerCode);
                    RedisHelper.ApplyConfig(settings, partnerCode);

                    OstrovokSearch.ApplyConfig(settings, partnerCode);
                    VizitHotelsSearch.ApplyConfig(settings, partnerCode);
                    WeAtlasExcSearcher.ApplyConfig(settings, partnerCode);
                    IWaySearcher.ApplyConfig(settings, partnerCode);
                }
                else
                    Logger.WriteToLog("not found config section " + partnerCode + "Config");
            }
        }

        #region ����� �����
        //������ ������� ����� �����
        [WebMethod]
        public RateCourse[] GetCourses(DateTime CoursesDate)
        {
            
            var _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", CoursesDate);

            List<RateCourse> rateCourses = new List<RateCourse>();

            foreach (KeyValuePair<string, decimal> cr in _courses)
                rateCourses.Add(new RateCourse() { Course = cr.Value, CurrencyFrom = "RUB", CurrencyTo = cr.Key });

            return rateCourses.ToArray();
        }
        #endregion

        #region ����� � ������������ �����������
        //������������� ������ �����������
        [WebMethod]
        public FlightInitSearchResult FlightInitSearch(int Adults, int Children, string ServiceClass, int[] ChildrenAges, Segment[] Segments, string PartnerCode)
        {
            #region check inputs
            //��������� ���� ������
            //11 - ����������� ������ ���������� ��������
            //12 - ������ � ����� ��������
            //13 - ���-�� ���������
            //14 - ������ � ��������� ����� 
            //15 - ������� �� ����������

            //��������� ���������� ��������
            if ((Adults == 0) || (Adults + Children > 6))
                return new FlightInitSearchResult() { ErrorCode = 11, ErrorMessage = "Check adults count or total turists count" };

            //��������� ���������� ������� �������� ��������
            if ((Segments.Length == 0) || (Segments.Length > 4))
                return new FlightInitSearchResult() { ErrorCode = 13, ErrorMessage = "Check route items count" };

            //��������� ���� � ���������
            DateTime startDate = DateTime.Today.AddDays(-1);
            foreach (Segment segment in Segments)
                if (segment.Date > startDate)
                    startDate = segment.Date;
                else
                    return new FlightInitSearchResult() { ErrorCode = 12, ErrorMessage = "Some route items has equals dates" };

            if (startDate > DateTime.Today.AddYears(1))
                return new FlightInitSearchResult() { ErrorCode = 12, ErrorMessage = "Date " + startDate.ToString("yyyy-MM-dd") + " is too large" };

            //��������, ���� �� ������� �����
            if (ChildrenAges.Length != Children)
                return new FlightInitSearchResult() { ErrorCode = 14, ErrorMessage = "Check children_ages array length" };

            int inf_count = 0;

            foreach (int age in ChildrenAges)
            {
                if ((age > CHILD_AGE_LIMIT) || (age < 0)) new FlightInitSearchResult() { ErrorCode = 4, ErrorMessage = "Check children_ages array" };
                if (age < 2)
                {
                    inf_count++;
                    Children--;
                }
            }

            //��������� ������� ��������
            string[] aps = new string[Segments.Length * 2];

            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i].ArrCode.ToUpper() == Segments[i].DepCode.ToUpper())
                {
                    return new FlightInitSearchResult() { ErrorCode = 15, ErrorMessage = "" };
                }

                aps[2 * i] = Segments[i].DepCode.ToUpper();
                aps[2 * i + 1] = Segments[i].ArrCode.ToUpper();
            }
            #endregion

            //init flights search
            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();
            SF_service.Route route = new SF_service.Route();

            route.Segments = new SF_service.RouteSegment[Segments.Length];

            for (int i = 0; i < Segments.Length; i++)
            {
                route.Segments[i] = new SF_service.RouteSegment()
                {
                    Date = Segments[i].Date,
                    LocationBegin = Segments[i].DepCode,
                    LocationEnd = Segments[i].ArrCode
                };
            }

            string searchId = sf_client.InitSearch(route, Adults, Children, inf_count, ServiceClass, PartnerCode);

            return new FlightInitSearchResult() { SearchId = searchId };
        }

        //������ ��������� ������
        [WebMethod]
        public FlightSearchState FlightGetSearchState(string SearchId, string PartnerCode)
        {
            //ErrorCode 17 -- �� ������ searchId
            //TODO: ��������� ������� searchId

            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            SF_service.SearchResultFlights res = sf_client.GetCurrentResultsFlights(SearchId, PartnerCode);

            if (res == null)
                return new FlightSearchState() { ErrorCode = 17, ErrorMessage = "Unknown search_id" };

            long hash = 0;

            foreach (Flight fl in res.Flights)
                hash += fl.Price;

            return new FlightSearchState()
            {
                ErrorCode = 0,
                FlightsCount = res.Flights.Length,
                Hash = hash.ToString() + res.Flights.Length.ToString(),
                IsFinished = res.IsFinished == 1
            };
        }

        private int getSearchSegmentsCount(string SearchId)
        {
            return Convert.ToInt32(Math.Floor(SearchId.Length / 10.0));
        }

        private FlightInitSearchResult FlightInitSearch(string SearchId, string PartnerCode)
        {
            try
            {
                int inf_age = 1;
                int chd_age = 11;
                int current_year = DateTime.Today.Year;

                int segm_count = getSearchSegmentsCount(SearchId);

                Segment[] segments = new Segment[segm_count];

                for (int i = 0; i < segm_count; i++)
                {
                    string segm_part = SearchId.Substring(0, 10);

                    DateTime flDate = DateTime.ParseExact(segm_part.Substring(0, 4) + current_year, "ddMMyyyy", null);

                    if (flDate <= DateTime.Today) flDate = flDate.AddYears(1);

                    segments[i] = new Segment() { ArrCode = segm_part.Substring(7, 3), DepCode = segm_part.Substring(4, 3), Date = flDate };

                    SearchId = SearchId.Remove(0, 10);
                }

                int ads_cnt = Convert.ToInt32(SearchId[1].ToString());
                int chd_cnt = Convert.ToInt32(SearchId[3].ToString());
                int inf_cnt = Convert.ToInt32(SearchId[5].ToString());
                string service_class = Convert.ToString(SearchId[7]);

                int[] ChildrenAges = new int[chd_cnt + inf_cnt].Select((x, indx) => x = indx >= inf_cnt ? chd_age : inf_age).ToArray();

                return FlightInitSearch(ads_cnt, chd_cnt + inf_cnt, service_class, ChildrenAges, segments, PartnerCode);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid search_id" + ex.Message);
            }
        }

        //������ ��������� �������
        [WebMethod]
        public FlightSearchResult FlightGetCurrentTickets(string SearchId, string PartnerCode)
        {
            //TODO: ��������� ������� searchId
            //ErrorCode 17 -- �� ������ searchId
            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            SF_service.SearchResultFlights res = sf_client.GetCurrentResultsFlights(SearchId, PartnerCode);

            if (res == null) // ���� �� ������ ���������� ��������� ������
            {
                //�������������� ����� �����
                try
                {
                    FlightInitSearchResult init_res = FlightInitSearch(SearchId, PartnerCode);
                    if (init_res.ErrorCode > 0)
                        return new FlightSearchResult() { ErrorCode = init_res.ErrorCode, ErrorMessage = init_res.ErrorMessage };

                    res = new SearchResultFlights() { Flights = new Flight[0], IsFinished = 0, RequestId = "", SearchId = 1 };
                }
                catch (Exception)
                {
                    return new FlightSearchResult() { ErrorCode = 17, ErrorMessage = "Invalid search_id" };
                }
            }

            long hash = 0;

            foreach (Flight fl in res.Flights)
                hash += fl.Price;

            FlightSearchResult result = new FlightSearchResult();
            result.ErrorCode = 0;
            result.SearchState = new FlightSearchState()
            {
                ErrorCode = 0,
                FlightsCount = res.Flights.Length,
                Hash = hash.ToString() + res.Flights.Length.ToString(),
                IsFinished = res.IsFinished == 1
            };

            result.FlightTickets = new Containers.Flights.FlightTicket[res.Flights.Length];

            int items_count = getSearchSegmentsCount(SearchId);                     //���������� ��������� �������� �������� ���� ����

            Int64 prefix = Convert.ToInt64(res.SearchId) * Convert.ToInt64(10000); //������� ��� ������������� �������

            //�������� ����� �����
            KeyValuePair<string, decimal>[] _courses = null;

            if (result.FlightTickets.Length > 0)
                _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);
            else
                return result;

            HashSet<string> ticketKeys = new HashSet<string>(); //����� ���������� �������
            List<Containers.Flights.FlightTicket> ticketsList = new List<Containers.Flights.FlightTicket>();

            for (Int64 i = 0; i < result.FlightTickets.Length; i++) //������������ ���������� ������
            {
                Containers.Flights.FlightTicket ticket = new Containers.Flights.FlightTicket(res.Flights[i], (prefix + i).ToString(), items_count, res.Flights[i].Price);

                ticket.Prices = MtHelper.ApplyCourses(res.Flights[i].Price, _courses);

                if (!ticketKeys.Contains(ticket.TicketHashKey))
                {
                    ticketsList.Add(ticket);
                    ticketKeys.Add(ticket.TicketHashKey);
                }
            }

            result.FlightTickets = ticketsList.ToArray();

            return result;
        }

        [WebMethod]
        public FlightSearchResult FlightWaitSearch(string SearchId , string PartnerCode)
        {
            //TODO: ��������� ������� searchId
            //ErrorCode 17 -- �� ������ searchId
            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            //���� ���������� ������
            int x = sf_client.WaitSearch(SearchId, PartnerCode);

            //���������� ������� ������
            return this.FlightGetCurrentTickets(SearchId, PartnerCode);
        }

        [WebMethod]
        public SegmentRule FlightGetTicketRules(string SearchId, string TicketId, string PartnerCode)
        {
            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            var rules = sf_client.GetFlightRulesByTicketId(Convert.ToInt64(TicketId), PartnerCode);

            if (rules == null)
                return new SegmentRule()
                {
                    AllowedChangesAfter = true,
                    AllowedChangesBefore = true,
                    AllowedReturnAfter = false,
                    AllowedReturnBefore = false,
                    RulesText = @"RU.RULE APPLICATION"
                };
            else
                return new SegmentRule()
                {
                    AllowedChangesAfter = rules.AllowedChangesAfter,
                    AllowedChangesBefore = rules.AllowedChangesBefore,
                    AllowedReturnAfter = rules.AllowedReturnAfter,
                    AllowedReturnBefore = rules.AllowedReturnBefore,
                    RulesText = rules.RulesText
                };

        }

        [WebMethod]
        public FlightCheckTicketResult FlightCheckTicket(string SearchId, string TicketId, string PartnerCode)
        {
            //!!!!!!!!!!!!!!!
            //TODO: ��������� ������� searchId � flightId
            //ErrorCode 17 -- �� ������ SearchId
            //ErrorCode 18 -- �� ������ TicketId
            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            SF_service.Flight res = sf_client.GetFlight(TicketId, SearchId, PartnerCode);

            int items_count = getSearchSegmentsCount(SearchId);

            KeyValuePair<string, decimal>[] _courses = null;

            if (res != null)
            {
                _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);


                return new FlightCheckTicketResult()
                {
                    ErrorCode = 0,

                    IsAvailable = true,
                    Prices = MtHelper.ApplyCourses(res.Price, _courses)
                };
            }
            else
                return new FlightCheckTicketResult()
                {
                    ErrorCode = 0,
                    IsAvailable = false,
                    Prices = new KeyValuePair<string, decimal>[0]
                };

        }

        [WebMethod]
        public Containers.Flights.FlightTicket FlightGetTicketInfo(string SearchId, string TicketId, string PartnerCode)
        {
            //!!!!!!!!!!!!!!!
            //TODO: ��������� ������� searchId � flightId
            //ErrorCode 17 -- �� ������ SearchId
            //ErrorCode 18 -- �� ������ TicketId

            SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

            SF_service.Flight res = sf_client.GetFlight(TicketId, SearchId, PartnerCode);

            int items_count = getSearchSegmentsCount(SearchId);

            KeyValuePair<string, decimal>[] _courses = null;

            if (res != null)
            {
                _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);

                Containers.Flights.FlightTicket ticket = new Containers.Flights.FlightTicket(res, TicketId, items_count, res.Price);

                ticket.Prices = MtHelper.ApplyCourses(res.Price, _courses);

                return ticket;
            }
            else
                throw new Exceptions.CatseException("Ticket not found", 18);

        }

        [WebMethod]
        public BookResult FlightBookTicket(string SearchId, string TicketId, UserInfo Info, TuristContainer[] turists, string PartnerCode)
        {
            try
            {

                // //ErrorCode 18 -- �� ������ TicketId
                //TODO: ��������� ������ �������� � ������ init search �� ������������ ���������� ����� � ��������, ��������
                //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
                // //ErrorCode 95: ������ � ����� �������� ��������
                // //ErrorCode 96: ����������� ������

                SF_serviceSoapClient sf_client = new SF_serviceSoapClient();

                SF_service.Flight res = sf_client.GetFlight(TicketId, SearchId, PartnerCode);

                KeyValuePair<string, decimal>[] _courses = null;

                if (res != null)
                    _courses = MtHelper.GetCourses(MtHelper.rate_codes, "RUB", DateTime.Today);
                else
                    throw new CatseException("unknown ticket", ErrorCodes.UnknownTicketId);

                int items_count = getSearchSegmentsCount(SearchId);
                Containers.Flights.FlightTicket flightTemp = new Containers.Flights.FlightTicket(res, "", items_count, res.Price);

                DateTime lastFlightDate = flightTemp.RouteItems.Last().Legs.Last().ArrivalTime;

                //��������� ��������� ��������
                List<string> turistsIds = new List<string>();
                List<string> turistsHashes = new List<string>();

                int ads = 0;
                int chd = 0;
                int inf = 0;

                for (int i = 0; i < turists.Length; i++)
                {
                    int turistAge = turists[i].GetAge(lastFlightDate);

                    if (turistAge > 12)
                        ads++;
                    else if (turistAge > 1)
                        chd++;
                    else
                        inf++;

                    MtHelper.SaveTuristToCache(turists[i]);

                    if ((turists[i].PassportDate > new DateTime(1970, 1, 2)) && (turists[i].PassportDate < lastFlightDate.AddDays(PASSPORT_DAYS_LIMIT)))
                        throw new CatseException("pasport date limit", ErrorCodes.TuristCanntPaspDate);

                    string turistHash = turists[i].HashSumm;

                    if ((!turistsIds.Contains(turists[i].Id.ToString())) && (!turistsIds.Contains(turistHash)))
                    {
                        turistsIds.Add(turists[i].Id.ToString());
                        turistsHashes.Add(turistHash);
                    }
                    else
                        throw new CatseException("turist duplcate", ErrorCodes.TuristDuplicate);
                }

                string check_string = "-" + ads + "-" + chd + "-" + inf + "-";

                Helpers.Logger.WriteToLog("check " + SearchId + " in " + check_string);

                if (!SearchId.Contains(check_string))
                    throw new CatseException("Wrong Turists Ages", ErrorCodes.WrongTuristAges);
                
                List<Passenger> pasangers = new List<Passenger>();

                foreach (TuristContainer tsc in turists)
                {
                    pasangers.Add(new Passenger()
                    {
                        Birth = tsc.BirthDate,
                        Citizen = tsc.Citizenship,
                        Fname = tsc.Name,
                        Name = tsc.FirstName,

                        Gender = ((tsc.Sex == 1) ? "M" : "F"),
                        PassportExpireDate = (tsc.PassportDate < DateTime.Today ? new DateTime(2050, 1, 1) : tsc.PassportDate),
                        Pasport = tsc.PassportNum,
                        FrequentFlyerAirline = "",
                        FrequentFlyerNumber = ""
                    });
                }

                var bookResult = sf_client.BookFlight(TicketId, //������������ ��������
                        new Customer() { Mail = "info@clickandtravel.ru", Name = "Irina", Phone = "4957846256" },
                        pasangers.ToArray(),
                        lastFlightDate,
                        PartnerCode
                    );

                TicketId = bookResult.code;
                Logger.WriteToInOutLog("book result " + bookResult.code);

                if (TicketId == "Exception")
                    return new BookResult()
                    {
                        BookingNumber = 0,
                        Prices = new KeyValuePair<string, decimal>[0]
                    };

                if (TicketId == "can not book")
                    return new BookResult()
                    {
                        BookingNumber = 0,
                        Prices = new KeyValuePair<string, decimal>[0]
                    };

                return new BookResult()
                {
                    BookingNumber = MtHelper.SaveFlightToCache(turistsIds.ToArray(), res, TicketId, SearchId),
                    Prices = MtHelper.ApplyCourses(res.Price, _courses)
                };

            }
            catch (CatseException ex)
            {
                Logger.WriteToInOutLog("exception in booking " + ex.Code);
                return new BookResult()
                {
                    
                    ErrorCode = ex.Code,
                    ErrorMessage = ex.Message
                };
            }
        }
        #endregion

        #region ����� ���������

        //����� ��������� �� ������
        [WebMethod]
        public ExcursionSearchResult ExcursionSearch(int CityId, DateTime MinDate, DateTime MaxDate, int TuristsCount)
        {
            //TODO: ��������� ��� ������, ��������� ����, ���������� ��������
            //ErrorCode 41: ����� ����������
            //ErrorCode 42: ������ � ����� (������ �������, ������ ������� + ���, ����������� ���� ������ ������������)
            //ErrorCode 43: �������� ������ ���� ������ 0 � �� ������ 10

            string searchId = "";

            #region generate_search_id
            searchId = MinDate.ToString("ddMM") + MaxDate.ToString("ddMM") + "-" + CityId + "-" + TuristsCount;

            #endregion

            #region ��������� �������� �� ����
            string redis_verify_key = "res_" + searchId;

            string redis_hash = RedisHelper.GetString(redis_verify_key);

            if ((redis_hash != null) && (redis_hash.Length > 0))
                return JsonConvert.Import<ExcursionSearchResult>(redis_hash);
            #endregion


            if ((MaxDate < MinDate) || (MaxDate < DateTime.Today) || (MaxDate > DateTime.Today.AddYears(1)))
                throw new Exceptions.CatseException("Check dates", 42);

            if ((TuristsCount < 1) || (TuristsCount > 10))
                throw new Exceptions.CatseException("Check turists count", 43);

            ExcursionSearchResult resp = null;

            resp = new ExcursionSearchResult()
            {
                SearchId = searchId,
                ExcursionVariants = new WeAtlasExcSearcher().SearchExcursions(CityId, MinDate, MaxDate, TuristsCount)
            };
 
            RedisHelper.SetString("res_" + searchId, JsonConvert.ExportToString(resp), new TimeSpan(HOTELS_RESULTS_LIFETIME));

            RedisHelper.SetString("res_book_" + searchId, JsonConvert.ExportToString(resp), new TimeSpan(3 * HOTELS_RESULTS_LIFETIME));

            return resp;
        }

        //��������� "��������" ��������� ��� ������
        private static ExcursionSearchResult ReturnFakeExcursionsResult(DateTime MinDate, DateTime MaxDate, string searchId)
        {
            return null;
        }

        [WebMethod]
        public ExcursionDatesResult ExcursionGetDates(string SearchId, int ExcursionId)
        {
            //������ ����
            var minDate = DateTime.ParseExact(SearchId.Substring(0, 4) + DateTime.Today.Year, "ddMMyyyy", null);

            if (minDate < DateTime.Today)
                minDate = minDate.AddYears(1);

            var maxDate = DateTime.ParseExact(SearchId.Substring(4, 4) + DateTime.Today.Year, "ddMMyyyy", null);

            if (maxDate < minDate)
                maxDate = maxDate.AddYears(1);

            JsonArray res = null;

            res = new WeAtlasExcSearcher().GetExcursionCalendar(ExcursionId, minDate, maxDate, Convert.ToInt32(SearchId.Split('-').Last()));


            return new ExcursionDatesResult() { excursionDates = res };
        }


        private static string MakeExcursionOrder(string offerId, int activityId, DateTime startDate, string startTime, TuristContainer[] turists)
        {
            if (activityId > 10000000) //� ��������� c ����� id ������� WeAtlas
                return new WeAtlasExcSearcher().MakeOrder(offerId, activityId, startDate, startTime, turists);

            return "";
        }

        [WebMethod]
        public BookResult ExcursionBook(string SearchId, int ExcursionId, string OfferId, DateTime excursionDate, string time, UserInfo Info, TuristContainer[] turists)
        {
            //ErrorCode 44 -- ������������ ���� ���������

            //ErrorCode 47 -- �� ������ SearchId
            //ErrorCode 48 -- �� ������ ExcursionId

            //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������

            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������
            try
            {

                if (DateTime.Today >= excursionDate)
                    throw new CatseException("wrong date", 44);

                #region ��������� �������� �� ����
                string redis_verify_key = "res_book_" + SearchId;

                string redis_hash = RedisHelper.GetString(redis_verify_key);

                if (String.IsNullOrEmpty(redis_hash))
                    throw new CatseException("unknown searchId", 47);

                var exRes = JsonConvert.Import<ExcursionSearchResult>(redis_hash);

                foreach (ExcursionVariant vr in exRes.ExcursionVariants)
                    if (vr.Id == ExcursionId)
                    {

                        string orderNum = MakeExcursionOrder(OfferId, ExcursionId, excursionDate, time, turists);

                        //��������� ��������
                        List<string> turistsIds = new List<string>();
                        List<string> turistsHashes = new List<string>();

                        for (int i = 0; i < turists.Length; i++)
                        {
                            MtHelper.SaveTuristToCache(turists[i]);

                            if ((turists[i].PassportDate > new DateTime(1970, 1, 2)) && (turists[i].PassportDate < excursionDate.AddDays(PASSPORT_DAYS_LIMIT)))
                                throw new CatseException("pasport date limit", ErrorCodes.TuristCanntPaspDate);

                            string turistHash = turists[i].HashSumm;

                            if ((!turistsIds.Contains(turists[i].Id.ToString())) && (!turistsIds.Contains(turistHash)))
                            {
                                turistsIds.Add(turists[i].Id.ToString());
                                turistsHashes.Add(turistHash);
                            }
                            else
                                throw new CatseException("turist duplcate", ErrorCodes.TuristDuplicate);
                        }

                        //��������� ���������
                        return new BookResult()
                        {
                            BookingNumber = MtHelper.SaveExcursionBookingToCache(new ExcursionBooking()
                            {
                                ExcVariant = vr,
                                SearchId = SearchId + "/" + orderNum,
                                SelectedDate = excursionDate.ToString("yyyy-MM-dd"),
                                Turists = turistsIds.ToArray()
                            }),
                            Prices = vr.Prices
                        };
                    }
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }
            #endregion
            throw new CatseException("unknown ExcursionId", 48);
        }

        #endregion

        #region ��������

        //��� ��������� ��������� ����� ��������� ��������� �������� �����
        [WebMethod]
        public int[] TransferGetPoints(int startPointId)
        {
            //ErrorCode 21: ����������� ����� ��������
            return IWaySearcher.GetPoints(startPointId);
        }

        private string GenerateSearchId(int startPointId, int endPointId, DateTime transferDate, DateTime returnDate, int turistsCount)
        {
            string searchId = "t-";

            if (startPointId < 1 || endPointId < 1)
                throw new CatseException("����������� ����� ��������", 21);
            else
                searchId += startPointId + "-" + endPointId;


            if (transferDate > DateTime.Today.AddYears(1) || transferDate < DateTime.Today)
                throw new CatseException("������ � �����", 23);

            if ((returnDate > DateTime.MinValue) && (returnDate < DateTime.Today && returnDate > DateTime.Today.AddYears(1)))
                throw new CatseException("������ � �����", 23);

            searchId += "-" + transferDate.ToString("ddMM") + "-";

            if (returnDate > DateTime.MinValue)
                searchId += returnDate.ToString("ddMM");

            searchId += "-" + turistsCount;

            return searchId;
        }

        //����� ��������� �� ����� � � ����� B 
        [WebMethod]
        public TransferSearchResult TransferSearch(int startPointId,
                                                    int endPointId,
                                                    DateTime transferDate,
                                                    DateTime returnDate,
                                                    int turistsCount)
        {
            //ErrorCode 21: ����������� ����� ��������
            //ErrorCode 22: ������������� ����� ��������
            //ErrorCode 23: ������ � ����� (������ �������, ������ ������� + ���, ����������� ���� ������ ������������)
            //ErrorCode 24: ���������� �������� ������ 6

            return new TransferSearchResult()
            {
                SearchId = GenerateSearchId(startPointId, endPointId, transferDate, returnDate, turistsCount),
                Variants = IWaySearcher.GetPriceVariants(startPointId, endPointId, turistsCount, returnDate > new DateTime(2010, 10, 10))
            };
        }

        [WebMethod]
        public object TransferGetInfoMask(string transferID)
        {
            return IWaySearcher.GetInfoMasks(transferID);
        }


        //������������ ���������
        [WebMethod]
        public BookResult TransferBook(string searchId, string transferId, TransferInfo[] transferInfo, UserInfo userInfo, TuristContainer[] turists)
        {
            //ErrorCode 27: �� ������ SearchId
            //ErrorCode 28: �� ������ TransferId
            //ErrorCode 29: �� �������� TransferInfo
            //ErrorCode 90: ������������ ������ �������� (�������� ����������)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������
            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������
            //transferInfo[0]

            //�������� ���� �� searchId
            string[] parts = searchId.Split('-');

            DateTime startDate = DateTime.ParseExact(parts[3] + DateTime.Today.Year, "ddMMyyyy", null);

            if (startDate < DateTime.Today)
                startDate = startDate.AddYears(1);

            DateTime endDate = DateTime.MinValue;

            if (parts[4] != "")
            {
                endDate = DateTime.ParseExact(parts[4] + startDate.Year, "ddMMyyyy", null);

                if (endDate < startDate)
                    endDate = endDate.AddYears(1);
            }

            //������� �����
            var tmpBooking = IWaySearcher.BookTransfer(startDate, endDate, transferId, transferInfo, userInfo, turists, searchId);

            return new BookResult()
            {
                BookingNumber = MtHelper.SaveTransferBookingToCache(tmpBooking),
                Prices = tmpBooking.TransferVariant.Prices
            };
        }
        #endregion

        #region �����
        [WebMethod]
        public HotelInitSearchResult HotelInitSearh(int CityId, DateTime StartDate, DateTime EndDate,
                                                    int[] Stars, int[] Pansions, RequestRoom[] Rooms, 
                                                    string _searchId = "")
        {
            //ErrorCode 31: ����������� �����
            //ErrorCode 32: ������ � �����
            //ErrorCode 33: ������ � ������� ���������� (����������� ����)
            //ErrorCode 34: ������ � ������� ������� (����������� ����)

            //ErrorCode 36: ������ �� �������� � ������, ���������� ��������, �����, ������ ������� �������� ����� (���������� ���������, �������� ��������) -- ����� � ������ �� 6 �������, �������� > 0 

            #region check_params

            //ErrorCode 35: �������� ���������� ������ (������ ���� 1 ��� 2)

            if ((Rooms.Length > 2) || (Rooms.Length < 1))
                throw new CatseException("check rooms count", ErrorCodes.HotelRoomsInvalidCount);

            #endregion

            string searchId = _searchId;

            if (searchId == "")
            {
                #region generate_search_id
                searchId = StartDate.ToString("ddMM") + EndDate.ToString("ddMM") + CityId + "-";

                if (Stars.Length == 0)
                    searchId += "0";
                else
                    foreach (int star in Stars) searchId += star;

                searchId += "-";

                if (Pansions.Length == 0)
                    searchId += "0";
                else
                    foreach (int pansion in Pansions) searchId += pansion;

                foreach (RequestRoom room in Rooms)
                {
                    searchId += "-" + room.Adults + "" + room.Children;

                    foreach (int age in room.ChildrenAges)
                        searchId += (age < 10 ? "0" : "") + age;
                }
                #endregion
            }

            string searchState = RedisHelper.GetString(searchId + "_state");
            if ((searchState == null) || (searchState.Length == 0))
            {
                //�������� ����� � ����� ������
                //�������� ����� �����
                Logger.WriteToLog("init_hotel_search");
                Thread searchThread = new Thread(() => FindHotels(CityId, StartDate, EndDate, Stars, Pansions, Rooms, searchId));
                searchThread.Start();

                //����� � ����� ���� � ������� ������
                RedisHelper.SetString(searchId + "_state", new HotelSearchState() { IsFinished = false, Hash = "00", HotelsCount = 0 }.ToJsonString(), new TimeSpan(HOTELS_RESULTS_LIFETIME));
            }

            return new HotelInitSearchResult() { SearchId = searchId };
        }

        private void FindHotels(int CityId, DateTime StartDate, DateTime EndDate, 
                                int[] Stars, int[] Pansions, RequestRoom[] Rooms, 
                                string SearchId)
        {
            try
            {
                HotelSearchResult res = new HotelSearchResult()
                {
                    FoundedHotels = new Hotel[0]
                };

                List<IHotelExt> searchers = new List<IHotelExt>(); //������ � ���������

                if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
                    searchers.Add(new VizitHotelsSearch(CityId, StartDate, EndDate, Stars, Pansions, Rooms[0], SearchId, SearchEngine.HOTELS_RESULTS_LIFETIME));

                if (ConfigurationManager.AppSettings["SearchHotelsInOstrovok"] == "true")
                    searchers.Add(new ClickAndTravelSearchEngine.HotelSearchExt.OstrovokSearch(CityId, StartDate, EndDate, Stars, Pansions, Rooms[0], SearchId, SearchEngine.HOTELS_RESULTS_LIFETIME));

                foreach (IHotelExt searcher in searchers)
                    new Thread(new ThreadStart(searcher.FindHotels)).Start();

                int max_iterations_cnt = 60;

                while (true)//�������� �� ���� ��������
                {
                    Logger.WriteToLog("iteration " + (60 - max_iterations_cnt) + " started");

                    Thread.Sleep(1000);

                    bool need_wait = false; // ������� ����, ��� ����� ��������

                    List<Hotel> hotelsToAdd = new List<Hotel>();

                    foreach (IHotelExt searcher in searchers) //���������� �� ��������
                    {
                        if (searcher.GetAdded() == false) // ���� ������ �� ���������
                        {
                            Hotel[] findedHotels = searcher.GetHotels();
                            if (findedHotels != null) //���� ���-�� �������
                            {
                                if (findedHotels.Length > 0)//���� ���-�� ������� � ������� ����������!!!
                                {
                                    hotelsToAdd.AddRange(findedHotels);
                                    //��������� ���� "���������"
                                }

                                if (searcher.GetFinished())
                                    searcher.SetAdded();//��������, ��� ����� �� ������� ��������
                            }
                            need_wait = true;
                        }
                    }

                    need_wait = (max_iterations_cnt-- > 0) && need_wait; //���������, ����������� �� �������

                    if (hotelsToAdd.Count > 0)
                        res = ComposeResults(res.FoundedHotels, hotelsToAdd.ToArray(), !need_wait); //���������� �� ������ ����������� 
                    else if (!need_wait)
                        res = ComposeResults(res.FoundedHotels, new Hotel[0], true);

                    if ((hotelsToAdd.Count > 0) || (!need_wait))
                    {
                        //��������� � �����
                        RedisHelper.SetString(SearchId + "_result", JsonConvert.ExportToString(res), new TimeSpan(HOTELS_RESULTS_LIFETIME));
                        RedisHelper.SetString(SearchId + "_state", JsonConvert.ExportToString(res.SearchState), new TimeSpan(HOTELS_RESULTS_LIFETIME));
                    }

                    Logger.WriteToLog("iteration " + (60 - max_iterations_cnt - 1) + " done");

                    if (!need_wait) break; // ���� �� ����� ������ ����� -- ������
                }

                res = null;
                searchers = null;

                GC.Collect();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "" + ex.StackTrace);
            }
        }


        private Room FindHotelRooms(int CityId, DateTime StartDate, DateTime EndDate,
                                    int[] Stars, int[] Pansions, RequestRoom Room,
                                    string SearchId, int HotelId)
        {

            Room res = null;

            try
            {
                List<IHotelExt> searchers = new List<IHotelExt>(); //������ � ���������

                if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
                    searchers.Add(new VizitHotelsSearch(CityId, StartDate, EndDate, Stars, Pansions, Room, SearchId, SearchEngine.HOTELS_RESULTS_LIFETIME));
                else
                    Logger.WriteToLog("not setted vizit");

                if (ConfigurationManager.AppSettings["SearchHotelsInOstrovok"] == "true")
                    searchers.Add(new ClickAndTravelSearchEngine.HotelSearchExt.OstrovokSearch(CityId, StartDate, EndDate, Stars, Pansions, Room, SearchId, SearchEngine.HOTELS_RESULTS_LIFETIME));
                else
                    Logger.WriteToLog("not setted ostrovok");

                foreach (IHotelExt searcher in searchers)
                    new Thread(new ParameterizedThreadStart(searcher.FindRooms)).Start(HotelId);

                Logger.WriteToLog(searchers.Count + " searchers started");

                int max_iterations_cnt = 10;

                while (true)//�������� �� ���� ��������
                {
                    Logger.WriteToLog("iteration " + (10 - max_iterations_cnt) + " started");

                    Thread.Sleep(1000);

                    bool need_wait = false; // ������� ����, ��� ����� ��������

                    foreach (IHotelExt searcher in searchers)
                    {
                        if (searcher.GetAdded() == false) // ���� ������ �� ���������
                        {
                            var findedRoom = searcher.GetRoom();

                            if (findedRoom != null) //���� ���-�� �������
                            {
                                if (findedRoom.Variants.Length > 0)//���� ���-�� �������
                                {
                                    res = ComposeRoomsResults(res, findedRoom);

                                    Logger.WriteToLog("rf =  " + findedRoom.Variants.Length);
                                }

                                searcher.SetAdded();//��������, ��� ����� �� ������� ��������
                            }
                            need_wait = true;
                        }
                    }

                    need_wait = (max_iterations_cnt-- > 0) && need_wait; //���������, ����������� �� �������

                    Logger.WriteToLog("iteration " + (10 - max_iterations_cnt - 1) + " done");

                    if (!need_wait) break; // ���� �� ����� ������ ����� -- ������
                }

                //�������� � �����
                RedisHelper.SetString(SearchId + "_hotel_rooms_" + HotelId, JsonConvert.ExportToString(res), new TimeSpan(HOTELS_RESULTS_LIFETIME));
                //������ ���������
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " \n" + ex.StackTrace + " " + ex.Source);
            }

            return res;
        }

        private Room ComposeRoomsResults(Room oldRoom, Room newRoom)
        {
            if (oldRoom == null) return newRoom;

            if (oldRoom.Equals(newRoom))
            {
                var roomsDict = new Dictionary<string, RoomVariant>();

                foreach (RoomVariant rVar in oldRoom.Variants)
                    roomsDict[rVar.RoomTitle + "_" + rVar.RoomCategory + "_" + rVar.PansionTitle] = rVar;

                foreach (RoomVariant rVar in newRoom.Variants)
                {
                   if ((!roomsDict.ContainsKey(rVar.RoomTitle + "_" + rVar.RoomCategory + "_" + rVar.PansionTitle)) ||
                            (roomsDict[rVar.RoomTitle + "_" + rVar.RoomCategory + "_" + rVar.PansionTitle].Prices[0].Value > rVar.Prices[0].Value))

                            roomsDict[rVar.RoomTitle + "_" + rVar.RoomCategory + "_" + rVar.PansionTitle] = rVar;
                }
                oldRoom.Variants = roomsDict.Values.ToArray();
             }
             else
                Logger.WriteToLog("equals room not founded");

           return oldRoom;
        }

        private HotelSearchResult ComposeResults(Hotel[] oldArray, Hotel[] newArray, bool is_finished)
        {
            Dictionary<int, Hotel> hotelsDict = new Dictionary<int, Hotel>();

            decimal hash = 0m;

            foreach (Hotel item in oldArray)
            {
                hotelsDict[item.HotelId] = item;
                hash += item.Rooms[0].Variants[0].Prices[0].Value;
            }

            foreach (Hotel item in newArray)
            {
                if (hotelsDict.ContainsKey(item.HotelId))
                {
                    Hotel old_item = hotelsDict[item.HotelId];

                    if ((old_item.Rooms[0].Variants[0].Prices[0].Key == item.Rooms[0].Variants[0].Prices[0].Key)
                        &&
                        (old_item.Rooms[0].Variants[0].Prices[0].Value > item.Rooms[0].Variants[0].Prices[0].Value))
                    {
                        hotelsDict[item.HotelId] = item;
                        hash += item.Rooms[0].Variants[0].Prices[0].Value;
                        hash -= old_item.Rooms[0].Variants[0].Prices[0].Value;
                    }
                }
                else
                {
                    hotelsDict[item.HotelId] = item;
                    hash += item.Rooms[0].Variants[0].Prices[0].Value;
                }
            }

            return new HotelSearchResult()
            {
                SearchState = new HotelSearchState()
                {
                    IsFinished = is_finished,
                    HotelsCount = hotelsDict.Values.Count,
                    Hash = "" + hotelsDict.Values.Count + "" + hash
                },
                FoundedHotels = hotelsDict.Values.ToArray()
            };
        }

        [WebMethod]
        public HotelSearchState HotelGetSearchState(string SearchId)
        {
            string stateFromCache = RedisHelper.GetString(SearchId + "_state");

            if ((stateFromCache == null) || (stateFromCache.Length == 0)) //ErrorCode 37: ����������� searchId
                throw new CatseException("unknown seachId", ErrorCodes.HotelUnknownSearchId);

            try
            {
                return JsonConvert.Import<HotelSearchState>(stateFromCache);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                throw new CatseException("unknown seachId", ErrorCodes.HotelUnknownSearchId);
            }
        }

        [WebMethod]
        public object HotelGetCurrentHotels(string SearchId)
        {
            try
            {
                string stateFromCache = RedisHelper.GetString(SearchId + "_state");

                if ((stateFromCache != null) && (stateFromCache.Length > 0)) //���� � ���� ���� ��������� ��� �������� �������
                {
                    //��������� ���������
                    HotelSearchState currentSate = JsonConvert.Import<HotelSearchState>(stateFromCache); //������ ������� ��������� �� ����

                    if (currentSate.HotelsCount > 0)
                    {
                        HotelSearchResult res = JsonConvert.Import<HotelSearchResult>(RedisHelper.GetString(SearchId + "_result"));

                        return res;
                    }
                    else
                        return new HotelSearchResult()
                        {
                            SearchState = currentSate,

                            FoundedHotels = new Hotel[0]
                        };
                }
                else
                {
                    //������ ���� ����
                    Hashtable pHash = ParseHotelSearchId(SearchId);


                    //�������� �����
                    HotelInitSearh((int)pHash["cityId"],
                                  (DateTime)pHash["dateStart"],
                                  (DateTime)pHash["dateEnd"],
                                  pHash["stars"] as int[],
                                  pHash["pansions"] as int[],
                                  pHash["rooms"] as RequestRoom[],
                                  SearchId);

                    //������ ������ ���������
                    return new HotelSearchResult()
                    {
                        SearchState = new HotelSearchState() { IsFinished = false, HotelsCount = 0, Hash = "00" },
                        FoundedHotels = new Hotel[0]
                    };
                }
            }
            catch (CatseException ex)
            {
                throw ex;
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                throw new CatseException("unknown search_id", ErrorCodes.HotelUnknownSearchId);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                throw new CatseException("unknown search_id", ErrorCodes.HotelUnknownSearchId);
            }
        }

        [WebMethod]
        public Hotel[] HotelWaitSearch(string SearchId)
        {
            throw new CatseException("method deprecated");
            //ErrorCode 37: ����������� searchId
            return new Hotel[]{

                            new Hotel(){

                            HotelId = 123,
                            Rooms = new Room[]{

                                new Containers.Hotels.Room(){
                                    Adults = 1,
                                    Children = 2,
                                    ChildrenAges = new int[]{10, 12},
                                    Variants = new RoomVariant[]
                                    {
                                        new RoomVariant()
                                        {
                                            PansionGroupId = 1,
                                            PansionTitle = "�������",
                                            Prices = new KeyValuePair<string,decimal>[]
                                            {
                                              ///  new KeyValuePair<string,decimal>("BYR", 123550M),
                                                new KeyValuePair<string,decimal>("USD", 15M),
                                                new KeyValuePair<string,decimal>("EUR", 11M)
                                            },
                                            RoomCategory = "Standard",
                                            RoomTitle = "DBL",
                                            VariantId = ""
                                        },
                                        new RoomVariant()
                                        {
                                            PansionGroupId = 2,
                                            PansionTitle = "��� ��������",
                                            Prices = new KeyValuePair<string,decimal>[]
                                            {
                                               // new KeyValuePair<string,decimal>("BYR", 5*123550M),
                                                new KeyValuePair<string,decimal>("USD", 5*15M),
                                                new KeyValuePair<string,decimal>("EUR", 5*11M)
                                            },
                                            RoomCategory = "Standard",
                                            RoomTitle = "DBL",
                                            VariantId = ""
                                        }
                                    }
                                }
                            }
                     }
            };
        }

        //�������� ������� �� �����
        [WebMethod]
        public Room[] HotelGetRooms(string SearchId, int HotelId)
        {
            //��������� ������� searchId � �����������
            // HotelSearchState state = HotelGetSearchState(SearchId);
            // if(state.HotelsCount == 0)
            //     throw new CatseException("unknown seachId", ErrorCodes.HotelSerchIdNotFound);
            //ErrorCode 37: ����������� searchId

            /*
                ���������� �� �������� ������� SearchId � �����, �.�. ���������� �� ����������, SearchId ���� ����������� ������ �����
            */
            try
            {
                string savedRooms = RedisHelper.GetString(SearchId + "_hotel_rooms_" + HotelId);

                if ((savedRooms != null) && (savedRooms.Length > 0))
                    return JsonConvert.Import<Room[]>(savedRooms);
            }
            catch (Exception)
            {
                Logger.WriteToLog("load from redis failed for " + SearchId + "_hotel_rooms_" + HotelId + " key");
            }

            //���������� ��������
            Hashtable pHash = ParseHotelSearchId(SearchId);

            //������� ������������� ����� ������� � ����������� ���������
            return new Room[] { FindHotelRooms((int)pHash["cityId"],
                                              (DateTime)pHash["dateStart"],
                                              (DateTime)pHash["dateEnd"],
                                              pHash["stars"] as int[],
                                              pHash["pansions"] as int[],
                                              (pHash["rooms"] as RequestRoom[])[0],
                                              SearchId, HotelId) };
        }

        public Room HotelGetRoomInfo(string SearchId, int HotelId, string VariantId)
        {
            try
            {
                string savedRooms = RedisHelper.GetString(SearchId + "_hotel_rooms_" + HotelId);

                if ((savedRooms == null) || (savedRooms.Length == 0))
                {
                    HotelGetRooms(SearchId, HotelId);
                    savedRooms = RedisHelper.GetString(SearchId + "_hotel_rooms_" + HotelId);
                }

                if ((savedRooms != null) && (savedRooms.Length > 0))
                {
                    Room[] rooms = JsonConvert.Import<Room[]>(savedRooms);

                    Room targetRoom = null;
                    RoomVariant targetVariant = null;

                    foreach (Room room in rooms)
                    {
                        foreach (RoomVariant rv in room.Variants)
                        {
                            if (rv.VariantId == VariantId)
                            {

                                targetRoom = room;
                                targetVariant = rv;
                                break;
                            }
                        }
                    }

                    if (targetVariant != null)
                        targetRoom.Variants = new RoomVariant[] { targetVariant };

                    return targetRoom;
                }
            }
            catch (Exception)
            {
                Logger.WriteToLog("load from redis failed for " + SearchId + "_hotel_rooms_" + HotelId + " key");
            }

            //������� ������������� ����� ������� � ����������� ���������
            return null;
        }
        private Hashtable ParseHotelSearchId(string SearchId) //throws invalid search id CatseException
        {
            Hashtable res = new Hashtable();

            #region DATES

            string strStartDate = SearchId.Substring(0, 4); //���� ���������
            string strEndDate = SearchId.Substring(4, 4); //���� ���������

            DateTime dateStart = DateTime.Now;
            DateTime dateEnd = DateTime.Now;

            dateStart = DateTime.ParseExact(strStartDate + DateTime.Today.ToString("yyyy"), "ddMMyyyy", null);
            dateEnd = DateTime.ParseExact(strEndDate + DateTime.Today.ToString("yyyy"), "ddMMyyyy", null);


            if (dateStart < DateTime.Today)
                dateStart = dateStart.AddYears(1);

            if (dateEnd < dateStart)
                dateEnd = dateEnd.AddYears(1);


            res["dateStart"] = dateStart;
            res["dateEnd"] = dateEnd;
            #endregion

            int cityId = Convert.ToInt32(SearchId.Substring(8, SearchId.IndexOf("-") - 8)); //id ������

            res["cityId"] = cityId;

            string[] parts = SearchId.Split('-');

            #region STARS
            //����������
            List<int> stars = new List<int>();
            if (parts[1] != "")
                for (int i = 0; i < parts[1].Length; i++)
                    stars.Add(Convert.ToInt32("" + parts[1][i]));

            res["stars"] = stars.ToArray();
            #endregion

            #region PANSION
            //�������
            List<int> pansions = new List<int>();
            if (parts[2] != "")
                for (int i = 0; i < parts[2].Length; i++)
                    pansions.Add(Convert.ToInt32("" + parts[2][i]));

            res["pansions"] = pansions.ToArray();
            #endregion

            #region ROOMS
            RequestRoom[] rooms = new RequestRoom[parts.Length - 3];

            for (int i = 3; i < parts.Length; i++) //���������� �� ��������
            {
                //parse room
                int ads = Convert.ToInt32("" + parts[i][0]);    //������ ���������� ��������
                int chd = Convert.ToInt32("" + parts[i][1]);   //�����

                int[] chd_ages = new int[chd];

                parts[i] = parts[i].Substring(2);

                if (chd > 0)
                    for (int j = 0; j < chd; j++)
                    {
                        string age_part = parts[i].Substring(2 * j, 2);

                        chd_ages[j] = Convert.ToInt32((age_part[0] == '0') ? age_part.Substring(1) : age_part);
                    }

                rooms[i - 3] = new RequestRoom()
                {
                    Adults = ads,
                    Children = chd,
                    ChildrenAges = chd_ages
                };
            }

            res["rooms"] = rooms;
            #endregion

            return res;
        }

        //���������� � ������� �� ������ � �����
        [WebMethod]
        public HotelPenalties[] HotelGetPenalties(string SearchId, int HotelId, string[] VariantsId)
        {
            //parse search_id
            Hashtable pHash = ParseHotelSearchId(SearchId);

            Dictionary<string, IHotelExt> searchers = new Dictionary<string, IHotelExt>();

            Logger.WriteToLog(VizitHotelsSearch.id_prefix);

            if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
                searchers.Add(VizitHotelsSearch.id_prefix, new VizitHotelsSearch((int)pHash["cityId"],
                                                            (DateTime)pHash["dateStart"],
                                                            (DateTime)pHash["dateEnd"],
                                                            pHash["stars"] as int[],
                                                            pHash["pansions"] as int[],
                                                            (pHash["rooms"] as RequestRoom[])[0],
                                                            SearchId, HOTELS_RESULTS_LIFETIME
                    ));
          
            if (ConfigurationManager.AppSettings["SearchHotelsInOstrovok"] == "true")
                searchers.Add(OstrovokSearch.id_prefix, new OstrovokSearch((int)pHash["cityId"],
                                                            (DateTime)pHash["dateStart"],
                                                            (DateTime)pHash["dateEnd"],
                                                            pHash["stars"] as int[],
                                                            pHash["pansions"] as int[],
                                                            (pHash["rooms"] as RequestRoom[])[0],
                                                            SearchId, HOTELS_RESULTS_LIFETIME
                    ));

            List<HotelPenalties> penalties = new List<HotelPenalties>();

            foreach (string vId in VariantsId)
            {
                string[] parts = vId.Split('_');

                try
                {
                    HotelPenalties penalty = searchers[parts[0] + "_"].GetHotelPenalties(HotelId, vId);

                    if (penalty == null)
                        throw new CatseException("unknown variantId", ErrorCodes.HotelUnknownVariantId);

                    penalties.Add(penalty);
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                }
            }

            return penalties.ToArray();
        }

        //��������� ������������ !!
        [WebMethod]
        public HotelVerifyResult[] HotelVerify(string SearchId, int HotelId, string[] VariantsId)
        {
            //parse search_id
            Hashtable pHash = ParseHotelSearchId(SearchId);

            Dictionary<string, IHotelExt> searchers = new Dictionary<string, IHotelExt>();

            if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
                searchers.Add(VizitHotelsSearch.id_prefix, new VizitHotelsSearch((int)pHash["cityId"],
                                                            (DateTime)pHash["dateStart"],
                                                            (DateTime)pHash["dateEnd"],
                                                            pHash["stars"] as int[],
                                                            pHash["pansions"] as int[],
                                                            (pHash["rooms"] as RequestRoom[])[0],
                                                            SearchId, HOTELS_RESULTS_LIFETIME
                    ));
      
            if (ConfigurationManager.AppSettings["SearchHotelsInOstrovok"] == "true")
                searchers.Add(OstrovokSearch.id_prefix, new OstrovokSearch((int)pHash["cityId"],
                                                            (DateTime)pHash["dateStart"],
                                                            (DateTime)pHash["dateEnd"],
                                                            pHash["stars"] as int[],
                                                            pHash["pansions"] as int[],
                                                            (pHash["rooms"] as RequestRoom[])[0],
                                                            SearchId, HOTELS_RESULTS_LIFETIME
                    ));

            List<HotelVerifyResult> verifyResult = new List<HotelVerifyResult>();

            foreach (string vId in VariantsId)
            {
                string[] parts = vId.Split('_');

                HotelVerifyResult verify = searchers[parts[0] + "_"].VerifyHotelVariant(HotelId, vId);

                if (verify == null)
                    throw new CatseException("unknown variantId", ErrorCodes.HotelUnknownVariantId);

                verifyResult.Add(verify);
            }

            return verifyResult.ToArray();
        }

        //����������� �����
        [WebMethod]
        public BookResult HotelBook(string SearchId, int HotelId, UserInfo Info, BookRoom[] bookRooms)
        {
            //ErrorCode 37: ����������� searchId
            //ErrorCode 38: ����������� hotelId
            //ErrorCode 39: �� ������ variantId

            //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������

            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������

            //��������� ����� � ��������
            Hashtable pHash  = ParseHotelSearchId(SearchId); //������ ���� ����
            DateTime dateEnd = (DateTime)pHash["dateEnd"]; //����� ���� ��������� ����
            RequestRoom[] requestRooms = pHash["rooms"] as RequestRoom[];

            //������� ����������� ����� ��� �������� ������� ��������
            string checkTuristString = requestRooms[0].ToCompareString();

            List<int> turistsIds = new List<int>();
            List<string> turistsHashes = new List<string>();

            List<List<int>> roomTurists = new List<List<int>>();

            #region//�� ������ ������� ��������� ������ ��������, ��������� �������� � ������

            BookRoom room = bookRooms[0];

            int adls = 0;
            int chd = 0;
            List<int> ages = new List<int>();

            foreach (TuristContainer tsc in room.Turists)
            {
               int turistAge = tsc.GetAge(dateEnd);

               if (turistAge > 17)
                    adls++;
               else
               {
                   chd++;
                   ages.Add(turistAge);
               }

               MtHelper.SaveTuristToCache(tsc);//������� ����������� � ��

               if ((tsc.PassportDate > new DateTime(1970, 1, 2)) && (tsc.PassportDate < dateEnd.AddDays(PASSPORT_DAYS_LIMIT)))
                   throw new CatseException("pasport date limit", ErrorCodes.TuristCanntPaspDate);

               string turistHash = tsc.HashSumm;

               if ((!turistsIds.Contains(tsc.Id)) && (!turistsHashes.Contains(turistHash)))
               {
                   turistsIds.Add(tsc.Id);
                   turistsHashes.Add(turistHash);
               }
               else
                   throw new CatseException("turist duplicate", ErrorCodes.TuristDuplicate);
            }

            string checkStr = new RequestRoom() { Adults = adls, Children = chd, ChildrenAges = ages.ToArray() }.ToCompareString();

            bool throwException = true;

           
            if (checkStr == checkTuristString)
            {
                 checkTuristString = "checked";
                 throwException = false;
            }

            if (throwException)
                throw new CatseException("check rooms' turists" + checkTuristString + " " + checkStr, ErrorCodes.HotelRoomsTurists);
            else
            {
                roomTurists.Add(turistsIds);
                turistsIds = new List<int>();
            }
            #endregion

            try
            {
                Dictionary<string, IHotelExt> searchers = new Dictionary<string, IHotelExt>();

                if (ConfigurationManager.AppSettings["SearchHotelsInVizit"] == "true")
                    searchers.Add(VizitHotelsSearch.id_prefix, new VizitHotelsSearch((int)pHash["cityId"],
                                                                (DateTime)pHash["dateStart"],
                                                                (DateTime)pHash["dateEnd"],
                                                                pHash["stars"] as int[],
                                                                pHash["pansions"] as int[],
                                                                (pHash["rooms"] as RequestRoom[])[0],
                                                                SearchId, HOTELS_RESULTS_LIFETIME
                        ));

                if (ConfigurationManager.AppSettings["SearchHotelsInOstrovok"] == "true")
                    searchers.Add(OstrovokSearch.id_prefix, new OstrovokSearch((int)pHash["cityId"],
                                                                (DateTime)pHash["dateStart"],
                                                                (DateTime)pHash["dateEnd"],
                                                                pHash["stars"] as int[],
                                                                pHash["pansions"] as int[],
                                                                (pHash["rooms"] as RequestRoom[])[0],
                                                                SearchId, HOTELS_RESULTS_LIFETIME
                    ));

                string prefix = bookRooms[0].VariantId.Split('_')[0] + "_";

                List<HotelBooking> operatorBooking = new List<HotelBooking>();
                operatorBooking.Add(searchers[prefix].BookRoom(SearchId, HotelId, bookRooms[0], roomTurists[0]));

                //��������� �����
                Dictionary<string, decimal> totalPrice = new Dictionary<string, decimal>();
                foreach (HotelBooking htb in operatorBooking)
                {
                    foreach (KeyValuePair<string, decimal> pricePart in htb.Prices)
                    {
                        if (!totalPrice.ContainsKey(pricePart.Key))
                            totalPrice.Add(pricePart.Key, 0M);

                        totalPrice[pricePart.Key] += pricePart.Value;
                    }
                }

                //������� bookId
                return new BookResult()
                {
                    BookingNumber = MtHelper.SaveHotelBookingToCache(operatorBooking),
                    Prices = totalPrice.ToArray<KeyValuePair<string, decimal>>()
                };
            }
            catch (Exception ex)
            {
                throw new CatseException(ex.Message + " " + ex.StackTrace, ErrorCodes.HotelRoomsTurists);
            }
            //  return null;
        }

        [WebMethod]
        public string OstrovokBook(int bookId, string dogovorCode)
        {
            return ClickAndTravelSearchEngine.HotelSearchExt.OstrovokSearch.CreateBooking(bookId, dogovorCode);
        }

        #endregion

        #region ���

        //����� ���������
        [WebMethod]
        public TourInitSearchResult TourRoutesInitSearch(int CityId, DateTime TourDate, int NightsMin, int NightsMax, bool IsRoughly, int[] Stars, int[] Pansions, RequestRoom Room)
        {
            //ErrorCode 71: ����������� �����
            //ErrorCode 72: ������ � ���� ���� (������ �������, ������ ������� + ���) 
            //ErrorCode 73: ������ � ������� ���������� (����������� ����)
            //ErrorCode 74: ������ � ������� ������� (����������� ����)
            //ErrorCode 75: ������ � ����������������� ����
            //ErrorCode 76: ������ �� �������� � ������, ���������� ��������, �����, ������ ������� �������� ����� (���������� ���������, �������� ��������) -- ����� � ������ �� 6 �������, �������� > 0 


            return new TourInitSearchResult { SearchId = "route_search_id" };
        }

        //����� ������
        [WebMethod]
        public TourInitSearchResult TourHotelsInitSearch(int CityId, DateTime TourDate, int NightsMin, int NightsMax, bool IsRoughly, int[] Stars, int[] Pansions, RequestRoom Room, int TourId)
        {

            //ErrorCode 71: ����������� �����
            //ErrorCode 72: ������ � ���� ���� (������ �������, ������ ������� + ���) 
            //ErrorCode 73: ������ � ������� ���������� (����������� ����)
            //ErrorCode 74: ������ � ������� ������� (����������� ����)
            //ErrorCode 75: ������ � ����������������� ����
            //ErrorCode 76: ������ �� �������� � ������, ���������� ��������, �����, ������ ������� �������� ����� (���������� ���������, �������� ��������) -- ����� � ������ �� 6 �������, �������� > 0 
            //ErrorCode 78: ����������� ��� (���� �� ����� 0)


            return new TourInitSearchResult { SearchId = "hot_search_id" };
        }

        //����� ������� �� ����� 
        [WebMethod]
        public TourInitSearchResult TourVariantsInitSearch(string TourHotelsSearchId, int VariantsGroupId)
        {
            //ErrorCode 77: ����������� searchId
            //ErrorCode 78: ����������� VariantsGroupId


            return new TourInitSearchResult { SearchId = "var_search_id" };
        }

        //��������� ������
        [WebMethod]
        public TourSearchState TourGetSearchState(string SearchId)
        {
            //ErrorCode 77: ����������� searchId
            return new TourSearchState()
            {
                ResultsCount = 100,
                IsFinished = true,
                Hash = "some hash"
            };
        }

        //���������� ������
        [WebMethod]
        public TourHotelsResult TourGetHotelsResult(string SearchId)
        {
            //ErrorCode 77: ����������� searchId
            return new TourHotelsResult()
            {
                SearchState = new TourSearchState()
                {
                    ResultsCount = 100,
                    IsFinished = true,
                    Hash = "some hash"
                },

                Routes = new Containers.Tours.TourRoute[]{
                new Containers.Tours.TourRoute(){
                TourType = 1,
                TourId = 1,
                Title = "��� 1",
                OperatorName = "�������� 1",
                Dates = new Containers.Tours.TourDates[]{
                    new Containers.Tours.TourDates(){
                        DaysLong = 7,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    },

                    new Containers.Tours.TourDates(){
                        DaysLong = 9,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    },
                    new Containers.Tours.TourDates(){
                        DaysLong = 12,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    }
                }


                },
            new Containers.Tours.TourRoute(){
                TourType = 1,
                TourId = 2,
                Title = "��� 2",
                OperatorName = "�������� 2",
                Dates = new Containers.Tours.TourDates[]{
                    new Containers.Tours.TourDates(){
                        DaysLong = 5,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    },

                    new Containers.Tours.TourDates(){
                        DaysLong = 6,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    },
                    new Containers.Tours.TourDates(){
                        DaysLong = 14,
                        StartDate = DateTime.Today.AddDays(10),
                        MinPrices = new KeyValuePair<string,decimal>[]
                        {
                            new KeyValuePair<string,decimal>("RUB",120000.5M),
                            new KeyValuePair<string,decimal>("USD",5000.5M),
                            new KeyValuePair<string,decimal>("EUR",3500.5M),
                        }
                    }
                }


                }
            },
                VariantsGroups = new Containers.Tours.TourVariantsGroup[]{
                new Containers.Tours.TourVariantsGroup()
                {
                    DaysLong = 7,
                    HotelIds = new int[]{1,2,3,4,5,6},
                    TourId = 1,
                    TurDate = DateTime.Today.AddDays(10),
                    VariantsGroupId = 1,
                    Variants = new Containers.Tours.TourVariant[]
                    {
                        new Containers.Tours.TourVariant()
                        {
                            VariantId = 1,
                            Prices = new KeyValuePair<string,decimal>[]
                            {
                               // new KeyValuePair<string,decimal>("BYR", 1405.4M),
                                new KeyValuePair<string,decimal>("USD", 105.4M),
                                new KeyValuePair<string,decimal>("EUR", 15.4M)
                            },
                            HotelRooms = new Containers.Tours.TourHotelRoom[]
                            {
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 1,
                                    Day = 1,
                                    DaysLong = 3,
                                    HotelId = 1,
                                    PansionGroupId = 1,
                                    PansionTitle = "�������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                },
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 3,
                                    Day = 4,
                                    DaysLong = 3,
                                    HotelId = 5,
                                    PansionGroupId = 1,
                                    PansionTitle = "�������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                }
                            }
                        },

                        new Containers.Tours.TourVariant()
                        {
                            VariantId = 2,
                            Prices = new KeyValuePair<string,decimal>[]
                            {
                            //    new KeyValuePair<string,decimal>("BYR", 3405.4M),
                                new KeyValuePair<string,decimal>("USD", 305.4M),
                                new KeyValuePair<string,decimal>("EUR", 35.4M)
                            },
                            HotelRooms = new Containers.Tours.TourHotelRoom[]
                            {
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 1,
                                    Day = 1,
                                    DaysLong = 3,
                                    HotelId = 1,
                                    PansionGroupId = 1,
                                    PansionTitle = "�����������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                },
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 3,
                                    Day = 4,
                                    DaysLong = 3,
                                    HotelId = 5,
                                    PansionGroupId = 1,
                                    PansionTitle = "�����������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                }
                            }
                        }
                    }
                },
                new Containers.Tours.TourVariantsGroup()
                {
                    DaysLong = 12,
                    HotelIds = new int[]{1,2,3,4,5,6},
                    TourId = 1,
                    TurDate = DateTime.Today.AddDays(10),
                    VariantsGroupId = 3,
                    Variants = new Containers.Tours.TourVariant[]
                    {
                        new Containers.Tours.TourVariant()
                        {
                            VariantId = 4,
                            Prices = new KeyValuePair<string,decimal>[]
                            {
                            //    new KeyValuePair<string,decimal>("BYR", 1405.4M),
                                new KeyValuePair<string,decimal>("USD", 105.4M),
                                new KeyValuePair<string,decimal>("EUR", 15.4M)
                            },
                            HotelRooms = new Containers.Tours.TourHotelRoom[]
                            {
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 1,
                                    Day = 1,
                                    DaysLong = 3,
                                    HotelId = 1,
                                    PansionGroupId = 1,
                                    PansionTitle = "�������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                },
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 3,
                                    Day = 4,
                                    DaysLong = 3,
                                    HotelId = 5,
                                    PansionGroupId = 1,
                                    PansionTitle = "�������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                }
                            }
                        },

                        new Containers.Tours.TourVariant()
                        {
                            VariantId = 5,
                            Prices = new KeyValuePair<string,decimal>[]
                            {
                          //      new KeyValuePair<string,decimal>("BYR", 3405.4M),
                                new KeyValuePair<string,decimal>("USD", 305.4M),
                                new KeyValuePair<string,decimal>("EUR", 35.4M)
                            },
                            HotelRooms = new Containers.Tours.TourHotelRoom[]
                            {
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 1,
                                    Day = 1,
                                    DaysLong = 3,
                                    HotelId = 1,
                                    PansionGroupId = 1,
                                    PansionTitle = "�����������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                },
                                new Containers.Tours.TourHotelRoom(){
                                    CityId = 3,
                                    Day = 4,
                                    DaysLong = 3,
                                    HotelId = 5,
                                    PansionGroupId = 1,
                                    PansionTitle = "�����������",
                                    RoomCategory = "standard",
                                    RoomType = "DBL"
                                }
                            }
                        }
                    }
                }
                }

            };

            return null;
        }

        [WebMethod]
        public TourRoutesResult TourGetRoutesResult(string SearchId)
        {
            //ErrorCode 77: ����������� searchId
            return new TourRoutesResult()
            {

                SearchState = new TourSearchState()
                {
                    ResultsCount = 100,
                    IsFinished = true,
                    Hash = "some hash"
                },

                Routes = new Containers.Tours.TourRoute[]
                {
                    new Containers.Tours.TourRoute()
                    {
                        OperatorName = "�������� 1",
                        Title = "��� 1",
                        TourId = 1,
                        TourType = 1,

                        Dates =  new Containers.Tours.TourDates[]
                        {
                            new Containers.Tours.TourDates(){
                                DaysLong = 8,
                                MinPrices = new KeyValuePair<string,decimal>[]
                                {
                                 //    new KeyValuePair<string,decimal>("BYR", 14234M),
                                     new KeyValuePair<string,decimal>("USD", 234M),
                                     new KeyValuePair<string,decimal>("EUR", 34M)
                                },
                                StartDate = DateTime.Today.AddDays(12)
                            },

                             new Containers.Tours.TourDates(){
                                DaysLong = 8,
                                MinPrices = new KeyValuePair<string,decimal>[]
                                {
                                //     new KeyValuePair<string,decimal>("BYR", 14234M),
                                     new KeyValuePair<string,decimal>("USD", 234M),
                                     new KeyValuePair<string,decimal>("EUR", 34M)
                                },
                                StartDate = DateTime.Today.AddDays(15)
                            },

                             new Containers.Tours.TourDates(){
                                DaysLong = 7,
                                MinPrices = new KeyValuePair<string,decimal>[]
                                {
                                 //    new KeyValuePair<string,decimal>("BYR", 14234M),
                                     new KeyValuePair<string,decimal>("USD", 234M),
                                     new KeyValuePair<string,decimal>("EUR", 34M)
                                },
                                StartDate = DateTime.Today.AddDays(29)
                            }
                        }
                    }
                }
            };

            return null;
        }

        //������ ����� �� ����
        [WebMethod]
        public TourService[] TourGetTourServices(string SearchId, int TourId)
        {
            //ErrorCode 77: ����������� searchId
            //ErrorCode 78: ����������� TourId
            return new TourService[] {
                new TourService(){
                    CityId = 1,
                    Day = 1,
                    DaysLong = 10,
                    ServiceClass = 5,
                    Title = "Some service"
                },
                new TourService(){
                    CityId = 1,
                    Day = 2,
                    DaysLong = 10,
                    ServiceClass = 3,
                    Title = "Some service 2"
                }
            };

            return null;
        }

        //���������� � ������� �� pricekey ����
        [WebMethod]
        public TourPenalties TourGetTourVariantPenalties(string SearchId, int TourId, string VariantId)
        {
            //ErrorCode 77: ����������� searchId
            //ErrorCode 78: ����������� TourId
            //ErrorCode 79: ����������� VariantId

            return new TourPenalties()
            {
                CancelingPenalties = new KeyValuePair<DateTime, string>[]
                    {
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(3),"10%"),
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(5),"30%"),
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(7),"50%")
                    },

                ChangingPenalties = new KeyValuePair<DateTime, string>[]
                    {
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(3),"10 euro"),
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(5),"20 euro"),
                        new KeyValuePair<DateTime,string>(DateTime.Today.AddDays(7),"full price")
                    },
                VariantId = ""
            };

            return null;
        }

        //��������� ������������ !!
        [WebMethod]
        public TourVerifyResult TourVerifyVariant(string SearchId, int TourId, string VariantId)
        {
            //ErrorCode 77: ����������� searchId
            //ErrorCode 78: ����������� TourId
            //ErrorCode 79: ����������� VariantId

            return new TourVerifyResult()
            {
                IsAvailable = true,
                Prices = new KeyValuePair<string, decimal>[]{
                                            new KeyValuePair<string,decimal>("EUR",37.5M),
                                            new KeyValuePair<string,decimal>("USD",50.5M),
                                            new KeyValuePair<string,decimal>("RUB",1500M)
                                            },
                VariantId = VariantId
            };


            return null;
        }
        //����������� ��� 
        [WebMethod]
        public BookResult TourBook(string SearchId, int TourId, UserInfo Info, BookRoom Room)
        {
            //ErrorCode 77: ����������� searchId
            //ErrorCode 78: ����������� TourId
            //ErrorCode 79: ����������� VariantId

            //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������

            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������

            return new BookResult()
            {
                BookingNumber = 8,
                Prices = new KeyValuePair<string, decimal>[] {  new KeyValuePair<string, decimal>("USD", new Decimal(3.2)),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(101.2))}
            };

            //   return null;
        }
        #endregion

        #region �/�
        //0%
        #endregion

        #region ���������
        //��������������� ������ ��������� �������
        [WebMethod]
        public InsuranceSearchResult InsuranceSearch(int DaysLong, int TuristsCount, bool IsExtraCountry, int PurposeOfTrip)
        {
            //ErrorCode 61: ������������ �����������������
            //ErrorCode 62: ���������� �������� ������ 10
            //ErrorCode 63: ����������� ���� �������


            int[] _coverages = new int[] { 1, 2, 3, 4 };
            int[] _programs = new int[] { 1, 2, 3, 4 };

            InsuranceVariant[] vars = new InsuranceVariant[_coverages.Length * _programs.Length];

            for (int j = 0; j < _programs.Length; j++)
                for (int i = 0; i < _coverages.Length; i++)
                    vars[j * _coverages.Length + i] = new InsuranceVariant()
                    {
                        Coverage = _coverages[i],
                        Program = _programs[j],
                        Prices = new KeyValuePair<string, decimal>[]{
                                                            new KeyValuePair<string, decimal>("EUR", new Decimal(i + j*0.5)),
                                                           // new KeyValuePair<string, decimal>("BYR", new Decimal((i + j*0.5)) * 10500),
                                                            new KeyValuePair<string, decimal>("USD", new Decimal(i + j*0.5) * 1.35M),
                                                            new KeyValuePair<string, decimal>("RUB", new Decimal(i + j*0.5) * 44),
                                                            }
                    };


            return new InsuranceSearchResult()
            {
                PurposeId = PurposeOfTrip,
                SearchId = "exc_search",
                Variants = vars
            };
        }

        [WebMethod]
        public InsuranceSearchResult InsuranceSearchByCountry(DateTime StartDate, DateTime EndDate, string CountryCode, TuristContainer[] Turists)
        {
            //ErrorCode 64: ������ � �����
            //ErrorCode 65: ����������� ������ ����������

            //ErrorCode 91: ������ � ���� �������� �������


            int[] _coverages = new int[] { 1, 2, 3, 4 };
            int[] _programs = new int[] { 1, 2, 3, 4 };

            InsuranceVariant[] vars = new InsuranceVariant[_coverages.Length * _programs.Length];

            for (int j = 0; j < _programs.Length; j++)
                for (int i = 0; i < _coverages.Length; i++)
                    vars[j * _coverages.Length + i] = new InsuranceVariant()
                    {
                        Coverage = _coverages[i],
                        Program = _programs[j],
                        Prices = new KeyValuePair<string, decimal>[]{
                                                                new KeyValuePair<string, decimal>("EUR", new Decimal(i + j*0.5)),
                                                              //  new KeyValuePair<string, decimal>("BYR", new Decimal(i + j*0.5) * 10500),
                                                                new KeyValuePair<string, decimal>("USD", new Decimal(i + j*0.5) * 1.35M),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(i + j*0.5) * 44),
                                                            }
                    };


            return new InsuranceSearchResult()
            {
                PurposeId = 1,
                SearchId = "exc_search_by_country",
                Variants = vars
            };
        }

        [WebMethod]
        public KeyValuePair<string, decimal>[] InsuranceCalculate(DateTime StartDate, DateTime EndDate, string[] CountryIds, TuristContainer[] Turists, int PurposeOfTrip, int InsuranceProgram, int Coverage)
        {
            //ErrorCode 63: ����������� ���� �������
            //ErrorCode 64: ������ � �����
            //ErrorCode 65: ����������� ������ ����������
            //ErrorCode 66: ����������� ��������� �����������
            //ErrorCode 67: ����������� ����� ��������

            //ErrorCode 91: ������ � ���� �������� �������

            return new KeyValuePair<string, decimal>[]{
                                                                new KeyValuePair<string, decimal>("EUR", new Decimal(PurposeOfTrip + InsuranceProgram*0.5)),
                                                          //      new KeyValuePair<string, decimal>("BYR", new Decimal(PurposeOfTrip + InsuranceProgram*0.5) * 10500),
                                                                new KeyValuePair<string, decimal>("USD", new Decimal(PurposeOfTrip + InsuranceProgram*0.5) * 1.35M),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(PurposeOfTrip + InsuranceProgram*0.5) * 44),
                                                            };

        }

        [WebMethod]
        public BookResult InsuranceBook(DateTime StartDate, DateTime EndDate, string[] CountryIds, TuristContainer[] Turists, int PurposeOfTrip, int InsuranceProgram, int Coverage)
        {
            //ErrorCode 63: ����������� ���� �������
            //ErrorCode 64: ������ � �����
            //ErrorCode 65: ����������� ������ ����������
            //ErrorCode 66: ����������� ��������� �����������
            //ErrorCode 67: ����������� ����� ��������

            //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������

            return new BookResult()
            {
                BookingNumber = 4,
                Prices = new KeyValuePair<string, decimal>[] {  new KeyValuePair<string, decimal>("USD", new Decimal(3.2)),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(101.2))}
            };
        }
        #endregion

        #region ������ ����
        [WebMethod]
        public CarRentLocation[] CarRentGetPickUpLocations(int CityId)
        {
            //ErrorCode 81: ����������� id ������

            return new CarRentLocation[]{

               new CarRentLocation(){
                    CityId = CityId,
                    Id = CityId*10 +  1,
                    Title = "Location 1 for city " + CityId
               },

               new CarRentLocation(){
                  CityId = CityId,
                   Id = CityId*10 +  2,
                   Title = "Location 2 for city " + CityId
               },

               new CarRentLocation(){
                  CityId = CityId,
                   Id = CityId*10 +  3,
                   Title = "Location 3 for city " + CityId
               }
           };

            //  return null;
        }

        [WebMethod]
        public CarRentLocation[] CarRentGetDropOffLocations(int PickUpLocationId)
        {
            //ErrorCode 82: ����������� location Id
            return new CarRentLocation[]{

               new CarRentLocation(){
                    CityId = 1,
                    Id = PickUpLocationId*10 +  1,
                    Title = "Location 1 for pickup location " + PickUpLocationId
               },

               new CarRentLocation(){
                  CityId = 1,
                   Id = PickUpLocationId*10 +  2,
                   Title = "Location 2 for pickup location " + PickUpLocationId
               },

               new CarRentLocation(){
                  CityId = 1,
                   Id = PickUpLocationId*10 +  3,
                   Title = "Location 3 for pickup location " + PickUpLocationId
               }
           };
        }

        [WebMethod]
        public CarRentSearchResult CarRentSearch(int PickUpLocationId,
                                                         int DropOffLocationId,
                                                         DateTime PickUpDateTime,
                                                         DateTime DropOffDateTime)
        {
            //ErrorCode 82: ����������� location Id

            //ErrorCode 83: ������������� locations
            //ErrorCode 84: ������ � �����

            return new CarRentSearchResult()
            {
                SearchId = "cr_search_id",
                Variants = new CarRentVariant[] {

                    new CarRentVariant(){
                    Ac = 0,
                    Doors = 3,
                    Image = "http://www.cardelmar.de/images/bpcs/287x164/1728.jpg",
                    Producer = "Renault",
                    Programs =  new CarRentPrice[]{
                                    new CarRentPrice(){
                                    Id =1,
                                    Incl = new string[]{"-one", "-two", "-three"},
                                    Name = "All inclusive",
                                    Prices = new KeyValuePair<string,decimal>[]{
                                    //    new KeyValuePair<string, decimal>("BYR", 123453.6M),
                                        new KeyValuePair<string, decimal>("USD", 15M),
                                    }
                                    },

                                    new CarRentPrice(){
                                    Id =2,
                                    Incl = new string[]{"-one", "-two", "-three", "-four"},
                                    Name = "All inclusive plus",
                                    Prices = new KeyValuePair<string,decimal>[]{
                                    //    new KeyValuePair<string, decimal>("BYR", 193453.6M),
                                        new KeyValuePair<string, decimal>("USD", 22M),
                                    }
                                    },

                                },

                                Seats =5,
                                SupplierId = 101,
                                SupplierName = "CarFax",
                                Transmission = 1,
                                TypeName = "Mini",
                                VariantId = 1
                    },

                    new CarRentVariant(){

                    Ac = 1,
                    Doors = 3,
                    Image = "http://www.cardelmar.de/images/bpcs/287x164/58.jpg",
                    Producer = "Renault",
                    Programs =  new CarRentPrice[]{
                                    new CarRentPrice(){
                                    Id =1,
                                    Incl = new string[]{"-one", "-two", "-three"},
                                    Name = "All inclusive",
                                    Prices = new KeyValuePair<string,decimal>[]{
                                      //  new KeyValuePair<string, decimal>("BYR", 223453.6M),
                                        new KeyValuePair<string, decimal>("USD", 23M),
                                    }
                                    },

                                    new CarRentPrice(){
                                    Id =2,
                                    Incl = new string[]{"-one", "-two", "-three", "-four"},
                                    Name = "All inclusive plus",
                                    Prices = new KeyValuePair<string,decimal>[]{
                                      //  new KeyValuePair<string, decimal>("BYR", 293453.6M),
                                        new KeyValuePair<string, decimal>("USD", 35M),
                                    }
                                    },

                                },

                                Seats = 5,
                                SupplierId = 101,
                                SupplierName = "Hertz",
                                Transmission = 1,
                                TypeName = "Standard",
                                VariantId = 2

                    }
                }
            };

            //  return null;
        }

        [WebMethod]
        public CarRentStation[] CarRentGetPickUpStations(int LocationId, int SupplierId)
        {
            //ErrorCode 82: ����������� location
            //ErrorCode 85: ����������� Supplier

            return new CarRentStation[] {
                new CarRentStation()
                {
                    Id  = 1,
                    Title = "station1"
                },

                new CarRentStation()
                {
                    Id  = 2,
                    Title = "station2"
                },
            };



            // return null;
        }

        //���� ������� ��������� ����
        [WebMethod]
        public CarRentStation[] CarRentGetDropOffStations(int LocationId, int PickUpStationId, int SupplierId)
        {
            //ErrorCode 82: ����������� location
            //ErrorCode 85: ����������� Supplier
            //ErrorCode 86: ����������� stationId

            return new CarRentStation[] {
                new CarRentStation()
                {
                    Id  = 1,
                    Title = "station1"
                },

                new CarRentStation()
                {
                    Id  = 2,
                    Title = "station2"
                },
            };
        }

        [WebMethod]
        public CarRentStationDetails CarRentGetStationDetails(int StationId, string SearchId)
        {
            //ErrorCode 86: ����������� stationId
            //ErrorCode 87: ����������� SearchId

            return new CarRentStationDetails()
            {

                CityName = "Some city name",
                Country = "Some country name",

                DropTime = "from 12:00 to 19:00",
                Fax = "+123453563453",
                Phone = "+123453563234",
                PickTime = "from 10:00 to 17:00",
                Street = "Some street",
                ZipCode = "121342121"
            };
        }

        [WebMethod]
        public CarRentVerifyCarResult CarRentVerifyCar(string SearchId, int PickUpStationId, int DropOffStationId,
                                                              DateTime PickUpDateTime, DateTime DropOffDateTime,
                                                              int VariantId, int PriceId)
        {
            //ErrorCode 84: ������ � �����
            //ErrorCode 86: ����������� stationId
            //ErrorCode 87: ����������� SearchId
            //ErrorCode 88: ����������� VariantId
            //ErrorCode 89: ����������� PriceId

            return new CarRentVerifyCarResult()
            {

                Message = "message text",
                NewPrices = new KeyValuePair<string, decimal>[]{
                   // new KeyValuePair<string,decimal>("BYR", 120000M),
                    new KeyValuePair<string,decimal>("USD", 15M),
                    new KeyValuePair<string,decimal>("EUR", 11.3M)
                }
            };

            return null;
        }

        [WebMethod]
        public CarRentExtra[] CarRentGetExtras(int SupplierId, DateTime PickUpDate)
        {
            //ErrorCode 84: ������ � �����
            //ErrorCode 85: ����������� Supplier
            return new CarRentExtra[] {
                new CarRentExtra(){
                     Id = 1,
                     Title = "snow chains",

                     Prices  = new KeyValuePair<string,decimal>[]
                     {
                         new KeyValuePair<string,decimal>("CZK", 500)
                     }
                },
                new CarRentExtra(){
                    Id = 2,
                    Title = "bulbavoz",
                     Prices  = new KeyValuePair<string,decimal>[]
                     {
                         new KeyValuePair<string,decimal>("CZK", 1200)
                     }

                }
            };

            return null;
        }

        [WebMethod]
        public BookResult CarRentBook(string SearchId, int PickUpStationId, int DropOffStationId, UserInfo UserInfo,
                                             TuristContainer Turist, int VariantId, int PriceId, CarRentBookExtra[] Extras)
        {
            //ErrorCode 86: ����������� stationId
            //ErrorCode 87: ����������� SearchId
            //ErrorCode 88: ����������� VariantId
            //ErrorCode 89: ����������� PriceId

            //ErrorCode 91: ������ � ���� �������� ������� (������ 25)
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������

            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������

            return new BookResult()
            {
                BookingNumber = 6,
                Prices = new KeyValuePair<string, decimal>[] {  new KeyValuePair<string, decimal>("USD", new Decimal(3.2)),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(101.2))}
            };

        }
        #endregion

        #region ���� �� ������
        //����� ��������� ����
        [WebMethod]
        public VisaSearchResult VisaSearch(string CountryCode,
                                                DateTime DateFrom,
                                                DateTime DateTo,
                                                string CitizenshipCode,
                                                int Age,
                                                int CityFromCode)
        {
            //ErrorCode 51: ������������ CountryCode
            //ErrorCode 52: ������ � ����� (������ �������, ������ ������� + ���, ����������� ���� ������ ������������)
            //ErrorCode 53: ������ � �����������
            //ErrorCode 54: ������ � �������� (������ 0, ������ 120)

            VisaSearchResult vRes = new VisaSearchResult()
            {
                SearchId = "test123",
                VisaDetails = new VisaDetails()
                {
                    Id = 1,
                    VisaName = "���� ��� ���������",
                    CountryCode = "CZ",
                    CitizenshipCode = "RU",
                    TitleCurrency = "60 �",
                    AgeLimit = Age + 10,
                    CityId = CityFromCode,
                    Prices = new KeyValuePair<string, decimal>[] { new KeyValuePair<string, decimal>("RUB", new Decimal(102.2)), new KeyValuePair<string, decimal>("EUR", new Decimal(10)) }
                }
            };

            return vRes;
        }

        //������������ ����
        [WebMethod]
        public BookResult VisaBook(string SearchId, int VisaId, UserInfo Info, TuristContainer Turist)
        {
            //ErrorCode 57: �� ������ SearchId
            //ErrorCode 58: �� ������ VisaId

            //ErrorCode 90: ������������ ������ �������� (�������� ����������, ������ ��������/����)
            //ErrorCode 91: ������ � ���� �������� �������
            //ErrorCode 92: ������ � �����/������� �������
            //ErrorCode 93: ������ � �����������
            //ErrorCode 94: ������ � ������ ��������
            //ErrorCode 95: ������ � ����� �������� ��������
            //ErrorCode 96: ����������� ������

            //ErrorCode 98: ���������� �-���� ������������
            //ErrorCode 99: ���������� ������� ������������

            return new BookResult()
            {
                BookingNumber = 3,
                Prices = new KeyValuePair<string, decimal>[] {  new KeyValuePair<string, decimal>("USD", new Decimal(3.2)),
                                                                new KeyValuePair<string, decimal>("RUB", new Decimal(101.2))}
            };
        }
        #endregion
    }
}