using Prinubes.Common.Kafka;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using System.ComponentModel.DataAnnotations;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using shortid;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "ComputePlatformKafkaMessage")]
    [MessageTopic("platform-compute-events", 0)]
    public class ComputePlatformKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("computeplatform_id")]
        public Guid ComputePlatformID { get; set; }
        [JsonProperty("compute_platform_model")]
        public ComputePlatformDatabaseModel ComputePlatform { get; set; }

    }
    [JsonObject(MemberSerialization.OptIn, Title = "ComputePlatformTestingRequestKafkaMessage")]
    [MessageTopic("platform-compute-testing-request-events", 0)]
    public class ComputePlatformTestingRequestKafkaMessage : IMessage
    {
        public ComputePlatformTestingRequestKafkaMessage()
        {
            RequestID = ShortId.Generate(true, true, 15);
            Action = ActionEnum.test;
        }
        [JsonProperty("request_id")]
        public string RequestID { get; set; }
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("compute_platform_model")]
        public ComputePlatformDatabaseModel ComputePlatform { get; set; }
        public string ReturnTopic => $"{typeof(ComputePlatformTestingResponseModel).GetCustomAttribute<MessageTopicAttribute>().Topic}-{RequestID}";
        

    }
    [JsonObject(MemberSerialization.OptIn, Title = "ComputePlatformTestingResponseKafkaMessage")]
    [MessageTopic("platform-compute-testing-respond-events", 0)]
    public class ComputePlatformTestingResponseModel : IMessage
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

    }
    [JsonObject(MemberSerialization.OptIn)]
    public class ComputePlatformDatabaseModel : ComputePlatformDisplayDataModel
    {
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("credential")]
        public CredentialDatabaseModel? Credential { get; set; }
        //public List<ComputePlatformsToTags> ComputePlatformTags { get; set; }
        //public ComputePlatformDatabaseModel()
        //{
        //    Tags = new List<TaggingCRUDDataModel>();
        //}
    }
    public class ComputePlatformCRUDDataModel : RowVersionModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("platform_type")]
        public ComputePlatformType PlatformType { get; set; }
        [JsonProperty("credential_id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("availability_zone")]
        public AvailabilityZoneType AvailabilityZone { get; set; }
        [JsonProperty("url_endpoint")]
        [StringLength(255)]
        public string UrlEndpoint { get; set; }
        [JsonProperty("verify_ssl_cert")]
        public bool VertifySSLCert { get; set; }
        [JsonProperty("tags")]
        public List<TaggingModel>? Tags { get; set; }
        [JsonProperty("datacenter_moid")]
        [StringLength(255)]
        public string? DatacenterMoid { get; set; }
        [JsonProperty("clusters_moid")]
        [StringLength(255)]
        public string? ClustersMoid { get; set; }
        [JsonProperty("folder_moid")]
        public string? FolderMoid { get; set; }
        [JsonProperty("resourcepool_moid")]
        public string? ResourcepoolMoid { get; set; }
    }
    public class ComputePlatformDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("platform_type")]
        public ComputePlatformType? PlatformType { get; set; }
        [JsonProperty("credential_id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("availability_zone")]
        public AvailabilityZoneType AvailabilityZone { get; set; }
        [JsonProperty("url_endpoint")]
        [StringLength(255)]
        public string? UrlEndpoint { get; set; }
        [JsonProperty("verify_ssl_cert")]
        public bool? VertifySSLCert { get; set; }
        [JsonProperty("credentials")]
        public CredentialDatabaseModel? Credential { get; set; }
        [JsonProperty("tags")]
        public List<TaggingModel> Tags { get; set; }
        [JsonProperty("datacenter_moid")]
        [StringLength(255)]
        public string? DatacenterMoid { get; set; }
        [JsonProperty("clusters_moid")]
        [StringLength(255)]
        public string? ClustersMoid { get; set; }
        [JsonProperty("folder_moid")]
        [StringLength(255)]
        public string? FolderMoid { get; set; }
        [JsonProperty("resourcepool_moid")]
        [StringLength(255)]
        public string? ResourcepoolMoid { get; set; }
    }
}
