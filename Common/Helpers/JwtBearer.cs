using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Prinubes.Common.Datamodels;

namespace Prinubes.Common.Helpers
{
    public class JwtBearer<T> : JwtBearerEvents
    {
        private Guid UserID { get; set; }

        private string UserEmail { get; set; }

        private string UserName { get; set; }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            try
            {

                IPrinubesDBContext userService = context.HttpContext.RequestServices.GetRequiredService<T>() as IPrinubesDBContext;
                UserID = Guid.Parse(context.Principal.Identity.Name);
                var user = userService.Users.Find(UserID);
                if (user == null)
                {
                    context.Fail($"{user} is unauthorized");
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
