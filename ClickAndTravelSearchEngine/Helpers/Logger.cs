using System;
using System.IO;

using System.Collections.Specialized;


namespace ClickAndTravelSearchEngine.Helpers
{
    public static class Logger
    {
        public static void ApplyConfig(NameValueCollection settings, string paramPartnerCode = "")
        {
            partnerCode = paramPartnerCode;
        }

        public static string partnerCode = "click";


        public static void WriteToErrorLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/"+ partnerCode + "error_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    var str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }
            }
            catch (Exception)
            { }
        }

        public static void WriteToLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode  + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str; //"example text" + Environment.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }
            }
            catch (Exception)
            { }
        }

        public static void WriteToLog(string[] messages)
        {
            try
            {
                StreamWriter outfile = new StreamWriter("" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode  + DateTime.Today.ToString("yyyy -MM-dd_HH") + ".log", true);
                {
                    outfile.WriteLine("_______________________________________");
                    outfile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    foreach (string message in messages)
                        outfile.WriteLine(message);
                }
                outfile.Close();
            }
            catch (Exception)
            { }
        }

        public static void WriteToHotelBookLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode + "hb_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";


                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str; //"example text" + Environment.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }

            }
            catch (Exception)
            { }
        }

        public static void WriteToRedisStuffLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode + "redis_all_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str; //"example text" + Environment.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }
            }
            catch (Exception)
            { }
        }


        public static void WriteToRedisLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode + "redis_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str; //"example text" + Environment.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }

            }
            catch (Exception)
            { }
        }

        public static void WriteToInOutLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode + "inout_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }
            }
            catch (Exception)
            { }
        }

        public static void WriteToOstrovokLog(string message)
        {
            try
            {
                string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + partnerCode + "ostrovok_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".log";

                string str = "";

                using (StreamWriter swriter = new StreamWriter(path, true))
                {
                    str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str;
                    swriter.Write(str);
                    swriter.Flush();
                    swriter.Close();
                }
            }
            catch (Exception)
            { }
        }
    }
}
