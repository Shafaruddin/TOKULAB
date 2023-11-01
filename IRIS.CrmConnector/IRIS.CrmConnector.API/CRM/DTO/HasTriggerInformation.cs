using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO
{
    public class HasTriggerInformation
    {
        //[JsonProperty("TriggeredFrom")]
        [Required]
        [MaxLength(100)]
        public string TriggeredFrom { get; set; }

        //[JsonProperty("TriggeredBy")]
        [Required]
        [MaxLength(100)]
        public string TriggeredBy { get; set; }
    }
}
