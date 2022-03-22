using Prinubes.Identity.Datamodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Prinubes.Common.DatabaseModels;
using System.Net;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Helpers;

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
