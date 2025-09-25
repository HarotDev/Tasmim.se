using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using RaqmiWeb.Models;
using RaqmiWeb.Services;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace RaqmiWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAppEmailSender _email;
        private readonly EmailOptions _emailOptions;
        private readonly IEmailTemplateService _templateService;
        private readonly IStringLocalizer<OrderController> T;

        public OrderController(
            IAppEmailSender email,
            IOptions<EmailOptions> opts,
            IEmailTemplateService templateService,
            IStringLocalizer<OrderController> localizer)
        {
            _email = email;
            _emailOptions = opts.Value;
            _templateService = templateService;
            T = localizer;
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
            if (int.TryParse(price, out var priceValue))
            {
                var culture = CultureInfo.CurrentUICulture;
                var arabicFormat = new CultureInfo("ar-SA");
                var swedishFormat = new CultureInfo("sv-SE");
                model.FormattedPrice = culture.Name.StartsWith("ar")
                    ? priceValue.ToString("N0", arabicFormat) + " kr"
                    : priceValue.ToString("N0", swedishFormat) + ":-";
            }
            else { model.FormattedPrice = price; }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(OrderRequest req)
        {
            var culture = CultureInfo.CurrentUICulture;

            if (!ModelState.IsValid)
            {
                if (int.TryParse(req.Price, out var priceVal))
                {
                    var arabicFormat = new CultureInfo("ar-SA");
                    var swedishFormat = new CultureInfo("sv-SE");
                    req.FormattedPrice = culture.Name.StartsWith("ar")
                       ? priceVal.ToString("N0", arabicFormat) + " kr"
                       : priceVal.ToString("N0", swedishFormat) + ":-";
                }
                return View("New", req);
            }

            var orderCode = OrderNumber.New(prefix: "TS", randomChars: 2);

            var translatedCategory = T[req.Category.ToString()];
            var translatedPlan = T[req.Plan];

            string formattedPriceForDisplay;
            if (int.TryParse(req.Price, out var priceValue))
            {
                var swedishFormat = new CultureInfo("sv-SE");
                var arabicFormat = new CultureInfo("ar-SA");
                formattedPriceForDisplay = culture.Name.StartsWith("ar")
                    ? priceValue.ToString("N0", arabicFormat) + " kr"
                    : priceValue.ToString("N0", swedishFormat) + ":-";
            }
            else { formattedPriceForDisplay = req.Price; }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();

            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted, timeoutCts.Token);

                var confirmHtml = _templateService.OrderConfirmationHtml(
                    customerName: req.Name,
                    category: translatedCategory,
                    plan: translatedPlan,
                    price: formattedPriceForDisplay,
                    orderCode: orderCode,
                    culture: culture
                );

                var adminHtml = _templateService.AdminOrderHtml(
                    _emailOptions.Brand, orderCode, DateTime.UtcNow, req.CustomerType, req.Name, req.Email,
                    req.Phone ?? "", req.Domain ?? "", translatedCategory, translatedPlan, formattedPriceForDisplay,
                    req.Notes ?? "", ip, ua
                );

                await _email.SendAsync(
                    subject: $"[Order {orderCode}] {req.Category} – {req.Plan}",
                    htmlBody: adminHtml,
                    toOverride: null,
                    replyTo: req.Email,
                    ct: linked.Token);

                await _email.SendAsync(
                    subject: $"{T["Order"]} {orderCode} – {T["Thank you! We have received your order"]}",
                    htmlBody: confirmHtml,
                    toOverride: req.Email,
                    replyTo: _emailOptions.From,
                    ct: linked.Token);

                TempData["Category"] = translatedCategory.ToString();
                TempData["Plan"] = translatedPlan.ToString();
                TempData["Price"] = formattedPriceForDisplay;

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

