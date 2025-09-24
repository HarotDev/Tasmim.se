using System.Threading;
using System.Threading.Tasks;

namespace RaqmiWeb.Services
{
    public interface IAppEmailSender
    {
        Task SendAsync(string subject, string htmlBody,
                       string? toOverride = null,
                       string? replyTo = null,
                       CancellationToken ct = default);
    }
}
