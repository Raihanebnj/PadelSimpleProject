using Microsoft.AspNetCore.Mvc;

namespace PadelSimple.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
