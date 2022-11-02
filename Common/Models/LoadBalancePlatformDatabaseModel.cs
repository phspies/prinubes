using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using Prinubes.Common.Models.Enums;
using shortid;
using shortid.Configuration;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "LoadBalancerPlatformKafkaMessage")]
    [MessageTopic("platform-loadbalancer-events", 0)]
    public class LoadBalancerPlatformKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organizationId")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("loadbalanceplatform_id")]
        public Guid LoadBalancerPlatformID { get; set; }
        [JsonProperty("loadbalance_platform_model")]
        public LoadBalancerPlatformDatabaseModel LoadBalancerPlatform { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn, Title = "LoadBalancerPlatformTestingRequestKafkaMessage")]
    [MessageTopic("platform-loadbalancer-testing-request-events", 0)]
    public class LoadBalancerPlatformTestingRequestKafkaMessage : IMessage
    {
        public LoadBalancerPlatformTestingRequestKafkaMessage()
        {
            RequestID = ShortId.Generate(new GenerationOptions(true, true, 15));
            Action = ActionEnum.test;
        }
        [JsonProperty("request_id")]
        public string RequestID { get; set; }
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("loadbalancer_platform_model")]
        public LoadBalancerPlatformDatabaseModel LoadBalancerPlatform { get; set; }
        public string ReturnTopic => $"{typeof(LoadBalancerPlatformTestingResponseModel).GetCustomAttribute<MessageTopicAttribute>().Topic}-{RequestID}";


    }
    [JsonObject(MemberSerialization.OptIn, Title = "LoadBalancerPlatformTestingResponseKafkaMessage")]
    [MessageTopic("platform-loadbalancer-testing-respond-events", 0)]
    public class LoadBalancerPlatformTestingResponseModel : IMessage
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

    }
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerPlatformDatabaseModel : LoadBalancerPlatformDisplayDataModel
    {
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("credentials")]
        public CredentialDatabaseModel Credential { get; set; }
        //public List<LoadBalancerPlatformsToTags> LoadBalancerPlatformTags { get; set; }

        public LoadBalancerPlatformDatabaseModel()
        {
            Tags = new List<TaggingModel>();
        }
    }
    public class LoadBalancerPlatformCRUDDataModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("platform_type")]
        public LoadBalancerPlatformType PlatformType { get; set; }
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
        [JsonProperty("cloudplatform_moid")]
        public string? CloudPlatformMoid { get; set; }
    }
    public class LoadBalancerPlatformDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("organization_id")]
        public virtual Guid OrganizationID { get; set; }
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("platform_type")]
        public LoadBalancerPlatformType PlatformType { get; set; }
        [JsonProperty("credential_id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("availability_zone")]
        public AvailabilityZoneType AvailabilityZone { get; set; }
        [JsonProperty("url_endpoint")]
        public string UrlEndpoint { get; set; }
        [JsonProperty("verify_ssl_cert")]
        public bool VertifySSLCert { get; set; }
        [JsonProperty("credentials")]
        public CredentialDatabaseModel Credential { get; set; }
        [JsonProperty("tags")]


        public List<TaggingModel> Tags { get; set; }
        [JsonProperty("cloudplatform_moid")]
        public string? CloudPlatformMoid { get; set; }

        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
        [JsonProperty("state")]
        public PlatformStateEnum? state { get; set; }
        [JsonProperty("message")]
        public string? message { get; set; }
    }
}
