using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Kafka;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "RoleKafkaMessage")]
    [MessageTopic("identity-role-events", 0)]
    public class RoleKafkaMessage : IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("organizationId")]
        public Guid OrganizationID { get; set; }
        [JsonProperty("id")]
        public OrganizationDatabaseModel RoleID { get; set; }
        [JsonProperty("role_data_model")]
        public RoleDatabaseModel Role { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RoleDatabaseModel : RoleDisplayDataModel
    {
        public IEnumerable<RolesToRoutePaths> RolesToPaths { get; set; }
        public List<RoutePathDatabaseModel> RoutePaths { get; set; }
    }
    public class RoleDisplayDataModel : FoundationDatabaseModel
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("organization_id")]
        public string OrganizationID { get; set; }
    }
    public class RoleCRUDDataModel
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("organization_id")]
        public string OrganizationID { get; set; }
    }
  

}
