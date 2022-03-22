using Prinubes.Common.Kafka;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.ManyToMany;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "GroupKafkaMessage")]
    [MessageTopic("identity-group-events", 0)]
    public class GroupKafkaMessage : RowVersionModel, IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("id")]
        public Guid GroupID { get; set; }
        [JsonProperty("organizationId")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("group_data_model")]
        public GroupKafkaDataModel Group { get; set; }
        [JsonProperty("user_id")]
        public Guid UserID{ get; set; }

    }
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupDatabaseModel : FoundationDatabaseModel
    {
        public GroupDatabaseModel()
        {
            Users = new List<UserDatabaseModel>();
            GroupsUsers = new List<GroupsToUsers>();
        }
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("organization")]
        public OrganizationDatabaseModel Organization { get; set; }
        [JsonProperty("users")]
        public List<UserDatabaseModel> Users { get; set; }
        public List<GroupsToUsers> GroupsUsers { get; set; }
    }
    public class GroupKafkaDataModel : FoundationDatabaseModel
    {
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }

    }
    public class GroupCRUDDataModel : RowVersionModel
    {
        [JsonProperty("group")]
        public virtual string Group { get; set; }
    }
    public class GroupDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
    }
    public class GroupDisplayUsersDataModel : FoundationDatabaseModel
    {
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("organization_id")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("users")]
        public List<UserSimpleDataModel> Users { get; set; }
    }
}
