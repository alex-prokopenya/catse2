using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Sider;
using System.Threading.Tasks;
using ClickAndTravelSearchEngine.Helpers;
using System.Threading;
using System.Collections.Specialized;


namespace ClickAndTravelSearchEngine
{
    public class RedisHelper
    {
        public static void ApplyConfig(NameValueCollection settings, string paramPartnerCode = "")
        {
            partnerCode = paramPartnerCode;
        }

        private static string host = ConfigurationManager.AppSettings["RedisHost"];

        public static string partnerCode = "click";

        public static void SetString(string key, string value)
        {
            key = partnerCode + key;

            RedisClient redis_clinet;

            #if DEBUG
            Logger.WriteToRedisStuffLog("get " + key);
            DateTime start = DateTime.Now;
            #endif

            int max = 10;
            while (max-- > 0)
            {
                try
                {
                    redis_clinet = new RedisClient(host);
                    redis_clinet.Set(key, value);

                    redis_clinet.Dispose();

                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }

            #if DEBUG

            if((DateTime.Now - start).TotalSeconds > 2)
                Logger.WriteToRedisLog("redis set key:" + key + ", " + (DateTime.Now - start).TotalSeconds);

            #endif
        }

        public static void SetString(string key, string value, TimeSpan lifetime)
        {
            key = partnerCode + key;

            #if DEBUG
            Logger.WriteToRedisStuffLog("set with lifetime " + key);
            DateTime start = DateTime.Now;
            #endif

            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                try
                {
                    redis_clinet = new RedisClient(host);
                    redis_clinet.SetEX(key, lifetime, value);

                    redis_clinet.Dispose();
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }

            #if DEBUG
                 if ((DateTime.Now - start).TotalSeconds > 2)
                      Logger.WriteToRedisLog("redis set lifetime key:" + key + ", " + (DateTime.Now - start).TotalSeconds);
            #endif
        }

        public static string GetString(string key)
        {
            key = partnerCode + key;

            #if DEBUG

            Logger.WriteToRedisStuffLog("get with lifetime " + key);
            DateTime start = DateTime.Now;
            #endif

            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                  try
                  {
                      redis_clinet = new RedisClient(host);
                      string value = redis_clinet.Get(key);

                        #if DEBUG
                            if ((DateTime.Now - start).TotalSeconds > 2)
                                Logger.WriteToRedisLog("redis get key:" + key + ", " + (DateTime.Now - start).TotalSeconds);
                        #endif

                    redis_clinet.Dispose();
                    return value;
                  }
                  catch (Exception)
                  {
                      Thread.Sleep(100);
                  }
            }

            #if DEBUG
                Logger.WriteToRedisLog("redis Exception set key:" + key + ", " + (DateTime.Now - start).TotalSeconds);
            #endif

            throw new Exception("redis get string exception");
        }
    }
}