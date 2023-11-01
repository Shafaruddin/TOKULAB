using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.Retail.DTO;

public class RetailOutputDto//: ITableEntity
{
    [Required]
    public string RetailerNo { get; set; }
    public string ContactNos { get; set; }
    public string OutletName { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string PostalCode { get; set; }
    public string AreaManager { get; set; }
    public string InCharge { get; set; }
    public string OutletStaff { get; set; }
    public string EmailAddress { get; set; }
    public string FEInChargeByArea { get; set; }

    //[JsonIgnore]
    //public string PartitionKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //[JsonIgnore]
    //public string RowKey { get => RetailerNo; set; }

    //[JsonIgnore]
    //public DateTimeOffset? Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    
    //[JsonIgnore]
    //public ETag ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
