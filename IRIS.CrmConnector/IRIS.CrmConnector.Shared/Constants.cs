namespace IRIS.CrmConnector.Shared
{
    public static class Constants
    {
        public const string CORS_POLICY = "defaultpolicy";
        public const string CORS_ORIGINS = "App:CorsOrigins";
        public const string APPLICATIONINSIGHTS_CONNECTIONSTRING = "ApplicationInsights:ConnectionString";

        public static class CRM
        {
            public const string BASEURL = "CRM:BaseUrl";
            public const string USERNAME = "CRM:Username";
            public const string PASSWORD = "CRM:Password";
            public const string X_SOURCE = "CRM:X-Source";
            public const string CASE_AUTO_UPDATE_IN_MINUTES = "CRM:CaseAutoUpdateInMinutes";
        }

        public static class AZURE
        {
            public const string CERTIFICATE_THUMBPRINT = "Azure:CertificateThumbprint";
            public const string CERTIFICATE_FILE_PATH = "Azure:CertificateFilePath";
            public const string UPDATE_DNS = "Azure:UpdateDNS";
            public const string TABLE_STORAGE_CONNECTION_STRING = "Azure:TableStorageConnectionString";
        }

        public const string SHARED_ADMIN_PASSWORD = "AdminPassword";

        public const string BUILD_NUMBER = "V1";

        public const string SVCS_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public static class CACHE
        {
            public const string CATEGORY_AND_ADHOC_CRITERIA = "CATEGORY_AND_ADHOC_CRITERIA_CACHE";
            public const string STATUS_FLAGS = "STATUS_FLAGS_CACHE";
            public const string RETAILERS = "RETAILERS";
            public const string USER_PASSWORD_HASH = "USER_PASSWORD_HASH_{0}";
        }

        public static class AUTHORIZATION_SCHEME
        {
            public const string ADMIN_TOKEN_AUTHORIZATION = "AdminTokenAuthentication";
            public const string BASIC_AUTHENTICATION = "BasicAuthentication";
        }

        public const string ADMIN_TOKEN = "AdminToken";
        public const string BASIC_TOKEN = "BasicToken";

        public const int OTP_RETRY_SENDING_TIME_SECONDS = 60 * 1;
        public const int OTP_VERIFICATION_WINDOW_SECONDS = 60 * 3;
    }
}
