using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Containers.Hotels
{
    public class Room: ParamsContainers.RequestRoom
    {
        public Room() { }

        public Room(ParamsContainers.RequestRoom inp)
        {
            this.Adults = inp.Adults;
            this.Children = inp.Children;
            this.ChildrenAges = inp.ChildrenAges;
        }

        private RoomVariant[] _variants;
        [JsonMemberName("variants")]
        public RoomVariant[] Variants
        {
            get { return _variants; }
            set { _variants = value; }
        }
    }
}