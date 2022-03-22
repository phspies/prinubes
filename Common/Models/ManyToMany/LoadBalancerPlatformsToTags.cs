namespace Prinubes.Common.DatabaseModels.ManyToMany
{
    public class LoadBalancerPlatformsToTags
    {
        public DateTime LinkDate { get; set; }

        public Guid LoadBalancerID { get; set; }
        public LoadBalancerPlatformDatabaseModel LoadBalancerPlatform { get; set; }

        public Guid TagID{ get; set; }
        public TaggingModel Tag { get; set; }
    }
}
