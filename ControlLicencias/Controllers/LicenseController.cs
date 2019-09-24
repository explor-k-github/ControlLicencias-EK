using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ControlLicencias.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;

namespace ControlLicencias.Controllers
{
    public class LicenseController : Controller {
        public IActionResult Index() {
            string isadmin = HttpContext.Session.GetString("Admin");
            string islogged = HttpContext.Session.GetString("Logged");
            ViewData["User"] = HttpContext.Session.GetString("User");
            if (islogged != "true") {
                return RedirectToAction("Index", "Home");
            } else {
                Company cc = new Company();
                var cd = cc.GetAllCompanies();
                string cdaux = cd.ToString();
                ViewData["selectComp"] = cdaux.Replace("company", "text");
                String timeStamp = GetTimestamp();
                Console.WriteLine("TS:" + timeStamp);
                return View(new RequestImage());
            }
        }

        public IActionResult NewUser() {
            string isadmin = HttpContext.Session.GetString("Admin");
            string islogged = HttpContext.Session.GetString("Logged");
            if(isadmin == "true") {
                return View();
            } else {
                return RedirectToAction("About", "Home");
            }
            
        }




        public static String GetTimestamp() {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long unixTime = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond);
            return unixTime + "";
        }

        [HttpPost]
        public async Task<IActionResult> UploadData(string rut, string name, string lastname, string birthdate, string company, string chore, string[] access, string[] vehicles, string restrictions, string specrest, string[] license, string[] anglolicense, string licexpdate, IFormFile photo, string drivermail, string driverphone, string companymail, string companyphone, string wcdate, string solname) {
            string vehicleaux = string.Join(",", vehicles);
            string licenseaux = string.Join(",", license);
            string accesaux = string.Join(",", access);
            string anglolicenseaux = string.Join(",", anglolicense);
            string uniquerut = rut.Replace(".","").Replace("-","").Replace("K","").Replace("k","");
            int rutid = int.Parse(uniquerut);
            String timeStamp = GetTimestamp();
            int uuid = int.Parse(timeStamp) - rutid;
            Console.WriteLine("UUID:" + uuid);
            //Console.WriteLine("Lic: " + license);
            //Console.WriteLine("Veh: " + vehicles[0] + " - " + vehicles[1]);
            rut = rut.Replace(".", "").ToUpper();
            name = name.ToUpper();
            lastname = lastname.ToUpper();
            drivermail = drivermail.ToUpper();
            companymail = companymail.ToUpper();
            Request raux = new Request();
            var exists = raux.GetExistentRequest(rut);
            Driver daux = new Driver();
            var dexists = daux.GetExistentDriver(rut);


            if (dexists == true) {
                Console.WriteLine("Estado Rut: " + rut + " En Masivo");
                return Content("MASIVO");
            } else if (exists == true) {
                Console.WriteLine("Estado Rut: " + rut + " Ya Solicitado");
                return Content("EXISTS");
            }  else if (exists == false && dexists == false) {
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
                } else if (chore.Contains("lt")) {
                    faenas = "LAS TORTOLAS";
                } else {
                    faenas = "LOS BRONCES/LAS TORTOLAS";
                }
                string pfname = "";
                if (photo != null) {
                    string rutline = rut.Replace(".", "").Replace("-", "");
                    string rtrt = rutline.Substring(0, rutline.Length - 1);
                    string rtcp = rutline.Substring(rutline.Length - 1).ToUpper();
                    pfname = rtrt + "-" + rtcp + Path.GetExtension(photo.FileName);
                    var memory = new MemoryStream();
                    using (var stream = new FileStream(@"/var/www/html/imgs/" + rtrt + "-" + rtcp + Path.GetExtension(photo.FileName), FileMode.Create)) {
                        await photo.CopyToAsync(stream);
                        Console.WriteLine("Foto Subida " + rut + Path.GetExtension(photo.FileName));
                    }
                } else {
                    Console.WriteLine("Sin Foto!");
                    return Content("NOIMG");
                }

                if (specrest != "") {
                    Console.WriteLine("Solicitud Entrante - " + rut);
                    Request req = new Request("", rut, name, lastname, birthday, aux, faenas, accesaux, vehicleaux, restrictions, specrest, licenseaux, anglolicenseaux, licenseexp, pfname, drivermail, driverphone, companymail, companyphone, webcontrol, solname, ""+uuid);
                    req.AddRequestToDB();
                    Console.WriteLine("Solicitud Almacenada - " + rut);
                    Mailer ml = new Mailer();
                    ml.SendMailExternal("Solicitud Almacenada",uuid + "", rut,name + " " + lastname ,solname,companymail);
                    ml.SendMailInternal("Solicitud Ingresada", uuid + "", solname);
                } else {
                    Console.WriteLine("Solicitud Entrante - " + rut);
                    Request req = new Request("", rut, name, lastname, birthday, aux, faenas, accesaux, vehicleaux, restrictions, licenseaux, anglolicenseaux, licenseexp, pfname, drivermail, driverphone, companymail, companyphone, webcontrol, solname);
                    req.AddRequestToDB();
                    Console.WriteLine("Solicitud Almacenada - " + rut);
                }

            }
            Console.WriteLine("OK - DATA");
            return Content("OK");
        } 

        [HttpPost]
        [Route("api/License/UploadBackup")]
        public void UploadBackup([FromBody] JObject content) {
            dynamic jdata = content;
            string rut = jdata.rut + "";
            string name = jdata.name + "";
            string lastname = jdata.lastname + "";
            string birthdate = jdata.birthdate + "";
            string company = jdata.company + "";
            string chore = jdata.chore + "";
            string access = jdata.access + "";
         // string[] vehicles, string restrictions, string specrest, string[] license, string[] anglolicense, string licexpdate, IFormFile photo, string drivermail, string driverphone, string companymail, string companyphone, string wcdate, string solname
            string vehicles = jdata.vehicles + "";
            string restrictions = jdata.restrictions + "";
            string specrest = jdata.specrest + "";
            string license = jdata.license + "";
            string anglolicense = jdata.anglolicense + "";
            string licexpdate = jdata.licexpdate + "";
            string drivermail = jdata.drivermail + "";
            string driverphone = jdata.driverphone + "";
            string companymail = jdata.companymail + "";
            string companyphone = jdata.companyphone + "";
            string wcdate = jdata.wcdate + "";
            string solname = jdata.solname + "";

            var logPath = @"/var/www/html/imgs/logs/log.txt";
            using (var writer = new StreamWriter(logPath, true)) {
               
               writer.WriteLine("{\"rut\":\""+rut+"\", \"name\":\""+name+"\", \"lastname\":\""+lastname+"\", \"birthdate\":\""+birthdate+"\", \"company\":\""+company+"\", \"chore\":\""+chore+"\",\"access\":\""+access+"\",\"vehicles\":\""+vehicles+"\", \"restrictions\":\""+restrictions+"\", \"specrest\":\""+specrest+"\", \"license\":\""+license+"\", \"anglolicense\":\""+anglolicense+"\", \"licexpdate\":\""+licexpdate+"\", \"drivermail\":\""+drivermail+"\", \"companymail\":\""+companymail+"\",\"companyphone\":\""+companyphone+"\",\"wcdate\":\""+wcdate+"\",\"solname\":\""+solname+"\"}"); //or .Write(), if you wish
            }
        }
        


        [HttpPost]
        [Route("api/License/LicenseChecker")]
        public JObject LicenseChecker([FromBody] JObject content) {
            WebClient wc = new WebClient();
            dynamic jsond = content;
            string rut = jsond.rut + "";
            rut = rut.Replace(".", "").Replace("-", "");
            string chore = jsond.chore + "";
            if (!chore.Contains("bt") && !chore.Contains("lt") && !chore.Contains("lb")) {
                JObject resultsnull = new JObject();
                return resultsnull;
            } else {
                if (chore.Contains("lb")) {
                    string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lb");
                    if (data.Contains("No hay registros en la faena consultada") || data.Contains("El Rut No se encuentra")) {
                        JObject resbadlb = new JObject();
                        resbadlb.Add("Acc", "False");
                        resbadlb.Add("Lic", "False");
                        resbadlb.Add("Psi", "False");
                        // resbadlb.Add("Alt", "False");
                        // results.Add("G21", g21.ToString());
                        resbadlb.Add("Aon", "False");
                        return resbadlb;
                    }
                    JObject job = JObject.Parse(data);
                    dynamic json = job;
                    string psico = json.Psico + "";
                    string license = json.License + "";
                    //string altamon = json.HighMountain + "";
                    string aone = json.A1 + "";
                    //string g2145 = json.G21G245 + "";
                    string lifep = json.LifePaper + "";
                    Console.WriteLine(">>>" + aone);

                    //Date licen
                    string explic = "01-01-1999";
                    string explicense = json.ExpLicense + "";
                    if (explicense != "" && string.IsNullOrWhiteSpace(explicense) == false && license.Contains("rue")) {
                        string exli = explicense;
                        explic = buildDate(exli);
                    } else if (json.LicenseType != "" && license.Contains("rue")) {
                        string exli = json.LicenseType + "";
                        exli = exli.Replace("&nbsp;", "");
                        explic = buildDate(exli);
                    }

                    //Date psico
                    string exppsi = "01-01-1999";
                    string exppsico = json.ExpPsico + "";
                    if (exppsico != "" && string.IsNullOrWhiteSpace(exppsico) == false && psico.Contains("rue")) {
                        string exsi = exppsico;
                        exppsi = buildDate(exsi);
                    }

                    //Date g21
                    /*string expg21 = "01-01-1999";
                    string expg21g245 = json.ExpG21G245 + "";
                    if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                        string eg21 = json.ExpG21G245 + "";
                        expg21 = buildDate(eg21);
                    }*/

                    //Date acc
                    string accdate = "01-01-1999";
                    string accdated = json.AccDate + "";
                    if (accdated != "" && string.IsNullOrWhiteSpace(accdated) == false) {
                        string adat = json.AccDate + "";
                    }

                    string format = "dd-MM-yyyy";
                    var ldt = DateTime.ParseExact(explic, format, new CultureInfo("es-CL"));
                    var pdt = DateTime.ParseExact(exppsi, format, new CultureInfo("es-CL"));
                    //var adt = DateTime.ParseExact(expalt, format, new CultureInfo("es-CL"));
                    //var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acd = DateTime.ParseExact(accdate, format, new CultureInfo("es-CL"));

                    //Validators
                    bool lic = false;
                    bool psi = false;
                    // bool alt = false;
                    // bool g21 = false;
                    bool acc = false;
                    bool aun = false;

                    /*if (DateTime.Now.AddMonths(-6) > acd) {
                        acc = true;
                        g21 = true;
                    } else {
                        acc = false;
                        if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }
                    }*/

                    if (DateTime.Today < ldt) {
                        lic = true;
                    } else {
                        lic = false;
                    }

                    if (DateTime.Today < pdt) {
                        psi = true;
                    } else {
                        psi = false;
                    }

                    if (aone.Contains("True")||aone.Contains("true")) {
                        aun = true;
                    } else {
                        aun = false;
                    }

                    /*if (altamon.Contains("True")||altamon.Contains("true")) {
                        alt = true;
                    } else {
                        alt = false;
                    }*/

                    Console.WriteLine("Acc: " + acc.ToString());
                    Console.WriteLine("Lic: " + lic.ToString());
                    Console.WriteLine("Psi: " + psi.ToString());
                    // Console.WriteLine("Alt: " + alt.ToString());
                    // Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aun.ToString());
                    JObject results = new JObject();
                    results.Add("Acc", acc.ToString());
                    results.Add("Lic", lic.ToString());
                    results.Add("Psi", psi.ToString());
                    // results.Add("Alt", alt.ToString());
                    // results.Add("G21", g21.ToString());
                    results.Add("Aon", aun.ToString());
                    return results;
                }
                if (chore.Contains("lt")) {
                    //LT DOESNT HAVE G21
                    string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lt");
                    if(data.Contains("No hay registros en la faena consultada")||data.Contains("El Rut No se encuentra")) {
                        JObject resbadlt = new JObject();
                        resbadlt.Add("Acc", "False");
                        resbadlt.Add("Lic", "False");
                        resbadlt.Add("Psi", "False");
                        resbadlt.Add("Alt", "False");
                        // results.Add("G21", g21.ToString());
                        resbadlt.Add("Aon", "False");
                        return resbadlt;
                    }
                    JObject job = JObject.Parse(data);
                    
                    dynamic json = job;
                    string psico = json.Psico + "";
                    string license = json.License + "";
                    string altamon = json.HighMountain + "";
                    string aone = json.A1 + "";
                    // string g2145 = json.G21G245 + "";
                    string lifep = json.LifePaper + "";
                    Console.WriteLine(">>>" + aone);

                    //Date licen
                    string explic = "01-01-1999";
                    string explicense = json.ExpLicense + "";
                    if (explicense != "" && string.IsNullOrWhiteSpace(explicense) == false && license.Contains("rue")) {
                        string exli = explicense;
                        explic = buildDate(exli);
                    } else if (json.LicenseType != "" && license.Contains("rue")) {
                        string exli = json.LicenseType + "";
                        exli = exli.Replace("&nbsp;", "");
                        explic = buildDate(exli);
                    }

                    //Date psico
                    string exppsi = "01-01-1999";
                    string exppsico = json.ExpPsico + "";
                    if (exppsico != "" && string.IsNullOrWhiteSpace(exppsico) == false && psico.Contains("rue")) {
                        string exsi = exppsico;
                        exppsi = buildDate(exsi);
                    }

                    //Date g21
                    /* string expg21 = "01-01-1999";
                     string expg21g245 = json.ExpG21G245 + "";
                     if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                         string eg21 = json.ExpG21G245 + "";
                         expg21 = buildDate(eg21);
                     }*/

                    //Date acc
                    string accdate = "01-01-1999";
                    string accdated = json.AccDate + "";
                    if (accdated != "" && string.IsNullOrWhiteSpace(accdated) == false) {
                        string adat = json.AccDate + "";
                    }

                    string format = "dd-MM-yyyy";
                    var ldt = DateTime.ParseExact(explic, format, new CultureInfo("es-CL"));
                    var pdt = DateTime.ParseExact(exppsi, format, new CultureInfo("es-CL"));
                    //var adt = DateTime.ParseExact(expalt, format, new CultureInfo("es-CL"));
                    //var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acd = DateTime.ParseExact(accdate, format, new CultureInfo("es-CL"));

                    //Validators
                    bool lic = false;
                    bool psi = false;
                    // bool alt = false;
                    // bool g21 = false;
                    bool acc = false;
                    bool aun = false;
                    string expl = "";
                    string expp = "";
                    string expa = "";

                    if (DateTime.Now.AddMonths(-6) > acd) {
                        acc = true;
                        // g21 = true;
                    } else {
                        acc = false;
                        /*if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }*/
                    }

                    if (DateTime.Today < ldt) {
                        lic = true;
                    } else {
                        lic = false;
                    }

                    if (DateTime.Today < pdt) {
                        psi = true;
                    } else {
                        psi = false;
                    }

                    if (aone.Contains("True")) {
                        aun = true;
                    } else {
                        aun = false;
                    }

                    /*if (altamon.Contains("True")) {
                        alt = true;
                    } else {
                        alt = false;
                    }*/

                    Console.WriteLine("Acc: " + acc.ToString());
                    Console.WriteLine("Lic: " + lic.ToString());
                    Console.WriteLine("Psi: " + psi.ToString());
                    // Console.WriteLine("Alt: " + alt.ToString());
                    //Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aun.ToString());
                    JObject results = new JObject();
                    results.Add("Acc", acc.ToString());
                    results.Add("Lic", lic.ToString());
                    results.Add("Psi", psi.ToString());
                    // results.Add("Alt", alt.ToString());
                    // results.Add("G21", g21.ToString());
                    results.Add("Aon", aun.ToString());
                    return results;
                }
                if (chore.Contains("bt")) {
                    JObject results = new JObject();
                    //JObject resbad = new JObject();
                    string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lb");
                    if (data.Contains("No hay registros en la faena consultada") || data.Contains("El Rut No se encuentra")) {
                        
                        results.Add("Acc", "False");
                        results.Add("Lic", "False");
                        results.Add("Psi", "False");
                        // results.Add("Alt", "False");
                        // results.Add("G21", g21.ToString());
                        results.Add("Aon", "False");
                        goto lastortolas;
                    }
                    JObject job = JObject.Parse(data);
                    dynamic json = job;
                    string psico = json.Psico + "";
                    string license = json.License + "";
                    // string altamon = json.HighMountain + "";
                    string aone = json.A1 + "";
                    // string g2145 = json.G21G245 + "";
                    string lifep = json.LifePaper + "";
                    Console.WriteLine(">>>" + aone);

                    //Date licen
                    string explic = "01-01-1999";
                    string explicense = json.ExpLicense + "";
                    if (explicense != "" && string.IsNullOrWhiteSpace(explicense) == false && license.Contains("rue")) {
                        string exli = explicense;
                        explic = buildDate(exli);
                    } else if (json.LicenseType != "" && license.Contains("rue")) {
                        string exli = json.LicenseType + "";
                        exli = exli.Replace("&nbsp;", "");
                        explic = buildDate(exli);
                    }

                    //Date psico
                    string exppsi = "01-01-1999";
                    string exppsico = json.ExpPsico + "";
                    if (exppsico != "" && string.IsNullOrWhiteSpace(exppsico) == false && psico.Contains("rue")) {
                        string exsi = exppsico;
                        exppsi = buildDate(exsi);
                    }

                    //Date g21
                    /*string expg21 = "01-01-1999";
                    string expg21g245 = json.ExpG21G245 + "";
                    if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                        string eg21 = json.ExpG21G245 + "";
                        expg21 = buildDate(eg21);
                    }*/

                    //Date acc
                    string accdate = "01-01-1999";
                    string accdated = json.AccDate + "";
                    if (accdated != "" && string.IsNullOrWhiteSpace(accdated) == false) {
                        string adat = json.AccDate + "";
                    }

                    string format = "dd-MM-yyyy";
                    var ldt = DateTime.ParseExact(explic, format, new CultureInfo("es-CL"));
                    var pdt = DateTime.ParseExact(exppsi, format, new CultureInfo("es-CL"));
                    //var adt = DateTime.ParseExact(expalt, format, new CultureInfo("es-CL"));
                    //var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acd = DateTime.ParseExact(accdate, format, new CultureInfo("es-CL"));

                    //Validators
                    bool lic = false;
                    bool psi = false;
                    //bool alt = false;
                    //bool g21 = false;
                    bool acc = false;
                    bool aun = false;

                    /*if (DateTime.Now.AddMonths(-6) > acd) {
                        acc = true;
                        g21 = true;
                    } else {
                        acc = false;
                        if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }
                    }*/

                    if (DateTime.Today < ldt) {
                        lic = true;
                    } else {
                        lic = false;
                    }

                    if (DateTime.Today < pdt) {
                        psi = true;
                    } else {
                        psi = false;
                    }

                    if (aone.Contains("True") || aone.Contains("true")) {
                        aun = true;
                    } else {
                        aun = false;
                    }

                    /*if (altamon.Contains("True") || altamon.Contains("true")) {
                        alt = true;
                    } else {
                        alt = false;
                    }*/

                    Console.WriteLine("Acc: " + acc.ToString());
                    Console.WriteLine("Lic: " + lic.ToString());
                    Console.WriteLine("Psi: " + psi.ToString());
                    //Console.WriteLine("Alt: " + alt.ToString());
                    //Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aun.ToString());
                    
                    results.Add("Acc", acc.ToString());
                    results.Add("Lic", lic.ToString());
                    results.Add("Psi", psi.ToString());
                    // results.Add("Alt", alt.ToString());
                    // results.Add("G21", g21.ToString());
                    results.Add("Aon", aun.ToString());
                    //CHECK LT
                    //LT DOESNT HAVE G21
                   lastortolas:
                    string datalt = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lt");
                    if (datalt.Contains("No hay registros en la faena consultada") || data.Contains("El Rut No se encuentra")) {
                        results.Add("Acclt", "False");
                        results.Add("Liclt", "False");
                        results.Add("Psilt", "False");
                        results.Add("Altlt", "False");
                        // results.Add("G21", g21.ToString());
                        results.Add("Aonlt", "False");
                        return results;
                    }
                    JObject joblt = JObject.Parse(data);
                    dynamic jsonlt = joblt;
                    string psicolt = jsonlt.Psico + "";
                    string licenselt = jsonlt.License + "";
                    string altamonlt = jsonlt.HighMountain + "";
                    string aonelt = jsonlt.A1 + "";
                    // string g2145 = json.G21G245 + "";
                    string lifeplt = jsonlt.LifePaper + "";
                    //Console.WriteLine(">>>" + aone);

                    //Date licen
                    string expliclt = "01-01-1999";
                    string explicenselt = jsonlt.ExpLicense + "";
                    if (explicenselt != "" && string.IsNullOrWhiteSpace(explicenselt) == false && licenselt.Contains("rue")) {
                        string exlilt = explicenselt;
                        expliclt = buildDate(exlilt);
                    } else if (jsonlt.LicenseType != "" && licenselt.Contains("rue")) {
                        string exlilt = jsonlt.LicenseType + "";
                        exlilt = exlilt.Replace("&nbsp;", "");
                        explic = buildDate(exlilt);
                    }

                    //Date psico
                    string exppsilt = "01-01-1999";
                    string exppsicolt = jsonlt.ExpPsico + "";
                    if (exppsicolt != "" && string.IsNullOrWhiteSpace(exppsicolt) == false && psicolt.Contains("rue")) {
                        string exsilt = exppsicolt;
                        exppsilt = buildDate(exsilt);
                    }

                    //Date g21
                    /* string expg21 = "01-01-1999";
                     string expg21g245 = json.ExpG21G245 + "";
                     if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                         string eg21 = json.ExpG21G245 + "";
                         expg21 = buildDate(eg21);
                     }*/

                    //Date acc
                    string accdatelt = "01-01-1999";
                    string accdatedlt = jsonlt.AccDate + "";
                    if (accdatedlt != "" && string.IsNullOrWhiteSpace(accdatedlt) == false) {
                        string adatlt = jsonlt.AccDate + "";
                    }

                    string formatlt = "dd-MM-yyyy";
                    var ldtlt = DateTime.ParseExact(expliclt, formatlt, new CultureInfo("es-CL"));
                    var pdtlt = DateTime.ParseExact(exppsilt, formatlt, new CultureInfo("es-CL"));
                    //var adt = DateTime.ParseExact(expalt, format, new CultureInfo("es-CL"));
                    //var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acdlt = DateTime.ParseExact(accdatelt, formatlt, new CultureInfo("es-CL"));

                    //Validators
                    bool liclt = false;
                    bool psilt = false;
                    // bool altlt = false;
                    // bool g21 = false;
                    bool acclt = false;
                    bool aunlt = false;

                    if (DateTime.Now.AddMonths(-6) > acdlt) {
                        acclt = true;
                        // g21 = true;
                    } else {
                        acclt = false;
                        /*if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }*/
                    }

                    if (DateTime.Today < ldtlt) {
                        liclt = true;
                    } else {
                        liclt = false;
                    }

                    if (DateTime.Today < pdtlt) {
                        psilt = true;
                    } else {
                        psilt = false;
                    }

                    if (aonelt.Contains("True")) {
                        aunlt = true;
                    } else {
                        aunlt = false;
                    }

                    /*if (altamonlt.Contains("True")) {
                        altlt = true;
                    } else {
                        altlt = false;
                    }*/

                    Console.WriteLine("Acc: " + acclt.ToString());
                    Console.WriteLine("Lic: " + liclt.ToString());
                    Console.WriteLine("Psi: " + psilt.ToString());
                    // Console.WriteLine("Alt: " + alt.ToString());
                    //Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aunlt.ToString());
                    results.Add("Acclt", acclt.ToString());
                    results.Add("Liclt", liclt.ToString());
                    results.Add("Psilt", psilt.ToString());
                    // results.Add("Altlt", altlt.ToString());
                    // results.Add("G21", g21.ToString());
                    results.Add("Aonlt", aunlt.ToString());

                    return results;
                }
            }
            JObject resultsnone = new JObject();
            return resultsnone;
        }


        public IActionResult Login(string Usr, string Pass) {
            DateTime dt = DateTime.Now;
            User user = new User("", Usr, Pass, "", dt);
            
            if (user.Authenticate(Usr, Pass)) {
                if(user.type == "1") {
                    HttpContext.Session.SetString("Admin", "true");
                }
                HttpContext.Session.SetString("Logged","true");
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


        public JObject Create(string Usr) {
            string isadmin = HttpContext.Session.GetString("Admin");
            string islogged = HttpContext.Session.GetString("Logged");
            if(isadmin == "true") {
                DateTime dt = DateTime.Now;
                if (Usr.Contains("@")) {
                    string password = Usr.Split('@')[0];
                    string hash = "";
                    using (var sha1 = new SHA1Managed()) {
                        hash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
                    }
                    User user = new User("", Usr, hash, "0", dt);
                    if (user.CheckIfExists()) {
                        return JObject.Parse("{ \"error\":\"El usuario ya existe.\" }");
                    } else {
                        bool stat = user.CreateUser(user);
                        if (stat == true) {
                            return JObject.Parse("{ \"usr\":\"" + Usr + "\", \"pass\":\"" + password + "\" }");
                        } else {
                            return JObject.Parse("{ \"error\":\"No se ha podido crear un usuario.\" }");
                        }
                    }
                } else {
                    return JObject.Parse("{ \"error\":\"El usuario debe ser un correo electrónico.\" }");
                }
            } else {
                return JObject.Parse("{ \"error\":\"Debes estar logueado como administrador para generar usuarios.\" }");
            }
            
            

        }

        public IActionResult Logout() {
            HttpContext.Session.SetString("Logged", "false");
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
            var outputDir = @"/var/www/html/imgs/data/";

            // Create the file using the FileInfo object
            var file = new FileInfo(outputDir + fileName);
            ViewData["File"] = "http://35.199.124.107/imgs/data/" + fileName;
            // Create the package and make sure you wrap it in a using statement
            using (var package = new ExcelPackage(file)) {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Hoja1");
                // Add some formatting to the worksheet
                //worksheet.TabColor = Color.Blue;
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
                List<Request> reqs = auxreq.GetInRequests();
                ViewData["Regs"] = reqs.Count;
                // Add the second row of header data
                for (int i = 0; i <= reqs.Count -1 ; i++) {
                        worksheet.Cells[i + 2, 1].Value = i + 1000;
                    string rutline = reqs[i].rut.Replace(".","").Replace("-","");
                    string rtrt = rutline.Substring(0, rutline.Length-1);
                    string rtcp = rutline.Substring(rutline.Length -1).ToUpper();
                    worksheet.Cells[i + 2, 2].Value = rtrt + "-" + rtcp;
                        worksheet.Cells[i + 2, 3].Value = reqs[i].name.ToUpper();
                        worksheet.Cells[i + 2, 4].Value = reqs[i].lastname.ToUpper();
                        worksheet.Cells[i + 2, 5].Value = reqs[i].birthdate.ToString("dd-MM-yyyy");
                        worksheet.Cells[i + 2, 6].Value = reqs[i].company.company;
                        worksheet.Cells[i + 2, 7].Value = reqs[i].chore.ToUpper();
                        worksheet.Cells[i + 2, 8].Value = reqs[i].access.ToUpper();
                        worksheet.Cells[i + 2, 9].Value = reqs[i].vehicles.ToUpper().Replace(",","-");
                        worksheet.Cells[i + 2, 10].Value = reqs[i].restrictions.ToUpper();
                        worksheet.Cells[i + 2, 11].Value = reqs[i].license.ToUpper().Replace(",", "-");
                        worksheet.Cells[i + 2, 12].Value = reqs[i].anglolicense.ToUpper().Replace(",", "-");
                        worksheet.Cells[i + 2, 13].Value = reqs[i].licexpdate.ToString("dd-MM-yyyy");
                        worksheet.Cells[i + 2, 14].Value = rtrt + "-" + rtcp;
                    worksheet.Cells[i + 2, 15].Value = reqs[i].drivermail;
                        worksheet.Cells[i + 2, 16].Value = reqs[i].driverphone;
                        worksheet.Cells[i + 2, 17].Value = reqs[i].companyphone;
                        worksheet.Cells[i + 2, 18].Value = reqs[i].wcdate.ToString("dd-MM-yyyy");
                        worksheet.Cells[i + 2, 19].Value = reqs[i].rut.Replace(".","").Replace("-","");
                }
                // worksheet.Cells[2, 1].Value = "Probando";
                // worksheet.Cells[2, 2].Value = "Probando 2";
                // Fit the columns according to its content
                /*worksheet.Column(1).AutoFit();
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
                worksheet.Column(19).AutoFit();*/

                // Set some document properties
                package.Workbook.Properties.Title = "Excel Impresion";
                package.Workbook.Properties.Author = "Informatica Explor-K";
                package.Workbook.Properties.Company = "Explor-K";

                // save our new workbook and we are done!
                package.Save();
                auxreq.MarkRequests();
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
            // Console.WriteLine(LicenseChecker(rut).ToString());
            if (chore.Contains("lt")){
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
                    dynamic retson = JObject.Parse(data);
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
                    dynamic retson = JObject.Parse(data);
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


        private string changeDate(string date){
            string month = date.Split('-')[1];
            switch (month)
            {
                case "ene":
                    return "01";
                case "feb":
                    return "02";
                case "mar":
                    return "03";
                case "abr":
                    return "04";
                case "may":
                    return "05";
                case "jun":
                    return "06";
                case "jul":
                    return "07";
                case "ago":
                    return "08";
                case "sep":
                    return "09";
                case "oct":
                    return "10";
                case "nov":
                    return "11";
                case "dic":
                    return "12";
                case "Ene":
                    return "01";
                case "Feb":
                    return "02";
                case "Mar":
                    return "03";
                case "Abr":
                    return "04";
                case "May":
                    return "05";
                case "Jun":
                    return "06";
                case "Jul":
                    return "07";
                case "Ago":
                    return "08";
                case "Sep":
                    return "09";
                case "Oct":
                    return "10";
                case "Nov":
                    return "11";
                case "Dic":
                    return "12";
                default: return "";
                    //break;
            }
        }

        private string buildDate(string date) {
            string day = date.Split('-')[0];
            string month = changeDate(date);
            string year = date.Split('-')[2];
            return day + "-" + month + "-" + year;
        }

    }
}