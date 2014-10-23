using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ClickAndTravelSearchEngine
{
    public class Validator
    {
        public static bool CheckDogovorCode(string code)
        {
            Regex rex = new Regex(@"^[A-Z0-9]{6,10}$");

            return rex.IsMatch(code.Trim());
        }

        public static bool CheckUserMail(string mail)
        {
            Regex rex = new Regex(@"^[0-9a-z]+[-\._0-9a-z]*@[0-9a-z]+[-\._^0-9a-z]*[0-9a-z]+[\.]{1}[a-z]{2,6}$");

            return rex.IsMatch(mail.ToLower());
        }

        public static bool CheckString(string input, string regExp)
        {
            Regex rex = new Regex(regExp);

            return rex.IsMatch(input.Trim());
        }
    }
}

