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
                Logger.WriteToRedisLog("set key:" + key + "\n\nvalue: " + value);
            #endif
                int max = 10;
                while (max-- > 0)
                {
                    try
                    {
                        redis_clinet = new RedisClient(host);
                        redis_clinet.Set(key, value);
                        
                        break;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(100);
                    }
                }
        }

        public static void SetString(string key, string value, TimeSpan lifetime)
        {

            RedisClient redis_clinet;
            #if DEBUG
                Logger.WriteToRedisLog("set key:" + key + "\nvalue: " + value + "\nfor " + lifetime.TotalSeconds + " sec.");
            #endif

              int max = 10;
              while (max-- > 0)
              {
                  try
                  {
                      redis_clinet = new RedisClient(host);
                      redis_clinet.SetEX(key, lifetime, value);
                  }
                  catch (Exception ex)
                  {
                      Thread.Sleep(100);
                  }
              }
        }

        public static string GetString(string key)
        {
            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                  try
                  {
                      redis_clinet = new RedisClient(host);
                      string value = redis_clinet.Get(key);

                        #if DEBUG
                                              if (value != null)
                                                  Logger.WriteToRedisLog("get key:" + key + "\n\nvalue: " + value);
                                              else
                                                  Logger.WriteToRedisLog("get key:" + key + "\n\nvalue: null");
                        #endif

                      return value;
                  }
                  catch (Exception ex)
                  {
                      Thread.Sleep(100);
                  }
            }

            throw new Exception("redis get string exception");
        }
    }
}