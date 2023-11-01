using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.Storage.DTOs;

public class StatusFlagDto
{
    [Required]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;
}
