using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class Company {
        public string id;
        public string company;

        public Company(string id, string company) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public Company() {

        }

        public JArray GetAllCompanies() {
            List<Company> companies = new List<Company>();
            Database db = new Database();
            string query = "SELECT * from company;";
            var xd = db.Select(query);
            Company caux = new Company();
            while (xd.Read()) {
                caux = new Company(xd.GetString(0), xd.GetString(1));
                companies.Add(caux);
            }
            xd.Close();
            db.CloseConnection();
            JArray jarr = JArray.Parse(JsonConvert.SerializeObject(companies, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));

            return jarr;
        }


        public Company GetCompany(int id) {
            Database db = new Database();
            string query = "SELECT * from company WHERE id = '"+id+"';";
            var xd = db.Select(query);
            Company caux = new Company();
            while (xd.Read()) {
                caux = new Company(xd.GetString(0), xd.GetString(1));
            }
            xd.Close();
            db.CloseConnection();
            return caux;
        }


    }
}
