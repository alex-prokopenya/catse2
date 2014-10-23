using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using ClickAndTravelSearchEngine.Exceptions;
using ClickAndTravelSearchEngine.Helpers;

namespace ClickAndTravelSearchEngine.ParamsContainers
{
    public class TuristContainer
    {
        public TuristContainer()
        { }

        public TuristContainer(JsonObject inp)
        { 
            try
            {
                #region BirthDate
                try
                {
                    _birthDate = DateTime.ParseExact(inp["birth_date"].ToString(), "yyyy-MM-dd", null);
                }
                catch (Exception)
                {
                    throw new CatseException("Cann't parse birthdate", ErrorCodes.TuristCanntParseBirthdate);
                }

                if (_birthDate >= DateTime.Today) throw new CatseException("Cann't parse birthdate", ErrorCodes.TuristCanntParseBirthdate);
                #endregion

                #region BONUS_CARD
                JsonObject bonusCard = inp["bonus_card"] as JsonObject;
                _bonusCard = new FlightBonusCard(bonusCard);
                #endregion

                #region CITIZEN
                _citizenship = inp["citizenship"].ToString();

                if (!Validator.CheckString(_citizenship, @"^[A-Z,a-z]{2}$")) throw new CatseException("Cann't parse citizenship", ErrorCodes.TuristCanntParseCitizen);
                #endregion

                #region FIRSTNAME
                _firstName = inp["first_name"].ToString();
                if (!Validator.CheckString(_firstName, @"^[A-Z,a-z]{1,15}$")) throw new CatseException("Cann't parse first_name", ErrorCodes.TuristCanntParseName);
                #endregion

                #region LASTNAME
                _name = inp["last_name"].ToString();
                if (!Validator.CheckString(_name, @"^[A-Z,a-z]{1,25}$")) throw new CatseException("Cann't parse last_name", ErrorCodes.TuristCanntParseName);
                #endregion

                #region PASSPORTNUM
                _passportNum = inp["passport_num"].ToString();
                if (!Validator.CheckString(_passportNum, @"^[0-9,a-z,A-Z]{1,15}$")) throw new CatseException("Cann't parse passport_num", ErrorCodes.TuristCanntPaspNum);
                #endregion

                #region PASSPORTDATE

                try
                {
                    _passportDate = DateTime.ParseExact(inp["passport_date"].ToString(), "yyyy-MM-dd", null);// new DateTime((int)turistPd[0], (int)turistPd[1], (int)turistPd[2]);
                }
                catch (Exception)
                {
                    throw new CatseException("Cannt parse pasport date", ErrorCodes.TuristCanntPaspDate);
                }

                #endregion

                try
                {
                    this._id = Convert.ToInt32(inp["id"]);
                    this._sex = Convert.ToInt32(inp["gender"]);
                }
                catch (Exception)
                { }
            }
            catch (CatseException ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw new CatseException("Cann't convert " + inp + " to Turist object ", ex.Code);
            }
        }

        public int GetAge(DateTime CurDate)
        { 
            DateTime zeroTime = new DateTime(1, 1, 1);
           
            TimeSpan span = CurDate - this._birthDate;
            return (zeroTime + span).Year - 1;
        }

        public string HashSumm
        {
            get {

                return Helpers.HashSumm.CalculateMD5Hash("" + this._name + "_" + this._firstName + "_" + this.BirthDate + "_" + this._sex + "_" + this.PassportDate + "_" + this.PassportNum);
            }
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _sex;

        public int Sex
        {
            get { return _sex; }
            set { _sex = value; }
        }

        //имя туриста
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //фамилия туриста
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        //дата рождения туриста
        private DateTime _birthDate;

        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }

        //код гражданства
        private string _citizenship;

        public string Citizenship
        {
            get { return _citizenship; }
            set { _citizenship = value; }
        }

        //код паспорта
        private string _passportNum;

        public string PassportNum
        {
            get { return _passportNum; }
            set { _passportNum = value; }
        }

        //дата действия паспорта
        private DateTime _passportDate;

        public DateTime PassportDate
        {
            get { return _passportDate; }
            set { _passportDate = value; }
        }

        //карта бонусных миль по а/к
        private FlightBonusCard _bonusCard;

        public FlightBonusCard BonusCard
        {
            get { return _bonusCard; }
            set { _bonusCard = value; }
        }
    }
}