using Azure;
using Azure.Data.Tables;

namespace IRIS.CrmConnector.API.Security.Authorization.DTO;

public record UserDto: ITableEntity
{
    #region implement ITableEntity
    public string PartitionKey { get; set; } = "UserName";
    public string RowKey { get => Username; set => Username = value; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    #endregion

    #region Control Variables
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    #endregion
}
