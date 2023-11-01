using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class GenerateOTPInputDto : HasTriggerInformation
    {
        //[JsonProperty("Mobile")]
        [MaxLength(20)]
        [Required]
        public string Mobile { get; set; }
    }


}
