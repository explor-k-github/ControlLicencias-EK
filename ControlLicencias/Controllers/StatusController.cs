using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ControlLicencias.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ControlLicencias.Controllers{
    public class StatusController : Controller{
        public IActionResult Index(){
            return View();
        }


        public IActionResult Search(string Ticket) {
            DateTime dt = DateTime.Now;
            Request req = new Request();
            Request rpx = req.GetRequestByTicket(Ticket);
            if (string.IsNullOrEmpty(rpx.rut) == true || rpx.rut == null) {
                Console.WriteLine("----!!!!----");
                return RedirectToAction("Index");
            } else {
                Console.WriteLine("LN: " + rpx.lastname);
                string rutline = rpx.rut;
                string rtrt = rutline.Substring(0, rutline.Length - 1);
                string rtcp = rutline.Substring(rutline.Length - 1).ToUpper();
                ViewData["Data"] = "http://35.199.124.107/license/lic.php?r=" + rtrt + "-" + rtcp + "&n=" + rpx.name + "&l=" + rpx.lastname + "&c=--&e=EC";
                string status = rpx.GetStatusByTicket(Ticket);
                ViewData["Solname"] = rpx.solname;
                ViewData["Status"] = status;
                ViewData["Ticket"] = rpx.ticket;
                ViewData["rut"] = rpx.rut.Replace("-", "");



                if (rpx.chore.Contains("S/L")) {
                    ViewData["reswc"] = LicenseChecker(rpx.rut.Replace("-", ""), "bt");
                    ViewData["chr"] = "bt";
                }
                if (rpx.chore.Contains("TORTOLAS") && !rpx.chore.Contains("BRONCES")) {
                    ViewData["reswc"] = LicenseChecker(rpx.rut.Replace("-", ""), "lt");
                    ViewData["chr"] = "lt";
                }
                if (rpx.chore.Contains("BRONCES") && !rpx.chore.Contains("TORTOLAS")) {
                    ViewData["reswc"] = LicenseChecker(rpx.rut.Replace("-", ""), "lb");
                    ViewData["chr"] = "lb";
                }

                switch (status) {
                    case "Ingresado":
                        ViewData["t1"] = "active";
                        ViewData["Info"] = "Su licencia se encuentra en el listado de solicitudes pendientes, en caso de que la solicitud haya superado la espera de 48 Horas, favor contactarse por mail a licencias@explor-k.cl indicando el número de ticket";
                        break;
                    case "En Proceso":
                        ViewData["t1"] = "active";
                        ViewData["t2"] = "active";
                        ViewData["Info"] = "Su licencia se encuentra en proceso de emisión y ha sido asignada al personal de Emisión de Licencias.";
                        break;
                    case "Impresion":
                        ViewData["t1"] = "active";
                        ViewData["t2"] = "active";
                        ViewData["t3"] = "active";
                        ViewData["Info"] = "Su licencia se encuentra en la cola para impresión en la oficina de Explor-K, debera recibir un correo en las próximas horas indicando que puede retirar la licencia.";
                        break;
                    case "Retiro":
                        ViewData["t1"] = "active";
                        ViewData["t2"] = "active";
                        ViewData["t3"] = "active";
                        ViewData["t4"] = "active";
                        ViewData["Info"] = "Su licencia está lista para retiro, si no ha recibido el correo favor contactarse a licencias@explor-k.cl indicando el número de ticket.";
                        break;
                    case "Lista":
                        ViewData["t1"] = "active";
                        ViewData["t2"] = "active";
                        ViewData["t3"] = "active";
                        ViewData["t4"] = "active";
                        ViewData["t5"] = "active";
                        ViewData["Info"] = "Su licencia está activa y con estado correcto en WebControl";
                        break;
                }
            }
            
            //User user = new User("", Usr, Pass, dt);
            /*if (user.Authenticate(Usr, Pass)) {
                HttpContext.Session.SetString("Admin", "true");
                HttpContext.Session.SetString("User", Usr);
                ViewData["User"] = Usr;
                return RedirectToAction("About", "Home");
            } else {
                HttpContext.Session.SetString("Admin", "false");
                HttpContext.Session.SetString("User", "none");
                ViewData["User"] = "";
                return Content("Fail");
            }*/
            return View();
        }


        public JObject LicenseChecker(string rut, string chr) {
            WebClient wc = new WebClient();
            rut = rut.Replace(".", "").Replace("-", "");
            string chore = chr;
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
                        resbadlb.Add("Alt", "False");
                        // results.Add("G21", g21.ToString());
                        resbadlb.Add("Aon", "False");
                        return resbadlb;
                    }
                    JObject job = JObject.Parse(data);
                    dynamic json = job;
                    string psico = json.Psico + "";
                    string license = json.License + "";
                    string altamon = json.HighMountain + "";
                    string aone = json.A1 + "";
                    string g2145 = json.G21G245 + "";
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
                    string expg21 = "01-01-1999";
                    string expg21g245 = json.ExpG21G245 + "";
                    if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                        string eg21 = json.ExpG21G245 + "";
                        expg21 = buildDate(eg21);
                    }

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
                    var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acd = DateTime.ParseExact(accdate, format, new CultureInfo("es-CL"));

                    //Validators
                    bool lic = false;
                    bool psi = false;
                    bool alt = false;
                    bool g21 = false;
                    bool acc = false;
                    bool aun = false;

                    if (DateTime.Now.AddMonths(-6) > acd) {
                        acc = true;
                        g21 = true;
                    } else {
                        acc = false;
                        if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }
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

                    if (aone.Contains("True") || aone.Contains("true")) {
                        aun = true;
                    } else {
                        aun = false;
                    }

                    if (altamon.Contains("True") || altamon.Contains("true")) {
                        alt = true;
                    } else {
                        alt = false;
                    }

                    Console.WriteLine("Acc: " + acc.ToString());
                    Console.WriteLine("Lic: " + lic.ToString());
                    Console.WriteLine("Psi: " + psi.ToString());
                    Console.WriteLine("Alt: " + alt.ToString());
                    Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aun.ToString());
                    JObject results = new JObject();
                    results.Add("Acc", acc.ToString());
                    results.Add("Lic", lic.ToString());
                    results.Add("Psi", psi.ToString());
                    results.Add("Alt", alt.ToString());
                    results.Add("G21", g21.ToString());
                    results.Add("Aon", aun.ToString());
                    return results;
                }
                if (chore.Contains("lt")) {
                    //LT DOESNT HAVE G21
                    string data = wc.DownloadString("http://35.199.124.107:3000/api/rut/" + rut + "/lt");
                    if (data.Contains("No hay registros en la faena consultada") || data.Contains("El Rut No se encuentra")) {
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
                    bool alt = false;
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

                    if (altamon.Contains("True")) {
                        alt = true;
                    } else {
                        alt = false;
                    }

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
                    results.Add("Alt", alt.ToString());
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
                        results.Add("Alt", "False");
                        // results.Add("G21", g21.ToString());
                        results.Add("Aon", "False");
                        goto lastortolas;
                    }
                    JObject job = JObject.Parse(data);
                    dynamic json = job;
                    string psico = json.Psico + "";
                    string license = json.License + "";
                    string altamon = json.HighMountain + "";
                    string aone = json.A1 + "";
                    string g2145 = json.G21G245 + "";
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
                    string expg21 = "01-01-1999";
                    string expg21g245 = json.ExpG21G245 + "";
                    if (expg21g245 != "" && string.IsNullOrWhiteSpace(expg21g245) == false && g2145.Contains("rue")) {
                        string eg21 = json.ExpG21G245 + "";
                        expg21 = buildDate(eg21);
                    }

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
                    var gdt = DateTime.ParseExact(expg21, format, new CultureInfo("es-CL"));
                    var acd = DateTime.ParseExact(accdate, format, new CultureInfo("es-CL"));

                    //Validators
                    bool lic = false;
                    bool psi = false;
                    bool alt = false;
                    bool g21 = false;
                    bool acc = false;
                    bool aun = false;

                    if (DateTime.Now.AddMonths(-6) > acd) {
                        acc = true;
                        g21 = true;
                    } else {
                        acc = false;
                        if (DateTime.Today < gdt) {
                            g21 = true;
                        } else {
                            g21 = false;
                        }
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

                    if (aone.Contains("True") || aone.Contains("true")) {
                        aun = true;
                    } else {
                        aun = false;
                    }

                    if (altamon.Contains("True") || altamon.Contains("true")) {
                        alt = true;
                    } else {
                        alt = false;
                    }

                    Console.WriteLine("Acc: " + acc.ToString());
                    Console.WriteLine("Lic: " + lic.ToString());
                    Console.WriteLine("Psi: " + psi.ToString());
                    Console.WriteLine("Alt: " + alt.ToString());
                    Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aun.ToString());

                    results.Add("Acc", acc.ToString());
                    results.Add("Lic", lic.ToString());
                    results.Add("Psi", psi.ToString());
                    results.Add("Alt", alt.ToString());
                    results.Add("G21", g21.ToString());
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
                    bool altlt = false;
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

                    if (altamonlt.Contains("True")) {
                        altlt = true;
                    } else {
                        altlt = false;
                    }

                    Console.WriteLine("Acc: " + acclt.ToString());
                    Console.WriteLine("Lic: " + liclt.ToString());
                    Console.WriteLine("Psi: " + psilt.ToString());
                    // Console.WriteLine("Alt: " + alt.ToString());
                    //Console.WriteLine("G21: " + g21.ToString());
                    Console.WriteLine("A1: " + aunlt.ToString());
                    results.Add("Acclt", acclt.ToString());
                    results.Add("Liclt", liclt.ToString());
                    results.Add("Psilt", psilt.ToString());
                    results.Add("Altlt", altlt.ToString());
                    // results.Add("G21", g21.ToString());
                    results.Add("Aonlt", aunlt.ToString());

                    return results;
                }
            }
            JObject resultsnone = new JObject();
            return resultsnone;
        }


        private string changeDate(string date) {
            string month = date.Split('-')[1];
            switch (month) {
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