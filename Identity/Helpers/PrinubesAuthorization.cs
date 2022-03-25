using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Prinubes.Common.Helpers;
using Prinubes.Identity.Datamodels;

namespace Prinubes.Identity.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PrinubesAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _someFilterParameter;

        public PrinubesAuthorizeAttribute()
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            PlatformAuthorization.Authorize<PrinubesIdentityDBContext>(context);
        }
    }

}
