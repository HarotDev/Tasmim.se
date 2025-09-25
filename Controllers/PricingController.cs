using Microsoft.AspNetCore.Mvc;
using RaqmiWeb.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RaqmiWeb.Controllers
{
    public class PricingController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new PricingVM
            {
                WebsitePackages = GetWebsitePackages(),
                ECommercePackages = GetECommercePackages()
            };

            var culture = CultureInfo.CurrentUICulture;
            var arabicFormat = new CultureInfo("ar-SA");
            var swedishFormat = new CultureInfo("sv-SE");

            foreach (var package in viewModel.WebsitePackages.Concat(viewModel.ECommercePackages))
            {
                if (culture.Name.StartsWith("ar"))
                {
                    package.FormattedPrice = package.Price.ToString("N0", arabicFormat) + " kr";
                }
                else
                {
                    package.FormattedPrice = package.Price.ToString("N0", swedishFormat) + ":-";
                }
            }
            return View(viewModel);
        }

        private static List<PackageViewModel> GetWebsitePackages() => new()
        {
            new() { Category = "Hemsida", Plan = "Liten", Price = 5000, Tone = "soft" },
            new() { Category = "Hemsida", Plan = "Mellan", Price = 7000, Tone = "brand", IsFeatured = true },
            new() { Category = "Hemsida", Plan = "Stor", Price = 10000, Tone = "deep" }
        };

        private static List<PackageViewModel> GetECommercePackages() => new()
        {
            new() { Category = "Webbutik", Plan = "Liten butik", Price = 12000, Tone = "amber" },
            new() { Category = "Webbutik", Plan = "Mellan", Price = 17000, Tone = "green", IsFeatured = true },
            new() { Category = "Webbutik", Plan = "Stor", Price = 22000, Tone = "violet" }
        };
    }
}