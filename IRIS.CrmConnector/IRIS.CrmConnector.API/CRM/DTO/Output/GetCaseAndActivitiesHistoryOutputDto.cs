namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class RecentCaseHistory
    {
        //[JsonProperty("CaseNumber")]
        public string CaseNumber { get; set; }

        //[JsonProperty("CaseTitle")]
        public string CaseTitle { get; set; }

        //[JsonProperty("CaseDetails")]
        public string CaseDetails { get; set; }

        //[JsonProperty("Owner")]
        public string Owner { get; set; }

        //[JsonProperty("StatusReason")]
        public string StatusReason { get; set; }
    }

    public class RecentCallActivity
    {
        //[JsonProperty("DateTime")]
        public string DateTime { get; set; }

        //[JsonProperty("Subject")]
        public string Subject { get; set; }

        //[JsonProperty("CallStatus")]
        public string CallStatus { get; set; }

        //[JsonProperty("Owner")]
        public string Owner { get; set; }
    }

    public class RecentEmailActivity
    {
        //[JsonProperty("DateTime")]
        public string DateTime { get; set; }

        //[JsonProperty("Subject")]
        public string Subject { get; set; }

        //[JsonProperty("StatusReason")]
        public string StatusReason { get; set; }

        //[JsonProperty("Owner")]
        public string Owner { get; set; }
    }

    public class RecentSMSActivity
    {
        //[JsonProperty("DateTime")]
        public string DateTime { get; set; }

        //[JsonProperty("Subject")]
        public string Subject { get; set; }

        //[JsonProperty("SMSMessage")]
        public string SMSMessage { get; set; }

        //[JsonProperty("Owner")]
        public string Owner { get; set; }
    }

    public class GetCaseAndActivitiesHistoryOutputDto : HasReturnStatus
    {
        //[JsonProperty("RecentCaseHistory")]
        public List<RecentCaseHistory> RecentCaseHistory { get; set; }

        //[JsonProperty("RecentCallActivities")]
        public List<RecentCallActivity> RecentCallActivities { get; set; }

        //[JsonProperty("RecentEmailActivities")]
        public List<RecentEmailActivity> RecentEmailActivities { get; set; }

        //[JsonProperty("RecentSMSActivities")]
        public List<RecentSMSActivity> RecentSMSActivities { get; set; }
    }


}
