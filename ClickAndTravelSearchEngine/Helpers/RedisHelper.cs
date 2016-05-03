using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Sider;
using System.Threading.Tasks;
using ClickAndTravelSearchEngine.Helpers;
using System.Threading;
namespace ClickAndTravelSearchEngine
{
    public class RedisHelper
    {
        private static string host = ConfigurationManager.AppSettings["RedisHost"];

        public static void SetString(string key, string value)
        {
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

            #if DEBUG
            Logger.WriteToRedisStuffLog("set lt" + key);
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


            #if DEBUG

            Logger.WriteToRedisStuffLog("get lt" + key);
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