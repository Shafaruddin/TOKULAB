using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.Retail.DTO;

public class RetailContactSearchByRetailInputDto
{
    [Required]
    [RegularExpression(@"^[0-9]{1,6}$")]
    public string Retail { get; init; } = string.Empty;
}
