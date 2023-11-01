using System.ComponentModel.DataAnnotations;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    public class GetShortenedUrlInputDto
    {
        [Required]
        public string ShortenedUrl { get; set; }
    }
}
