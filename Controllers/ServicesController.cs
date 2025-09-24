using Microsoft.AspNetCore.Mvc;

public class ServicesController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Web() => View();
    public IActionResult Design() => View();
    public IActionResult Marketing() => View();
}
