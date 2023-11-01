using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Westcon.CrmClient.Model;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class UpdateCaseInputDto : HasTriggerInformation
    {
        //[JsonProperty("CaseID")]
        [MaxLength(100)]
        [Required]
        public string CaseID { get; set; }

        //[JsonProperty("CustomerGUID")]
        [Required]
        public Guid CustomerGUID { get; set; }

        //[JsonProperty("CaseTitle")]
        [Required]
        [MaxLength(300)]
        public string CaseTitle { get; set; }

        //[JsonProperty("CaseDetails")]
        [Required]
        [MaxLength(10000)]
        public string CaseDetails { get; set; }

        //[JsonProperty("SystemAuthenticated")]
        public bool SystemAuthenticated { get; set; }
        //[JsonProperty("ManualVerification")]
        public bool ManualVerification { get; set; }

        //[JsonProperty("IPCCCallExtensionID")]
        //[MaxLength(32)]
        [Required]
        public Guid IPCCCallExtensionID { get; set; }

        //[JsonProperty("Owner")]
        [Required]
        [MaxLength(100)]
        public string Owner { get; set; }

        //[JsonProperty("CaseCategory1")]
        [Required]
        public Guid CaseCategory1 { get; set; }

        //[JsonProperty("CaseCategory2")]
        [Required]
        public Guid CaseCategory2 { get; set; }

        //[JsonProperty("CaseCategory3")]
        [Required]
        public Guid CaseCategory3 { get; set; }


        //[JsonProperty("AdHocCriteria")]
        public Guid? AdHocCriteria { get; set; }

        //[JsonProperty("FollowupAction")]
        [MaxLength(20000)]
        [Required]
        public string FollowupAction { get; set; }

        //[JsonProperty("ContactMode")]
        public ContactModeInputEnum ContactMode { get; set; }

        //[JsonProperty("ModifiedBy")]
        [MaxLength(100)]
        [Required]
        public string ModifiedBy { get; set; }

        //[JsonProperty("FollowupRequired")]
        public bool FollowupRequired { get; set; }

        //[JsonProperty("CaseStatus")]
        public CaseStatusInputEnum CaseStatus { get; set; }

        //[JsonProperty("Resolution")]
        [MaxLength(200)]
        public string? Resolution { get; set; }

        //[JsonProperty("CallBack")]
        //public bool CallBack { get; set; }
    }
}
