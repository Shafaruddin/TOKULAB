/* 
 * SPPL SVCS API 1.0 - Modified by Sache
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 1.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IO.Swagger.CrmClient.Model
{
    /// <summary>
    /// GenerateOTP
    /// </summary>
    [DataContract]
    public partial class GenerateOTP : IEquatable<GenerateOTP>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateOTP" /> class.
        /// </summary>
        /// <param name="mobile">mobile (required).</param>
        /// <param name="triggeredFrom">triggeredFrom (required).</param>
        /// <param name="triggeredBy">triggeredBy (required).</param>
        public GenerateOTP(string mobile = default(string), string triggeredFrom = default(string), string triggeredBy = default(string))
        {
            // to ensure "mobile" is required (not null)
            if (mobile == null)
            {
                throw new InvalidDataException("mobile is a required property for GenerateOTP and cannot be null");
            }
            else
            {
                this.Mobile = mobile;
            }
            // to ensure "triggeredFrom" is required (not null)
            if (triggeredFrom == null)
            {
                throw new InvalidDataException("triggeredFrom is a required property for GenerateOTP and cannot be null");
            }
            else
            {
                this.TriggeredFrom = triggeredFrom;
            }
            // to ensure "triggeredBy" is required (not null)
            if (triggeredBy == null)
            {
                throw new InvalidDataException("triggeredBy is a required property for GenerateOTP and cannot be null");
            }
            else
            {
                this.TriggeredBy = triggeredBy;
            }
        }

        /// <summary>
        /// Gets or Sets Mobile
        /// </summary>
        [DataMember(Name = "Mobile", EmitDefaultValue = false)]
        public string Mobile { get; set; }

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

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class GenerateOTP {\n");
            sb.Append("  Mobile: ").Append(Mobile).Append("\n");
            sb.Append("  TriggeredFrom: ").Append(TriggeredFrom).Append("\n");
            sb.Append("  TriggeredBy: ").Append(TriggeredBy).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as GenerateOTP);
        }

        /// <summary>
        /// Returns true if GenerateOTP instances are equal
        /// </summary>
        /// <param name="input">Instance of GenerateOTP to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(GenerateOTP input)
        {
            if (input == null)
                return false;

            return
                (
                    this.Mobile == input.Mobile ||
                    (this.Mobile != null &&
                    this.Mobile.Equals(input.Mobile))
                ) &&
                (
                    this.TriggeredFrom == input.TriggeredFrom ||
                    (this.TriggeredFrom != null &&
                    this.TriggeredFrom.Equals(input.TriggeredFrom))
                ) &&
                (
                    this.TriggeredBy == input.TriggeredBy ||
                    (this.TriggeredBy != null &&
                    this.TriggeredBy.Equals(input.TriggeredBy))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Mobile != null)
                    hashCode = hashCode * 59 + this.Mobile.GetHashCode();
                if (this.TriggeredFrom != null)
                    hashCode = hashCode * 59 + this.TriggeredFrom.GetHashCode();
                if (this.TriggeredBy != null)
                    hashCode = hashCode * 59 + this.TriggeredBy.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
