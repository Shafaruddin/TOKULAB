using System.Runtime.Serialization;

namespace Westcon.CrmClient.Model
{
    [DataContract]
    public class HasTriggerInformation
    {

        /// <summary>
        /// Gets or Sets TriggeredFrom
        /// </summary>
        [DataMember(Name = "TriggeredFrom", EmitDefaultValue = false)]
        public string TriggeredFrom { get; set; }

        /// <summary>
        /// Gets or Sets TriggeredBy
        /// </summary>
        [DataMember(Name = "TriggeredBy", EmitDefaultValue = false)]
        public string TriggeredBy { get; set; }

        public string GetTriggerInformation()
        {
            return $"Triggered from \"{TriggeredFrom}\" by \"{TriggeredBy}\"";
        }
    }
}
