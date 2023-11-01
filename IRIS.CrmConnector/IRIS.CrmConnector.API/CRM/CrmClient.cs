using Abp.Dependency;
using Abp.Extensions;
using Abp.UI;
using IO.Swagger.CrmClient.Model;
using IRIS.CrmConnector.API.CRM.DTO.Input;
using IRIS.CrmConnector.API.CRM.DTO.Output;
using IRIS.CrmConnector.Shared;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Westcon.CrmClient.Model;

namespace IRIS.CrmConnector.API.CRM
{
    public class CrmClient : ITransientDependency
    {
        readonly RestClient _client;

        readonly ILogger<CrmClient> _logger;
        private string Username { get; set; }
        private string Password { get; set; }
        private string XSource { get; set; }
        private string BaseUrl { get; set; }
        private string CertificateThumbprint { get; set; }
        private string CertificateFilePath { get; set; }

        public CrmClient(
            IConfiguration Configuration,
            ILogger<CrmClient> logger)
        {
            _logger = logger;

            Username = Configuration[Constants.CRM.USERNAME];
            Password = Configuration[Constants.CRM.PASSWORD];
            BaseUrl = Configuration[Constants.CRM.BASEURL].RemovePostFix("/");
            XSource = Configuration[Constants.CRM.X_SOURCE];
            CertificateThumbprint = Configuration[Constants.AZURE.CERTIFICATE_THUMBPRINT];
            CertificateFilePath = Configuration[Constants.AZURE.CERTIFICATE_FILE_PATH];

            var options = new RestClientOptions(BaseUrl);
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            options.ClientCertificates = new X509CertificateCollection();

            if (!CertificateThumbprint.IsNullOrWhiteSpace())
            {
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    certStore.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, CertificateThumbprint, false);

                    var cert = certCollection.OfType<X509Certificate2>().FirstOrDefault();

                    if (cert is null)
                    {
                        _logger.LogError($"Certificate with thumbprint {CertificateThumbprint} was not found");
                    }
                    else
                    {
                        _logger.LogInformation($"Certificate: {cert.Thumbprint}");
                        options.ClientCertificates.Add(cert);
                    }
                }
            }
            if (!CertificateFilePath.IsNullOrWhiteSpace())
            {
                _logger.LogInformation($"Certificate path {CertificateFilePath}");
                if (File.Exists(CertificateFilePath))
                {
                    var bytes = File.ReadAllBytes(CertificateFilePath);

                    var certificate = new X509Certificate2(CertificateFilePath);

                    _logger.LogInformation($"Certificate: {certificate.Thumbprint}");
                    options.ClientCertificates.Add(certificate);
                }
                else
                {
                    _logger.LogError($"Certificate {CertificateFilePath} was not found");
                }
            }

            options.UserAgent = string.Format("IRISRuntime/{0}", Constants.BUILD_NUMBER);

            _client = new RestClient(options);
            _client.Authenticator = new HttpBasicAuthenticator(Username, Password);
            _client.AddDefaultHeader("X-Source", XSource);

            JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
            {
                ContractResolver = null,
                DefaultValueHandling = DefaultValueHandling.Include,
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            _client.UseNewtonsoftJson(DefaultSettings);
        }

        public async Task<GetCustomerOutputDto> GetCustomerAsync(GetCustomerInputDto input)
        {
            var requestInput = new GetCustomer(input.IDNumber, input.CustomerGUID, input.SPAAccountNumber, input.Anonymous, input.DataSet, input.TriggeredFrom,input.TriggeredBy);

            return await CommonApiCaller<GetCustomerOutputDto>("GetCustomer", Method.Post, requestInput);
        }

        public async Task<FindCustomerOutputDto> FindCustomerAsync(FindCustomerInputDto input)
        {
            var requestInput = new FindCustomer(input.Name, input.IDNumber, input.AccountNumber, input.MobileNo, input.EmailAddress, input.TriggeredFrom, input.TriggeredBy);

            if (input.Name.IsNullOrEmpty() && input.IDNumber.IsNullOrEmpty() && input.AccountNumber.IsNullOrEmpty() && input.MobileNo.IsNullOrEmpty() && input.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Atleast one field should not be empty");
            }

            return await CommonApiCaller<FindCustomerOutputDto>("FindCustomer", Method.Post, requestInput);
        }

