using Azure;
using Azure.Data.Tables;
using IRIS.CrmConnector.API.CRM.DTO.Input;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Westcon.CrmClient.Model;

namespace IRIS.CrmConnector.API.CRM.DAO;

public record UpdateCaseAutoSaveDao : UpdateCaseAutoSaveInputDto, ITableEntity
{
    #region implement ITableEntity
    [JsonIgnore]
    public string PartitionKey { get; set; } = "Case";
    [JsonIgnore]
    public string RowKey { get => CaseID; set => CaseID = value; }
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
    [JsonIgnore]
    public bool IsSubmitted { get; set; }
    [JsonIgnore]
    public bool ShouldNotRetry { get; set; }
    [JsonIgnore]
    public string? SubmitException { get; set; }
    #endregion
}
