using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MemoriaDinamica
{
    public partial class FormPrincipal : Form
    {
        // Lista enlazada que el usuario manipula en la pestaña 2 (vive en el heap).
        private ListaEnlazada<Alumno> grupo = new ListaEnlazada<Alumno>();

        // ---------- Pestaña 1: valor y referencia ----------
        private readonly Button btnValorReferencia = new() { Text = "Caso 1: valor contra referencia", AutoSize = true };
        private readonly Button btnReferencias = new() { Text = "Caso 2: referencias compartidas", AutoSize = true };
        private readonly TextBox txtResultadoReferencias = CrearBitacora();

        // ---------- Pestaña 2: lista enlazada ----------
        private readonly TextBox txtNombre = new() { Width = 140 };
        private readonly NumericUpDown nudCalificacion = new() { Minimum = 0, Maximum = 100, Value = 90, Width = 70 };
        private readonly Button btnInsertar = new() { Text = "Insertar al final", AutoSize = true };
        private readonly Button btnEliminar = new() { Text = "Eliminar por nombre", AutoSize = true };
        private readonly Button btnEjemplo = new() { Text = "Cargar ejemplo", AutoSize = true };
        private readonly Button btnVaciar = new() { Text = "Vaciar lista", AutoSize = true };
        private readonly Label lblCantidad = new() { AutoSize = true, Padding = new Padding(4), Text = "Cantidad de nodos: 0" };
        private readonly FlowLayoutPanel panelNodos = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke };
        private readonly TextBox txtBitacora = CrearBitacora();

        // ---------- Pestaña 3: memoria y rendimiento ----------
        private readonly NumericUpDown nudCantidad = new()
        {
            Minimum = 100_000,
            Maximum = 5_000_000,
            Increment = 100_000,
            Value = 1_000_000,
            Width = 120,
            ThousandsSeparator = true
        };
        private readonly Button btnMedir = new() { Text = "Medir memoria y rendimiento", AutoSize = true };
        private readonly DataGridView dgvMedicion = CrearTabla("Medición", "Arreglo int[]", "ListaEnlazada<int>");

        public FormPrincipal()
        {
            Text = "Tarea 1.2 - Programa 2: memoria dinámica con instancias y referencias";
            Size = new Size(1050, 680);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            var pestanas = new TabControl { Dock = DockStyle.Fill };
            pestanas.TabPages.Add(CrearPestanaReferencias());
            pestanas.TabPages.Add(CrearPestanaLista());
            pestanas.TabPages.Add(CrearPestanaMedicion());
            Controls.Add(pestanas);

            btnValorReferencia.Click += (s, e) => MostrarResultado(PruebasMemoria.ValorContraReferencia());
            btnReferencias.Click += (s, e) => MostrarResultado(PruebasMemoria.ReferenciasCompartidas());
            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnEjemplo.Click += BtnEjemplo_Click;
            btnVaciar.Click += BtnVaciar_Click;
            btnMedir.Click += BtnMedir_Click;

            DibujarLista();
        }

        // =====================================================================
        //  Construcción de la interfaz
        // =====================================================================

        private TabPage CrearPestanaReferencias()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            barra.Controls.AddRange(new Control[] { btnValorReferencia, btnReferencias });

            var tabla = TablaVertical(SizeType.AutoSize, SizeType.Percent);
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(txtResultadoReferencias, 0, 1);

            var pagina = new TabPage("1. Valor y referencia") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        private TabPage CrearPestanaLista()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            barra.Controls.AddRange(new Control[]
            {
            Etiqueta("Nombre:"), txtNombre, Etiqueta("Calificación:"), nudCalificacion,
            btnInsertar, btnEliminar, btnEjemplo, btnVaciar
            });

            var tabla = TablaVertical(SizeType.AutoSize, SizeType.AutoSize, SizeType.Percent, SizeType.Absolute);
            tabla.RowStyles[3].Height = 160;
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(lblCantidad, 0, 1);
            tabla.Controls.Add(panelNodos, 0, 2);
            tabla.Controls.Add(txtBitacora, 0, 3);

            var pagina = new TabPage("2. Lista enlazada") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        private TabPage CrearPestanaMedicion()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            barra.Controls.AddRange(new Control[] { Etiqueta("Cantidad de enteros:"), nudCantidad, btnMedir });

            var nota = Etiqueta("Los tiempos dependen del equipo. Para resultados más estables ejecuta en modo Release.");
            nota.ForeColor = Color.DimGray;

            var tabla = TablaVertical(SizeType.AutoSize, SizeType.Percent, SizeType.AutoSize);
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(dgvMedicion, 0, 1);
            tabla.Controls.Add(nota, 0, 2);

            var pagina = new TabPage("3. Memoria y rendimiento") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        // =====================================================================
        //  Eventos
        // =====================================================================

        private void MostrarResultado(string texto)
        {
            txtResultadoReferencias.AppendText(texto + Environment.NewLine);
        }

        private void BtnInsertar_Click(object? sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            if (nombre.Length == 0)
            {
                MessageBox.Show("Escribe un nombre.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var alumno = new Alumno(nombre, (int)nudCalificacion.Value);   // nuevo objeto en el heap
            grupo.InsertarAlFinal(alumno);                                 // nuevo nodo en el heap
            Escribir($"Insertar: se crearon un objeto Alumno y un Nodo con {alumno}. Cantidad = {grupo.Cantidad}");
            txtNombre.Clear();
            txtNombre.Focus();
            DibujarLista();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            bool eliminado = grupo.EliminarPrimero(a => a.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (eliminado)
                Escribir($"Eliminar: el nodo de \"{nombre}\" quedó sin referencias; el GC liberará su memoria. Cantidad = {grupo.Cantidad}");
            else
                Escribir($"Eliminar: no existe ningún alumno llamado \"{nombre}\".");

            DibujarLista();
        }

        private void BtnEjemplo_Click(object? sender, EventArgs e)
        {
            string[] nombres = { "Ana", "Luis", "Marta", "Pedro", "Sofía" };
            int[] calificaciones = { 85, 92, 78, 88, 95 };

            for (int i = 0; i < nombres.Length; i++)
            {
                grupo.InsertarAlFinal(new Alumno(nombres[i], calificaciones[i]));
            }
            Escribir($"Ejemplo: se insertaron {nombres.Length} alumnos. Cantidad = {grupo.Cantidad}");
            DibujarLista();
        }

        private void BtnVaciar_Click(object? sender, EventArgs e)
        {
            // La lista anterior deja de estar referenciada: el GC liberará todos sus nodos.
            grupo = new ListaEnlazada<Alumno>();
            Escribir("Vaciar: la lista anterior quedó sin referencias y el recolector de basura la liberará.");
            DibujarLista();
        }

        private async void BtnMedir_Click(object? sender, EventArgs e)
        {
            int cantidad = (int)nudCantidad.Value;
            btnMedir.Enabled = false;
            UseWaitCursor = true;
            dgvMedicion.Rows.Clear();

            try
            {
                // Se mide en un hilo secundario para que la ventana no se congele.
                ResultadoMedicion r = await Task.Run(() => PruebasMemoria.Medir(cantidad));

                dgvMedicion.Rows.Add("Memoria total", Megabytes(r.BytesArreglo), Megabytes(r.BytesLista));
                dgvMedicion.Rows.Add("Bytes por elemento", (r.BytesArreglo / (double)cantidad).ToString("F1"), (r.BytesLista / (double)cantidad).ToString("F1"));
                dgvMedicion.Rows.Add("Recorrer 10 veces", $"{r.MsRecorridoArreglo:F2} ms", $"{r.MsRecorridoLista:F2} ms");
                dgvMedicion.Rows.Add($"Acceso a la posición {cantidad / 2:N0}", $"{r.MsAccesoArreglo:F4} ms", $"{r.MsAccesoLista:F4} ms");
                dgvMedicion.Rows.Add("Memoria tras soltar referencias y recolectar", Megabytes(Math.Max(0, r.BytesAlFinal)), "(misma medición)");
            }
            finally
            {
                btnMedir.Enabled = true;
                UseWaitCursor = false;
            }
        }

        // =====================================================================
        //  Auxiliares
        // =====================================================================

        /// <summary>Dibuja la lista como cajas unidas por flechas, terminando en null.</summary>
        private void DibujarLista()
        {
            panelNodos.SuspendLayout();
            panelNodos.Controls.Clear();

            panelNodos.Controls.Add(Caja("cabeza", Color.Khaki, 70));
            panelNodos.Controls.Add(Flecha());

            foreach (Alumno alumno in grupo.Recorrer())
            {
                panelNodos.Controls.Add(Caja($"{alumno.Nombre}{Environment.NewLine}{alumno.Calificacion}", Color.LightSkyBlue, 90));
                panelNodos.Controls.Add(Flecha());
            }
            panelNodos.Controls.Add(Caja("null", Color.Gainsboro, 60));

            panelNodos.ResumeLayout();
            lblCantidad.Text = $"Cantidad de nodos: {grupo.Cantidad}";
        }

        private static Label Caja(string texto, Color color, int ancho) => new()
        {
            Text = texto,
            Size = new Size(ancho, 52),
            Margin = new Padding(2, 6, 2, 6),
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = color
        };

        private static Label Flecha() => new()
        {
            Text = "→",
            Size = new Size(26, 52),
            Margin = new Padding(0, 6, 0, 6),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        private static string Megabytes(long bytes) => $"{bytes / 1024.0 / 1024.0:F2} MB";

        private void Escribir(string mensaje) => txtBitacora.AppendText(mensaje + Environment.NewLine);

        private static Label Etiqueta(string texto) =>
            new() { Text = texto, AutoSize = true, Margin = new Padding(3, 9, 3, 3) };

        private static TextBox CrearBitacora() => new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10F),
            BackColor = Color.White
        };

        private static DataGridView CrearTabla(params string[] columnas)
        {
            var tabla = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = SystemColors.Window
            };
            foreach (string columna in columnas)
            {
                tabla.Columns.Add(columna, columna);
            }
            return tabla;
        }

        /// <summary>Crea un TableLayoutPanel de una columna con filas del tipo indicado.</summary>
        private static TableLayoutPanel TablaVertical(params SizeType[] filas)
        {
            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = filas.Length };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            foreach (SizeType tipo in filas)
            {
                tabla.RowStyles.Add(tipo == SizeType.Percent ? new RowStyle(SizeType.Percent, 100F) : new RowStyle(tipo));
            }
            return tabla;
        }
    }
}
