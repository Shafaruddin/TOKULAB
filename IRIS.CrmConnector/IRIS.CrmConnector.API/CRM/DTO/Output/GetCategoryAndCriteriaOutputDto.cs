namespace IRIS.CrmConnector.API.CRM.DTO.Output
{
    public class AdhocCriterion
    {
        //[JsonProperty("Name")]
        public string Name { get; set; }

        //[JsonProperty("Status")]
        public string Status { get; set; }

        //[JsonProperty("ModifiedDate")]
        public string ModifiedDate { get; set; }

        //[JsonProperty("AdhocCriteriaId")]
        public Guid AdhocCriteriaId { get; set; }
    }

    public class Category
    {
        //[JsonProperty("Name")]
        public string Name { get; set; }

        //[JsonProperty("Type")]
        public string Type { get; set; }

        //[JsonProperty("CategoryId")]
        public Guid CategoryId { get; set; }

        //[JsonProperty("RelatedCategory1ID")]
        public Guid? RelatedCategory1ID { get; set; }

        //[JsonProperty("RelatedCategory2ID")]
        public Guid? RelatedCategory2ID { get; set; }

        //[JsonProperty("Status")]
        public string Status { get; set; }

        //[JsonProperty("ModifiedDate")]
        public string ModifiedDate { get; set; }
    }

    public class GetCategoryAndCriteriaOutputDto : HasReturnStatus
    {
        //[JsonProperty("Categories")]
        public List<Category> Categories { get; set; }

        //[JsonProperty("AdhocCriteria")]
        public List<AdhocCriterion> AdhocCriteria { get; set; }
    }


}
