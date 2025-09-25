using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using MailKitSmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace RaqmiWeb.Services
{
    public class SmtpEmailSender : IAppEmailSender
    {
        private readonly EmailOptions _cfg;
        public SmtpEmailSender(IOptions<EmailOptions> cfg) => _cfg = cfg.Value;

        private static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var text = Regex.Replace(html, "<br ?/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "</p>", "\n\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "<.*?>", string.Empty, RegexOptions.Singleline);
            return WebUtility.HtmlDecode(text).Trim();
        }

        public async Task SendAsync(string subject, string htmlBody,
                                     string? toOverride = null,
                                     string? replyTo = null,
                                     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_cfg.Host)) throw new InvalidOperationException("Email.Host saknas.");
            if (_cfg.Port <= 0) throw new InvalidOperationException("Email.Port ogiltig.");
            if (string.IsNullOrWhiteSpace(_cfg.From)) throw new InvalidOperationException("Email.From saknas.");
            if (string.IsNullOrWhiteSpace(_cfg.User)) throw new InvalidOperationException("Email.User saknas.");
            if (string.IsNullOrWhiteSpace(_cfg.To) && string.IsNullOrWhiteSpace(toOverride))
                throw new InvalidOperationException("Ingen mottagare angiven (Email.To eller toOverride).");

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_cfg.FromName ?? "", _cfg.From));

            var toRaw = string.IsNullOrWhiteSpace(toOverride) ? _cfg.To : toOverride;
            foreach (var r in toRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                msg.To.Add(MailboxAddress.Parse(r));

            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyTo.Add(MailboxAddress.Parse(replyTo));

            msg.Subject = subject ?? "(No subject)";
            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody ?? "",
                TextBody = ToPlainText(htmlBody ?? "")
            };
            msg.Body = builder.ToMessageBody();

            var secure = (_cfg.Secure ?? "StartTls").Trim().ToLowerInvariant();
            var options = secure switch
            {
                "sslonconnect" => SecureSocketOptions.SslOnConnect,
                "starttls" => SecureSocketOptions.StartTls,
                "auto" => SecureSocketOptions.Auto,
                _ => SecureSocketOptions.StartTls
            };

            using var smtp = new MailKitSmtpClient { Timeout = Math.Max(5000, _cfg.TimeoutSeconds * 1000) };
            try
            {
                await smtp.ConnectAsync(_cfg.Host, _cfg.Port, options, ct);
                await smtp.AuthenticateAsync(_cfg.User, _cfg.Pass, ct);
                await smtp.SendAsync(msg, ct);
            }
            finally
            {
                try { await smtp.DisconnectAsync(true, ct); } catch { /* ignore */ }
            }
        }
    }
}
