using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickAndTravelSearchEngine;
using System.Configuration;
using ClickAndTravelSearchEngine.HotelSearchExt;
using ClickAndTravelSearchEngine.ExcursionSearchExt;
using ClickAndTravelSearchEngine.ParamsContainers;
using ClickAndTravelSearchEngine.Containers.Hotels;
using ClickAndTravelSearchEngine.Containers.Transfers;
using ClickAndTravelSearchEngine.MasterTour;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.TransferSearchExt;


namespace consoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WeAtlasExcSearcher ws = new WeAtlasExcSearcher();

            var res = ws.SearchExcursions(43030, new DateTime(2015,11,11), new DateTime(2015, 11, 15), 1);

            Console.ReadKey();
        }
    }
}
