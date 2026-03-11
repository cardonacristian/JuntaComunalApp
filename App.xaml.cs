using JuntaComunalApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JuntaComunalApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                SeedDatabase();

                LoginWindow login = new LoginWindow();
                login.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al iniciar la base de datos: {ex.Message}", "Error de Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
        private void SeedDatabase()
        {
            string cs = Conexion.ObtenerCadena();

            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                conn.Open();

                string sqlCheck = "SELECT COUNT(*) FROM Usuarios";
                SQLiteCommand cmdCheck = new SQLiteCommand(sqlCheck, conn);
                int conteo = Convert.ToInt32(cmdCheck.ExecuteScalar());

                if (conteo == 0)
                {


                    string sqlRoles = "INSERT OR IGNORE INTO Roles (Rol) VALUES ('ADMINISTRADOR'), ('INVITADO');";
                    using (var cmdRoles = new SQLiteCommand(sqlRoles, conn)) { cmdRoles.ExecuteNonQuery(); }


                    string passAdmin = Seguridad.EncriptarHash("admin1234");
                    string passInvitado = Seguridad.EncriptarHash("1234$");


                    string sqlInsert = @"
                           INSERT INTO Usuarios (Usuario, Password, IdRol, Activo) 
                           VALUES (@User, @Pass, (SELECT Id FROM Roles WHERE Rol = @Rol), 1)";


                    using (var cmd = new SQLiteCommand(sqlInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@User", "admin");
                        cmd.Parameters.AddWithValue("@Pass", passAdmin);
                        cmd.Parameters.AddWithValue("@Rol", "ADMINISTRADOR");
                        cmd.ExecuteNonQuery();
                    }


                    using (var cmd = new SQLiteCommand (sqlInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@User", "Invitado");
                        cmd.Parameters.AddWithValue("@Pass", passInvitado);
                        cmd.Parameters.AddWithValue("@Rol", "INVITADO");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
