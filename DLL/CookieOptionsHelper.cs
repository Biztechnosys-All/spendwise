﻿namespace Spendwise_WebApp.DLL
{
    public class CookieOptionsHelper
    {
        public static CookieOptions GetDefaultOptions()
        {
            return new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(3)
            };
        }
    }
}
