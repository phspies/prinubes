using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using System.Net;

namespace Prinubes.Common.Helpers
{
    public class PlatformAuthorization
    {
        public static void Authorize<T>(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context?.ActionDescriptor);

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
            var DBService = context.HttpContext.RequestServices.GetRequiredService<IPrinubesDBContext>();
            ControllerActionDescriptor descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            ArgumentNullException.ThrowIfNull(descriptor?.AttributeRouteInfo?.Template);
            string routePathUnique = $"{descriptor.ActionName.ToLower()}:{descriptor.AttributeRouteInfo.Template}";
            //RoutePathDatabaseModel routePathEntry = DBService.RoutePaths.FirstOrDefault(x => x.RoutePathUnique == routePathUnique);

            OrganizationDatabaseModel organization;
            ArgumentNullException.ThrowIfNull(context?.HttpContext?.GetRouteData()?.Values);
            if (context.HttpContext.GetRouteData().Values.Any(x => x.Key == "organizationId"))
            {
                System.Guid organizationId;
                if (System.Guid.TryParse(context.HttpContext.GetRouteData().Values.Single<KeyValuePair<string, object>>(y => y.Key == "organizationId").Value.ToString(), out organizationId))
                {
                    organization = DBService.Organizations.Single(x => x.Id == organizationId);
                    if (organization == null)
                    {
                        context.Result = new NotFoundObjectResult(new ErrorReturnType(HttpStatusCode.NotFound, $"Organization {organizationId} not found"));
                        return;
                    }
                }
                else
                {
                    context.Result = new NotFoundObjectResult(new ErrorReturnType(HttpStatusCode.NotFound, $"Organization {context.HttpContext.GetRouteData().Values.Single<KeyValuePair<string, object>>(y => y.Key == "organizationId").Value} not found"));
                    return;
                }

            }
            else
            {
                return;
            }
        }
    }
}
