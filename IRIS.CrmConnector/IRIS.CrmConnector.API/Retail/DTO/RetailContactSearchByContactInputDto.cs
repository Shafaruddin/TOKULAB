using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.Retail.DTO;

public class RetailContactSearchByContactInputDto
{
    [Required]
    [RegularExpression(@"^[0-9]{8}$")]
    public string Contact { get; init; } = string.Empty;
}
