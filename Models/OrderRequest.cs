using System.ComponentModel.DataAnnotations;

namespace RaqmiWeb.Models
{
    public enum PackageCategory { Hemsida, Webbutik }

    public class OrderRequest
    {
        [Required, Display(Name = "Namn")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Phone, Display(Name = "Mobil")]
        public string? Phone { get; set; }

        [Display(Name = "Domän (om du har)")]
        public string? Domain { get; set; }

        [Required, Display(Name = "Kundtyp")]
        public string CustomerType { get; set; } = "Företag"; // "Privat" eller "Företag"

        [Display(Name = "Övrig information")]
        public string? Notes { get; set; }

        // Hidden metadata about selected package
        public PackageCategory Category { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
