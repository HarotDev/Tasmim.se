using System.Net;
using System.Text.RegularExpressions;
using System;

namespace RaqmiWeb.Services
{
    public static class EmailTemplates
    {
        private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

        // ---- Plain-text fallback ----
        public static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var text = Regex.Replace(html, "<br ?/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "</p>", "\n\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "<.*?>", string.Empty, RegexOptions.Singleline);
            return WebUtility.HtmlDecode(text).Trim();
        }

        // ---- Delad header/footer för ADMIN-mail ----
        private static string Header(BrandOptions b, string title)
        {
            var logoHtml = string.IsNullOrWhiteSpace(b.LogoUrl)
                ? $"<div style=\"font-weight:800;color:{b.PrimaryColor};font-size:18px\">{E(b.BrandName)}</div>"
                : $"<img src=\"{E(b.LogoUrl)}\" alt=\"{E(b.BrandName)}\" height=\"28\" style=\"display:block;border:0;outline:none;\"/>";

            return $@"
<!doctype html><html><head><meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>{E(b.BrandName)} – {E(title)}</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7f9;color:#0f1220;font-family:Inter,Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f6f7f9;padding:24px 0;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:760px;background:#fff;border:1px solid #e6e8eb;border-radius:16px;box-shadow:0 8px 20px rgba(16,20,24,.06);overflow:hidden;"">
        <tr>
          <td style=""padding:18px 22px;border-bottom:1px solid #eef1f4;"">
            <table width=""100%"" role=""presentation"">
              <tr>
                <td align=""left"">{logoHtml}</td>
                <td align=""right"" style=""font-size:12px;color:#667085"">{E(b.BrandName)}</td>
              </tr>
            </table>
          </td>
        </tr>
        <tr><td style=""padding:22px 22px 12px 22px"">
          <h1 style=""margin:0 0 6px 0;font-size:22px;line-height:1.25;"">{E(title)}</h1>";
        }

        private static string Footer(BrandOptions b)
        {
            return $@"
        </td></tr>
        <tr>
          <td style=""padding:16px 22px;border-top:1px solid #eef1f4;background:#fbfbfc;"">
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
              <tr>
                <td style=""font-size:12px;color:#667085;"">
                  <div><strong>{E(b.BrandName)}</strong></div>
                  {(string.IsNullOrWhiteSpace(b.Address) ? "" : $"<div>{E(b.Address)}</div>")}
                  {(string.IsNullOrWhiteSpace(b.SupportPhone) ? "" : $"<div>Tel: {E(b.SupportPhone)}</div>")}
                  <div>E-post: <a href=""mailto:{E(b.SupportEmail)}"" style=""color:#111827;text-decoration:none"">{E(b.SupportEmail)}</a></div>
                  <div><a href=""{E(b.SiteUrl)}"" style=""color:#111827;text-decoration:none"">{E(b.SiteUrl)}</a></div>
                </td>
                <td align=""right"" style=""font-size:11px;color:#98a2b3;"">
                  © {DateTime.UtcNow.Year} {E(b.BrandName)}.
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body></html>";
        }

