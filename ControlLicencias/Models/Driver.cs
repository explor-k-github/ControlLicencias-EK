using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class Driver {
        public string id;
        public string rut;

        public Driver() {
        }

        public Driver(string id, string rut) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.rut = rut ?? throw new ArgumentNullException(nameof(rut));
        }

        public Driver GetDriver(int rut) {
            Database db = new Database();
            string query = "SELECT * from Conductor WHERE `RUN_CORREGIDO` = '" + rut + "';";
            var xd = db.Select(query);
            Driver caux = new Driver();
            while (xd.Read()) {
                caux = new Driver(xd.GetString(0), xd.GetString(5));
            }
            xd.Close();
            db.CloseConnection();
            return caux;
        }

        public bool GetExistentDriver(string rut) {
            Database db = new Database();
            string query = "SELECT * FROM `Conductor` WHERE `RUN_CORREGIDO`='" + rut + "';";
            var xd = db.Select(query);
            Driver caux = new Driver();
            while (xd.Read()) {
                try {
                    caux = new Driver(xd.GetString(0), xd.GetString(5));
                    return true;
                } catch (Exception e) {
                    Console.WriteLine("ERROR: " + e);
                    return false;
                }
            }

            return false;
        }

    }
}
