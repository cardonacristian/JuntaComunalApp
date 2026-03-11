using JuntaComunalApp.Models;
using Org.BouncyCastle.Asn1.BC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
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
    /// Interaction logic for CrearUsuarioWindow.xaml
    /// </summary>
    public partial class CrearUsuarioWindow : Window
    {
        public CrearUsuarioWindow()
        {
            InitializeComponent();
            List<Rol> roles = ObtenerRoles();
            cmbRol.ItemsSource = roles;
            cmbRol.DisplayMemberPath = "NombreRol";
            cmbRol.SelectedValuePath = "Id"; 
        }
        public List<Rol> ObtenerRoles()
        {
            var lista = new List<Rol>();
            string cs = Conexion.ObtenerCadena();
            using(SQLiteConnection conn = new SQLiteConnection(cs))
            {
                string sql = "SELECT Id, Rol FROM Roles";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                conn.Open();
                using(SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Rol
                        {
                            Id = dr.GetInt32(0),
                            NombreRol = dr.GetString(1)
                        });
                    }
                }
            }
            return lista;
        }
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                cmbRol.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtPasswordConfirm.Password))
            {
                txtUsuario.BorderBrush = Brushes.Red;
                txtUsuario.BorderThickness = new Thickness(1);
                txtPassword.BorderBrush = Brushes.Red;
                txtPassword.BorderThickness = new Thickness(1);
                txtPasswordConfirm.BorderBrush = Brushes.Red;
                txtPasswordConfirm.BorderThickness = new Thickness(1);
                cmbRol.BorderBrush = Brushes.Red;
                cmbRol.BorderThickness = new Thickness(1);
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
                cmbRol.BorderBrush = Brushes.Black;
                cmbRol.BorderThickness = new Thickness(1);
            }
            if(txtPassword.Password != txtPasswordConfirm.Password)
            {
                MessageBox.Show("Las Contraseñas no coinciden", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Clear();
                txtPasswordConfirm.Clear();

                txtPassword.Focus();
                return;
            }
            if(cmbRol.SelectedValue == null)
            {
                MessageBox.Show("Selecciona un rol.");
                return;
            }
            Usuario usuario = new Usuario();
            usuario.Username = txtUsuario.Text.Trim();
            usuario.Password = Seguridad.EncriptarHash(txtPassword.Password);
            usuario.IdRol = (int)cmbRol.SelectedValue;
            string cs = Conexion.ObtenerCadena();
            string sql = @"
            INSERT INTO Usuarios
            (Usuario, Password, IdRol)
            VALUES
            (@Usuario, @Password, @IdRol)";
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(cs))
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Username);
                    cmd.Parameters.AddWithValue("@Password", usuario.Password);
                    cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Usuario creado con éxito.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n {ex.Message}");
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
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
