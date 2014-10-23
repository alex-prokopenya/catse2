using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class BookRoom
    {
        public BookRoom() { }

        public BookRoom(JsonObject inp)
        {
            try
            {
                this._variantId = inp["variant_id"].ToString();

                JsonArray arrTurists = inp["tourists"] as JsonArray;

                this._turists = new TuristContainer[arrTurists.Length];

                for (int i = 0; i < arrTurists.Length; i++)
                {
                    this._turists[i] = new TuristContainer(arrTurists[i] as JsonObject);
                }
            }
            catch (Exception)
            { 
                throw new Exception("cann't parse bookRoom from " + inp.ToString());
            }
        }

        private string _variantId;

        public string VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private TuristContainer[] _turists;

        public TuristContainer[] Turists
        {
            get { return _turists; }
            set { _turists = value; }
        }
    }
}