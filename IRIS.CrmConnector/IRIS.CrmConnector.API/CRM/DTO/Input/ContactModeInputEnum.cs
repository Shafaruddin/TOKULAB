using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace IRIS.CrmConnector.API.CRM.DTO.Input
{
    /// <summary>
    /// Defines ContactMode
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactModeInputEnum
    {
        /// <summary>
        /// Enum Inbound for value: Inbound
        /// </summary>
        [EnumMember(Value = "Inbound")]
        Inbound = 1,
        /// <summary>
        /// Enum Outbound for value: Outbound
        /// </summary>
        [EnumMember(Value = "Outbound")]
        Outbound = 2
    }
}
