using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class User {
        public string id;
        public string user;
        public string password;
        public DateTime lastlogin;

        public User(string id, string user, string password, DateTime lastlogin) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.user = user ?? throw new ArgumentNullException(nameof(user));
            this.password = password ?? throw new ArgumentNullException(nameof(password));
            this.lastlogin = lastlogin;
        }

        public User() {

        }

        public bool Authenticate(string user, string password) {
            Database db = new Database();
            string query = "SELECT * from users where company = '" + user + "';";
            var xd = db.Select(query);
            User usr = new User();
            try {
                while (xd.Read()) {
                    usr = new User(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetDateTime(3));
                }
            }catch(Exception e) {
                Console.WriteLine("Error: " + e);
                return false;
            }
            Console.WriteLine(usr.password);
            Console.WriteLine(Hash(password));
            if (usr.password == Hash(password)) {
                return true;
            } else {
                return false;
            }

        }

        public static string Hash(string stringToHash) {
            using (var sha1 = new SHA1Managed()) {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).Replace("-", "");
            }
        }
    }
}
