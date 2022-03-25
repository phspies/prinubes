namespace Prinubes.Common.Helpers
{
    public class PrinubesDescriptionAttribute : Attribute
    {
        public PrinubesDescriptionAttribute(params string[] friendlyName)
        {
            FriendlyName = friendlyName;
        }

        public string[] FriendlyName { get; set; }

    }
}
