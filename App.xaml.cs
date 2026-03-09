using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
            string cs = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                string sqlCheck = "SELECT COUNT(*) FROM Usuarios";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                int conteo = (int)cmdCheck.ExecuteScalar();

                if (conteo == 0)
                {
                    
                    string sqlInsert = @"
                        IF NOT EXISTS (SELECT * FROM Roles WHERE Rol = 'ADMINISTRADOR')
                        INSERT INTO Roles (Rol) VALUES ('ADMINISTRADOR');

                        IF NOT EXISTS (SELECT * FROM Roles WHERE Rol = 'INVITADO')
                        INSERT INTO Roles (Rol) VALUES ('INVITADO');

                        DECLARE @IdAdmin INT = (SELECT Id FROM Roles WHERE Rol = 'ADMINISTRADOR');
                        DECLARE @IdInvitado INT = (SELECT Id FROM Roles WHERE Rol = 'INVITADO');

                        INSERT INTO Usuarios (Usuario, Password, IdRol, Activo) 
                        VALUES ('admin', 'admin1234', @IdAdmin, 1), 
                               ('Invitado', '1234$', @IdInvitado, 1);";

                    SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                    cmdInsert.ExecuteNonQuery();
                }
            }
        }
    }
}
