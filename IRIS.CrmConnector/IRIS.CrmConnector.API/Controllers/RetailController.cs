using Abp.UI;
using IRIS.CrmConnector.API.Retail.DTO;
using IRIS.CrmConnector.API.Storage.Interfaces;
using IRIS.CrmConnector.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RetailController : ControllerBase
    {
        private readonly ILogger<RetailController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IStorageService _storageService;

        public RetailController(
            ILogger<RetailController> logger,
            IMemoryCache memoryCache,
            IStorageService storageService
            )
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _storageService = storageService;
        }

        private async Task<List<RetailOutputDto>> GetRetailersAsync()
        {
            return await _memoryCache.GetOrCreateAsync(Constants.CACHE.RETAILERS, async context =>
            {
                context.SetPriority(CacheItemPriority.NeverRemove);
                return await _storageService.GetRetailersAsync();
            });
        }

        [HttpPost]
        public async Task<RetailOutputDto> GetRetailByContact(RetailContactSearchByContactInputDto inputDto)
        {
            var result = (await GetRetailersAsync()).Where(x => !string.IsNullOrEmpty(x.ContactNos) && x.ContactNos.Contains(inputDto.Contact)).FirstOrDefault();

            if (result == null)
            {
                _logger.LogInformation($"GetRetailByContact not found for '{inputDto.Contact}'");
                throw new UserFriendlyException("Not Found");
            }

            return result;
        }

        [HttpPost]
        public async Task<RetailOutputDto> GetRetailByRetail(RetailContactSearchByRetailInputDto inputDto)
        {
            var result = (await GetRetailersAsync()).Where(x => x.RetailerNo == inputDto.Retail).FirstOrDefault();

            if (result == null)
            {
                _logger.LogInformation($"GetRetailByRetail not found for '{inputDto.Retail}'");
                throw new UserFriendlyException("Not Found");
            }

            return result;
        }

        // todo - remove when UAT is updated
        [HttpGet]
        public async Task<RetailOutputDto> GetRetailByContact([Required][RegularExpression(@"^[0-9]{8}$")] string contact)
        {
            var result = (await GetRetailersAsync()).Where(x => !string.IsNullOrEmpty(x.ContactNos) && x.ContactNos.Contains(contact)).FirstOrDefault();

            if (result == null)
            {
                _logger.LogInformation($"GetRetailByContact not found for '{contact}'");
                throw new UserFriendlyException("Not Found");
            }

            return result;
        }

        // todo - remove when UAT is updated
        [HttpGet]
        public async Task<RetailOutputDto> GetRetailByRetail([Required][RegularExpression(@"^[0-9]{1,6}$")] string retail)
        {
            var result = (await GetRetailersAsync()).Where(x => x.RetailerNo == retail).FirstOrDefault();

            if (result == null)
            {
                _logger.LogInformation($"GetRetailByRetail not found for '{retail}'");
                throw new UserFriendlyException("Not Found");
            }

            return result;
        }
    }
}