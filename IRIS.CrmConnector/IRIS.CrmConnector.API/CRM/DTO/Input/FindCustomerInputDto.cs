using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class FindCustomerInputDto : HasTriggerInformation
    {
        //[JsonProperty("Name")]
        [MaxLength(338)]
        public string? Name { get; set; }

        //[JsonProperty("IDNumber")]
        [MaxLength(20)]
        public string? IDNumber { get; set; }

        //[JsonProperty("AccountNumber")]
        [MaxLength(9)]
        public string? AccountNumber { get; set; }

        //[JsonProperty("MobileNo")]
        [MaxLength(20)]
        public string? MobileNo { get; set; }

        //[JsonProperty("EmailAddress")]
        [MaxLength(100)]
        public string? EmailAddress { get; set; }
    }
}