        // ============ ADMIN: Beställning ============
        public static string AdminOrderHtml(
            BrandOptions b,
            string orderCode,
            DateTime timestampUtc,
            string customerType,
            string name,
            string email,
            string phone,
            string? domain,
            string category,
            string plan,
            string price,
            string? notes,
            string? ip,
            string? userAgent)
        {
            var head = Header(b, "Ny beställning");
            var timeLocal = timestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            return head + $@"
          <p style=""margin:0 0 12px 0;color:#5e6676;font-size:15px;"">
            En ny beställning har inkommit. <strong>Order-ID:</strong> {E(orderCode)}
          </p>

          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #eee;border-radius:12px;overflow:hidden;margin:12px 0"">
            <tr style=""background:#fbfbfd"">
              <td style=""padding:12px 14px;font-weight:700;border-bottom:1px solid #eee"">Paket</td>
              <td style=""padding:12px 14px;border-bottom:1px solid #eee"">{E(category)} • {E(plan)}</td>
              <td style=""padding:12px 14px;border-bottom:1px solid #eee;text-align:right;color:{b.PrimaryColor};font-weight:800"">{E(price)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700;width:160px"">Kundtyp</td>
              <td colspan=""2"" style=""padding:12px 14px"">{E(customerType)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Namn</td>
              <td colspan=""2"" style=""padding:12px 14px"">{E(name)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">E-post</td>
              <td colspan=""2"" style=""padding:12px 14px""><a href=""mailto:{E(email)}"">{E(email)}</a></td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Telefon</td>
              <td colspan=""2"" style=""padding:12px 14px"">{E(phone)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Domän</td>
              <td colspan=""2"" style=""padding:12px 14px"">{E(domain)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700;vertical-align:top"">Övrigt</td>
              <td colspan=""2"" style=""padding:12px 14px"">{E(notes).Replace("\n", "<br/>")}</td>
            </tr>
          </table>

          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:8px 0;color:#667085;font-size:12px"">
            <tr>
              <td>Order-ID: <strong>{E(orderCode)}</strong></td>
              <td style=""text-align:right"">Tid: {E(timeLocal)} (lokal)</td>
            </tr>
            <tr>
              <td>IP: {E(ip)}</td>
              <td style=""text-align:right"">UA: {E(userAgent)}</td>
            </tr>
          </table>

          <div style=""margin-top:14px"">
            <a href=""mailto:{E(email)}"" style=""display:inline-block;padding:10px 14px;border-radius:10px;background:{b.PrimaryColor};color:#fff;text-decoration:none;font-weight:700"">Svara kunden</a>
          </div>
" + Footer(b);
        }

        // ============ ADMIN: Kontakt ============
        public static string AdminContactHtml(
            BrandOptions b,
            DateTime timestampUtc,
            string name,
            string email,
            string phone,
            string? company,
            string? website,
            string interest,
            string message,
            string? ip,
            string? userAgent)
        {
            var head = Header(b, "Ny kontaktförfrågan");
            var timeLocal = timestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            return head + $@"
          <p style=""margin:0 0 12px 0;color:#5e6676;font-size:15px;"">
            En ny förfrågan har skickats via kontaktformuläret.
          </p>

          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #eee;border-radius:12px;overflow:hidden;margin:12px 0"">
            <tr>
              <td style=""padding:12px 14px;font-weight:700;width:160px"">Namn</td>
              <td style=""padding:12px 14px"">{E(name)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">E-post</td>
              <td style=""padding:12px 14px""><a href=""mailto:{E(email)}"">{E(email)}</a></td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Telefon</td>
              <td style=""padding:12px 14px"">{E(phone)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Företag</td>
              <td style=""padding:12px 14px"">{E(company)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Webb</td>
              <td style=""padding:12px 14px"">{E(website)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700"">Ärende</td>
              <td style=""padding:12px 14px"">{E(interest)}</td>
            </tr>
            <tr>
              <td style=""padding:12px 14px;font-weight:700;vertical-align:top"">Meddelande</td>
              <td style=""padding:12px 14px"">{E(message).Replace("\n", "<br/>")}</td>
            </tr>
          </table>

          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:8px 0;color:#667085;font-size:12px"">
            <tr>
              <td>IP: {E(ip)}</td>
              <td style=""text-align:right"">Tid: {E(timeLocal)} (lokal)</td>
            </tr>
            <tr>
              <td colspan=""2"">UA: {E(userAgent)}</td>
            </tr>
          </table>

          <div style=""margin-top:14px"">
            <a href=""mailto:{E(email)}"" style=""display:inline-block;padding:10px 14px;border-radius:10px;background:{b.PrimaryColor};color:#fff;text-decoration:none;font-weight:700"">Svara {E(name)}</a>
          </div>
" + Footer(b);
        }

        // ============ KUND: Bekräftelse (du hade redan denna – full version) ============
        public static string OrderConfirmationHtml(
            BrandOptions b,
            string customerName,
            string category,
            string plan,
            string price,
            string orderCode)
        {
            var logoHtml = string.IsNullOrWhiteSpace(b.LogoUrl)
                ? $"<div style=\"font-weight:800;color:{b.PrimaryColor};font-size:18px\">{E(b.BrandName)}</div>"
                : $"<img src=\"{E(b.LogoUrl)}\" alt=\"{E(b.BrandName)}\" height=\"28\" style=\"display:block;border:0;outline:none;\"/>";

            return $@"<!doctype html><html><head><meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>{E(b.BrandName)} – Beställning mottagen</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7f9;color:#0f1220;font-family:Inter,Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f6f7f9;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:640px;background:#fff;border:1px solid #e6e8eb;border-radius:16px;box-shadow:0 8px 20px rgba(16,20,24,.06);overflow:hidden;"">
          <tr>
            <td style=""padding:18px 22px;border-bottom:1px solid #eef1f4;"">
              <table width=""100%"" role=""presentation"">
                <tr>
                  <td align=""left"">{logoHtml}</td>
                  <td align=""right"" style=""font-size:12px;color:#667085"">{E(b.BrandName)}</td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px 22px 8px 22px"">
              <h1 style=""margin:0 0 8px 0;font-size:22px;line-height:1.25;"">Tack! Din beställning är mottagen.</h1>
              <p style=""margin:0;color:#5e6676;font-size:15px;line-height:1.6;"">
                Hej {E(customerName)},<br/>
                vi återkommer snart för att bekräfta detaljer och nästa steg.
              </p>

              <!-- Orderrad -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:16px 0 4px 0;background:#fafafa;border:1px solid #eee;border-radius:12px;"">
                <tr>
                  <td style=""padding:14px 16px;"">
                    <div style=""font-weight:700;margin-bottom:4px;"">{E(category)} • {E(plan)}</div>
                    {(string.IsNullOrWhiteSpace(price) ? "" : $"<div style='color:{b.PrimaryColor};font-weight:800;margin-bottom:6px'>{E(price)}</div>")}
                    <div style=""font-size:13px;color:#667085""><strong>Ordernummer:</strong> {E(orderCode)}</div>
                  </td>
                </tr>
              </table>

              <p style=""margin:12px 0 0 0;font-size:14px;color:#667085;"">
                Behöver du justera något? Svara bara på detta mail så fixar vi det.
              </p>

              <div style=""margin:18px 0 6px 0;"">
                <a href=""{E(b.SiteUrl)}"" style=""display:inline-block;padding:12px 18px;border-radius:12px;background:{b.PrimaryColor};color:#fff;text-decoration:none;font-weight:700"">
                  Besök {E(b.BrandName)}
                </a>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""padding:16px 22px;border-top:1px solid #eef1f4;background:#fbfbfc;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-size:12px;color:#667085;"">
                    <div><strong>{E(b.BrandName)}</strong></div>
                    {(string.IsNullOrWhiteSpace(b.Address) ? "" : $"<div>{E(b.Address)}</div>")}
                    {(string.IsNullOrWhiteSpace(b.SupportPhone) ? "" : $"<div>Tel: {E(b.SupportPhone)}</div>")}
                    <div>E-post: <a href=""mailto:{E(b.SupportEmail)}"" style=""color:#111827;text-decoration:none"">{E(b.SupportEmail)}</a></div>
                    <div><a href=""{E(b.SiteUrl)}"" style=""color:#111827;text-decoration:none"">{E(b.SiteUrl)}</a></div>
                  </td>
                  <td align=""right"" style=""font-size:11px;color:#98a2b3;"">
                    © {DateTime.UtcNow.Year} {E(b.BrandName)}.
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body></html>";
        }
    }
}
