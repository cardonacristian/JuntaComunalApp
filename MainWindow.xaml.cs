using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using JuntaComunalApp.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Paragraph = iText.Layout.Element.Paragraph;
using Table = iText.Layout.Element.Table;
using iText.Kernel.Geom;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Data.SQLite;

namespace JuntaComunalApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ICollectionView vistaMiembros;
        int paginaActual = 1;
        int registrosPorPagina = 20;
        int totalPaginas = 1;
        int contadorFiltrados = 0;
        public MainWindow()
        {
            InitializeComponent();
            CargarDatos();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Sesion.EsAdmin)
            {
                borderMenu.Visibility = Visibility.Collapsed;
                columnEditar.Visibility = Visibility.Collapsed;
                colMenu.Width = new GridLength(0);
            }
            lblBienvenida.Text = $"Bienvenido, {Sesion.UsuarioActual.Username.ToUpper()}";
            CargarDatos();
            
        }
        private void ExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            var lista = dgMiembros.ItemsSource.Cast<Miembro>().ToList();
            if (lista == null || lista.Count == 0) return;
            
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" };
            if(sfd.ShowDialog() == true)
            {
                using(var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Lista de Miembros");
                    ws.Cell(1, 1).Value = "Cédula";
                    ws.Cell(1, 2).Value = "Nombre";
                    ws.Cell(1, 3).Value = "Telefono";
                    ws.Cell(1, 4).Value = "Dirección";
                    ws.Cell(1, 5).Value = "Email";
                    ws.Cell(1, 6).Value = "Firma";

                    var rangoHeader = ws.Range("A1:F1");
                    rangoHeader.Style.Font.Bold = true;
                    rangoHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                    rangoHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int fila = 2;
                    foreach(var m in lista)
                    {
                        ws.Cell(fila, 1).Value = m.Cedula;
                        ws.Cell(fila, 2).Value = m.NombreCompleto;
                        ws.Cell(fila, 3).Value = m.Telefono;
                        ws.Cell(fila, 4).Value = m.Direccion;
                        ws.Cell(fila, 5).Value = m.Email;

                        ws.Row(fila).Height = 30;
                        fila++;
                    }

                    var rangoDatos = ws.Range(1, 1, fila - 1, 6);
                    rangoDatos.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rangoDatos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    ws.Columns(1,5).AdjustToContents();
                    ws.Column(6).Width = 30;
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("¡Excel generado con éxito!");
                }
            }
        }
        private void ExportarPDF_Click(object sender, EventArgs e)
        {
            var lista = dgMiembros.ItemsSource.Cast<Miembro>().ToList();
            if (lista == null) return;

            SaveFileDialog sfd = new SaveFileDialog() { Filter = "PDF Document|*.pdf" };
            if(sfd.ShowDialog() == true)
            {
                using(PdfWriter writer = new PdfWriter(sfd.FileName))
                using(PdfDocument pdf =  new PdfDocument(writer))
                {
                    pdf.SetDefaultPageSize(PageSize.A4.Rotate());
                    Document doc = new Document(pdf);
                    doc.Add(new Paragraph("LISTADO OFICIAL DE MIEMBROS").SetFontSize(18).SimulateBold());
                    doc.Add(new Paragraph($"Fecha de reporte: {DateTime.Now.ToShortDateString()}"));

                    Table tabla = new Table(UnitValue.CreatePercentArray(new float[] {12,35,13,15,25})).UseAllAvailableWidth();

                    tabla.AddHeaderCell("Cedula");
                    tabla.AddHeaderCell("Nombre");
                    tabla.AddHeaderCell("Telefono");
                    tabla.AddHeaderCell("Dirección");
                    tabla.AddHeaderCell("Email");

                    foreach(var m in lista)
                    {
                        tabla.AddCell(new Cell().Add(new Paragraph(m.Cedula)));
                        tabla.AddCell(new Cell().Add(new Paragraph(m.NombreCompleto)));
                        tabla.AddCell(new Cell().Add(new Paragraph(m.Telefono ?? "")));
                        tabla.AddCell(new Cell().Add(new Paragraph(m.Direccion ?? "")));
                        tabla.AddCell(new Cell().Add(new Paragraph(m.Email ?? "")));

                    }

                    doc.Add(tabla);
                    doc.Close();
                }
                MessageBox.Show("¡PDF generado con éxito!");
            }

        }
        private List<Miembro> ObtenerMiembros()
        {
            var lista = new List<Miembro>();
            string cs = Conexion.ObtenerCadena();
            using (SQLiteConnection conn = new SQLiteConnection (cs)) {
                conn.Open();

                string sql = @"
                SELECT Cedula, NombreCompleto, Telefono, Direccion, Genero, Email,
                FechaIngreso, Id, PrimerNombre, PrimerApellido, SegundoNombre, SegundoApellido
                FROM Miembros
                WHERE Activo = 1";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Miembro
                        {
                            Cedula = dr.GetString(0),
                            NombreCompleto = dr.GetString(1),
                            Telefono = dr.IsDBNull(2) ? null: dr.GetString(2),
                            Direccion = dr.IsDBNull(3) ? null: dr.GetString(3),
                            Genero = dr.IsDBNull(4) ? null: dr.GetString(4),
                            Email = dr.IsDBNull(5) ? null : dr.GetString(5),
                            FechaIngreso = dr.GetDateTime(6),
                            Id = dr.GetInt32(7),
                            PrimerNombre = dr.GetString(8),
                            PrimerApellido = dr.GetString(9),
                            SegundoNombre = dr.IsDBNull(10) ? null : dr.GetString(10),
                            SegundoApellido = dr.IsDBNull(11) ? null : dr.GetString(11)
                        });
                    }
                }
            }
            return lista;
        }
        private List<Usuario> ObtenerUsuarios()
        {
            var lista = new List<Usuario>();
            string cs = Conexion.ObtenerCadena();
            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                conn.Open();

                string sql = @"SELECT u.Usuario, r.Rol, u.IdRol, u.Id, u.Activo
                FROM Usuarios u
                INNER JOIN Roles r
                ON u.IdRol = r.Id
                WHERE u.Activo = 1
                AND u.Usuario <> 'Invitado'";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Username = dr.GetString(0),
                            Rol = dr.GetString(1),
                            IdRol = dr.GetInt32(2),
                            Id = dr.GetInt32(3),
                            Activo = dr.GetBoolean(4),
                        });
                    }
                }
            }
            return lista;
        }
        private List<Miembro> ObtenerMiembrosTodos()
        {
            var lista = new List<Miembro>();
            string cs = Conexion.ObtenerCadena();
            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                conn.Open();

                string sql = @"
                SELECT Cedula, NombreCompleto, Telefono, Direccion, Genero, Email,
                FechaIngreso, FechaSalida, Id, PrimerNombre, PrimerApellido, SegundoNombre, SegundoApellido, Activo
                FROM Miembros";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Miembro
                        {
                            Cedula = dr.GetString(0),
                            NombreCompleto = dr.GetString(1),
                            Telefono = dr.IsDBNull(2) ? null : dr.GetString(2),
                            Direccion = dr.IsDBNull(3) ? null : dr.GetString(3),
                            Genero = dr.IsDBNull(4) ? null : dr.GetString(4),
                            Email = dr.IsDBNull(5) ? null : dr.GetString(5),
                            FechaIngreso = dr.GetDateTime(6),
                            FechaSalida = dr.IsDBNull(7) ? (DateTime?)null : dr.GetDateTime(7),
                            Id = dr.GetInt32(8),
                            PrimerNombre = dr.GetString(9),
                            PrimerApellido = dr.GetString(10),
                            SegundoNombre = dr.IsDBNull(11) ? null : dr.GetString(11),
                            SegundoApellido = dr.IsDBNull(12) ? null : dr.GetString(12),
                            Activo = dr.GetBoolean(13)
                        });
                    }
                }
            }
            return lista;
        }
        private List<Usuario> ObtenerUsuariosTodos()
        {
            var lista = new List<Usuario>();
            string cs = Conexion.ObtenerCadena();
            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                conn.Open();

                string sql = @"SELECT u.Usuario, r.Rol, u.IdRol, u.Id, u.Activo
                FROM Usuarios u
                INNER JOIN Roles r
                ON u.IdRol = r.Id
                WHERE u.Usuario NOT IN ('Invitado', 'admin')";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Username = dr.GetString(0),
                            Rol = dr.GetString(1),
                            IdRol = dr.GetInt32(2),
                            Id = dr.GetInt32(3),
                            Activo = dr.GetBoolean(4),
                        });
                    }
                }
            }
            return lista;
        }
        private void ActualizarEstadoBD(int id, bool estaActivo)
        {
            try
            {
                string cs = Conexion.ObtenerCadena();
                using(SQLiteConnection conn = new SQLiteConnection(cs))
                {
                    conn.Open();
                    string sql = @"UPDATE Miembros
                                   SET Activo = @Activo,
                                       FechaSalida = @Fecha
                                   WHERE Id = @Id";
                    using(SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Activo", estaActivo);
                        cmd.Parameters.AddWithValue("@Id", id);
                        if (estaActivo)
                        {
                            cmd.Parameters.AddWithValue("@Fecha", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el estado: {ex.Message}");
            }
        }
        private void ActualizarEstadoUsuarioBD(int id, bool estaActivo)
        {
            try
            {
                string cs = Conexion.ObtenerCadena();
                using (SQLiteConnection conn = new SQLiteConnection(cs))
                {
                    conn.Open();
                    string sql = @"UPDATE Usuarios
                                   SET Activo = @Activo
                                   WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Activo", estaActivo);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el estado: {ex.Message}");
            }
        }
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarMiembroWindow();
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                CargarDatos();
            }
        }
        private void BtnGestionarUsuario_Click(object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Collapsed;
            borderDesactivar.Visibility = Visibility.Collapsed;
            borderUsuarios.Visibility = Visibility.Visible;
            dgUsuarios.ItemsSource = ObtenerUsuarios();
        }
        private void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearUsuarioWindow();
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                dgUsuarios.ItemsSource = ObtenerUsuarios();
            }
        }
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var miembroParaEditar = boton.DataContext as Miembro;
            if(miembroParaEditar != null)
            {
                var ventana = new EditarMiembroWindow(miembroParaEditar);
                bool? resultado = ventana.ShowDialog();
                if (resultado == true)
                {
                    CargarDatos();
                }
            }
        }
        private void BtnEditarUsuario_Click (object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var usuarioParaEditar = boton.DataContext as Usuario;
            if (usuarioParaEditar != null)
            {
                var ventana = new EditarUsuarioWindow(usuarioParaEditar);
                bool? resultado = ventana.ShowDialog();
                if (resultado == true)
                {
                    dgUsuarios.ItemsSource = ObtenerUsuarios();
                }
            }
        }
        private void BtnDesactivar_Click(object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Collapsed;
            borderDesactivar.Visibility = Visibility.Visible;
            borderUsuarios.Visibility = Visibility.Collapsed;
            borderUsuariosDesactivar.Visibility = Visibility.Collapsed;
            dgDesactivar.ItemsSource = ObtenerMiembrosTodos();
        }
        private void BtnDesactivarUsuario_Click(object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Collapsed;
            borderDesactivar.Visibility = Visibility.Collapsed;
            borderUsuarios.Visibility = Visibility.Collapsed;
            borderUsuariosDesactivar.Visibility = Visibility.Visible;
            dgUsuariosDesactivados.ItemsSource = ObtenerUsuariosTodos();
        }
        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Visible;
            borderDesactivar.Visibility = Visibility.Collapsed;
            borderUsuarios.Visibility= Visibility.Collapsed;
            borderUsuariosDesactivar.Visibility= Visibility.Collapsed;
            CargarDatos();
        }
        private void BtnRegresarDesactivado_Click(object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Collapsed;
            borderDesactivar.Visibility = Visibility.Collapsed;
            borderUsuarios.Visibility = Visibility.Visible;
            borderUsuariosDesactivar.Visibility = Visibility.Collapsed;
            List<Usuario> lista = ObtenerUsuarios();
            dgUsuarios.ItemsSource = lista;
        }
        private void ChkEstado_click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var miembro = cb.DataContext as Miembro;
            if (miembro != null)
            {
                bool nuevoEstado = cb.IsChecked ?? false;
                ActualizarEstadoBD(miembro.Id, nuevoEstado);
                miembro.Activo = nuevoEstado;
                if (nuevoEstado) miembro.FechaSalida = null;
                else miembro.FechaSalida = DateTime.Now;
                dgDesactivar.Items.Refresh();
            }
        }
        private void ChkEstadoUsuario_click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var usuario = cb.DataContext as Usuario;
            if (usuario != null)
            {
                bool nuevoEstado = cb.IsChecked ?? false;
                ActualizarEstadoUsuarioBD(usuario.Id, nuevoEstado);
                usuario.Activo = nuevoEstado;
                dgUsuariosDesactivados.Items.Refresh();
            }
        }
        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = true;
        }
        private void BtnHome_Click (object sender, RoutedEventArgs e)
        {
            borderPrincipal.Visibility = Visibility.Visible;
            borderDesactivar.Visibility = Visibility.Collapsed;
            borderUsuarios.Visibility = Visibility.Collapsed;
            borderUsuariosDesactivar.Visibility = Visibility.Collapsed;
            paginaActual = 1;
            contadorFiltrados = 0;
            CargarDatos();
        }
        private void CargarDatos()
        {
            paginaActual = 1;
            contadorFiltrados = 0;

            List<Miembro> lista = ObtenerMiembros();

            vistaMiembros = CollectionViewSource.GetDefaultView(lista);

            vistaMiembros.Filter = FilterLogic;

            ActualizarPaginacion();

            dgMiembros.ItemsSource = vistaMiembros;
        }
        private bool FilterLogic(object obj)
        {
            var miembro = obj as Miembro;
            if (miembro == null) return false;

            string busqueda = txtBuscar.Text.Trim().ToLower();

            bool coincideBusqueda = string.IsNullOrWhiteSpace(busqueda) || (miembro.NombreCompleto?.ToLower().Contains(busqueda) ?? false)
                                    || (miembro.Cedula?.ToLower().Contains(busqueda) ?? false);

            if(!coincideBusqueda) return false;

            contadorFiltrados++;

            int inicio = (paginaActual - 1) * registrosPorPagina + 1;
            int fin = paginaActual * registrosPorPagina;

            return contadorFiltrados >= inicio && contadorFiltrados <= fin;
        }
        private void txtBuscar_TextChanged (object sender, TextChangedEventArgs e)
        {
            paginaActual = 1;
            ActualizarPaginacion();
        }
        private void ActualizarPaginacion()
        {
            var listaTotal = vistaMiembros.SourceCollection.Cast<Miembro>().ToList();

            string busqueda = txtBuscar?.Text.ToLower();
            int conteo = listaTotal.Count(m => string.IsNullOrWhiteSpace(busqueda) || m.NombreCompleto.ToLower().Contains(busqueda)
                                          || m.Cedula.Contains(busqueda));

            totalPaginas = (int)Math.Ceiling((double)conteo / registrosPorPagina);
            if (totalPaginas == 0) totalPaginas = 1;

            lblPaginacion.Text = $"Página {paginaActual} de {totalPaginas}";

            contadorFiltrados = 0;
            vistaMiembros.Refresh();
        }
        private void BtnAnterior_Click (object sender, RoutedEventArgs e)
        {
            if(paginaActual > 1) { paginaActual--; ActualizarPaginacion(); }
        }
        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual < totalPaginas) { paginaActual++; ActualizarPaginacion(); }
        }

        private void BtnPrimera_Click(object sender, RoutedEventArgs e)
        {
            paginaActual = 1; ActualizarPaginacion();
        }

        private void BtnUltima_Click(object sender, RoutedEventArgs e)
        {
            paginaActual = totalPaginas; ActualizarPaginacion();
        }
        private void BtnUsuario_Click (object sender, RoutedEventArgs e)
        {
            btnUsuario.ContextMenu.PlacementTarget = btnUsuario;
            btnUsuario.ContextMenu.IsOpen = true;
        }
        private void MenuItem_CambiarUsuario_Click (object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
        private void MenuItem_CerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var respuesta = MessageBox.Show("¿Está seguro que desea cerrar sesión?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void ImprimirReporte_Click (object sender, RoutedEventArgs e)
        {
            FlowDocument doc = new FlowDocument();
            doc.PagePadding = new Thickness(50);

            System.Windows.Documents.Paragraph titulo = new System.Windows.Documents.Paragraph();

            titulo.Inlines.Add(new System.Windows.Documents.Run("JUNTA DE ACCIÓN COMUNAL - LISTADO DE MIEMBROS"));

            titulo.FontSize = 22;
            titulo.FontWeight = FontWeights.Bold;
            titulo.TextAlignment = System.Windows.TextAlignment.Center;

            doc.Blocks.Add(titulo);

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("Fecha de emisión: " + DateTime.Now.ToShortDateString())) { 
                TextAlignment = System.Windows.TextAlignment.Right,
                FontSize = 12,
                Margin = new Thickness(0,0,0,20)
            });

            System.Windows.Documents.Table tabla = new System.Windows.Documents.Table();
            tabla.CellSpacing = 0;
            tabla.BorderBrush = Brushes.Black;
            tabla.BorderThickness = new Thickness(0,1,0,1);

            tabla.Columns.Add(new TableColumn() { Width = new GridLength(120) });
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(480) });
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(150) });

            TableRowGroup filaGrupo = new TableRowGroup();
            TableRow encabezado = new TableRow();
            encabezado.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("Cédula"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5) });
            encabezado.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("Nombre"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5) });
            encabezado.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("Telefono"))) { FontWeight = FontWeights.Bold, Padding = new Thickness(5) });
            filaGrupo.Rows.Add(encabezado);

            foreach(Miembro m in vistaMiembros.Cast<Miembro>())
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(m.Cedula))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(m.NombreCompleto))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(m.Telefono))) { Padding = new Thickness(5) });
                filaGrupo.Rows.Add(row);
            }

            tabla.RowGroups.Add(filaGrupo);
            doc.Blocks.Add(tabla);

            PrintDialog pd = new PrintDialog();
            if(pd.ShowDialog() == true)
            {
                doc.PageWidth = pd.PrintableAreaWidth;
                doc.PageHeight = pd.PrintableAreaHeight;
                doc.ColumnWidth = pd.PrintableAreaWidth;
                DocumentPaginator paginador = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                
                pd.PrintDocument(paginador, "Reporte_Junta_Comunal");
            }
        }
    }
    

    
}
