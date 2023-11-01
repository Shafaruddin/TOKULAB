namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Customer
    {
        //[JsonProperty("CustomerName")]
        public string CustomerName { get; set; }

        //[JsonProperty("IDNumber")]
        public string IDNumber { get; set; }

        //[JsonProperty("AccountNumber")]
        public string AccountNo { get; set; }

        //[JsonProperty("MobileNo")]
        public string MobileNo { get; set; }

        //[JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        //[JsonProperty("CustomerGUID")]
        public string CustomerGUID { get; set; }
    }

    public class FindCustomerOutputDto
    {
        //[JsonProperty("Customers")]
        public List<Customer> Customers { get; set; }

        //[JsonProperty("TotalRecordsFound")]
        public string TotalRecordsFound { get; set; }

        //[JsonProperty("ReturnStatus")]
        public ReturnStatusDto ReturnStatus { get; set; }
    }
}
