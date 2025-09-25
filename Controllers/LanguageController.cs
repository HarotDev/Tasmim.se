using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace RaqmiWeb.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Set(string culture, string returnUrl = "/")
        {
            if (culture != "sv" && culture != "ar")
            {
                culture = "sv";
            }

            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cookieValue,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = true,
                    HttpOnly = true, 
                    Path = "/"
                }
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                var path = returnUrl;
                var queryString = string.Empty;
                var queryIndex = returnUrl.IndexOf('?');

                if (queryIndex > -1)
                {
                    path = returnUrl.Substring(0, queryIndex);
                    queryString = returnUrl.Substring(queryIndex);
                }

                var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString)
                    .Where(kvp => kvp.Key != "lang" && kvp.Key != "culture" && kvp.Key != "ui-culture")
                    .ToDictionary(k => k.Key, v => v.Value.ToString());

                var cleanUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(path, queryParams);

                return LocalRedirect(cleanUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

