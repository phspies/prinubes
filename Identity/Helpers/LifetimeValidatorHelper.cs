using Microsoft.IdentityModel.Tokens;

namespace Prinubes.Identity.Helpers
{
    public class LifetimeValidatorHelper
    {
        public static bool LifetimeValidator(DateTime? notBefore,DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return expires != null && expires > DateTime.Now;
        }
    }
}
