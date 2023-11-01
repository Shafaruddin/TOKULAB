using IRIS.CrmConnector.API.CRM.DAO;
using IRIS.CrmConnector.API.Retail.DTO;
using IRIS.CrmConnector.API.Storage.DTOs;

namespace IRIS.CrmConnector.API.Storage.Interfaces;

public interface IStorageService
{
    Task<List<StatusFlagDto>> GetStatusFlagsAsync();
    Task UpsertStatusFlagsAsync(StatusFlagDto accountStatusDto);
    Task DeleteStatusFlagsAsync(string accountStatusCode);
    Task<List<RetailOutputDto>> GetRetailersAsync();
    Task UpsertRetailersAsync(List<RetailOutputDto> retailersList);
    Task UpsertCaseAutoSaveRecordAsync(UpdateCaseAutoSaveDao updateCaseInputAuto);
    Task MarkAutoSaveRecordStatusAsync(UpdateCaseAutoSaveDao updateCaseAutoSaveDto, bool IsSubmitted, string? SubmitException, bool ShouldNotRetry = true);
    Task DeleteAutoSaveRecordAsync(string CaseID);
    Task<List<UpdateCaseAutoSaveDao>> GetAutoSavedCasesToSubmitAsync(DateTime currentTime);
    Task AuthenticateUser(string username, string password);
    Task UpsertUser(string username, string password);
    Task<string> UpsertShortenUrl(ShortenUpdateCaseUrlDao shortenUpdateCaseUrlDao);
    Task<ShortenUpdateCaseUrlDao> GetShortenedUrl(string url);
}
