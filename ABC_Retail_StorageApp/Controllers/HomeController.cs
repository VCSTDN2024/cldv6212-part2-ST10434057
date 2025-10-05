using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail_StorageApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
