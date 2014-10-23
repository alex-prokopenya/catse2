using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelSearchEngine.Responses
{
    public class SegmentRule:_Response
    {
        
        //разрешен ли возврат до вылета
        private bool _allowedReturnBefore;

         [JsonMemberName("allowed_return_before")]
        public bool AllowedReturnBefore
        {
            get { return _allowedReturnBefore; }
            set { _allowedReturnBefore = value; }
        }

        //разрешен ли возврат после вылета
        private bool _allowedReturnAfter;

         [JsonMemberName("allowed_return_after")]
        public bool AllowedReturnAfter
        {
            get { return _allowedReturnAfter; }
            set { _allowedReturnAfter = value; }
        }

        //разрешен ли обмен до вылета
        private bool _allowedChangesBefore;

         [JsonMemberName("allowed_changes_before")]
        public bool AllowedChangesBefore
        {
            get { return _allowedChangesBefore; }
            set { _allowedChangesBefore = value; }
        }

        //разрешен ли обмен после вылета
        private bool _allowedChangesAfter;

         [JsonMemberName("allowed_changes_after")]
        public bool AllowedChangesAfter
        {
            get { return _allowedChangesAfter; }
            set { _allowedChangesAfter = value; }
        }

        //текст правил тарифа
        private string _rulesText;

         [JsonMemberName("rules_text")]
        public string RulesText
        {
            get { return _rulesText; }
            set { _rulesText = value; }
        }
    }
}