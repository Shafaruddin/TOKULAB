using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageCodeEnum
    {
        Failed = 1,
        ArgumentNullException = 2,
        FieldLengthError = 3,
        AuthenticationError = 4,
        InvalidUrl = 5,
        Success = 6,
        Created = 7,
        Updated = 8
    }
}
