using IRIS.CrmConnector.API.CRM.DTO.Output;

namespace IRIS.CrmConnector.API.CRM.DTO
{
    public class ReturnStatusDto
    {
        //[JsonProperty("Message")]
        public string Message { get; set; }

        //[JsonProperty("MessageCode")]
        public MessageCodeEnum MessageCode { get; set; }
    }
}
