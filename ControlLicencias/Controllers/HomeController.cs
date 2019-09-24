using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControlLicencias.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ControlLicencias.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {


            ViewData["User"] = HttpContext.Session.GetString("User");
            return View();
        }

        public IActionResult About() {
            string isadmin = HttpContext.Session.GetString("Admin");
            string islogged = HttpContext.Session.GetString("Logged");
            ViewData["User"] = HttpContext.Session.GetString("User");
            ViewData["Admin"] = isadmin;
            if (islogged != "true") {
                return RedirectToAction("Index", "Home");
            } else {
                ViewData["Message"] = "No estás Logueado.";

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
