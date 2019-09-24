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
        public string type;
        public DateTime lastlogin;

        public User(string id, string user, string password, string type, DateTime lastlogin) {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.user = user ?? throw new ArgumentNullException(nameof(user));
            this.password = password ?? throw new ArgumentNullException(nameof(password));
            this.type = type ?? throw new ArgumentNullException(nameof(type));
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
                    usr = new User(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetInt32(3) + "", xd.GetDateTime(4));
                }
            }catch(Exception e) {
                Console.WriteLine("Error: " + e);
                return false;
            }
            Console.WriteLine(usr.password);
            Console.WriteLine(Hash(password));
            if (usr.password == Hash(password)) {
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                db.Modify("UPDATE `users` SET lastlogin ='"+date+"' WHERE company = '" +usr.user + "'");
                this.id = usr.id;
                this.user = usr.user;
                this.password = usr.password;
                this.type = usr.type;
                this.lastlogin = usr.lastlogin;
                return true;
            } else {
                return false;
            }

        }

        public bool CreateUser(User user) {
            Database db = new Database();
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string query = "INSERT INTO `users` (`id`, `company`, `password`, `type`, `lastlogin` ) VALUES (NULL, '" + user.user + "', '" + this.password + "', '" + this.type + "', '" + date + "')";
            bool status = db.Modify(query);
            db.CloseConnection();
            return status;
        }

        public bool CheckIfExists() {
            Database db = new Database();
            
            string query = "SELECT * from users where company = '" + this.user + "';";
            var xd = db.Select(query);
            User usr = new User();
            List<User> x = new List<User>();
            try {
                while (xd.Read()) {
                    usr = new User(xd.GetString(0), xd.GetString(1), xd.GetString(2), xd.GetInt32(3) + "", xd.GetDateTime(4));
                    x.Add(usr);
                }
            } catch (Exception e) {
                Console.WriteLine("Error: " + e);
                return false;
            }
            if (x.Count >= 1) {
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
