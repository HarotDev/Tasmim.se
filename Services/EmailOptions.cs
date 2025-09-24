namespace RaqmiWeb.Services
{
    public class BrandOptions
    {
        public string BrandName { get; set; } = "Tasmim";
        public string PrimaryColor { get; set; } = "#8559c3";
        public string LogoUrl { get; set; } = "";                // absolut https-URL
        public string SiteUrl { get; set; } = "https://tasmim.se";
        public string SupportEmail { get; set; } = "info@tasmim.se";
        public string SupportPhone { get; set; } = "";
        public string Address { get; set; } = "";
    }

    public class EmailOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;                     // 465 (SslOnConnect) eller 587 (StartTls)
        /// <summary> "StartTls", "SslOnConnect", "Auto" </summary>
        public string Secure { get; set; } = "StartTls";
        public string User { get; set; } = "";                   // full e-postadress
        public string Pass { get; set; } = "";                   // lösen/app-lösen
        public string From { get; set; } = "";                   // avsändaradress (oftast samma som User)
        public string FromName { get; set; } = "Tasmim";
        public string To { get; set; } = "";                     // adminmottagare (komma/semikolon-separerad)
        public int TimeoutSeconds { get; set; } = 15;

        public BrandOptions Brand { get; set; } = new();
    }
}
