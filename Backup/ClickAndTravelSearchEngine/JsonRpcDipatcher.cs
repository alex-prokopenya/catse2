using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using System.Collections;
using Jayrock.Services;

using ClickAndTravelSearchEngine.Helpers;


namespace ClickAndTravelSearchEngine
{
    public class JsonRpcDispatcher : Jayrock.JsonRpc.JsonRpcDispatcher
    {
        public JsonRpcDispatcher(IService service):base(service)
        {
             
        }
        
        protected override IDictionary CreateResponse(IDictionary request, object result, object error)
        {
           
            JsonObject error2 = null;

            if (error != null)
            {
                error2 = new JsonObject();
                error2["code"] = -32000 - Convert.ToInt32((error as JsonObject)["code"]);
                error2["message"] = (error as IDictionary)["message"];
                error2["data"] = (error as IDictionary)["errors"];
                //  (  error as IDictionary)["code"] = -32000;
            }

            var response = base.CreateResponse(request, result, error2);

            var response2 = new JsonObject();

            response2["jsonrpc"] = "2.0";

            foreach (string key in response.Keys)
                response2.Add(key, response[key]);

            try
            {
                Logger.WriteToLog(JsonConvert.ExportToString(response2));
            }
            catch (Exception) { }

            return response2;
        }

        protected override IDictionary ParseRequest(JsonReader input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            JsonReader reader = input; // alias for clarity
            JsonImportHandler importer = JsonImporter;

            JsonObject request = new JsonObject();
            Method method = null;
            JsonReader paramsReader = null;
            object args = null;

            //reader.
            try
            {
                reader.ReadToken(JsonTokenClass.Object);

                while (reader.TokenClass != JsonTokenClass.EndObject)
                {
                    string memberName = reader.ReadMember();

                    switch (memberName)
                    {
                        case "id":
                            {
                                request["id"] = importer(AnyType.Value, reader);
                                break;
                            }

                        case "method":
                            {
                                string methodName = reader.ReadString();
                                request["method"] = methodName;
                                method = Service.GetClass().GetMethodByName(methodName);

                                if (paramsReader != null)
                                {
                                    //
                                    // If the parameters were already read in and
                                    // buffer, then deserialize them now that we know
                                    // the method we're dealing with.
                                    //

                                    args = ReadParameters(method, paramsReader, importer);
                                    paramsReader = null;
                                }

                                break;
                            }

                        case "params":
                            {
                                //
                                // Is the method already known? If so, then we can
                                // deserialize the parameters right away. Otherwise
                                // we record them until hopefully the method is
                                // encountered.
                                //

                                if (method != null)
                                {
                                    args = ReadParameters(method, reader, importer);
                                }
                                else
                                {
                                    JsonRecorder recorder = new JsonRecorder();
                                    recorder.WriteFromReader(reader);
                                    paramsReader = recorder.CreatePlayer();
                                }

                                break;
                            }

                        default:
                            {
                                reader.Skip();
                                break;
                            }
                    }
                }

                reader.Read();

                if (args != null)
                    request["params"] = args;

                try
                {
                    Logger.WriteToLog(JsonConvert.ExportToString(request));
                }
                catch (Exception) { }

                return request;
            }
            catch (JsonException e)
            {
                try
                {
                    Logger.WriteToLog(JsonConvert.ExportToString(request));
                }
                catch (Exception) { }

                throw new BadRequestException(e.Message, e, request);
            }
            catch (MethodNotFoundException e)
            {
                try
                {
                    Logger.WriteToLog(JsonConvert.ExportToString(request));
                }
                catch (Exception) { }
                throw new BadRequestException(e.Message, e, request);
            }
        }
    }
}