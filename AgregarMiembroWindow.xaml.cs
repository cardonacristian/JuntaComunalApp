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
using System.Text.RegularExpressions;
using System.Data.SQLite;
using JuntaComunalApp.Models;

namespace JuntaComunalApp
{
    /// <summary>
    /// Interaction logic for AgregarMiembroWindow.xaml
    /// </summary>
    public partial class AgregarMiembroWindow : Window
    {
        public AgregarMiembroWindow()
        {
            InitializeComponent();
        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtPrimerNombre.Text) ||
                string.IsNullOrWhiteSpace(txtPrimerApellido.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                cmbGenero.SelectedValue == null)
            {
                txtCedula.BorderBrush = Brushes.Red;
                txtCedula.BorderThickness = new Thickness(1);
                txtPrimerNombre.BorderBrush = Brushes.Red;
                txtPrimerNombre.BorderThickness = new Thickness(1);
                txtPrimerApellido.BorderBrush = Brushes.Red;
                txtPrimerApellido.BorderThickness = new Thickness(1);
                txtTelefono.BorderBrush = Brushes.Red;
                txtTelefono.BorderThickness = new Thickness(1);
                txtDireccion.BorderBrush = Brushes.Red;
                txtDireccion.BorderThickness = new Thickness(1);
                cmbGenero.BorderBrush = Brushes.Red;
                cmbGenero.BorderThickness = new Thickness(1);
                MessageBox.Show("Por favor, complete todos los campos obligatorios.");
                return;
            }
            else
            {
                txtCedula.BorderBrush = Brushes.Black;
                txtCedula.BorderThickness = new Thickness(1);
                txtPrimerNombre.BorderBrush = Brushes.Black;
                txtPrimerNombre.BorderThickness = new Thickness(1);
                txtPrimerApellido.BorderBrush = Brushes.Black;
                txtPrimerApellido.BorderThickness = new Thickness(1);
                txtTelefono.BorderBrush = Brushes.Black;
                txtTelefono.BorderThickness = new Thickness(1);
                txtDireccion.BorderBrush = Brushes.Black;
                txtDireccion.BorderThickness = new Thickness(1);
                cmbGenero.BorderBrush = Brushes.Black;
                cmbGenero.BorderThickness = new Thickness(1);
            }
            if(!Regex.IsMatch(txtCedula.Text, @"^\d+$") || !Regex.IsMatch(txtTelefono.Text, @"^\d+$"))
            {
                MessageBox.Show("La Cedula y el Telefono solo deben contener números.");
                return;
            }
            string patronLetras = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$";
            if(!Regex.IsMatch(txtPrimerNombre.Text, patronLetras) || !Regex.IsMatch(txtPrimerApellido.Text,patronLetras)
                || !Regex.IsMatch(txtSegundoNombre.Text, patronLetras) || !Regex.IsMatch(txtSegundoApellido.Text, patronLetras))
            {
                MessageBox.Show("Los Nombres y Apellidos solo pueden contener letras.");
                return;
            }
            if(!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if(!Regex.IsMatch(txtEmail.Text,patronEmail))
                {
                    MessageBox.Show("El formato del correo electrónico no es válido.");
                    return;
                }
            }

            string generoFinal = cmbGenero.SelectedValue.ToString().Trim().ToUpper();

            if (generoFinal == "OTRO")
            {
                if(string.IsNullOrWhiteSpace(txtOtroGenero.Text))
                {
                    MessageBox.Show("Por favor, Especifique el género.");
                    return;
                }
                generoFinal = txtOtroGenero.Text.ToUpper().Trim();
            }
            string cs = Conexion.ObtenerCadena();
            string sql = @"
            INSERT INTO Miembros
            (Cedula, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, Email, Telefono, Direccion, Genero)
            VALUES
            (@Cedula, @PrimerNombre, @SegundoNombre, @PrimerApellido, @SegundoApellido, @Email, @Telefono, @Direccion, @Genero)";
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(cs))
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Cedula", txtCedula.Text.Trim());
                    cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@SegundoNombre", txtSegundoNombre.Text.ToUpper().Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PrimerApellido", txtPrimerApellido.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@SegundoApellido", txtSegundoApellido.Text.ToUpper().Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text.Trim());
                    cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@Genero", generoFinal);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Miembro guardado con éxito.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n {ex.Message}");
            }
        }
    }
}
