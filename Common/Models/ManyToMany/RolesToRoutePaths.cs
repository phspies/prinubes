namespace Prinubes.Common.DatabaseModels.ManyToMany
{
    public class RolesToRoutePaths
    {
        public DateTime LinkDate { get; set; }
        public Guid RoleID { get; set; }
        public RoleDatabaseModel Role { get; set; }
        public Guid RoutePathID { get; set; }
        public RoutePathDatabaseModel RoutePath { get; set; }
    }
}
