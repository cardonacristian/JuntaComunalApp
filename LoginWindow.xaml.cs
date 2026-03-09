using JuntaComunalApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JuntaComunalApp
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            ProcesarLogin(txtUsuario.Text.Trim(), txtPassword.Password);
        }
        private void ProcesarLogin(string user, string pass)
        {
            string passEncriptada = Seguridad.EncriptarHash(pass);

            if (ValidarUsuario(user, passEncriptada))
            {
                Sesion.UsuarioActual = ObtenerUsuarios(user);
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o Contraseña incorrectos.", "Error de acceso", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Usuario ObtenerUsuarios(string user)
        {
            string cs = ConfigurationManager
                .ConnectionStrings["ConexionDB"]
                .ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"SELECT u.Usuario, r.Rol, u.IdRol, u.Id, u.Activo
                FROM Usuarios u
                INNER JOIN Roles r
                ON u.IdRol = r.Id
                WHERE u.Usuario = @user";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", user);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Usuario
                            {
                                Username = dr.GetString(0),
                                Rol = dr.GetString(1),
                                IdRol = dr.GetInt32(2),
                                Id = dr.GetInt32(3),
                                Activo = dr.GetBoolean(4),
                            };
                        }
                    }
                }
                
            }
            return null;
        }
        private bool ValidarUsuario(string usuario, string password)
        {
            string cs = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
            string sql = "SELECT COUNT(1) FROM Usuarios WHERE Usuario = @user AND Password = @pass AND Activo = 1";

            using (SqlConnection conn = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@user", usuario);
                cmd.Parameters.AddWithValue("@pass", password);
                conn.Open();
                int resultado = Convert.ToInt32(cmd.ExecuteScalar());
                return resultado > 0;
            }
        }
        private void btnVerPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordRevelado.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordRevelado.Visibility = Visibility.Visible;

            btnVerPassword.Content = "🔒";
        }
        private void btnVerPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordRevelado.Text;

            txtPasswordRevelado.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;

            btnVerPassword.Content = "👁";
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("¿Desea salir de la aplicación?",
                                                 "Confirmar",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void lnkOlvidoPassword_Click (object sender, RoutedEventArgs e)
        {
            ForgetPasswordWindow dlg = new ForgetPasswordWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }
         private void lnkIniciarInvitado_Click (object sender, RoutedEventArgs e)
        {
            ProcesarLogin("Invitado", "1234$");
        }
    }
}
