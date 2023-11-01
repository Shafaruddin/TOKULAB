namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class GetCustomerOutputDto: HasReturnStatus
    {
        //[JsonProperty("Salutation")]
        public string Salutation { get; set; }

        //[JsonProperty("CustomerName")]
        public string CustomerName { get; set; }

        //[JsonProperty("CustomerGUID")]
        public string CustomerGUID { get; set; }

        //[JsonProperty("CustomerType")]
        public string CustomerType { get; set; }

        //[JsonProperty("AccountNumber")]
        public string AccountNumber { get; set; }

        //[JsonProperty("AccountStatus")]
        public string AccountStatus { get; set; }

        //[JsonProperty("CustomerNote")]
        public string CustomerNote { get; set; }

        //[JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        //[JsonProperty("MobileNo")]
        public string MobileNo { get; set; }

        //[JsonProperty("HomePhone")]
        public object HomePhone { get; set; }

        //[JsonProperty("ResidentialAddress")]
        public string ResidentialAddress { get; set; }

        //[JsonProperty("IDType")]
        public string IDType { get; set; }

        //[JsonProperty("IDNumber")]
        public string IDNumber { get; set; }

        //[JsonProperty("FinPassportExpiryDate")]
        public string FinPassportExpiryDate { get; set; }

        //[JsonProperty("DOB")]
        public string DOB { get; set; }

        //[JsonProperty("Gender")]
        public string Gender { get; set; }

        //[JsonProperty("StatusFlags")]
        public List<StatusFlagOutputDto> StatusFlags { get; set; }
    }
}
