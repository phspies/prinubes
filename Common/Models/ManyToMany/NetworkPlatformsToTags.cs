namespace Prinubes.Common.DatabaseModels.ManyToMany
{
    public class NetworkPlatformsToTags
    {
        public DateTime LinkDate { get; set; }

        public Guid NetworkPlatformID { get; set; }
        public NetworkPlatformDatabaseModel NetworkPlatform { get; set; }

        public Guid TagID { get; set; }
        public TaggingModel Tag { get; set; }
    }
}
