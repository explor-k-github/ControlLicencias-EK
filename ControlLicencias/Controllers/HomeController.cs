using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControlLicencias.Models;
using Microsoft.AspNetCore.Http;

namespace ControlLicencias.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            ViewData["User"] = HttpContext.Session.GetString("User");
            return View();
        }

        public IActionResult About() {
            string isadmin = HttpContext.Session.GetString("Admin");
            ViewData["User"] = HttpContext.Session.GetString("User");
            if (isadmin != "true") {
                return RedirectToAction("Index", "Home");
            } else {
                ViewData["Message"] = "Your application description page.";

                return View();
            }

           
        }

        public IActionResult Contact() {
            ViewData["User"] = HttpContext.Session.GetString("User");
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Privacy() {
            ViewData["User"] = HttpContext.Session.GetString("User");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
