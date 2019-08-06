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
            try {
                // Only get files that begin with the letter "c".
                string[] dirs = Directory.GetFiles(@"/var/www/html/imgs/data/", "*");
                Console.WriteLine("The number of files starting with c is {0}.", dirs.Length);
                foreach (string dir in dirs) {
                    Console.WriteLine(dir);
                }
            } catch (Exception e) {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

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
