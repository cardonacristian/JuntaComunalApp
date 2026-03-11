using JuntaComunalApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for EditarMiembroWindow.xaml
    /// </summary>
    public partial class EditarMiembroWindow : Window
    {
        private Miembro _miembro;
        public EditarMiembroWindow(Miembro miembroRecibido)
        {
            InitializeComponent();
            _miembro = miembroRecibido;

            txtCedula.Text = _miembro.Cedula;
            txtPrimerNombre.Text = _miembro.PrimerNombre;
            txtPrimerApellido.Text = _miembro.PrimerApellido;
            txtSegundoNombre.Text = _miembro.SegundoNombre;
            txtSegundoApellido.Text = _miembro.SegundoApellido;
            txtEmail.Text = _miembro.Email;
            txtTelefono.Text = _miembro.Telefono;
            txtDireccion.Text = _miembro.Direccion;

            string generoDB = _miembro.Genero;

            if(generoDB == "Masculino" || generoDB == "Femenino")
            {
                cmbGenero.SelectedValue = generoDB;
            }
            else
            {
                cmbGenero.SelectedValue = "Otro";
                txtOtroGenero.Text = generoDB;
            }

        }
        public void BtnActualizar_Click(object sender, EventArgs e)
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
            if (!Regex.IsMatch(txtCedula.Text, @"^\d+$") || !Regex.IsMatch(txtTelefono.Text, @"^\d+$"))
            {
                MessageBox.Show("La Cedula y el Telefono solo deben contener números.");
                return;
            }
            string patronLetras = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$";
            if (!Regex.IsMatch(txtPrimerNombre.Text, patronLetras) || !Regex.IsMatch(txtPrimerApellido.Text, patronLetras)
                || !Regex.IsMatch(txtSegundoNombre.Text, patronLetras) || !Regex.IsMatch(txtSegundoApellido.Text, patronLetras))
            {
                MessageBox.Show("Los Nombres y Apellidos solo pueden contener letras.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(txtEmail.Text, patronEmail))
                {
                    MessageBox.Show("El formato del correo electrónico no es válido.");
                    return;
                }
            }

            string generoFinal = cmbGenero.SelectedValue.ToString();

            if (generoFinal == "Otro")
            {
                if (string.IsNullOrWhiteSpace(txtOtroGenero.Text))
                {
                    MessageBox.Show("Por favor, Especifique el género.");
                    return;
                }
                generoFinal = txtOtroGenero.Text;
            }
            string cs = Conexion.ObtenerCadena();    
            string sql = @"
            UPDATE Miembros
            SET Cedula = @Cedula, 
                PrimerNombre = @PrimerNombre,
                SegundoNombre = @SegundoNombre,
                PrimerApellido = @PrimerApellido,
                SegundoApellido = @SegundoApellido,
                Email = @Email,
                Telefono = @Telefono,
                Direccion = @Direccion,
                Genero = @Genero
            WHERE Id = @Id";
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(cs))
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    string generoGuardar = cmbGenero.SelectedValue.ToString().Trim() == "Otro" ? txtOtroGenero.Text.ToUpper().Trim() : generoFinal.ToUpper().Trim();
                    cmd.Parameters.AddWithValue("@Cedula", txtCedula.Text.Trim());
                    cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@SegundoNombre", txtSegundoNombre.Text.ToUpper().Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PrimerApellido", txtPrimerApellido.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@SegundoApellido", txtSegundoApellido.Text.ToUpper().Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim() ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text.Trim());
                    cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text.ToUpper().Trim());
                    cmd.Parameters.AddWithValue("@Genero", generoGuardar);
                    cmd.Parameters.AddWithValue("@Id", _miembro.Id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Miembro actualizado con éxito.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar:\n {ex.Message}");
            }
        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
