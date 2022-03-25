using Prinubes.Common.DatabaseModels;

namespace Prinubes.Common.Models.Interfaces
{
    public interface ITag
    {
        public List<TaggingModel> Tags { get; set; }
        public Guid OrganizationID { get; set; }
    }
}
