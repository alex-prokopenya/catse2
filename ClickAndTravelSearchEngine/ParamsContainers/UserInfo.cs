using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using ClickAndTravelSearchEngine.Containers;

using ClickAndTravelSearchEngine.Exceptions;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class UserInfo
    {
        public UserInfo()
        { }

        public void SetupStuff(JsonObject inp)
        {
            try
            {
                _email = inp["email"].ToString();
                _phone = inp["phone"].ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot convert " + inp.ToString() + " to UserInfo object", ex);
            }

            //ErrorCode 98: невалидный и-мэйл пользователя
            //ErrorCode 99: невалидный телефон пользователя

            if (!Validator.CheckUserMail(_email)) throw new CatseException("Invalid e-mail", ErrorCodes.UserInvalidMail);

            if (!Validator.CheckString(_phone, @"^[0-9]{6,20}$")) throw new CatseException("Invalid phone", ErrorCodes.UserInvalidPshone);
        }

        public UserInfo(JsonObject inp)
        {
            SetupStuff(inp);
        }

        private string _phone;

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }
        private string _email;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
    }
}