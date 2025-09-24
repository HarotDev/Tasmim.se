using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RaqmiWeb.Models;
using RaqmiWeb.Services;
using System.Threading;

namespace RaqmiWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAppEmailSender _email;
        private readonly EmailOptions _emailOptions;

        public OrderController(IAppEmailSender email, IOptions<EmailOptions> opts)
        {
            _email = email;
            _emailOptions = opts.Value;
        }

        [HttpGet]
        public IActionResult New(string category, string plan, string price)
        {
            var model = new OrderRequest
            {
                Category = Enum.TryParse<PackageCategory>(category ?? "Hemsida", true, out var cat) ? cat : PackageCategory.Hemsida,
                Plan = plan ?? "",
                Price = price ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(OrderRequest req)
        {
            if (!ModelState.IsValid) return View("New", req);

            // 1) Skapa ordernummer
            var orderCode = OrderNumber.New(prefix: "TS", randomChars: 2);

            // 2) Meta för admin-mailet (kan vara kvar även om du dolt IP/UA i mallen)
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();

            // 3) Admin-mail (du hade redan AdminOrderHtml som tar orderCode)
            var adminHtml = EmailTemplates.AdminOrderHtml(
                _emailOptions.Brand,
                orderCode: orderCode,
                timestampUtc: DateTime.UtcNow,
                customerType: req.CustomerType,
                name: req.Name,
                email: req.Email,
                phone: req.Phone ?? "",
                domain: req.Domain ?? "",
                category: req.Category.ToString(),
                plan: req.Plan,
                price: req.Price,
                notes: req.Notes ?? "",
                ip: ip,
                userAgent: ua
            );

            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted, timeoutCts.Token);

                // a) Till dig
                await _email.SendAsync(
                    subject: $"[Order {orderCode}] {req.Category} – {req.Plan}",
                    htmlBody: adminHtml,
                    toOverride: null,
                    replyTo: req.Email,
                    ct: linked.Token);

                // b) Bekräftelse till kunden (med ordernummer)
                var confirmHtml = EmailTemplates.OrderConfirmationHtml(
                    _emailOptions.Brand,
                    customerName: req.Name,
                    category: req.Category.ToString(),
                    plan: req.Plan,
                    price: req.Price,
                    orderCode: orderCode
                );

                await _email.SendAsync(
                    subject: $"Order {orderCode} – Tack! Vi har mottagit din beställning",
                    htmlBody: confirmHtml,
                    toOverride: req.Email,
                    replyTo: _emailOptions.From,
                    ct: linked.Token);

                // (valfritt) om du vill visa i Thanks-vyn
                TempData["OrderCode"] = orderCode;
                TempData["Category"] = req.Category.ToString();
                TempData["Plan"] = req.Plan;
                TempData["Price"] = req.Price;

                return RedirectToAction("Thanks");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kunde inte skicka beställningen. Försök igen senare eller kontakta oss.");
                ModelState.AddModelError("", ex.Message);
                return View("New", req);
            }
        }

        public IActionResult Thanks() => View();
    }
}
