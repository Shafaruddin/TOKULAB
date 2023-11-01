using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class GetCaseAndActivitiesHistoryInputDto : HasTriggerInformation
    {
        [Required]
        //[JsonProperty("CustomerGUID")]
        public Guid CustomerGuid { get; set; }
    }
}
