using IRIS.CrmConnector.API.Storage.Interfaces;

namespace IRIS.CrmConnector.API.Storage;

using Abp.UI;
using Azure;
using Azure.Data.Tables;
using CSharpVitamins;
using IRIS.CrmConnector.API.CRM.DAO;
using IRIS.CrmConnector.API.Retail.DTO;
using IRIS.CrmConnector.API.Security.Authorization;
using IRIS.CrmConnector.API.Security.Authorization.DTO;
using IRIS.CrmConnector.API.Storage.DTOs;
using IRIS.CrmConnector.Shared;
using Medienstudio.Azure.Data.Tables.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class AzureStorageService : IStorageService
{
    private readonly ILogger<AzureStorageService> _logger;
    private TableServiceClient _tableServiceClient;
    private readonly IMemoryCache _memoryCache;

    private const string STATUS_FLAGS_TABLE_NAME = "AccountStatus";
    private const string STATUS_FLAGS_TABLE_PARTITION_KEY = "StatusFlag";
    private const string STATUS_FLAGS_TABLE_VALUE_COLUMN_NAME = "Description";

    private const string RETAIL_TABLE_NAME = "Retail";
    private const string RETAIL_TABLE_PARTITION_KEY = "RetailerNo";

    private const string USER_TABLE_NAME = "User";
    private const string USER_TABLE_PARTITION_KEY = "UserName";

    private const string CASE_AUTO_SAVE_TABLE_NAME = "CaseAutoSave";

    private readonly int CASE_AUTO_UPDATE_IN_MINUTES;

    private const string UPDATE_CASE_SHORT_URL_SAVE_TABLE_NAME = "UpdateCaseShortUrl";

    public AzureStorageService(
        ILogger<AzureStorageService> logger,
        IConfiguration Configuration,
        IMemoryCache memoryCache
    )
    {
        _memoryCache = memoryCache;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(Configuration[Constants.AZURE.TABLE_STORAGE_CONNECTION_STRING]))
        {
            _logger.LogCritical($"{Constants.AZURE.TABLE_STORAGE_CONNECTION_STRING} is empty");
            throw new ArgumentNullException(Constants.AZURE.TABLE_STORAGE_CONNECTION_STRING);
        }

        _tableServiceClient = new TableServiceClient(Configuration[Constants.AZURE.TABLE_STORAGE_CONNECTION_STRING]);

        CASE_AUTO_UPDATE_IN_MINUTES = int.Parse(Configuration[Constants.CRM.CASE_AUTO_UPDATE_IN_MINUTES]);
    }

    private async Task<TableClient> GetTableClient(string tableName)
    {
        await _tableServiceClient.CreateTableIfNotExistsAsync(tableName);

        return _tableServiceClient.GetTableClient(tableName);
    }

    #region StatusFlags

    public async Task<List<StatusFlagDto>> GetStatusFlagsAsync()
    {
        List<StatusFlagDto> cachedValue;
        if (_memoryCache.TryGetValue(Constants.CACHE.STATUS_FLAGS, out cachedValue))
        {
            _logger.LogInformation($"returning cached value for {Constants.CACHE.STATUS_FLAGS}");
            return cachedValue;
        }

        var tableClient = await GetTableClient(STATUS_FLAGS_TABLE_NAME);

        try
        {
            // query entities with linq and select
            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(
                filter: x => x.PartitionKey == STATUS_FLAGS_TABLE_PARTITION_KEY,
                select: new List<string> { STATUS_FLAGS_TABLE_VALUE_COLUMN_NAME, "RowKey" });

            List<StatusFlagDto> returnList = new();

            // Iterate the list in order to access individual queried entities.
            await foreach (var qEntity in queryResults)
            {
                returnList.Add(new()
                {
                    Code = qEntity.RowKey,
                    Description = qEntity.GetString(STATUS_FLAGS_TABLE_VALUE_COLUMN_NAME)
                });
            }

            _memoryCache.Set<List<StatusFlagDto>>(Constants.CACHE.STATUS_FLAGS, returnList.OrderBy(x => x.Code).ToList());

            return returnList.OrderBy(x => x.Code).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to GetAll {STATUS_FLAGS_TABLE_NAME}");
            _logger.LogTrace(ex.StackTrace);
            throw new UserFriendlyException("Unable retrieve Account Status Mappings");
        }
    }

    public async Task UpsertStatusFlagsAsync(StatusFlagDto accountStatusDto)
    {
        var tableClient = await GetTableClient(STATUS_FLAGS_TABLE_NAME);

        var entity = new TableEntity(STATUS_FLAGS_TABLE_PARTITION_KEY, accountStatusDto.Code)
            {
                { STATUS_FLAGS_TABLE_VALUE_COLUMN_NAME, accountStatusDto.Description }
            };

        try
        {
            await tableClient.UpsertEntityAsync(entity);
            _memoryCache.Remove(Constants.CACHE.STATUS_FLAGS);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to UpsertEntityAsync accountStatus {@accountStatusDto}");
            _logger.LogTrace(ex.StackTrace);
            throw new UserFriendlyException("Unable create/modify Account Status Mapping");
        }
    }

    public async Task DeleteStatusFlagsAsync([NotNull] string accountStatusCode)
    {
        var tableClient = await GetTableClient(STATUS_FLAGS_TABLE_NAME);

        try
        {
            await tableClient.DeleteEntityAsync(STATUS_FLAGS_TABLE_PARTITION_KEY, accountStatusCode);
            _memoryCache.Remove(Constants.CACHE.STATUS_FLAGS);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to delete accountStatus {accountStatusCode}");
            _logger.LogTrace(ex.StackTrace);
            throw new UserFriendlyException("Unable to delete account status");
        }
    }

    #endregion

    #region Retailers

    public async Task<List<RetailOutputDto>> GetRetailersAsync()
    {
        List<RetailOutputDto> cachedValue;
        if (_memoryCache.TryGetValue<List<RetailOutputDto>>(Constants.CACHE.RETAILERS, out cachedValue))
        {
            _logger.LogInformation($"returning cached value for {Constants.CACHE.RETAILERS}");
            return cachedValue;
        }

        var tableClient = await GetTableClient(RETAIL_TABLE_NAME);

        var classPropertyNames = typeof(RetailOutputDto).GetProperties().Select(p => p.Name).ToList();

        try
        {
            // query entities with linq and select
            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(
                filter: x => x.PartitionKey == RETAIL_TABLE_PARTITION_KEY,
                select: classPropertyNames);

            List<RetailOutputDto> returnList = new();

            // Iterate the list in order to access individual queried entities.
            await foreach (var qEntity in queryResults)
            {
                returnList.Add(new()
                {
                    RetailerNo = qEntity.GetString("RetailerNo"),
                    ContactNos = qEntity.GetString("ContactNos"),
                    OutletName = qEntity.GetString("OutletName"),
                    AddressLine1 = qEntity.GetString("AddressLine1"),
                    AddressLine2 = qEntity.GetString("AddressLine2"),
                    PostalCode = qEntity.GetString("PostalCode"),
                    AreaManager = qEntity.GetString("AreaManager"),
                    InCharge = qEntity.GetString("InCharge"),
                    OutletStaff = qEntity.GetString("OutletStaff"),
                    EmailAddress = qEntity.GetString("EmailAddress"),
                    FEInChargeByArea = qEntity.GetString("FEInChargeByArea")
                });
            }

            _memoryCache.Set<List<RetailOutputDto>>(Constants.CACHE.RETAILERS, returnList);

            return returnList;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to GetAll {RETAIL_TABLE_NAME}");
            _logger.LogTrace(ex.StackTrace);
            throw new UserFriendlyException("Unable retrieve Account Status Mappings");
        }
    }

    public async Task UpsertRetailersAsync(List<RetailOutputDto> retailersList)
    {
        if (!retailersList.Any())
        {
            return;
        }

        var tableClient = await GetTableClient(RETAIL_TABLE_NAME);

        try
        {
            await tableClient.DeleteAllEntitiesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to clear table {RETAIL_TABLE_NAME}");
            _logger.LogTrace(ex.StackTrace);
        }

        foreach (var retailer in retailersList)
        {
            var retailerPropertyDict = retailer.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => (string)prop.GetValue(retailer, null));

            var entity = new TableEntity(RETAIL_TABLE_PARTITION_KEY, retailer.RetailerNo);

            foreach (var property in retailerPropertyDict)
            {
                entity.Add(property.Key, property.Value);
            }

            try
            {
                await tableClient.UpsertEntityAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to UpsertEntityAsync retailer {retailer}");
                _logger.LogTrace(ex.StackTrace);
            }
        }
        _memoryCache.Set<List<RetailOutputDto>>(Constants.CACHE.RETAILERS, retailersList);
    }

    #endregion

    #region AutoSave Case
    public async Task UpsertCaseAutoSaveRecordAsync(UpdateCaseAutoSaveDao updateCaseInputAuto)
    {
        var tableClient = await GetTableClient(CASE_AUTO_SAVE_TABLE_NAME);

        updateCaseInputAuto.LastUpdatedDateTimeUtc = DateTime.UtcNow;

        try
        {
            await tableClient.UpsertEntityAsync(updateCaseInputAuto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to UpsertCaseAutoSaveRecordAsync {updateCaseInputAuto.CaseID}");
            _logger.LogTrace(ex.StackTrace);
        }
    }

    public async Task MarkAutoSaveRecordStatusAsync(UpdateCaseAutoSaveDao updateCaseAutoSaveDao, bool IsSubmitted, string? SubmitException, bool ShouldNotRetry = true)
    {
        var tableClient = await GetTableClient(CASE_AUTO_SAVE_TABLE_NAME);

        updateCaseAutoSaveDao.IsSubmitted = IsSubmitted;
        updateCaseAutoSaveDao.SubmitException = SubmitException;
        updateCaseAutoSaveDao.ShouldNotRetry = ShouldNotRetry;

        try
        {
            await tableClient.UpsertEntityAsync(updateCaseAutoSaveDao);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to UpdateAutoSaveRecordStatusAsync {updateCaseAutoSaveDao.CaseID} {IsSubmitted} {SubmitException}");
            _logger.LogTrace(ex.StackTrace);
        }
    }

    public async Task DeleteAutoSaveRecordAsync(string CaseID)
    {
        var tableClient = await GetTableClient(CASE_AUTO_SAVE_TABLE_NAME);
        try
        {
            await tableClient.DeleteEntityAsync("Case", CaseID);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to DeleteAutoSaveRecordAsync {CaseID}");
            _logger.LogTrace(ex.StackTrace);
        }
    }

    public async Task<List<UpdateCaseAutoSaveDao>> GetAutoSavedCasesToSubmitAsync(DateTime currentTime)
    {
        var tableClient = await GetTableClient(CASE_AUTO_SAVE_TABLE_NAME);
        try
        {
            var AutoSavedCasesList = (await tableClient.GetAllEntitiesByPartitionKeyAsync<UpdateCaseAutoSaveDao>("Case")).ToList();
            return AutoSavedCasesList
                .Where(x => x.IsSubmitted == false && x.ShouldNotRetry == false && x.LastUpdatedDateTimeUtc.AddMinutes(CASE_AUTO_UPDATE_IN_MINUTES) < currentTime)
                .OrderByDescending(x => x.LastUpdatedDateTimeUtc)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to GetAutoSavedCasesToSubmitAsync {currentTime}");
            _logger.LogTrace(ex.StackTrace);
        }
        return new();
    }
    #endregion

    #region auth
    public async Task AuthenticateUser(string username, string password)
    {
        username = username.Trim().ToLowerInvariant();
        var tableClient = await GetTableClient(USER_TABLE_NAME);
        try
        {
            string passwordHash;
            if (!_memoryCache.TryGetValue(string.Format(Constants.CACHE.USER_PASSWORD_HASH, username), out passwordHash))
            {
                var matching_user = (await tableClient.GetEntityAsync<UserDto>(USER_TABLE_PARTITION_KEY, username));
                if (matching_user == null)
                {
                    throw new UserFriendlyException("User {username} not found");
                }
                passwordHash = matching_user.Value.PasswordHash;
                _memoryCache.Set<string>(string.Format(Constants.CACHE.USER_PASSWORD_HASH, username), passwordHash, absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
            }
            if (SecurePasswordHasher.Verify(password, passwordHash))
            {
                return;
            }
            else
            {
                throw new UserFriendlyException("Invalid Credentials");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unable to AuthenticateUser {username}");
            throw new UserFriendlyException("Authentitation Failure");
        }
    }

    public async Task UpsertUser(string username, string password)
    {
        username = username.Trim().ToLowerInvariant();
        var tableClient = await GetTableClient(USER_TABLE_NAME);
        try
        {
            string passwordHash = SecurePasswordHasher.Hash(password);
            await tableClient.UpsertEntityAsync<UserDto>(new UserDto()
            {
                Username = username,
                PasswordHash = passwordHash
            });
            _memoryCache.Set<string>(string.Format(Constants.CACHE.USER_PASSWORD_HASH, username), passwordHash, absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unable to UpsertUser {username}");
            throw new UserFriendlyException("Failure");
        }
    }
    #endregion

    #region ShortenUrl
    public async Task<string> UpsertShortenUrl(ShortenUpdateCaseUrlDao shortenUpdateCaseUrlDao)
    {
        var tableClient = await GetTableClient(UPDATE_CASE_SHORT_URL_SAVE_TABLE_NAME);

        Guid guid = Guid.NewGuid();
        ShortGuid sguid1 = guid;
        shortenUpdateCaseUrlDao.LastUpdatedDateTimeUtc = DateTime.UtcNow;
        shortenUpdateCaseUrlDao.Url = sguid1;

        try
        {
            await tableClient.UpsertEntityAsync(shortenUpdateCaseUrlDao);
            return shortenUpdateCaseUrlDao.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to UpsertShortenUrl {shortenUpdateCaseUrlDao.CaseID}");
            _logger.LogTrace(ex.StackTrace);
            throw new UserFriendlyException("Unable to generate short url");
        }
    }

    public async Task<ShortenUpdateCaseUrlDao> GetShortenedUrl(string url)
    {
        var tableClient = await GetTableClient(UPDATE_CASE_SHORT_URL_SAVE_TABLE_NAME);
        try
        {
            var shortenedUrlRecord = await tableClient.GetEntityAsync<ShortenUpdateCaseUrlDao>("Url", url);
            return shortenedUrlRecord;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new UserFriendlyException("Url Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to GetShortenedUrl {url}");
            _logger.LogTrace(ex.StackTrace);
            throw;
        }
    }
    #endregion
}
