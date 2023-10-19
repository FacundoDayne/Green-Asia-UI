using Green_Asia_UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Green_Asia_UI.Controllers
{
    public class NewHomeController : Controller
    {
        private readonly ILogger<NewHomeController> _logger;

        public NewHomeController(ILogger<NewHomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult adminAddContractor()
        {
            return View();
        }

        public IActionResult adminAddProject()
        {
            return View();
        }

        public IActionResult adminAddSupplier()
        {
            return View();
        }

        public IActionResult adminContractorDash()
        {
            return View();
        }

        public IActionResult adminDashboard()
        {
            return View();
        }

        public IActionResult adminProjectDash()
        {
            return View();
        }

        public IActionResult adminSupplierDash()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}