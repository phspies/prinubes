using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Kafka;
using Prinubes.Common.Structures;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "UserKafkaMessage")]
    [MessageTopic("identity-user-events", 0)]
    public class UserKafkaMessage : IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("id")]
        public Guid UserID{ get; set; }
        [JsonProperty("user_data_model")]
        public UserDatabaseModel User { get; set; }
        [JsonProperty("row_version")]
        public byte[] RowVersion { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserDatabaseModel : UserDisplayDataModel
    {
        [JsonProperty("password_hash")]
        public byte[] PasswordHash { get; set; }
        [JsonProperty("password_salt")]
        public byte[] PasswordSalt { get; set; }
        public List<GroupsToUsers> UsersGroups { get; set; }

    }
    public class UserDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("firstname")]
        public string Firstname { get; set; }
        [JsonProperty("lastname")]
        public string Lastname { get; set; }
        [JsonProperty("groups")]
        public List<GroupDatabaseModel> Groups { get; set; }
        [JsonProperty("role")]
        public UserRole Role { get; set; }
    }
    public class UserCRUDDataModel : RowVersionModel
    {
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("password")]
        public string? Password { get; set; }
        [JsonProperty("firstname")]
        public string? Firstname { get; set; }
        [JsonProperty("lastname")]
        public string? Lastname { get; set; }
        [JsonProperty("role")]
        public UserRole? Role { get; set; }
    }
    public class UserSimpleDataModel : FoundationDatabaseModel
    {
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("firstname")]
        public string Firstname { get; set; }
        [JsonProperty("lastname")]
        public string Lastname { get; set; }
        [JsonProperty("role")]
        public UserRole Role { get; set; }
        [JsonProperty("groups")]
        public List<GroupDisplayDataModel> Groups { get; set; }

    }

}
