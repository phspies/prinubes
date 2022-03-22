using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Prinubes.Identity.Helpers
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
