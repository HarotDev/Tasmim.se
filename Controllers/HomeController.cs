using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RaqmiWeb.Models;
using RaqmiWeb.Services;
using System;

namespace RaqmiWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAppEmailSender _email;
        private readonly EmailOptions _emailOptions;

        public HomeController(IAppEmailSender email, IOptions<EmailOptions> opts)
        {
            _email = email;
            _emailOptions = opts.Value;
        }

        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult References() => View();

        [HttpGet]
        public IActionResult Contact() => View(new ContactVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();

            var adminHtml = EmailTemplates.AdminContactHtml(
                _emailOptions.Brand,
                timestampUtc: DateTime.UtcNow,
                name: vm.Name ?? "",
                email: vm.Email ?? "",
                phone: vm.Phone ?? "",
                company: vm.Company ?? "",
                website: vm.Website ?? "",
                interest: vm.Interest ?? "",
                message: vm.Message ?? "",
                ip: ip,
                userAgent: ua
            );

            try
            {
                await _email.SendAsync(
                    subject: $"[Kontakt] {vm.Name} – {vm.Interest}",
                    htmlBody: adminHtml,
                    replyTo: vm.Email,
                    ct: HttpContext.RequestAborted);

                TempData["ContactOk"] = true;
                return RedirectToAction(nameof(Contact));
            }
            catch
            {
                ModelState.AddModelError("", "Kunde inte skicka meddelandet just nu. Försök igen senare.");
                return View(vm);
            }
        }
    }
}
