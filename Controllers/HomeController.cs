using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RaqmiWeb.Models;
using RaqmiWeb.Services;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RaqmiWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAppEmailSender _email;
        private readonly EmailOptions _emailOptions;
        private readonly IEmailTemplateService _templateService;

        public HomeController(
            IAppEmailSender email,
            IOptions<EmailOptions> opts,
            IEmailTemplateService templateService)
        {
            _email = email;
            _emailOptions = opts.Value;
            _templateService = templateService;
        }

        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult References() => View();
        public IActionResult Thanks() => View();

        [HttpGet]
        public IActionResult Contact() => View(new ContactVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();

            var adminHtml = _templateService.AdminContactHtml(
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

        public IActionResult Privacy([FromServices] IWebHostEnvironment env)
        {
            var viewPath = Path.Combine(env.ContentRootPath, "Views", "Home", "Privacy.cshtml");
            DateTime updated;

            if (System.IO.File.Exists(viewPath))
                updated = System.IO.File.GetLastWriteTimeUtc(new FileInfo(viewPath).FullName);
            else
                updated = DateTime.Now;

            ViewData["PolicyUpdatedAt"] = updated;
            return View();
        }
    }
}