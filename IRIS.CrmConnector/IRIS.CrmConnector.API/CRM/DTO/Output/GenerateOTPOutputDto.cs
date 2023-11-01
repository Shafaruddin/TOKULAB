namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class GenerateOTPOutputDto
    {
        #region angular manipulation
        //[JsonProperty("Angular_CanRetry")]
        public bool CanRetry { get; set; }

        //[JsonProperty("Angular_RetryAfterSeconds")]
        public int RetryAfterSeconds { get; set; }

        //[JsonProperty("Angular_ShouldChangeMobileNumber")]
        public bool ShouldChangeMobileNumber { get; set; }

        //[JsonProperty("Angular_OTPSent")]
        public bool OTPSent { get; set; }

        //[JsonProperty("Angular_SecondsRemainingToValidate")]
        public int SecondsRemaining { get; set; }
        #endregion


        //[JsonProperty("ResultCode")]
        public string ResultCode { get; set; }

        //[JsonProperty("ResultMessage")]
        public string ResultMessage { get; set; }

        //[JsonProperty("ReturnStatus")]
        public ReturnStatusDto ReturnStatus { get; set; }
    }
}
