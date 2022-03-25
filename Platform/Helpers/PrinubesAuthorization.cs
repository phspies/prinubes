using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Prinubes.Common.Helpers;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.Platforms.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PrinubesAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _someFilterParameter;

        public PrinubesAuthorizeAttribute()
        {
        }
        public void test()
        {

        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            PlatformAuthorization.Authorize<PrinubesPlatformDBContext>(context);
        }
    }

}
