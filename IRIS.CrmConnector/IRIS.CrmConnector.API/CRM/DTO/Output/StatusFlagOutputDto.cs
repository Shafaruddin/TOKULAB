namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class StatusFlagOutputDto
    {
        public string? StatusFlag { get; set; }
        //[JsonProperty("StatusName")]
        public string StatusName { get; set; }

        //[JsonProperty("StatusChangedReason")]
        public string StatusChangedReason { get; set; }
    }
}
