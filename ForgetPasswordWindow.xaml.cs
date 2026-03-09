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
    /// Interaction logic for ForgetPasswordWindow.xaml
    /// </summary>
    public partial class ForgetPasswordWindow : Window
    {
        public ForgetPasswordWindow()
        {
            InitializeComponent();
        }
        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtPasswordConfirm.Password))
            {
                txtUsuario.BorderBrush = Brushes.Red;
                txtUsuario.BorderThickness = new Thickness(1);
                txtPassword.BorderBrush = Brushes.Red;
                txtPassword.BorderThickness = new Thickness(1);
                txtPasswordConfirm.BorderBrush = Brushes.Red;
                txtPasswordConfirm.BorderThickness = new Thickness(1);
                MessageBox.Show("Por favor, complete todos los campos obligatorios.");
                return;
            }
            else
            {
                txtUsuario.BorderBrush = Brushes.Black;
                txtUsuario.BorderThickness = new Thickness(1);
                txtPassword.BorderBrush = Brushes.Black;
                txtPassword.BorderThickness = new Thickness(1);
                txtPasswordConfirm.BorderBrush = Brushes.Black;
                txtPasswordConfirm.BorderThickness = new Thickness(1);
            }
            if (txtPassword.Password != txtPasswordConfirm.Password)
            {
                MessageBox.Show("Las Contraseñas no coinciden", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Clear();
                txtPasswordConfirm.Clear();

                txtPassword.Focus();
                return;
            }
            Usuario usuario = new Usuario();
            usuario.Username = txtUsuario.Text.Trim();
            usuario.Password = Seguridad.EncriptarHash(txtPassword.Password);
            string cs = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
            string sql = @"
            UPDATE Usuarios
            SET Password = @Password
            WHERE Usuario = @Usuario";
            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Password", usuario.Password);
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Username);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Contraseña actualizada con éxito.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar:\n {ex.Message}");
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
        private void btnVerPassword_CheckedConfirm(object sender, RoutedEventArgs e)
        {
            txtPasswordConfirmRevelado.Text = txtPasswordConfirm.Password;
            txtPasswordConfirm.Visibility = Visibility.Collapsed;
            txtPasswordConfirmRevelado.Visibility = Visibility.Visible;

            btnVerPasswordConfirm.Content = "🔒";
        }
        private void btnVerPassword_UncheckedConfirm(object sender, RoutedEventArgs e)
        {
            txtPasswordConfirm.Password = txtPasswordConfirmRevelado.Text;

            txtPasswordConfirmRevelado.Visibility = Visibility.Collapsed;
            txtPasswordConfirm.Visibility = Visibility.Visible;

            btnVerPasswordConfirm.Content = "👁";
        }
        private void BtnRegresar_Click (object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
