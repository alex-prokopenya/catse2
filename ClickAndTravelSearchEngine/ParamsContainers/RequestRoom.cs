using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class RequestRoom
    {
        private int[] jsonArrayToIntArray(JsonArray inp)
        {
            int[] res = new int[inp.Length];

            for (int i = 0; i < inp.Length; i++)
                res[i] = Convert.ToInt32(inp[i]);

            return res;
        }

        public RequestRoom() { }

        public RequestRoom(JsonObject inp)
        {
            try
            {
                this._adults = Convert.ToInt32(inp["adults"]);
                this._children = Convert.ToInt32(inp["children"]);

                if (this._adults == 0) throw new Exception();

                if (this._children > 0)
                    this._childrenAges = jsonArrayToIntArray(inp["children_ages"] as JsonArray);//.ToArray(typeof(int));


                if (this._children != this._childrenAges.Length) throw new Exception();
            }
            catch (Exception)
            {
                throw new Exception("cann't parse room object from " + inp.ToString());
            
            }
        }


        private int[] _childrenAges = new int[0];
        [JsonMemberName("children_ages")]
        public int[] ChildrenAges
        {
            get { return _childrenAges; }
            set { _childrenAges = value; }
        }

        private int _children;
        [JsonMemberName("children")]
        public int Children
        {
            get { return _children; }
            set { _children = value; }
        }

        private int _adults;
        [JsonMemberName("adults")]
        public int Adults
        {
            get { return _adults; }
            set { _adults = value; }
        }


        public bool Equals(RequestRoom compareRoom)
        {
            return this.ToCompareString().Trim() == compareRoom.ToCompareString().Trim();
        }

        public string ToCompareString()
        {
            string res = "";

            res +="" + this._adults + "_" + this._children;

            List<int> ages1 = new List<int>();
            ages1.AddRange(this.ChildrenAges);
            ages1.Sort();

            foreach (int age in ages1)
                res += "_" + age;

            return res;
        }
    }
}