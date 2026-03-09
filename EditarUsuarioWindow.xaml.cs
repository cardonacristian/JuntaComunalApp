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
    /// Interaction logic for EditarUsuarioWindow.xaml
    /// </summary>
    public partial class EditarUsuarioWindow : Window
    {
        private Usuario _usuario;
        public EditarUsuarioWindow(Usuario usuarioRecibido)
        {
            InitializeComponent();
            _usuario = usuarioRecibido;
            List<Rol> roles = ObtenerRoles();
            cmbRol.ItemsSource = roles;
            cmbRol.DisplayMemberPath = "NombreRol";
            cmbRol.SelectedValuePath = "Id";
            cmbRol.SelectedValue = _usuario.IdRol;
            if (_usuario.Username.ToLower() == "admin")
            {
                cmbRol.IsEnabled = false;
                cmbRol.ToolTip = "El rol del administrador principal no puede ser modificado.";
            }

        }
        public List<Rol> ObtenerRoles()
        {
            var lista = new List<Rol>();
            string cs = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                string sql = "SELECT Id, Rol FROM Roles";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
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
        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                cmbRol.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtPasswordConfirm.Password))
            {
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
                txtPassword.BorderBrush = Brushes.Black;
                txtPassword.BorderThickness = new Thickness(1);
                txtPasswordConfirm.BorderBrush = Brushes.Black;
                txtPasswordConfirm.BorderThickness = new Thickness(1);
                cmbRol.BorderBrush = Brushes.Black;
                cmbRol.BorderThickness = new Thickness(1);
            }
            if (txtPassword.Password != txtPasswordConfirm.Password)
            {
                MessageBox.Show("Las Contraseñas no coinciden", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Clear();
                txtPasswordConfirm.Clear();

                txtPassword.Focus();
                return;
            }
            if (cmbRol.SelectedValue == null)
            {
                MessageBox.Show("Selecciona un rol.");
                return;
            }
            Usuario usuario = new Usuario();
            usuario.Password = Seguridad.EncriptarHash(txtPassword.Password);
            string cs = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
            string sql = @"
            UPDATE Usuarios
            SET Password = @Password,
                IdRol = @IdRol
            WHERE Id = @Id";
            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Password", usuario.Password);
                    cmd.Parameters.AddWithValue("@IdRol", (int)cmbRol.SelectedValue);
                    cmd.Parameters.AddWithValue("@Id", _usuario.Id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Usuario actualizado con éxito.");
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
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
