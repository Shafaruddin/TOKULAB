namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class ChallengeOTPOutputDto
    {
        //#region angular manipulation
        ////[JsonProperty("Angular_ValidationStatus")]
        //public bool Accepted { get; set; }

        ////[JsonProperty("Angular_ShouldRetry")]
        //public bool CanRetry { get; set; }

        ////[JsonProperty("Angular_ShouldCancel")]
        //public bool ShouldCancel { get; set; }
        ////[JsonProperty("Angular_ResultMessage")]
        //public string ResultMessage { get; set; }
        //#endregion

        //[JsonProperty("ValidationStatus")]
        public string ValidationStatus { get; set; }

        //[JsonProperty("ReturnStatus")]
        public ReturnStatusDto ReturnStatus { get; set; }
    }
}
