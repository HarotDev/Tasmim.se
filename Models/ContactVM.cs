using System.ComponentModel.DataAnnotations;

namespace RaqmiWeb.Models
{
    public class ContactVM
    {
        [Required(ErrorMessage = "Ange ditt namn.")]
        [MaxLength(120)]
        public string Name { get; set; } = "";

        [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
        [Required(ErrorMessage = "Ange din e-post.")]
        public string Email { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }

        [MaxLength(160)]
        public string? Company { get; set; }

        [Url(ErrorMessage = "Ogiltig URL.")]
        public string? Website { get; set; }

        [Required(ErrorMessage = "Välj vad ärendet gäller.")]
        public string Interest { get; set; } = "";

        [MaxLength(4000)]
        public string? Message { get; set; }

        public string? InternalRef { get; set; }
    }
}
