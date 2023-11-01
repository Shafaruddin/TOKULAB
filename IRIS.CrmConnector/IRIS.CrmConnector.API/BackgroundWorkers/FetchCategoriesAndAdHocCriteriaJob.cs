using Abp.UI;
using Hangfire;
using Hangfire.Server;
using IRIS.CrmConnector.API.CRM;
using IRIS.CrmConnector.API.CRM.DTO.Output;
using IRIS.CrmConnector.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace IRIS.CrmConnector.API.BackgroundWorkers
{
    public interface IFetchCategoriesAndAdHocCriteriaJob
    {
        Task Run(PerformContext context);
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new int[] { 60 * 60 })]
    public class FetchCategoriesAndAdHocCriteriaJob : IFetchCategoriesAndAdHocCriteriaJob
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMemoryCache _memoryCache;
        private readonly CrmClient _client;
        private readonly ILogger<FetchCategoriesAndAdHocCriteriaJob> _logger;

        public FetchCategoriesAndAdHocCriteriaJob(
            IMemoryCache memoryCache,
            CrmClient client,
            ILogger<FetchCategoriesAndAdHocCriteriaJob> logger,
            IBackgroundJobClient backgroundJobClient
        )
        {
            _memoryCache = memoryCache;
            _backgroundJobClient = backgroundJobClient;
            _client = client;
            _logger = logger;
        }

        //[DisableConcurrentExecution(timeoutInSeconds: 10)]
        public async Task Run(PerformContext context)
        {
            var result = await _client.GetCategoryAndCriteriaAsync();
            if (result == null)
            {
                _logger.LogError($"GetCategoryAndCriteriaAsync is null");
                throw new ApplicationException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Success)
            {
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while getting categories and criterias");
            }

            //filter only active categories

            if (result.Categories != null)
            {
                result.Categories = result.Categories.Where(x => x.Status == "Active").ToList();
            }

            if (result.AdhocCriteria != null)
            {
                result.AdhocCriteria = result.AdhocCriteria.Where(x => x.Status == "Active").ToList();
            }

            _memoryCache.Set<GetCategoryAndCriteriaOutputDto>(Constants.CACHE.CATEGORY_AND_ADHOC_CRITERIA, result);

            await Task.CompletedTask;
        }
    }
}
