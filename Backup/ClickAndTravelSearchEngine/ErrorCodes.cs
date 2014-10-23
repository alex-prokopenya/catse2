using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelSearchEngine
{
    public class ErrorCodes
    {
        //ErrorCode 18: Неизвестный тикет айди
        public const int UnknownTicketId = 18;

        #region TURIST
        //ErrorCode 90: ошибка в дате рождения туриста
        public const int WrongTuristAges = 90;

        //ErrorCode 91: ошибка в дате рождения туриста
        public const int TuristCanntParseBirthdate = 91;

        //ErrorCode 92: ошибка в имени/фамилии
        public const int TuristCanntParseName = 92;

        //ErrorCode 93: ошибка в гражданстве
        public const int TuristCanntParseCitizen=93;

        //ErrorCode 94: ошибка в номере паспорта
        public const int TuristCanntPaspNum = 94;

        //ErrorCode 95: ошибка в сроке действия паспорта
        public const int TuristCanntPaspDate = 95;

        //ErrorCode 96: дублируется турист
        public const int TuristDuplicate = 96;

        //ErrorCode 97:  ошибка в бонусной карте а/к
        public const int TuristBonusCardError = 94;

        #endregion

        #region USERINFO
        //ErrorCode 98:  ошибка в бонусной карте а/к
        public const int UserInvalidMail = 98;

        //ErrorCode 99:  ошибка в бонусной карте а/к
        public const int UserInvalidPshone = 99;
        #endregion

        #region HOTEL_INPS
        public const int HotelCityUnknown = 31;
        public const int HotelDatesWrong = 32;
        public const int HotelStarsWrong = 33;
        public const int HotelPansionsWrong = 34;
        public const int HotelRoomsInvalidCount = 35;
        public const int HotelRoomsTurists = 36;
        public const int HotelUnknownSearchId = 37;
        public const int HotelUnknownHotelId = 38;
        public const int HotelUnknownVariantId = 39;

        #endregion

    }
}