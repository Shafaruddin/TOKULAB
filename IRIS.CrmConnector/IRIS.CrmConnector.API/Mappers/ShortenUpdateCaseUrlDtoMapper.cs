using IRIS.CrmConnector.API.CRM.DAO;
using IRIS.CrmConnector.API.CRM.DTO.Input;

namespace IRIS.CrmConnector.API.Mappers
{
    public static class ShortenUpdateCaseUrlDtoMapper
    {
        public static ShortenUpdateCaseUrlDto ToShortenUpdateCaseUrlDto(this ShortenUpdateCaseUrlDao dao)
        {
            ShortenUpdateCaseUrlDto dto = new();
            dto.SystemAuthenticatedNAReason = dao.SystemAuthenticatedNAReason;
            dto.CaseTitle = dao.CaseTitle;
            dto.AuthToken = dao.AuthToken;
            dto.IPCCCallExtensionID = dao.IPCCCallExtensionID;
            dto.ContactMode = dao.ContactMode;
            dto.CustomerIsAnonymous = dao.CustomerIsAnonymous;
            dto.CaseId = dao.CaseId;
            dto.CustomerId = dao.CustomerId;
            dto.CustomerAccountNumber = dao.CustomerAccountNumber;
            dto.AgentName = dao.AgentName;
            dto.AgentUsername = dao.AgentUsername;
            dto.CustomerIsAuthenticated = dao.CustomerIsAuthenticated;
            return dto;
        }

        public static ShortenUpdateCaseUrlDao FromShortenUpdateCaseUrlDto(this ShortenUpdateCaseUrlDto dto)
        {
            var dao = new ShortenUpdateCaseUrlDao();
            dao.SystemAuthenticatedNAReason = dto.SystemAuthenticatedNAReason;
            dao.CaseTitle = dto.CaseTitle;
            dao.AuthToken = dto.AuthToken;
            dao.IPCCCallExtensionID = dto.IPCCCallExtensionID;
            dao.ContactMode = dto.ContactMode;
            dao.CustomerIsAnonymous = dto.CustomerIsAnonymous;
            dao.CaseId = dto.CaseId;
            dao.CustomerId = dto.CustomerId;
            dao.CustomerAccountNumber = dto.CustomerAccountNumber;
            dao.AgentName = dto.AgentName;
            dao.AgentUsername = dto.AgentUsername;
            dao.CustomerIsAuthenticated = dto.CustomerIsAuthenticated;
            return dao;
        }
    }
}
