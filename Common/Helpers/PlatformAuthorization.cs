using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Datamodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Prinubes.Common.Helpers
{
    public class PlatformAuthorization
    {
        public static void Authorize<T>(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                // it isn't needed to set unauthorized result 
                // as the base class already requires the user to be authenticated
                // this also makes redirect to a login page work properly
                // context.Result = new UnauthorizedResult();
                return;
            }

            // you can also use registered services
            var DBService = context.HttpContext.RequestServices.GetService<T>() as IPrinubesDBContext;
            ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            string routePathUnique = $"{descriptor.ActionName.ToLower()}:{descriptor.AttributeRouteInfo.Template}";
            RoutePathDatabaseModel routePathEntry = DBService.RoutePaths.FirstOrDefault(x => x.RoutePathUnique == routePathUnique);

            DatabaseModels.OrganizationDatabaseModel organization;
            if (context.HttpContext.GetRouteData().Values.Any(x => x.Key == "organizationId"))
            {
                System.Guid organizationId;
                if (System.Guid.TryParse(context.HttpContext.GetRouteData().Values.FirstOrDefault<KeyValuePair<string, object>>(y => y.Key == "organizationId").Value.ToString(), out organizationId))
                {
                    organization = DBService.Organizations.FirstOrDefault(x => x.Id == organizationId);
                    if (organization == null)
                    {
                        context.Result = new NotFoundObjectResult(new ErrorReturnType(HttpStatusCode.NotFound, $"Organization {organizationId} not found"));
                        return;
                    }
                }
                else
                {
                    context.Result = new NotFoundObjectResult(new ErrorReturnType(HttpStatusCode.NotFound, $"Organization {context.HttpContext.GetRouteData().Values.FirstOrDefault<KeyValuePair<string, object>>(y => y.Key == "organizationId").Value} not found"));
                    return;
                }    

            }
            else
            {
                return;
            }

            //context.Result = new OkResult();


            //var isAuthorized = someService.IsUserAuthorized(user.Prinubes.Identity.Name, _someFilterParameter);
            //if (!isAuthorized)
            //{
            //    context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            //    return;
            //}
        }
    }
}
