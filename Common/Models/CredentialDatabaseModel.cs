using Newtonsoft.Json;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "CredentialKafkaMessage")]
    [MessageTopic("identity-credential-events", 0)]
    public class CredentialKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("id")]
        public Guid CredentialID { get; set; }
        [JsonProperty("compute_platform_model")]
        public CredentialKafkaDataModel Credential { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class CredentialDatabaseModel : FoundationDatabaseModel
    {
        [JsonProperty("credential")]
        public string Credential { get; set; }
        [JsonProperty("computeplatform")]
        public List<ComputePlatformDatabaseModel> ComputePlatforms { get; set; }
        [JsonProperty("networkplatform")]
        public List<NetworkPlatformDatabaseModel> NetworkPlatforms { get; set; }
        [JsonProperty("loadbalanceplatform")]
        public List<LoadBalancerPlatformDatabaseModel> LoadBalancerPlatforms { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("encrypted_password")]
        public string EncryptedPassword { get; set; }
        [JsonProperty("encrypted_key")]
        public string EncryptedKey { get; set; }

        public virtual string DecryptedPassword => CipherService.DecryptString(EncryptedPassword, Convert.FromBase64String(EncryptedKey));

    }
    public class CredentialKafkaDataModel : FoundationDatabaseModel
    {
        [JsonProperty("credential")]
        public string Credential { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("encrypted_password")]
        public string EncryptedPassword { get; set; }
        [JsonProperty("encrypted_key")]
        public string EncryptedKey { get; set; }
    }
    public class CredentialCRUDDataModel : RowVersionModel
    {
        [JsonProperty("credential")]
        public string Credential { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string? Password { get; set; }
    }
    public class CredentialDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("credential")]
        public string Credential { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
