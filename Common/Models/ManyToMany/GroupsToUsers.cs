namespace Prinubes.Common.DatabaseModels.ManyToMany
{
    public class GroupsToUsers
    {
        public DateTime LinkDate { get; set; }

        public Guid GroupID { get; set; }
        public GroupDatabaseModel Group { get; set; }

        public Guid UserID{ get; set; }
        public UserDatabaseModel User { get; set; }
    }
}
