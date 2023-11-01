
using Abp.UI;
using IRIS.CrmConnector.API.CRM.DTO.Input;
using IRIS.CrmConnector.API.CRM.DTO.Output;
using IRIS.CrmConnector.API.Storage.Interfaces;
using IRIS.CrmConnector.Shared;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace IRIS.CrmConnector.API.CRM
{
    public interface ICrmManager
    {
        Task<GetCustomerOutputDto> GetCustomerAsync(GetCustomerInputDto input);
        Task<FindCustomerOutputDto> FindCustomerAsync(FindCustomerInputDto input);
        Task<CreateUpdateCaseOutputDto> CreateCaseAsync(CreateCaseInputDto input);
        Task<CreateUpdateCaseOutputDto> UpdateCaseAsync(UpdateCaseInputDto input);
        Task<GetCaseAndActivitiesHistoryOutputDto> GetCaseAndActivitiesHistoryAsync(GetCaseAndActivitiesHistoryInputDto input);
        Task<GetCategoryAndCriteriaOutputDto> GetCategoryAndCriteriaAsync();
        Task<GenerateOTPOutputDto> GenerateOTPAsync(GenerateOTPInputDto input);
        Task<ChallengeOTPOutputDto> ChallengeOTPAsync(ChallengeOTPInputDto input);
    }

    public class CrmManager : ICrmManager
    {
        private readonly ILogger<ICrmManager> _logger;
        private readonly CrmClient _client;
        private readonly IMemoryCache _memoryCache;
        private readonly IStorageService _storageService;

        public CrmManager(
            CrmClient client,
            IConfiguration Configuration,
            ILogger<ICrmManager> logger,
            IMemoryCache memoryCache,
            IStorageService storageService
        )
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _client = client;
            _storageService = storageService;
        }

        public async Task<GetCustomerOutputDto> GetCustomerAsync(GetCustomerInputDto input)
        {
            _logger.LogInformation($"Trying to GetCustomer");
            var result = await _client.GetCustomerAsync(input);
            if (result == null)
            {
                _logger.LogError($"GetCustomer is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Success)
            {
                LogApiInput(nameof(GetCustomerAsync), input);
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while getting customer");
            }

            //map statusFlag to statusFlagOutput
            if (result.StatusFlags == null)
            {
                return result;
            }

            _logger.LogInformation($"Trying to GetStatusFlagsAsync");
            var statusFlags = await _storageService.GetStatusFlagsAsync();

            foreach (var statusFlagRecord in result.StatusFlags)
            {
                statusFlagRecord.StatusFlag = statusFlagRecord.StatusName;
                var statusFlagMatchedResult = statusFlags.Where(x=>x.Code== statusFlagRecord.StatusName).FirstOrDefault();
                if (statusFlagMatchedResult != null)
                {
                    statusFlagRecord.StatusFlag = statusFlagMatchedResult.Description;
                }
                else
                {
                    _logger.LogWarning($"Unmapped value (key not present): {statusFlagRecord.StatusName}");
                }
            }

            return result;
        }


        public async Task<FindCustomerOutputDto> FindCustomerAsync(FindCustomerInputDto input)
        {
            _logger.LogInformation($"Trying to FindCustomerAsync");
            var result = await _client.FindCustomerAsync(input);
            if (result == null)
            {
                _logger.LogError($"FindCustomerAsync is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Success)
            {
                LogApiInput(nameof(GetCustomerAsync), input);
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while finding customer");
            }
            return result;
        }

        public async Task<CreateUpdateCaseOutputDto> CreateCaseAsync(CreateCaseInputDto input)
        {
            _logger.LogInformation($"Trying to CreateCaseAsync");
            var result = await _client.CreateCaseAsync(input);
            if (result == null)
            {
                _logger.LogError($"CreateCaseAsync is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Created)
            {
                LogApiInput(nameof(GetCustomerAsync), input);
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while creating case");
            }
            return result;
        }
        public async Task<CreateUpdateCaseOutputDto> UpdateCaseAsync(UpdateCaseInputDto input)
        {
            _logger.LogInformation($"Trying to UpdateCaseAsync");
            var result = await _client.UpdateCaseAsync(input);
            if (result == null)
            {
                _logger.LogError($"UpdateCaseAsync is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Updated)
            {
                LogApiInput(nameof(GetCustomerAsync), input);
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while updating case");
            }
            return result;
        }

        public async Task<GetCaseAndActivitiesHistoryOutputDto> GetCaseAndActivitiesHistoryAsync(GetCaseAndActivitiesHistoryInputDto input)
        {
            _logger.LogInformation($"Trying to GetCaseAndActivitiesHistoryAsync for {input.CustomerGuid} (Agent {input.TriggeredBy})");
            var result = await _client.GetCaseAndActivitiesHistoryAsync(input);
            if (result == null)
            {
                _logger.LogError($"GetCaseAndActivitiesHistoryAsync is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus.MessageCode} {result?.ReturnStatus.Message}");
            if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Success)
            {
                LogApiInput(nameof(GetCustomerAsync), input);
                throw new UserFriendlyException(result?.ReturnStatus.Message ?? "Error while getting case and activities history");
            }
            return result;
        }

        public async Task<GetCategoryAndCriteriaOutputDto> GetCategoryAndCriteriaAsync()
        {
            _logger.LogInformation($"Trying to GetCategoryAndCriteriaAsync");

            GetCategoryAndCriteriaOutputDto cachedValue;
            if (_memoryCache.TryGetValue(Constants.CACHE.CATEGORY_AND_ADHOC_CRITERIA, out cachedValue))
            {
                _logger.LogInformation($"returning cached value");
                return cachedValue;
            }

            var result = await _client.GetCategoryAndCriteriaAsync();
            if (result == null)
            {
                _logger.LogError($"GetCategoryAndCriteriaAsync is null");
                throw new UserFriendlyException("Server Error");
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
            _logger.LogInformation($"Cached value updated");
            return result;
        }

        public async Task<GenerateOTPOutputDto> GenerateOTPAsync(GenerateOTPInputDto input)
        {
            _logger.LogInformation($"Trying to GenerateOTPAsync for {input.Mobile}");
            var result = await _client.GenerateOTPAsync(input);
            if (result == null)
            {
                _logger.LogError($"GenerateOTPAsync is null");
                throw new UserFriendlyException("Server Error");
            }
            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus?.MessageCode} {result?.ReturnStatus?.Message}");
            _logger.LogInformation($"Result {result?.ResultCode} {result?.ResultMessage}");
            switch (result.ResultCode)
            {
                case "01010":
                    result.OTPSent = true;
                    result.SecondsRemaining = Constants.OTP_VERIFICATION_WINDOW_SECONDS;
                    break;
                case "01011":
                    throw new UserFriendlyException("Invalid Request Format");
                    result.ShouldChangeMobileNumber = true;
                    break;
                case "01012":
                    throw new UserFriendlyException("Unauthorized Access");
                    result.CanRetry = true;
                    result.RetryAfterSeconds = Constants.OTP_RETRY_SENDING_TIME_SECONDS;
                    break;
                case "01013":
                    throw new UserFriendlyException("Transient system error, please retry after 60 seconds");
                    result.CanRetry = true;
                    result.RetryAfterSeconds = 60;
                    break;
                case "01014":
                    throw new UserFriendlyException("Unable to route to mobile operator for the attempted mobile number");
                    result.ShouldChangeMobileNumber = true;
                    break;
                case "01015":
                    throw new UserFriendlyException("The credit balance for the API account is not enough.");
                    result.CanRetry = true;
                    result.RetryAfterSeconds = Constants.OTP_RETRY_SENDING_TIME_SECONDS;
                    break;
                case "01018":
                    throw new UserFriendlyException("The mobile number attempted Is blacklisted.");
                    result.ShouldChangeMobileNumber = true;
                    break;
                default:
                    throw new UserFriendlyException("Unknown Error");
                    result.ResultMessage = "Unknown Error";
                    result.CanRetry = true;
                    result.RetryAfterSeconds = Constants.OTP_RETRY_SENDING_TIME_SECONDS;
                    break;
            }

            //if (result?.ReturnStatus.MessageCode != MessageCodeEnum.Success)
            //{
            //    throw new UserFriendlyException(result?.ResultMessage, result?.ReturnStatus.Message);
            //}
            return result;
        }

        public async Task<ChallengeOTPOutputDto> ChallengeOTPAsync(ChallengeOTPInputDto input)
        {
            _logger.LogInformation($"Trying to GenerateOTPAsync for {input.Mobile}");
            var result = await _client.ChallengeOTPAsync(input);
            if (result == null)
            {
                _logger.LogError($"GenerateOTPAsync is null");
                throw new UserFriendlyException("Server Error");
            }

            _logger.LogInformation($"ReturnStatus {result?.ReturnStatus?.MessageCode} {result?.ReturnStatus?.Message}");
            _logger.LogInformation($"Result {result?.ValidationStatus}");
            if (result?.ValidationStatus == "ACCEPT")
            {
                return result;
            }

            switch (result.ValidationStatus)
            {
                //case "ACCEPT":
                //    result.Accepted = true;
                //    result.ResultMessage = "OTP is validated and accepted";
                //    break;
                case "REJECT":
                    throw new UserFriendlyException("OTP value is rejected");
                case "01011":
                    throw new UserFriendlyException("Invalid request format");
                case "01012":
                    throw new UserFriendlyException("Unauthorized access");
                case "01013":
                    throw new UserFriendlyException("System error");
                default:
                    throw new UserFriendlyException("Request Failed");
            }
        }

        private void LogApiInput(string methodName, object input)
        {
            _logger.LogInformation($"Method: {nameof(GetCustomerAsync)} Input: {JsonConvert.SerializeObject(input)}");
        }
    }
}
