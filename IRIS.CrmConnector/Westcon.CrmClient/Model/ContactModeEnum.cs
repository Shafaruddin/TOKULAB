using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Westcon.CrmClient.Model
{
    /// <summary>
    /// Defines ContactMode
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactModeEnum
    {
        /// <summary>
        /// Enum Inbound for value: Phone(Inbound)
        /// </summary>
        [EnumMember(Value = "Phone(Inbound)")]
        Inbound = 1,
        /// <summary>
        /// Enum Outbound for value: Phone(Outbound)
        /// </summary>
        [EnumMember(Value = "Phone(Outbound)")]
        Outbound = 2
    }
}
