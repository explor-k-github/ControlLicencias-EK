using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ControlLicencias.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ControlLicencias.Controllers
{
    public class LicenseController : Controller
    {
        public IActionResult Index(){
            string isadmin = HttpContext.Session.GetString("Admin");
            ViewData["User"] = HttpContext.Session.GetString("User");
            if (isadmin != "true") {
                return RedirectToAction("Index", "Home");
            } else {
                Company cc = new Company();
                var cd = cc.GetAllCompanies();
                string cdaux = cd.ToString();
                ViewData["selectComp"] = cdaux.Replace("company", "text");
                return View(new RequestImage());
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadData(string rut, string name, string lastname, string birthdate, string company, string chore, string access, string[] vehicles, string restrictions, string specrest, string[] license, string[] anglolicense, string licexpdate, IFormFile photo, string drivermail, string driverphone, string companymail, string companyphone, string wcdate, string solname) {
            string vehicleaux = string.Join(",", vehicles);
            string licenseaux = string.Join(",", license);
            string anglolicenseaux = string.Join(",", anglolicense);
            //Console.WriteLine("Lic: " + license);
            //Console.WriteLine("Veh: " + vehicles[0] + " - " + vehicles[1]);
            rut = rut.Replace(".","").ToUpper();
            name = name.ToUpper();
            lastname = lastname.ToUpper();
            drivermail = drivermail.ToUpper();
            companymail = companymail.ToUpper();
            Request raux = new Request();
            var exists = raux.GetExistentRequest(rut);
            Driver daux = new Driver();
            var dexists = daux.GetExistentDriver(rut);
            if(exists == true) {
                Console.WriteLine("Estado Rut: " + rut + " Ya Solicitado");
                return Content("EXISTS");
            } else if(dexists == true){
                Console.WriteLine("Estado Rut: " + rut + " En Masivo");
                return Content("MASIVO");
            } else if (exists==false && dexists == false) {
                CultureInfo culture = new CultureInfo("es-ES");

                Company comp = new Company();
                Company aux = comp.GetCompany(int.Parse(company));
                //2019-07-31
                DateTime birthday = DateTime.ParseExact(birthdate, "yyyy-MM-dd", culture);
                DateTime licenseexp = DateTime.ParseExact(licexpdate, "yyyy-MM-dd", culture);
                DateTime webcontrol = DateTime.ParseExact(wcdate, "yyyy-MM-dd", culture);
                string faenas = "NINGUNA";
                if (chore.Contains("lb")) {
                    faenas = "LOS BRONCES";
                }else if (chore.Contains("lt")) {
                    faenas = "LAS TORTOLAS";
                } else {
                    faenas = "LOS BRONCES/LAS TORTOLAS";
                }
                string pfname = "";
                if (photo != null) {
                    pfname = rut + Path.GetExtension(photo.FileName);
                    var memory = new MemoryStream();
                    using (var stream = new FileStream(@"/var/www/html/imgs/" + rut + Path.GetExtension(photo.FileName), FileMode.Create)) {
                        await photo.CopyToAsync(stream);
                        Console.WriteLine("Foto Subida " + rut + Path.GetExtension(photo.FileName));
                    }
                } else {
                    Console.WriteLine("Sin Foto!");
                    return Content("NOIMG");
                }

                

                if(specrest != "") {
                    Console.WriteLine("Solicitud Entrante - " + rut);
                    Request req = new Request("", rut, name, lastname, birthday, aux, faenas, access, vehicleaux, restrictions, specrest, licenseaux, anglolicenseaux, licenseexp, pfname, drivermail, driverphone, companymail, companyphone, webcontrol, solname);
                    req.AddRequestToDB();
                    Console.WriteLine("Solicitud Almacenada - " + rut);
                } else {
                    Console.WriteLine("Solicitud Entrante - " + rut);
                    Request req = new Request("", rut, name, lastname, birthday, aux, faenas, access, vehicleaux, restrictions, licenseaux, anglolicenseaux, licenseexp, pfname, drivermail, driverphone, companymail, companyphone, webcontrol, solname);
                    req.AddRequestToDB();
                    Console.WriteLine("Solicitud Almacenada - " + rut);
                }
                
            }
            Console.WriteLine("OK - DATA");
            return Content("OK");
        }


        public IActionResult Login(string Usr, string Pass) {
            DateTime dt = DateTime.Now;
            User user = new User("", Usr, Pass, dt);
            if (user.Authenticate(Usr, Pass)) {
                HttpContext.Session.SetString("Admin", "true");
                HttpContext.Session.SetString("User", Usr);
                ViewData["User"] = Usr;
                return RedirectToAction("About", "Home");
            } else {
                HttpContext.Session.SetString("Admin", "false");
                HttpContext.Session.SetString("User", "none");
                ViewData["User"] = "";
                return Content("Fail");
            }

        }

        public IActionResult Logout() {
                HttpContext.Session.SetString("Admin", "false");
                HttpContext.Session.SetString("User", "--");
                ViewData["User"] = "";
                return RedirectToAction("Index", "Home");

        }


        public IActionResult Requests() {
            Request q = new Request();
            var aux = q.GetAllRequests();
            string jsonString = JsonConvert.SerializeObject(aux, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });
            ViewData["Table1"] = jsonString;
            return View();
        }


        public IActionResult Export() {
            // Set the file name and get the output directory
            var fileName = "Export" + DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss") + ".xlsx";
            var outputDir = @"/data/";

            // Create the file using the FileInfo object
            var file = new FileInfo(outputDir + fileName);

            // Create the package and make sure you wrap it in a using statement
            using (var package = new ExcelPackage(file)) {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Para Imprimir" + DateTime.Now.ToShortDateString());
                // Add some formatting to the worksheet
                worksheet.TabColor = Color.Blue;
                worksheet.DefaultRowHeight = 12;
                worksheet.HeaderFooter.FirstFooter.LeftAlignedText = string.Format("Generated: {0}", DateTime.Now.ToShortDateString());
                worksheet.Row(1).Height = 20;
                worksheet.Row(2).Height = 18;
                // --------- Data and styling goes here -------------- //
                // Start adding the header
                // First of all the first row
                worksheet.Cells[1, 1].Value = "a";
                worksheet.Cells[1, 2].Value = "RUT";
                worksheet.Cells[1, 3].Value = "NOMBRE";
                worksheet.Cells[1, 4].Value = "APELLIDOS";
                worksheet.Cells[1, 5].Value = "FECHA DE NACIMIENTO";
                worksheet.Cells[1, 6].Value = "EMPRESA";
                worksheet.Cells[1, 7].Value = "FAENA";
                worksheet.Cells[1, 8].Value = "ACCESOS";
                worksheet.Cells[1, 9].Value = "TIPO DE VEHICULO";
                worksheet.Cells[1, 10].Value = "RESTRICCIONES";
                worksheet.Cells[1, 11].Value = "MUNICIPAL";
                worksheet.Cells[1, 12].Value = "ACREDITADA";
                worksheet.Cells[1, 13].Value = "FECHA DE VENCIMIENTO";
                worksheet.Cells[1, 14].Value = "FOTO";
                worksheet.Cells[1, 15].Value = "MAIL CONDUCTOR";
                worksheet.Cells[1, 16].Value = "FONO CONDUCTOR";
                worksheet.Cells[1, 17].Value = "FONO EMPRESA";
                worksheet.Cells[1, 18].Value = "FECHA DE ACREDITACION DE CONDUCCION";
                worksheet.Cells[1, 19].Value = "RUT CORREGIDO";
                Request auxreq = new Request();
                List<Request> reqs = auxreq.GetAllRequests();

                // Add the second row of header data
                for (int i = 0; i <= reqs.Count -1 ; i++) {
                        worksheet.Cells[i + 2, 1].Value = i + 1000;
                        worksheet.Cells[i + 2, 2].Value = reqs[i].rut;
                        worksheet.Cells[i + 2, 3].Value = reqs[i].name.ToUpper();
                        worksheet.Cells[i + 2, 4].Value = reqs[i].lastname.ToUpper();
                        worksheet.Cells[i + 2, 5].Value = reqs[i].birthdate.ToString("yyyy-MM-dd");
                        worksheet.Cells[i + 2, 6].Value = reqs[i].company.company;
                        worksheet.Cells[i + 2, 7].Value = reqs[i].chore.ToUpper();
                        worksheet.Cells[i + 2, 8].Value = reqs[i].access.ToUpper();
                        worksheet.Cells[i + 2, 9].Value = reqs[i].vehicles.ToUpper().Replace(",","-");
                        worksheet.Cells[i + 2, 10].Value = reqs[i].restrictions.ToUpper();
                        worksheet.Cells[i + 2, 11].Value = reqs[i].license.ToUpper().Replace(",", "-");
                        worksheet.Cells[i + 2, 12].Value = reqs[i].anglolicense.ToUpper().Replace(",", "-");
                        worksheet.Cells[i + 2, 13].Value = reqs[i].licexpdate.ToString("yyyy-MM-dd");
                        worksheet.Cells[i + 2, 14].Value = reqs[i].photo;
                        worksheet.Cells[i + 2, 15].Value = reqs[i].drivermail;
                        worksheet.Cells[i + 2, 16].Value = reqs[i].driverphone;
                        worksheet.Cells[i + 2, 17].Value = reqs[i].companyphone;
                        worksheet.Cells[i + 2, 18].Value = reqs[i].wcdate.ToString();
                        worksheet.Cells[i + 2, 19].Value = reqs[i].rut.Replace(".","").Replace("-","");

                    
                }
                // worksheet.Cells[2, 1].Value = "Probando";
                // worksheet.Cells[2, 2].Value = "Probando 2";

               

                // Fit the columns according to its content
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Column(9).AutoFit();
                worksheet.Column(10).AutoFit();
                worksheet.Column(11).AutoFit();
                worksheet.Column(12).AutoFit();
                worksheet.Column(13).AutoFit();
                worksheet.Column(14).AutoFit();
                worksheet.Column(15).AutoFit();
                worksheet.Column(16).AutoFit();
                worksheet.Column(17).AutoFit();
                worksheet.Column(19).AutoFit();

                // Set some document properties
                package.Workbook.Properties.Title = "Excel Impresion";
                package.Workbook.Properties.Author = "Informatica Explor-K";
                package.Workbook.Properties.Company = "Explor-K";

                // save our new workbook and we are done!
                package.Save();

                //-------- Now leaving the using statement
            } // Outside the using statement


            return View();
        }


        

        [HttpPost]
        [Route("api/License/FillForm")]
        public string FillForm([FromBody] JObject content) {
            WebClient wc = new WebClient();
            dynamic json = content;
            string rut = json.content + "";
            rut = rut.Replace(".","").Replace("-","");
            string chore = json.chore + "";
            if (rut == "") {
                return "{\"error\":\"00\"}";
            }
            Console.WriteLine("-> " + rut);
            if(chore.Contains("lt")){
                string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lt");
                if (data.Contains("No")) {
                    return "{\"error\":\"02\"}";
                } else {
                    dynamic retson = JObject.Parse(data);
                    string fn = retson.FullNames + "";
                    string ln = retson.LastNames + "";
                    string el = retson.ExpLicense + "";
                    string ad = retson.AccDate + "";
                    string lt = retson.LicenseType + "";
                    string jsontxt = "{\"FullNames\":\"" + fn + "\", \"LastNames\":\"" + ln + "\", \"ExpLicense\":\"" + el + "\", \"AccDate\":\"" + ad + "\", \"LicenseType\":\"" + lt + "\"  }";
                    return jsontxt;
                }
            }
            if (chore.Contains("lb")) {
                string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lb");
                if (data.Contains("No")) {
                    return "{\"error\":\"01\"}";
                } else {
                    dynamic retson = data;
                    string fn = retson.FullNames + "";
                    string ln = retson.LastNames + "";
                    string el = retson.ExpLicense + "";
                    string ad = retson.AccDate + "";
                    string lt = retson.LicenseType + "";
                    string jsontxt = "{\"FullNames\":\"" + fn + "\", \"LastNames\":\"" + ln + "\", \"ExpLicense\":\"" + el + "\", \"AccDate\":\""+ad+"\", \"LicenseType\":\""+lt+"\" }";
                    return jsontxt;
                }
            }

            if(chore.Contains("bt")){
                string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lb");
                if (data.Contains("No")) {
                    return "{\"error\":\"01\"}";
                } else {
                    dynamic retson = data;
                    string fn = retson.FullNames + "";
                    string ln = retson.LastNames + "";
                    string el = retson.ExpLicense + "";
                    string ad = retson.AccDate + "";
                    string lt = retson.LicenseType + "";
                    string jsontxt = "{\"FullNames\":\"" + fn + "\", \"LastNames\":\"" + ln + "\", \"ExpLicense\":\"" + el + "\", \"AccDate\":\"" + ad + "\", \"LicenseType\":\"" + lt + "\" }";
                    return jsontxt;
                }
            }
            
            return "";
        }


        //public bool CheckVehicles(string veh, string licen, string alicen) {


        //}


    }
}