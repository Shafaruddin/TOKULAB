using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class ChallengeOTPInputDto : HasTriggerInformation
    {
        //[JsonProperty("Mobile")]
        [MaxLength(20)]
        [Required]
        public string Mobile { get; set; }

        //[JsonProperty("OTPValidate")]
        [MaxLength(6)]
        [Required]
        public string OTPValidate { get; set; }
    }


}
