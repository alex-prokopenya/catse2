using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine.Containers.Excursions
{
    public class ExcursionDetails
    {
        //идентификатор экскурсии
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        //типы экскурсии (городская, историческая....)
        private int[] _types;

        public int[] Types
        {
            get { return _types; }
            set { _types = value; }
        }

        //есть ли гид
        private bool _withGuide;

        public bool WithGuide
        {
            get { return _withGuide; }
            set { _withGuide = value; }
        }

        //есть ли гид
        private bool _isIndividual;

        public bool IsIndividual
        {
            get { return _isIndividual; }
            set { _isIndividual = value; }
        }

        //набор фоток
        private Photo[] _photos;

        public Photo[] Photos
        {
            get { return _photos; }
            set { _photos = value; }
        }

        //краткое описание
        private string _shortText;

        public string ShortText
        {
            get { return _shortText; }
            set { _shortText = value; }
        }
        private string _detailsText;

        //детальное описание
        public string DetailsText
        {
            get { return _detailsText; }
            set { _detailsText = value; }
        }

        //продолжительность
        private string _timeLong;

        public string TimeLong
        {
            get { return _timeLong; }
            set { _timeLong = value; }
        }

        //дни проведения
        private string _daysText;

        public string DaysText
        {
            get { return _daysText; }
            set { _daysText = value; }
        }

        //транспорт
        private Vehicle _vehicle;

        public Vehicle Vehicle
        {
            get { return _vehicle; }
            set { _vehicle = value; }
        }
    }
}