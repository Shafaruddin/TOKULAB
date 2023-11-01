namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class CreateUpdateCaseOutputDto
    {
        //[JsonProperty("CaseID")]
        public string CaseID { get; set; }

        //[JsonProperty("ReturnStatus")]
        public ReturnStatusDto ReturnStatus { get; set; }
    }


}