        public async Task<CreateUpdateCaseOutputDto> CreateCaseAsync(CreateCaseInputDto input)
        {
            if (input?.CreatedOn == DateTime.MinValue)
            {
                _logger.LogWarning($"IVR - CreateCaseInputDto CreatedOn  had bad datetime ipcc:{input.IPCCCallExtensionID}");
                input.CreatedOn = DateTime.UtcNow.AddHours(8);
            }
            if (input?.DateTimeReceived == DateTime.MinValue)
            {
                _logger.LogWarning($"IVR - CreateCaseInputDto DateTimeReceived had bad datetime ipcc:{input.IPCCCallExtensionID}");
                input.DateTimeReceived = DateTime.UtcNow.AddHours(8);
            }

            var requestInput = new CreateCase(
                input.CustomerGUID,
                input.CaseTitle,
                input.CaseDetails,
                input.SystemAuthenticated,
                input.DateTimeReceived.ToString(Constants.SVCS_DATE_FORMAT),
                input.IPCCCallExtensionID,
                input.PrimaryCaseOfficer,
                input.Owner,
                input.CaseCategory1,
                input.CaseCategory2,
                input.CaseCategory3,
                (ContactModeEnum)(int)input.ContactMode,
                input.CreatedBy,
                input.CreatedOn.ToString(Constants.SVCS_DATE_FORMAT),
                input.ModifiedBy,
                input.CallBack,
                input.TriggeredFrom,
                input.TriggeredBy
            );

            //#region simulate api calls
            //switch (input.TriggeredBy)
            //{
            //    case "DONNYTEST":
            //        return new CreateUpdateCaseOutputDto()
            //        {
            //            CaseID = "C-2022-07-12-0002",
            //            ReturnStatus = new DTO.ReturnStatusDto()
            //            {
            //                Message = "Created - Dummy",
            //                MessageCode = MessageCodeEnum.Created
            //            }
            //        };
            //    default:
            //        break;
            //}
            //#endregion


            var request = new RestRequest("CreateCase", Method.Post);

            return await CommonApiCaller<CreateUpdateCaseOutputDto>("CreateCase", Method.Post, requestInput);
        }

        public async Task<CreateUpdateCaseOutputDto> UpdateCaseAsync(UpdateCaseInputDto input)
        {
            var requestInput = new UpdateCase(
                input.CaseID,
                input.CustomerGUID,
                input.CaseTitle,
                input.CaseDetails,
                input.SystemAuthenticated,
                input.ManualVerification,
                input.IPCCCallExtensionID,
                input.Owner,
                input.CaseCategory1,
                input.CaseCategory2,
                input.CaseCategory3,
                input.AdHocCriteria,
                input.FollowupAction,
                (ContactModeEnum)(int)input.ContactMode,
                input.ModifiedBy,
                input.FollowupRequired,
                (CaseStatusEnum)(int)input.CaseStatus,
                input.Resolution,
                input.TriggeredFrom,
                input.TriggeredBy
            );

            //if (requestInput.CaseStatus == CaseStatusEnum.Resolved)
            //{
            //    if (requestInput.Resolution == null)
            //    {
            //        requestInput.Resolution = String.Empty;
            //    }
            //    requestInput.Resolution = String.Format("{0} {1}", requestInput.Resolution, "Resolved on Webex").Trim();
            //    requestInput.Resolution = requestInput.Resolution.Truncate(200);
            //}

            return await CommonApiCaller<CreateUpdateCaseOutputDto>("UpdateCase", Method.Post, requestInput);
        }

        public async Task<GetCaseAndActivitiesHistoryOutputDto> GetCaseAndActivitiesHistoryAsync(GetCaseAndActivitiesHistoryInputDto input)
        {
            var requestInput = new GetCaseAndActivitiesHistory(input.CustomerGuid, input.TriggeredFrom, input.TriggeredBy);

            return await CommonApiCaller<GetCaseAndActivitiesHistoryOutputDto>("GetCaseAndActivitiesHistory", Method.Post, requestInput);
        }

        public async Task<GetCategoryAndCriteriaOutputDto> GetCategoryAndCriteriaAsync()
        {
            return await CommonApiCaller<GetCategoryAndCriteriaOutputDto>("GetCategoryAndCriteria", Method.Get, new { });
        }

