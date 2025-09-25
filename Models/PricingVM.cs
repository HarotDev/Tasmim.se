using System.Collections.Generic;

namespace RaqmiWeb.Models
{
    public class PricingVM
    {
        public List<PackageViewModel> WebsitePackages { get; set; } = new();
        public List<PackageViewModel> ECommercePackages { get; set; } = new();
    }

    public class PackageViewModel
    {
        public string Category { get; set; } = "";
        public string Plan { get; set; } = "";
        public int Price { get; set; }
        public string FormattedPrice { get; set; } = "";
        public bool IsFeatured { get; set; }
        public string Tone { get; set; } = "";
    }
}