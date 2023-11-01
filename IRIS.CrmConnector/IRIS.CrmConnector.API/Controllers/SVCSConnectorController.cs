using Abp.UI;
using Hangfire;
using IRIS.CrmConnector.API.CRM;
using IRIS.CrmConnector.API.CRM.DTO.Input;
using IRIS.CrmConnector.API.CRM.DTO.Output;
using IRIS.CrmConnector.API.Mappers;
using IRIS.CrmConnector.API.Storage.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nito.AsyncEx;
using static IRIS.CrmConnector.Shared.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IRIS.CrmConnector.API.Controllers
{
    //[AllowAnonymous]
    [Authorize(AuthenticationSchemes = AUTHORIZATION_SCHEME.BASIC_AUTHENTICATION)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SVCSConnectorController : ControllerBase
    {
        private readonly ICrmManager _crmManager;
        private ILogger<SVCSConnectorController> _logger;
        private readonly IStorageService _storageService;

        public SVCSConnectorController(
            ICrmManager crmManager,
            ILogger<SVCSConnectorController> logger,
            IStorageService storageService)
        {
            _crmManager = crmManager;
            _logger = logger;
            _storageService = storageService;
        }

        #region SVCS Proxy

        [HttpPost]
        public async Task<GetCustomerOutputDto> GetCustomerAsync(GetCustomerInputDto input)
        {
            var result = await _crmManager.GetCustomerAsync(input);
            if (result == null)
                throw new UserFriendlyException("Not Found");
            return result;
        }

        [HttpPost]
        public async Task<FindCustomerOutputDto> FindCustomerAsync(FindCustomerInputDto input)
        {
            var result = await _crmManager.FindCustomerAsync(input);
            return result;
        }

        [HttpPost]
        public async Task<CreateUpdateCaseOutputDto> CreateCaseAsync(CreateCaseInputDto input)
        {
            
            _logger.LogInformation($"CreateCaseAsync case started for IPCCCallExtensionID:{input.IPCCCallExtensionID}");
            var result = await _crmManager.CreateCaseAsync(input);
            _logger.LogInformation($"CreateCaseAsync case created {result.CaseID}");
            return result;
        }

        [HttpPost]
        public async Task<CreateUpdateCaseOutputDto> UpdateCaseAsync(UpdateCaseInputDto input)
        {
            var result = await _crmManager.UpdateCaseAsync(input);
            _logger.LogInformation($"Deleting autosave record for case {input.CaseID}");
            await _storageService.DeleteAutoSaveRecordAsync(input.CaseID);
            _logger.LogInformation($"UpdateCaseAsync case updated {input.CaseID}");
            return result;
        }

        [HttpPost]
        public async Task<GetCaseAndActivitiesHistoryOutputDto> GetCaseAndActivitiesHistoryAsync(GetCaseAndActivitiesHistoryInputDto input)
        {
            var result = await _crmManager.GetCaseAndActivitiesHistoryAsync(input);
            return result;
        }

        [HttpPost]
        public async Task<GetCategoryAndCriteriaOutputDto> GetCategoryAndCriteriaAsync()
        {
            var result = await _crmManager.GetCategoryAndCriteriaAsync();
            return result;
        }

        [HttpPost]
        public async Task<GenerateOTPOutputDto> GenerateOTPAsync(GenerateOTPInputDto input)
        {
            var result = await _crmManager.GenerateOTPAsync(input);
            return result;
        }

        [HttpPost]
        public async Task<bool> ChallengeOTPAsync(ChallengeOTPInputDto input)
        {
            var result = await _crmManager.ChallengeOTPAsync(input);
            return result.ValidationStatus == "ACCEPT";
        }

        #endregion

        [HttpPut]
        [Route("AutoSaveCaseAsync/{caseId}")]
        public async Task AutoSaveCaseAsync(string caseId, UpdateCaseAutoSaveInputDto input)
        {
            var apiInput = input.FromUpdateCaseAutoSaveInputDto(caseId);
            await _storageService.UpsertCaseAutoSaveRecordAsync(apiInput);
        }

        [HttpPost]
        public async Task<string> ShortenUpdateCaseUrl(ShortenUpdateCaseUrlDto input)
        {
            var apiInput = input.FromShortenUpdateCaseUrlDto();
            string url = await _storageService.UpsertShortenUrl(apiInput);
            _logger.LogInformation($"ShortenUpdateCaseUrl for case {input.CaseId}");
            return url;
        }

        [HttpPost]
        public async Task<ShortenUpdateCaseUrlDto> GetShortenUrl(GetShortenedUrlInputDto getShortenedUrlInputDto)
        {
            var dao = await _storageService.GetShortenedUrl(getShortenedUrlInputDto.ShortenedUrl);
            var dto = dao.ToShortenUpdateCaseUrlDto();
            return dto;
        }
    }
}
