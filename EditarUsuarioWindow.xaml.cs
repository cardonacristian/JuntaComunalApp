using JuntaComunalApp.Models;
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
            string cs = Conexion.ObtenerCadena();
            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                string sql = "SELECT Id, Rol FROM Roles";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                conn.Open();
                using (SQLiteDataReader dr = cmd.ExecuteReader())
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
            if (cmbRol.SelectedValue == null)
            {
                cmbRol.BorderBrush = Brushes.Red;
                MessageBox.Show("Por favor, seleccione un rol.");
                return; 
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                if (txtPassword.Password != txtPasswordConfirm.Password)
                {
                    MessageBox.Show("Las Contraseñas no coinciden", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Clear();
                    txtPasswordConfirm.Clear();

                    txtPassword.Focus();
                    return;
                }
            }
            string cs = Conexion.ObtenerCadena();
            string sql;
            bool cambiaPassword = !string.IsNullOrWhiteSpace(txtPassword.Password);
            if (cambiaPassword)
            {
                sql = @"UPDATE Usuarios 
                SET Password = @Password, IdRol = @IdRol 
                WHERE Id = @Id";
            }
            else
            {
                sql = @"UPDATE Usuarios 
                SET IdRol = @IdRol 
                WHERE Id = @Id";
            }
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(cs))
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IdRol", (int)cmbRol.SelectedValue);
                    cmd.Parameters.AddWithValue("@Id", _usuario.Id);

                    if(cambiaPassword)
                    {
                        string passwordEncriptada = Seguridad.EncriptarHash(txtPassword.Password);
                        cmd.Parameters.AddWithValue("@Password", passwordEncriptada);
                    }

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
