using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Westcon.CrmClient.Model;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public record UpdateCaseAutoSaveInputDto
    {
        #region Properties
        public Guid? CustomerGUID { get; set; }

        [MaxLength(300)]
        public string? CaseTitle { get; set; }

        [MaxLength(10000)]
        public string? CaseDetails { get; set; }

        public bool? SystemAuthenticated { get; set; }

        public bool? ManualVerification { get; set; }

        public Guid? IPCCCallExtensionID { get; set; }

        [MaxLength(100)]
        public string? Owner { get; set; }

        public Guid? CaseCategory1 { get; set; }

        public Guid? CaseCategory2 { get; set; }

        public Guid? CaseCategory3 { get; set; }

        public Guid? AdHocCriteria { get; set; }

        [MaxLength(20000)]
        public string? FollowupAction { get; set; }

        public ContactModeInputEnum? ContactMode { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public bool? FollowupRequired { get; set; }

        public CaseStatusInputEnum? CaseStatus { get; set; }

        [MaxLength(200)]
        public string? Resolution { get; set; }

        //public bool CallBack { get; set; }

        [MaxLength(100)]
        public string? TriggeredFrom { get; set; }

        [MaxLength(100)]
        public string? TriggeredBy { get; set; }
        #endregion
    }
}
