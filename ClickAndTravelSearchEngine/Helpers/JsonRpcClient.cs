using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Services.Protocols;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Helpers
{
    public class JsonRpcClient : HttpWebClientProtocol
    {
        private int _id;
        public virtual object Invoke(string method, object args)
        {
            JsonObject jsonrequest = new JsonObject();
            jsonrequest["jsonrpc"] = "2.0";
            jsonrequest["method"] = method;
            jsonrequest["params"] = args;
            jsonrequest["id"] = ++_id;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(Url));
            webRequest.Method = "POST";
            TextWriter writer = new StreamWriter(webRequest.GetRequestStream());
            writer.Write(jsonrequest.ToString());
            writer.Close();
            WebResponse response = webRequest.GetResponse();

            ImportContext import = new ImportContext();
            JsonReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
            object jsonresponse_ = import.Import(reader);
            if (!(jsonresponse_ is JsonObject))
                throw new Exception("Something weird happened to the request, check the foobar or something");

            JsonObject jsonresponse = (JsonObject)jsonresponse_;

            if (jsonresponse["error"] != null)
                throw new Exception(jsonresponse["error"].ToString());

            webRequest.Abort();

           return jsonresponse["result"];
           
        }

        protected virtual void OnError(object errorObject)
        {
            JsonObject error = errorObject as JsonObject;

            if (error != null)
                throw new Exception(error["message"] as string);

            throw new Exception(errorObject as string);
        }
    }
}
