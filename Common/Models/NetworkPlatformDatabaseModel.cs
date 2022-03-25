using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using shortid;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "NetworkPlatformKafkaMessage")]
    [MessageTopic("platform-network-events", 0)]
    public class NetworkPlatformKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organizationId")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("networkplatform_id")]
        public Guid NetworkPlatformID { get; set; }
        [JsonProperty("network_platform_model")]
        public NetworkPlatformDatabaseModel NetworkPlatform { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn, Title = "NetworkPlatformTestingRequestKafkaMessage")]
    [MessageTopic("platform-network-testing-request-events", 0)]
    public class NetworkPlatformTestingRequestKafkaMessage : IMessage
    {
        public NetworkPlatformTestingRequestKafkaMessage()
        {
            RequestID = ShortId.Generate(true, true, 15);
            Action = ActionEnum.test;
        }
        [JsonProperty("request_id")]
        public string RequestID { get; set; }
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("network_platform_model")]
        public NetworkPlatformDatabaseModel NetworkPlatform { get; set; }
        public string ReturnTopic => $"{typeof(NetworkPlatformTestingResponseModel).GetCustomAttribute<MessageTopicAttribute>().Topic}-{RequestID}";


    }
    [JsonObject(MemberSerialization.OptIn, Title = "NetworkPlatformTestingResponseKafkaMessage")]
    [MessageTopic("platform-network-testing-respond-events", 0)]
    public class NetworkPlatformTestingResponseModel : IMessage
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

    }
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkPlatformDatabaseModel : NetworkPlatformDisplayDataModel
    {
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("credential")]
        public CredentialDatabaseModel Credential { get; set; }
        //public List<NetworkPlatformsToTags> NetworkPlatformTags { get; set; }

        public NetworkPlatformDatabaseModel()
        {
            Tags = new List<TaggingModel>();
        }
    }
    public class NetworkPlatformCRUDDataModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("platform_type")]
        public NetworkPlatformType PlatformType { get; set; }
        [JsonProperty("credential_id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("availability_zone")]
        public AvailabilityZoneType AvailabilityZone { get; set; }
        [JsonProperty("url_endpoint")]
        public string UrlEndpoint { get; set; }
        [JsonProperty("verify_ssl_cert")]
        public bool VertifySSLCert { get; set; }
        [JsonProperty("tags")]
        public List<TaggingModel>? Tags { get; set; }
    }
    public class NetworkPlatformDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("organization_id")]
        public virtual Guid OrganizationID { get; set; }
        [JsonProperty("organization")]
        public OrganizationDisplayDataModel Organization { get; set; }
        [JsonProperty("platform_type")]
        public NetworkPlatformType PlatformType { get; set; }
        [JsonProperty("credential_id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("credential")]
        public CredentialDisplayDataModel Credential { get; set; }
        [JsonProperty("availability_zone")]
        public AvailabilityZoneType AvailabilityZone { get; set; }
        [JsonProperty("url_endpoint")]
        public string UrlEndpoint { get; set; }
        [JsonProperty("verify_ssl_cert")]
        public bool VertifySSLCert { get; set; }

        [JsonProperty("tags")]
        public List<TaggingModel> Tags { get; set; }
    }
}
