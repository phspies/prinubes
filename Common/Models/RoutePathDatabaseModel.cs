using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Kafka;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn, Title = "RoutePathKafkaMessage")]
    [MessageTopic("routepath-events", 0)]
    public class RoutePathKafkaMessage : IMessage
    {
        [JsonProperty("action")]
        public ActionEnum Action { get; set; }
        [JsonProperty("id")]
        public Guid RoutePathID { get; set; }
        [JsonProperty("routepath_data_model")]
        public RoutePathDatabaseModel RoutePath { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RoutePathDatabaseModel 
    {
        [JsonRequired]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("microservice")]
        public string MicroService { get; set; }
        [JsonProperty("controller")]
        public string Controller { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("method_name")]
        public string MethodName { get; set; }
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
        [JsonProperty("route_template")]
        public string RouteTemplate { get; set; }
        [JsonProperty("routePathUnique")]
        public string RoutePathUnique { get; set; }
        public IEnumerable<RolesToRoutePaths> RolesToPaths { get; set; }
        public List<RoleDatabaseModel> Roles { get; set; }

    }
}
