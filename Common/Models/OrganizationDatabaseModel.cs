using Newtonsoft.Json;
using Prinubes.Common.Kafka;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "OrganizationKafkaMessage")]
    [MessageTopic("identity-organization-events", 0)]
    public class OrganizationKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("organization_data_model")]
        public OrganizationDatabaseModel Organization { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganizationDatabaseModel : FoundationDatabaseModel
    {
        public OrganizationDatabaseModel()
        {
            Groups = new List<GroupDatabaseModel>();
            ComputePlatforms = new List<ComputePlatformDatabaseModel>();
            NetworkPlatforms = new List<NetworkPlatformDatabaseModel>();
            LoadBalancerPlatforms = new List<LoadBalancerPlatformDatabaseModel>();
            Credentials = new List<CredentialDatabaseModel>();
        }
        [JsonProperty("organization")]
        public string Organization { get; set; }
        [JsonProperty("groups")]
        public virtual List<GroupDatabaseModel> Groups { get; set; }
        public virtual List<ComputePlatformDatabaseModel> ComputePlatforms { get; set; }
        public virtual List<NetworkPlatformDatabaseModel> NetworkPlatforms { get; set; }
        public virtual List<LoadBalancerPlatformDatabaseModel> LoadBalancerPlatforms { get; set; }
        public virtual List<CredentialDatabaseModel> Credentials { get; set; }
        //public virtual List<TaggingCRUDDataModel> Tags { get; set; }
    }

    public class OrganizationCRUDDataModel : RowVersionModel
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }
    }
    public class OrganizationDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }
        [JsonProperty("groups")]
        public virtual ICollection<GroupDatabaseModel> Groups { get; set; }
    }
    public class OrganizationSimpleDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }
    }

}
