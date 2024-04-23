using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers.V1
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
