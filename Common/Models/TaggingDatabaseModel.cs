using Newtonsoft.Json;
using Prinubes.Common.Kafka;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "TaggingKafkaMessage")]
    [MessageTopic("tagging-events", 0)]
    public class TaggingKafkaMessage : IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organizationId")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("id")]
        public Guid TagID { get; set; }
        [JsonProperty("tagging_model")]
        public object Tag { get; set; }

    }
    [JsonObject(MemberSerialization.OptIn)]
    //public class TaggingDatabaseModel
    //{
    //    [JsonProperty("key")]
    //    public string Key { get; set; }
    //    [JsonProperty("value")]
    //    public string Value { get; set; }


    //    //public virtual List<NetworkPlatformDatabaseModel>? NetworkPlatforms { get; set; }
    //    //public virtual List<ComputePlatformDatabaseModel>? ComputePlatforms { get; set; }
    //    //public virtual List<LoadBalancerPlatformDatabaseModel>? LoadBalancerPlatforms { get; set; }

    //    //public virtual List<NetworkPlatformsToTags>? NetworkPlatformTags { get; set; }
    //    //public virtual List<ComputePlatformsToTags>? ComputePlatformTags { get; set; }
    //    //public virtual List<LoadBalancerPlatformsToTags>? LoadBalancerPlatformTags { get; set; }


    //}
    //public class TaggingCRUDDataModel
    //{
    //    [JsonProperty("key")]
    //    public string Key { get; set; }
    //    [JsonProperty("value")]
    //    public string Value { get; set; }
    //}
    public class TaggingModel
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class TaggingDisplayDataModel
    {
        [JsonProperty("id")]
        public Guid? Id { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