        public async Task<GenerateOTPOutputDto> GenerateOTPAsync(GenerateOTPInputDto input)
        {
            var requestInput = new GenerateOTP(input.Mobile, input.TriggeredFrom, input.TriggeredBy);

            //#region simulate api calls
            //switch (input.Mobile)
            //{
            //    case "10":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01010",
            //            ResultMessage = "sms request sucesfully submitted"
            //        };
            //    case "11":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01011",
            //            ResultMessage = "should change mobile number!!!"
            //        };
            //    case "12":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01012",
            //            ResultMessage = "can retry after x seconds (unauthorized access)"
            //        };
            //    case "13":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01013",
            //            ResultMessage = "can retry after x seconds"
            //        };
            //    case "14":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01014",
            //            ResultMessage = "Unable to route to mobile operator for the attempted mobile number."
            //        };
            //    case "15":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01015",
            //            ResultMessage = "can retry after x seconds (credit probs)"
            //        };
            //    case "18":
            //        return new GenerateOTPOutputDto()
            //        {
            //            ResultCode = "01018",
            //            ResultMessage = "should change mobile number (blacklisted)"
            //        };
            //    default:
            //        break;
            //}
            //#endregion

            return await CommonApiCaller<GenerateOTPOutputDto>("GenerateOTP", Method.Post, requestInput);
        }

        public async Task<ChallengeOTPOutputDto> ChallengeOTPAsync(ChallengeOTPInputDto input)
        {
            var requestInput = new ValidateOTP(input.Mobile, input.OTPValidate, input.TriggeredFrom, input.TriggeredBy);

            //#region simulate api calls
            //switch (input.OTPValidate)
            //{
            //    case "111111":
            //        return new ChallengeOTPOutputDto()
            //        {
            //            ValidationStatus = "ACCEPT",
            //        };
            //    case "000000":
            //        return new ChallengeOTPOutputDto()
            //        {
            //            ValidationStatus = "REJECT"
            //        };
            //    case "123456":
            //        return new ChallengeOTPOutputDto()
            //        {
            //            ValidationStatus = "01013"
            //        };
            //    default:
            //        break;
            //}
            //#endregion

            return await CommonApiCaller<ChallengeOTPOutputDto>("ChallengeOTP", Method.Post, requestInput);
        }

        private async Task<T> CommonApiCaller<T>(string resource, Method httpMethod, object input)
        {
            var request = new RestRequest(resource, httpMethod);

            try
            {
                if (input is HasTriggerInformation)
                {
                    MethodInfo getTriggerInformationMethod = typeof(HasTriggerInformation).GetMethod(nameof(HasTriggerInformation.GetTriggerInformation));
                    var methodResult = getTriggerInformationMethod.Invoke(input, new object[] { });
                    if (methodResult is string)
                    {
                        _logger.LogInformation($"{methodResult.ToString()}");
                    }
                    else
                    {
                        _logger.LogError($"CAUTION! MethodResult information was not a string");
                    }
                }
                else
                {
                    _logger.LogError($"No trigger information was found");
                }
            }
            catch (Exception) { }

            if (httpMethod == Method.Post)
            {
                request.AddJsonBody(input);
                request.AddOrUpdateHeader("Content-Type", "application/json");
            }

            SetServerValidationRules();
            _logger.LogInformation($"Trying to call endpoint: {resource}");
            var response = await _client.ExecuteAsync(request);
            try
            {
                _client.EnsureResponseWasSuccessful(request, response);
            }
            catch (Exception ex)
            {
                var commonErrorMessage = "We are unable to reach SVCS right now. Please contact System Administrator. You can still proceed with the call but case creation will not happen";
                // HTTP Error 404. The requested resource is not found.  
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    if (response.Content.Contains("HTTP Error 404. The requested resource is not found."))
                    {
                        _logger.LogError($"Endpoint: {resource} 404 Exception", ex);
                        throw new UserFriendlyException(404, commonErrorMessage);
                    }
                }
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError($"Endpoint: {resource} 503 Exception", ex);
                    throw new UserFriendlyException(503, commonErrorMessage);
                    //throw new UserFriendlyException(503, "We are unable to connect to SVCS. Please check with the SVCS administrator.");
                }

                // new error from 2023-07-09 from F5+CRM
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError($"Endpoint: {resource} 401 Exception", ex);
                    //throw new UserFriendlyException(503, commonErrorMessage);
                }
                // support id
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation("F5 Response", response?.Content);
                }
                _logger.LogError($"Endpoint: {resource} Exception", ex);
                _logger.LogInformation($"Endpoint: {resource} Input: {JsonConvert.SerializeObject(input)}");
                throw new UserFriendlyException("Server Error");
            }

            var output = JsonConvert.DeserializeObject<T>(response?.Content);

            return output;
        }

        private void SetServerValidationRules()
        {
            //bypass ssl validation check globally for whole application.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            //ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            //{
            //    return true;
            //};
        }
    }
}
