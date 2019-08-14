using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class Request {
        public string id;
        public string rut;
        public string name;
        public string lastname;
        public DateTime birthdate;
        public Company company;
        public string chore;
        public string access;
        public string vehicles;
        public string restrictions;
        public string specrest;
        public string license;
        public string anglolicense;
        public DateTime licexpdate;
        public string photo;
        public string drivermail;
        public string driverphone;
        public string companymail;
        public string companyphone;
        public DateTime wcdate;
        public string solname;
        public string ticket;
        public DateTime datereq;

        public Request(string id, string rut, string name, string lastname, DateTime birthdate, Company company, string chore, string access, string vehicles, string restrictions, string specrest, string license, string anglolicense, DateTime licexpdate, string photo, string drivermail, string driverphone, string companymail, string companyphone, DateTime wcdate, string solname, string ticket) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.rut = rut ?? throw new ArgumentNullException(nameof(rut));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
            this.birthdate = birthdate;
            this.company = company ?? throw new ArgumentNullException(nameof(company));
            this.chore = chore ?? throw new ArgumentNullException(nameof(chore));
            this.access = access ?? throw new ArgumentNullException(nameof(access));
            this.vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
            this.restrictions = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
            this.specrest = specrest ?? throw new ArgumentNullException(nameof(specrest));
            this.license = license ?? throw new ArgumentNullException(nameof(license));
            this.anglolicense = anglolicense ?? throw new ArgumentNullException(nameof(anglolicense));
            this.licexpdate = licexpdate;
            this.photo = photo ?? throw new ArgumentNullException(nameof(photo));
            this.drivermail = drivermail ?? throw new ArgumentNullException(nameof(drivermail));
            this.driverphone = driverphone ?? throw new ArgumentNullException(nameof(driverphone));
            this.companymail = companymail ?? throw new ArgumentNullException(nameof(companymail));
            this.companyphone = companyphone ?? throw new ArgumentNullException(nameof(companyphone));
            this.wcdate = wcdate;
            this.ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            this.solname = solname ?? throw new ArgumentNullException(nameof(solname));
        }

        public Request(string id, string rut, string name, string lastname, DateTime birthdate, Company company, string chore, string access, string vehicles, string restrictions, string license, string anglolicense, DateTime licexpdate, string photo, string drivermail, string driverphone, string companymail, string companyphone, DateTime wcdate, string solname) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.rut = rut ?? throw new ArgumentNullException(nameof(rut));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
            this.birthdate = birthdate;
            this.company = company ?? throw new ArgumentNullException(nameof(company));
            this.chore = chore ?? throw new ArgumentNullException(nameof(chore));
            this.access = access ?? throw new ArgumentNullException(nameof(access));
            this.vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
            this.restrictions = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
            this.specrest = " NA ";
            this.license = license ?? throw new ArgumentNullException(nameof(license));
            this.anglolicense = anglolicense ?? throw new ArgumentNullException(nameof(anglolicense));
            this.licexpdate = licexpdate;
            this.photo = photo ?? throw new ArgumentNullException(nameof(photo));
            this.drivermail = drivermail ?? throw new ArgumentNullException(nameof(drivermail));
            this.driverphone = driverphone ?? throw new ArgumentNullException(nameof(driverphone));
            this.companymail = companymail ?? throw new ArgumentNullException(nameof(companymail));
            this.companyphone = companyphone ?? throw new ArgumentNullException(nameof(companyphone));
            this.wcdate = wcdate;
            this.solname = solname ?? throw new ArgumentNullException(nameof(solname));
        }

        public Request() {

        }

        public bool AddRequestToDB() {
            

            Database db = new Database();
            string birth = birthdate.ToString("yyyy-MM-dd");
            string exp = licexpdate.ToString("yyyy-MM-dd");
            string wc = wcdate.ToString("yyyy-MM-dd");
            string query = "INSERT INTO `requests` (`id`, `rut`, `name`, `lastname`, `birthdate`, `company`, `chore`, `access`, `vehicles`, `restrictions`, `specrest`, `license`, `anglolicense`, `licexpdate`, `photo`, `drivermail`, `driverphone`, `companymail`, `companyphone`, `wcdate`, `solname`, `status`, `ticket` ) VALUES (NULL, '"+this.rut+"', '"+this.name+"', '"+this.lastname+"', '"+birth+"', '"+this.company.id+"', '"+this.chore+"', '"+this.access+"', '"+this.vehicles+"', '"+this.restrictions+"','" + this.specrest + "', '"+this.license+"', '"+this.anglolicense+"', '"+exp+"', '"+this.photo+"', '"+this.drivermail+"', '"+this.driverphone+"', '"+this.companymail+"', '"+this.companyphone+"', '"+wc+"', '"+this.solname+"', 'Ingresado', '"+this.ticket+"')";
            bool status = db.Modify(query);
            db.CloseConnection();
            return status;
        }

        //214*116 BAD
        //512 min.

        public bool GetExistentRequest(string rut) {
            Database db = new Database();
            string query = "SELECT * FROM `requests` WHERE rut='"+rut+"';";
            var xd = db.Select(query);
            Request caux = new Request();
            while (xd.Read()) {
                try {
                    Company cx = new Company();
                    cx.GetCompany(int.Parse(xd.GetString(5)));
                    caux = new Request(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetString(3), xd.GetDateTime(4),cx, xd.GetString(6), xd.GetString(7), xd.GetString(8), xd.GetString(9), xd.GetString(10), xd.GetString(11), xd.GetString(12), xd.GetDateTime(13), xd.GetString(14), xd.GetString(15), xd.GetString(16), xd.GetString(17), xd.GetString(18), xd.GetDateTime(19), xd.GetString(20), xd.GetString(22));
                    caux.datereq = xd.GetDateTime(23);
                    return true;
                } catch (Exception e) {
                    Console.WriteLine("ERROR: " + e);
                    return false;
                }
            }

            return false;
        }

        public Request GetRequestByTicket(string ticket) {
            Database db = new Database();
            string query = "SELECT * FROM `requests` WHERE ticket='" + ticket + "';";
            var xd = db.Select(query);
            Request caux = new Request();
            while (xd.Read()) {
                try {
                    Company cx = new Company();
                    cx.GetCompany(int.Parse(xd.GetString(5)));
                    caux = new Request(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetString(3), xd.GetDateTime(4), cx, xd.GetString(6), xd.GetString(7), xd.GetString(8), xd.GetString(9), xd.GetString(10), xd.GetString(11), xd.GetString(12), xd.GetDateTime(13), xd.GetString(14), xd.GetString(15), xd.GetString(16), xd.GetString(17), xd.GetString(18), xd.GetDateTime(19), xd.GetString(20), xd.GetString(22));
                    caux.datereq = xd.GetDateTime(23);
                    return caux;
                } catch (Exception e) {
                    Console.WriteLine("ERROR: " + e);
                    Request cr = new Request();
                    return cr;
                }
            }
            Request ca = new Request();
            return ca;
        }

        public string GetStatusByTicket(string ticket) {
            Database db = new Database();
            string query = "SELECT `status` FROM `requests` WHERE ticket='" + ticket + "';";
            var xd = db.Select(query);
            while (xd.Read()) {
                try {
                    string status = xd.GetString(0);
                    return status;
                } catch (Exception e) {
                    Console.WriteLine("ERROR: " + e);
                    return "No se encuentra";
                }
            }
            return "Error 0x000103";
        }

        public List<Request> GetAllRequests() {
            List<Request> requests = new List<Request>();
            Database db = new Database();
            string query = "SELECT * FROM `requests`;";
            var xd = db.Select(query);
            Request aux = new Request();
            while (xd.Read()) {
                try {
                    // Company cx = new Company();
                    // cx.GetCompany(int.Parse(xd.GetString(5)));
                    aux = new Request(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetString(3), xd.GetDateTime(4), new Company().GetCompany(int.Parse(xd.GetString(5))), xd.GetString(6), xd.GetString(7), xd.GetString(8), xd.GetString(9), xd.GetString(10), xd.GetString(11), xd.GetString(12), xd.GetDateTime(13), xd.GetString(14), xd.GetString(15), xd.GetString(16), xd.GetString(17), xd.GetString(18), xd.GetDateTime(19), xd.GetString(20), xd.GetString(22));
                    aux.datereq = xd.GetDateTime(23);
                    requests.Add(aux);
                } catch (Exception e) {
                    Console.WriteLine("Error: " + e);
                }

            }
            return requests;
        }


        public List<Request> GetInRequests() {
            List<Request> requests = new List<Request>();
            Database db = new Database();
            string query = "SELECT * FROM `requests` WHERE status = 'Ingresado';";
            var xd = db.Select(query);
            Request aux = new Request();
            while (xd.Read()) {
                try {
                    // Company cx = new Company();
                    // cx.GetCompany(int.Parse(xd.GetString(5)));
                    aux = new Request(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetString(3), xd.GetDateTime(4), new Company().GetCompany(int.Parse(xd.GetString(5))), xd.GetString(6), xd.GetString(7), xd.GetString(8), xd.GetString(9), xd.GetString(10), xd.GetString(11), xd.GetString(12), xd.GetDateTime(13), xd.GetString(14), xd.GetString(15), xd.GetString(16), xd.GetString(17), xd.GetString(18), xd.GetDateTime(19), xd.GetString(20), xd.GetString(22));
                    aux.datereq = xd.GetDateTime(23);
                    requests.Add(aux);
                } catch (Exception e) {
                    Console.WriteLine("Error: " + e);
                }

            }
            return requests;
        }

        public bool MarkRequests() {
            List<Request> requests = new List<Request>();
            Database db = new Database();
            string query = "UPDATE `requests` SET status = 'Impresion' WHERE status = 'Ingresado';";
            bool xd = db.Modify(query);
            /*Request aux = new Request();
            while (xd.Read()) {
                try {
                    // Company cx = new Company();
                    // cx.GetCompany(int.Parse(xd.GetString(5)));
                    aux = new Request(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetString(3), xd.GetDateTime(4), new Company().GetCompany(int.Parse(xd.GetString(5))), xd.GetString(6), xd.GetString(7), xd.GetString(8), xd.GetString(9), xd.GetString(10), xd.GetString(11), xd.GetString(12), xd.GetDateTime(13), xd.GetString(14), xd.GetString(15), xd.GetString(16), xd.GetString(17), xd.GetString(18), xd.GetDateTime(19), xd.GetString(20), xd.GetString(22));
                    aux.datereq = xd.GetDateTime(23);
                    requests.Add(aux);
                } catch (Exception e) {
                    Console.WriteLine("Error: " + e);
                }

            }*/
            return xd;
        }
    }
}
