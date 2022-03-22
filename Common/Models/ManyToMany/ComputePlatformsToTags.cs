namespace Prinubes.Common.DatabaseModels.ManyToMany
{
    public class ComputePlatformsToTags
    {
        public DateTime LinkDate { get; set; }

        public Guid ComputePlatformID { get; set; }
        public ComputePlatformDatabaseModel ComputePlatform { get; set; }

        public Guid TagID{ get; set; }
        public TaggingModel Tag { get; set; }
    }
}
