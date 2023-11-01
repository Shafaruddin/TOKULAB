using Azure;
using Azure.Data.Tables;
using IRIS.CrmConnector.API.CRM.DTO.Input;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DAO;

public class ShortenUpdateCaseUrlDao : ShortenUpdateCaseUrlDto, ITableEntity
{
    #region implement ITableEntity
    [JsonIgnore]
    public string PartitionKey { get; set; } = "Url";
    [JsonIgnore]
    public string RowKey { get => Url; set => Url = value; }
    [JsonIgnore]
    public DateTimeOffset? Timestamp { get; set; }
    [JsonIgnore]
    public ETag ETag { get; set; }
    #endregion

    #region Control Variables
    [Required]
    [MaxLength(100)]
    [JsonIgnore]
    public string CaseID { get; set; }
    [JsonIgnore]
    public DateTime LastUpdatedDateTimeUtc { get; set; }
    public string Url { get; set; }
    #endregion
}
