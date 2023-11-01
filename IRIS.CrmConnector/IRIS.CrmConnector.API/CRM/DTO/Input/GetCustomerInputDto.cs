using Westcon.CrmClient.Model;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class GetCustomerInputDto : HasTriggerInformation
    {
        /// <summary>
        /// Gets or Sets IDNumber
        /// </summary>
        //[JsonProperty("IDNumber")]
        public string? IDNumber { get; set; }

        /// <summary>
        /// Gets or Sets CustomerGUID
        /// </summary>
        //[JsonProperty("CustomerGUID")]
        public Guid? CustomerGUID { get; set; }

        /// <summary>
        /// Gets or Sets SPAAccountNumber
        /// </summary>
        //[JsonProperty("SPAAccountNumber")]
        public string? SPAAccountNumber { get; set; }

        /// <summary>
        /// Gets or Sets Anonymous
        /// </summary>
        //[JsonProperty("Anonymous")]
        public bool? Anonymous { get; set; }

        //[JsonProperty("DataSet")]
        public DataSetEnum DataSet { get; set; }
    }
}
