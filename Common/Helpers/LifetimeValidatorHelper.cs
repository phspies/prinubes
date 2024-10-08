﻿using Microsoft.IdentityModel.Tokens;

namespace Prinubes.Common.Helpers
{
    public class LifetimeValidatorHelper
    {
        public static bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return expires != null && expires > DateTime.Now;
        }
    }
}
