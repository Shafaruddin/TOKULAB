using Abp.UI;
using Hangfire;
using Hangfire.Server;
using IRIS.CrmConnector.API.CRM;
using IRIS.CrmConnector.API.CRM.DTO.Output;
using IRIS.CrmConnector.API.Mappers;
using IRIS.CrmConnector.API.Storage.Interfaces;
using IRIS.CrmConnector.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace IRIS.CrmConnector.API.BackgroundWorkers
{
    public interface ISubmitAutoSavedCasesJob
    {
        Task Run(PerformContext context);
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new int[] { 60 * 60 })]
    public class SubmitAutoSavedCasesJob : ISubmitAutoSavedCasesJob
    {
        private readonly ILogger<SubmitAutoSavedCasesJob> _logger;
        private readonly IStorageService _storageService;
        private readonly ICrmManager _crmManager;
        public SubmitAutoSavedCasesJob(
            ILogger<SubmitAutoSavedCasesJob> logger,
            IStorageService storageService,
            ICrmManager crmManager
        )
        {
            _logger = logger;
            _storageService = storageService;
            _crmManager = crmManager;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10)]
        public async Task Run(PerformContext context)
        {
            var pendingCasesToAutoSave = await _storageService.GetAutoSavedCasesToSubmitAsync(DateTime.UtcNow);
            if (!pendingCasesToAutoSave.Any())
            {
                _logger.LogInformation($"No pending jobs to submit {DateTime.UtcNow} (UTC)");
                return;
            }

            _logger.LogInformation($"Trying to autosubmit {pendingCasesToAutoSave.Count} cases");
            _logger.LogInformation($"Pulling category defaults");
            var catsAndAdhocCriterias = await _crmManager.GetCategoryAndCriteriaAsync();
            var cat1Default = catsAndAdhocCriterias.Categories.First(x => x.Type == "Category1").CategoryId;
            var cat2Default = catsAndAdhocCriterias.Categories.First(x => x.Type == "Category2" && x.RelatedCategory1ID == cat1Default).CategoryId;
            var cat3Default = catsAndAdhocCriterias.Categories.First(x => x.Type == "Category3" && x.RelatedCategory2ID == cat2Default).CategoryId;

            foreach (var pendingCase in pendingCasesToAutoSave)
            {
                _logger.LogInformation($"Step 1: {pendingCase.CaseID} Pending Case Mapping");
                var mappedPendingCase = pendingCase.ToUpdateCaseInputDto(cat1Default, cat2Default, cat3Default);
                _logger.LogInformation($"Mapped object  {pendingCase.CaseID}  {mappedPendingCase}");

                _logger.LogInformation($"Step 2: {pendingCase.CaseID} Pending Case Submitting");
                try
                {
                    await _crmManager.UpdateCaseAsync(mappedPendingCase);
                    _logger.LogInformation($"Step 3: {pendingCase.CaseID} Deleting Pending Case");
                    pendingCase.IsSubmitted = true;
                    await _storageService.MarkAutoSaveRecordStatusAsync(pendingCase, pendingCase.IsSubmitted, "ok", true);
                    await _storageService.DeleteAutoSaveRecordAsync(mappedPendingCase.CaseID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{pendingCase.CaseID} submitting error");
                    await _storageService.MarkAutoSaveRecordStatusAsync(pendingCase, false, ex.Message, true);
                }
                await Task.Delay(1000);
            }
            await Task.CompletedTask;
        }
    }
}
