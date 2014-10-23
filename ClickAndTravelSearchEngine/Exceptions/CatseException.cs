using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Exceptions
{
    public class CatseException : Exception
    {
        public CatseException()
        { }

        public CatseException(string Message)
            : base(Message)
        { }

        public CatseException(string Message, int Code)
            : base("" + Code + "~" + Message)
        {
            this.Code = Code;
        }

        public int Code;
    }
}