using Abp.UI;
using ClosedXML.Excel;
using IRIS.CrmConnector.API.Retail.DTO;
using IRIS.CrmConnector.API.Storage.DTOs;
using IRIS.CrmConnector.API.Storage.Interfaces;
using IRIS.CrmConnector.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static IRIS.CrmConnector.Shared.Constants;

namespace IRIS.CrmConnector.Web.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AUTHORIZATION_SCHEME.ADMIN_TOKEN_AUTHORIZATION)]
    [Route("api/[controller]/[action]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IStorageService _storageService;
        private readonly Dictionary<string, List<byte[]>> _fileSignaturesMap =
            new Dictionary<string, List<byte[]>>
            {

                { ".xls", new List<byte[]>
                    {
                        // Compound Binary File format by Microsoft / OLE CF
                        // https://www.garykessler.net/library/file_sigs.html#:~:text=D0%20CF%2011%20E0%20A1%20B1%201A%20E1
                        new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
                    }
                },
                { ".xlsx", new List<byte[]>
                    {
                        // XLSX
                        // https://www.garykessler.net/library/file_sigs.html#:~:text=50%204B%2003%2004%2014%2000%2006%2000
                        new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 },
                        new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x08 },
                    }
                },
            };

        public AdminController(
            IMemoryCache memoryCache,
            ILogger<AdminController> logger,
            IStorageService storageService
            )
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _storageService = storageService;
        }

        #region StatusFlags

        [HttpGet]
        public async Task<List<StatusFlagDto>> GetStatusFlagsAsync()
        {
            return await _storageService.GetStatusFlagsAsync();
        }

        [HttpPost]
        public async Task UpsertStatusFlagsAsync(StatusFlagDto accountStatusDto)
        {
            await _storageService.UpsertStatusFlagsAsync(accountStatusDto);
        }

        [HttpDelete]
        public async Task DeleteStatusFlagsAsync(string accountStatusCode)
        {
            await _storageService.DeleteStatusFlagsAsync(accountStatusCode);
        }

        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5Mb
        public async Task UploadRetailersExcelFile(IFormFile file)
        {
            var supportedTypes = new[] { ".xls", ".xlsx" };
            var supportedMimeTypes = new[] { 
                "application/vnd.ms-excel", // Microsoft Excel
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // Microsoft Excel (OpenXML) 
            };

            var fileExtension = Path.GetExtension(file.FileName);

            if (!supportedTypes.Contains(fileExtension))
            {
                throw new UserFriendlyException("Unsupported file. Only upload excel files");
            }

            if (!supportedMimeTypes.Contains(file.ContentType))
            {
                throw new UserFriendlyException("Unsupported file. Only upload Excel files. (no templates/no macro enabled documents)");
            }

            try
            {
                await using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var signatures = _fileSignaturesMap[fileExtension];
                var headerBytes = memoryStream.ToArray().Take(signatures.Max(m => m.Length)).ToList();

                if (!signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature)))
                {
                    _logger.LogCritical("bad file input");
                    throw new UserFriendlyException("Invalid File");
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception)
            {
                _logger.LogError("file header validation failed");
                throw new UserFriendlyException("Unable to validate file");
            }

            List<RetailOutputDto> excelRetailers = new();

            try
            {
                var fileName = $"{DateTime.Now.ToString("yyyyMMddTHHmmss")}-{Guid.NewGuid()}{fileExtension}";
                var folderLocation = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
                if (!Directory.Exists(folderLocation))
                {
                    Directory.CreateDirectory(folderLocation);
                }
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
                using (FileStream fs = System.IO.File.Create(filepath))
                {
                    file.CopyTo(fs);
                }
                int rowno = 1;
                XLWorkbook workbook = XLWorkbook.OpenFromTemplate(filepath);
                var sheets = workbook.Worksheets.First();
                var rows = sheets.Rows().ToList();
                foreach (var row in rows)
                {
                    if (rowno != 1)
                    {
                        var test = row.Cell(1).Value.ToString();
                        if (string.IsNullOrWhiteSpace(test))
                        {
                            break;
                        }
                        RetailOutputDto temp = new()
                        {
                            RetailerNo = row.Cell(2).GetString(),
                            ContactNos = row.Cell(3).GetString(),
                            OutletName = row.Cell(4).GetString(),
                            AddressLine1 = row.Cell(5).GetString(),
                            AddressLine2 = row.Cell(6).GetString(),
                            PostalCode = row.Cell(7).GetString(),
                            AreaManager = row.Cell(8).GetString(),
                            InCharge = row.Cell(9).GetString(),
                            OutletStaff = row.Cell(10).GetString(),
                            EmailAddress = row.Cell(11).GetString(),
                            FEInChargeByArea = row.Cell(12).GetString()
                        };
                        if (string.IsNullOrWhiteSpace(temp.RetailerNo))
                            throw new UserFriendlyException($"RetailerNo cannot be empty (ID: {row.Cell(1).GetString()})");

                        excelRetailers.Add(temp);
                    }
                    else
                    {
                        rowno = 2;
                    }
                    if (row.IsEmpty())
                    {
                        break;
                    }
                }

                if (!excelRetailers.Any())
                {
                    throw new UserFriendlyException("No valid records. Please check the excel file");
                }
                await _storageService.UpsertRetailersAsync(excelRetailers);
            }
            catch (Exception e)
            {
                _logger.LogTrace(e.StackTrace);
                throw new UserFriendlyException("An error occured");
            }
        }

        #endregion

        [HttpGet]
        //[AllowAnonymous]
        public async Task<bool> ClearCategoryAndAdhocCriteriaCache()
        {
            try
            {
                _memoryCache.Remove(CACHE.CATEGORY_AND_ADHOC_CRITERIA);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Failed to clear cache", ex);
            }
            await Task.CompletedTask;
            return true;
        }

        [HttpGet]
        //[AllowAnonymous]
        public async Task<bool> ClearStatusFlagsCache()
        {
            try
            {
                _memoryCache.Remove(CACHE.STATUS_FLAGS);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Failed to clear cache", ex);
            }
            await Task.CompletedTask;
            return true;
        }

        [HttpGet]
        //[AllowAnonymous]
        public async Task<bool> ClearRetailersCache()
        {
            try
            {
                _memoryCache.Remove(CACHE.RETAILERS);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Failed to clear cache", ex);
            }
            await Task.CompletedTask;
            return true;
        }


        [HttpGet]
        private async Task<object> Test()
        {
            var client = new RestClient("https://iriscrmapi.devsingaporepools.com/svcsirissit/GetCategoryAndCriteria");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("X-Source", "WEBEX");
            request.AddHeader("Authorization", "Basic V2ViZXhTVkNTSXJpc1VBVDpXZWJleElyaXNVQVRQYXNzd29yZA==");

            try
            {
                var response = await client.ExecuteAsync(request);
                _logger.LogInformation(response.Content);
                _logger.LogInformation(response.StatusDescription);
                return response?.Content;
            }
            catch (Exception ex)
            {
                //_logger.LogInformation("ex",ex);
                //_logger.LogInformation("ex-message", ex.Message);
                //_logger.LogInformation("ie",ex.InnerException);
                throw ex;
            }

        }

        private void SetServerValidationRules()
        {
            //bypass ssl validation check globally for whole application.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };
        }

        [HttpGet]
        private async Task<object> TestWithCert()
        {
            var options = new RestClientOptions("https://iriscrmapi.devsingaporepools.com/");
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;



            string certificateFilePath = "/var/ssl/private/4775D25AAEC9A72587CE1F7ABE21CDE9B72DAFAF.p12";
            if (System.IO.File.Exists(certificateFilePath))
            {
                var certificate = new X509Certificate2(certificateFilePath);
                options.ClientCertificates = new X509CertificateCollection();
                options.ClientCertificates.Add(certificate);

                _logger.LogInformation($"Certificate: {certificate.Thumbprint}");
            }

            var client = new RestClient(options);
            client.Authenticator = new HttpBasicAuthenticator("WebexSVCSIrisUAT", "WebexIrisUATPassword");
            var request = new RestRequest("svcsirissit/GetCategoryAndCriteria", Method.Get);
            request.AddHeader("X-Source", "WEBEX");

            try
            {
                SetServerValidationRules();
                var response = await client.GetAsync(request);
                //_logger.LogInformation(response.Content);
                //_logger.LogInformation(response.StatusDescription);

                var output = JsonConvert.DeserializeObject(response?.Content);
                return output;
            }
            catch (Exception ex)
            {
                //_logger.LogInformation("ex", ex);
                //_logger.LogInformation("ex-message", ex.Message);
                //_logger.LogInformation("ie", ex.InnerException);
                throw ex;
            }

        }

        [HttpPost]
        public async Task UpsertUser([Required] string username, [Required] string password)
        {
            await _storageService.UpsertUser(username, password);
        }
    }
}