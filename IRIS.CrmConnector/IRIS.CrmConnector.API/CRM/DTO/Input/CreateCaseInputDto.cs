using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class CreateCaseInputDto : HasTriggerInformation
    {
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


        //[JsonProperty("DateTimeReceived")]
        [Required]
        public DateTime DateTimeReceived { get; set; }

        //[JsonProperty("IPCCCallExtensionID")]
        [Required]
        [MaxLength(100)]
        public string IPCCCallExtensionID { get; set; }

        //[JsonProperty("PrimaryCaseOfficer")]
        [Required]
        [MaxLength(100)]
        public string PrimaryCaseOfficer { get; set; }

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

        //[JsonProperty("ContactMode")]
        public ContactModeInputEnum ContactMode { get; set; }

        //[JsonProperty("CreatedOn")]
        [Required]
        public DateTime CreatedOn { get; set; }

        //[JsonProperty("CreatedBy")]
        [Required]

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        //[JsonProperty("ModifiedBy")]
        [Required]

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        //[JsonProperty("CallBack")]
        public bool CallBack { get; set; }
    }
}
