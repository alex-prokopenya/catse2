using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Sider;
using System.Threading.Tasks;
using ClickAndTravelSearchEngine.Helpers;
namespace ClickAndTravelSearchEngine
{
    public class RedisHelper
    {
   
        private static string host = ConfigurationManager.AppSettings["RedisHost"];
        private static RedisClient redis_clinet = null;

        public static void SetString(string key, string value)
        {
            redis_clinet = new RedisClient(host);
            Logger.WriteToRedisLog("set key:" + key + "\n\nvalue: " + value);
            redis_clinet.Set(key, value);
            
        }

        public static void SetString(string key, string value, TimeSpan lifetime)
        {
            redis_clinet = new RedisClient(host);
            Logger.WriteToRedisLog("set key:" + key + "\nvalue: " + value + "\nfor " + lifetime.TotalSeconds + " sec.");
            redis_clinet.SetEX(key, lifetime, value);
            //Logger.WriteToRedisLog("set key:" + key + "\nvalue: " + value + "\nfor " + lifetime.TotalSeconds +" sec.");
        }

        public static string GetString(string key)
        {
            redis_clinet = new RedisClient(host);
            string value = redis_clinet.Get(key);

            if (value!= null)
                Logger.WriteToRedisLog("get key:" + key + "\n\nvalue: " + value);
            else
                Logger.WriteToRedisLog("get key:" + key + "\n\nvalue: null");

            return value;//redis_clinet.Get(key);
        }
    }
}