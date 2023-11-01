using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input;

public class ShortenUpdateCaseUrlDto
{
    public string SystemAuthenticatedNAReason { get; set; }
    public string CaseTitle { get; set; }
    public string AuthToken { get; set; }
    public string IPCCCallExtensionID { get; set; }
    public string ContactMode { get; set; }
    public string CustomerIsAnonymous { get; set; }

    [Required]
    public string CaseId { get; set; }
    public string CustomerId { get; set; }
    public string CustomerAccountNumber { get; set; }
    public string AgentName { get; set; }
    public string AgentUsername { get; set; }
    public string CustomerIsAuthenticated { get; set; }
}
