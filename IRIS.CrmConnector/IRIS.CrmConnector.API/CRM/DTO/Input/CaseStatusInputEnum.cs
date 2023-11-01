using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Westcon.CrmClient.Model
{
    /// <summary>
    /// Defines CaseStatus
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CaseStatusInputEnum
    {
        /// <summary>
        /// Enum InProgress for value: In-Progress
        /// </summary>
        [EnumMember(Value = "InProgress")]
        InProgress = 1,
        /// <summary>
        /// Enum Resolved for value: Resolved
        /// </summary>
        [EnumMember(Value = "Resolved")]
        Resolved = 2
    }
}
