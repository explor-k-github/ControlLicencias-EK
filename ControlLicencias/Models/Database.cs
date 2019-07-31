using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class Database {
        //Conexion a la Base de Datos
        MySqlConnection connect;
        //172.16.1.66
        //Se comprueba entorno (DEV/PROD)
        static bool dev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        //Se indican las conexiones de BD en los dos entornos
        static string test = (dev ? "172.16.1.68" : "localhost");
        //Conexion a la BD
        string connection = "Data Source=" + test + ";Database=explorkc_formlic;Uid=explor;Pwd=explor2019";


        //Función para querys SELECT
        public MySqlDataReader Select(string query) {
            if (query.Contains("DELETE") || query.Contains("UPDATE") || query.Contains("INSERT")|| query.Contains("DROP")) {
                return null;
            } else {
                connect = new MySqlConnection(connection);
                connect.Close();
                MySqlCommand command = connect.CreateCommand();
                command.CommandText = query;
                connect.Open();
                MySqlDataReader reader;
                try {
                    var executed = reader = command.ExecuteReader();
                    return executed;
                } catch (Exception e) {
                    Console.WriteLine("Error: " + e);
                    return null;
                }
            }

            
            
           
        }

        //Función para Modificaciones UPDATE, SET, DELETE
        public Boolean Modify(string query) {
            connect = new MySqlConnection(connection);
            MySqlCommand command = connect.CreateCommand();
            command.CommandText = query;
            connect.Open();
            try {
                command.ExecuteNonQuery();
                connect.Close();
                return true;
            } catch (Exception ex) {
                Console.WriteLine("EXCEPTION: " + ex);
                connect.Close();
                return false;
            }
        }

        public void CloseConnection() {
            connect.Close();
        }
    }
}
