using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Net;

namespace RaqmiWeb.Services
{
    public interface IEmailTemplateService
    {
        string OrderConfirmationHtml(string customerName, string category, string plan, string price, string orderCode, CultureInfo culture);
        string AdminOrderHtml(BrandOptions b, string orderCode, DateTime timestampUtc, string customerType, string name, string email, string phone, string? domain, string category, string plan, string price, string? notes, string? ip, string? userAgent);
        string AdminContactHtml(BrandOptions b, DateTime timestampUtc, string name, string email, string phone, string? company, string? website, string interest, string message, string? ip, string? userAgent);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IStringLocalizer<EmailTemplateService> T;

        public EmailTemplateService(IStringLocalizer<EmailTemplateService> localizer)
        {
            T = localizer;
        }

        private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

        private string GetBaseTemplate(string title, string bodyContent, BrandOptions b, CultureInfo culture)
        {
            bool isArabic = culture.Name.StartsWith("ar");
            string dir = isArabic ? "rtl" : "ltr";
            string brandColor = b.PrimaryColor ?? "#8559c3";

            return $@"
<!DOCTYPE html>
<html lang=""{culture.Name}"" dir=""{dir}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{E(title)}</title>
    <style>
        body {{ margin: 0; padding: 0; width: 100% !important; background-color: #f4f4f7; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol""; color: #3d4451; }}
        .container {{ width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .card {{ background-color: #ffffff; border-radius: 12px; padding: 30px; box-shadow: 0 4px 15px rgba(0,0,0,0.07); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #e9e9e9; padding-bottom: 20px; }}
        .header h1 {{ font-size: 24px; color: #111; margin: 0; }}
        .content-section {{ font-size: 16px; line-height: 1.6; margin-bottom: 20px; }}
        .details-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .details-table td {{ padding: 10px; border-bottom: 1px solid #e9e9e9; }}
        .details-table tr:last-child td {{ border-bottom: none; }}
        .details-table .label {{ font-weight: bold; color: #555; width: 120px; }}
        .message-block {{ background-color: #fbfbfd; border: 1px solid #e9e9e9; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .cta-button {{ display: block; width: fit-content; margin: 25px auto 0 auto; padding: 12px 25px; background-color: {brandColor}; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: bold; text-align: center; }}
        .footer {{ text-align: center; margin-top: 25px; font-size: 12px; color: #999; }}
        .footer a {{ color: {brandColor}; text-decoration: none; }}
        .tech-details {{ font-size: 11px; color: #aaa; text-align: center; margin-top: 20px; }}
        {(isArabic ? "body { font-family: 'Tajawal', sans-serif; } .details-table .label { padding-left: 15px; }" : "")}
    </style>
    {(isArabic ? @"<link rel=""preconnect"" href=""https://fonts.googleapis.com""><link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin><link href=""https://fonts.googleapis.com/css2?family=Tajawal:wght@400;700&display=swap"" rel=""stylesheet"">" : "")}
</head>
<body>
    <div class=""container"">
        <div class=""card"">
            {bodyContent}
        </div>
        <div class=""footer"">
            &copy; {DateTime.Now.Year} {E(b.BrandName)}. Alla rättigheter förbehållna.<br>
            <a href=""mailto:{E(b.SupportEmail)}"">{E(b.SupportEmail)}</a>
        </div>
    </div>
</body>
</html>";
        }

        public string OrderConfirmationHtml(string customerName, string category, string plan, string price, string orderCode, CultureInfo culture)
        {
            var body = $@"
                <div class=""header"">
                    <h1>{T["OrderConfirmationTitle"]}</h1>
                </div>
                <div class=""content-section"">
                    <p>{T["HelloCustomer", E(customerName)]}</p>
                    <p>{T["OrderReceivedMessage"]}</p>
                </div>
                <div class=""details-table"">
                    <table>
                        <tr><td class=""label"">{T["Package"]}:</td><td>{E(category)} - {E(plan)}</td></tr>
                        <tr><td class=""label"">{T["Price"]}:</td><td>{E(price)}</td></tr>
                        <tr><td class=""label"">{T["OrderNumber"]}:</td><td>{E(orderCode)}</td></tr>
                    </table>
                </div>
                <div class=""content-section"">
                    <p>{T["ContactUsMessage"]}</p>
                </div>
                <a href=""https://tasmim.se"" class=""cta-button"">{T["VisitWebsite"]}</a>";

            return GetBaseTemplate(T["Order Confirmation"], body, new BrandOptions(), culture);
        }

        public string AdminOrderHtml(BrandOptions b, string orderCode, DateTime timestampUtc, string customerType, string name, string email, string phone, string? domain, string category, string plan, string price, string? notes, string? ip, string? userAgent)
        {
            var body = $@"
                <div class=""header"">
                    <h1>Ny Beställning</h1>
                </div>
                <div class=""content-section"">
                    <p>En ny beställning har lagts på hemsidan. Se detaljer nedan.</p>
                </div>
                <table class=""details-table"">
                    <tr><td class=""label"">Order-ID:</td><td><strong>{E(orderCode)}</strong></td></tr>
                    <tr><td class=""label"">Paket:</td><td>{E(category)} - {E(plan)}</td></tr>
                    <tr><td class=""label"">Pris:</td><td>{E(price)}</td></tr>
                    <tr><td class=""label"">Kundtyp:</td><td>{E(customerType)}</td></tr>
                    <tr><td class=""label"">Namn:</td><td>{E(name)}</td></tr>
                    <tr><td class=""label"">E-post:</td><td><a href=""mailto:{E(email)}"">{E(email)}</a></td></tr>
                    <tr><td class=""label"">Telefon:</td><td>{E(phone)}</td></tr>
                    <tr><td class=""label"">Domän:</td><td>{E(domain)}</td></tr>
                </table>";

            if (!string.IsNullOrWhiteSpace(notes))
            {
                body += $@"
                    <div class=""message-block"">
                        <strong>Noteringar från kund:</strong><br/>
                        {E(notes).Replace("\n", "<br/>")}
                    </div>";
            }

            body += $@"
                <a href=""mailto:{E(email)}"" class=""cta-button"">Svara kunden</a>
                <div class=""tech-details"">
                    Tid: {timestampUtc.ToLocalTime():yyyy-MM-dd HH:mm} | IP: {E(ip)}
                </div>";

            return GetBaseTemplate($"Ny Beställning: {orderCode}", body, b, CultureInfo.InvariantCulture); // Använder neutral kultur för admin-mejl
        }

        public string AdminContactHtml(BrandOptions b, DateTime timestampUtc, string name, string email, string phone, string? company, string? website, string interest, string message, string? ip, string? userAgent)
        {
            var body = $@"
                <div class=""header"">
                    <h1>Ny Kontaktförfrågan</h1>
                </div>
                <div class=""content-section"">
                    <p>Du har fått en ny förfrågan från <strong>{E(name)}</strong> via kontaktformuläret.</p>
                </div>
                <table class=""details-table"">
                    <tr><td class=""label"">Ärende:</td><td><strong>{E(interest)}</strong></td></tr>
                    <tr><td class=""label"">Namn:</td><td>{E(name)}</td></tr>
                    <tr><td class=""label"">E-post:</td><td><a href=""mailto:{E(email)}"">{E(email)}</a></td></tr>
                    <tr><td class=""label"">Telefon:</td><td>{E(phone)}</td></tr>
                    <tr><td class=""label"">Företag:</td><td>{E(company)}</td></tr>
                    <tr><td class=""label"">Webbplats:</td><td>{E(website)}</td></tr>
                </table>";

            if (!string.IsNullOrWhiteSpace(message))
            {
                body += $@"
                    <div class=""message-block"">
                        <strong>Meddelande:</strong><br/>
                        {E(message).Replace("\n", "<br/>")}
                    </div>";
            }

            body += $@"
                <a href=""mailto:{E(email)}"" class=""cta-button"">Svara {E(name)}</a>
                <div class=""tech-details"">
                    Tid: {timestampUtc.ToLocalTime():yyyy-MM-dd HH:mm} | IP: {E(ip)}
                </div>";

            return GetBaseTemplate($"Kontaktförfrågan från {name}", body, b, CultureInfo.InvariantCulture);
        }
    }
}

