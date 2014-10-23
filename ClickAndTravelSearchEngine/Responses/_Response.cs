using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Jayrock.Json.Conversion;
namespace ClickAndTravelSearchEngine.Responses
{
    public class _Response
    {
        private int _errorCode;

        [JsonIgnore]
        public int ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        private string _errorMessage;
        [JsonIgnore]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
}