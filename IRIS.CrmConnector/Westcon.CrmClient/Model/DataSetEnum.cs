using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Westcon.CrmClient.Model
{
    /// <summary>
    /// Defines DataSet
    ///// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataSetEnum
    {
        /// <summary>
        /// Enum Subset for value: Subset
        /// </summary>
        [EnumMember(Value = "Subset")]
        Subset = 1,

        /// <summary>
        /// Enum FullData for value: FullData
        /// </summary>
        [EnumMember(Value = "FullData")]
        FullData = 2,
    }
}
