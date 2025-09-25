using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RaqmiWeb.Controllers
{
    public class CookieController : Controller
    {
        public const string ConsentCookieName = "CookieConsent";

        public class CookiePreferences
        {
            public bool Necessary { get; set; } = true;
            public bool Analytics { get; set; }
            public bool Marketing { get; set; }
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        [HttpGet]
        public IActionResult Get()
        {
            if (Request.Cookies.TryGetValue(ConsentCookieName, out var val))
                return Content(val, "application/json");

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Set(
            [FromForm] bool? acceptAll,
            [FromForm] bool? rejectAll,
            [FromForm] bool? analytics,
            [FromForm] bool? marketing,
            string? returnUrl = "/")
        {
            var pref = new CookiePreferences();

            if (acceptAll == true)
            {
                pref.Analytics = true;
                pref.Marketing = true;
            }
            else if (rejectAll == true)
            {
                pref.Analytics = false;
                pref.Marketing = false;
            }
            else
            {
                pref.Analytics = analytics == true;
                pref.Marketing = marketing == true;
            }

            var json = JsonSerializer.Serialize(pref, JsonOpts);

            Response.Cookies.Append(
                ConsentCookieName,
                json,
                new CookieOptions
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    MaxAge = TimeSpan.FromDays(365),
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = true
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reset(string? returnUrl = "/")
        {
            Response.Cookies.Delete(ConsentCookieName, new CookieOptions { Path = "/" });
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
