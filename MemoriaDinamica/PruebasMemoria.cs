using System.Diagnostics;
using System.Text;

namespace MemoriaDinamica;

/// <summary>Resultados de comparar un arreglo con una lista enlazada del mismo tamaño.</summary>
public record ResultadoMedicion(
    int Cantidad,
    long BytesArreglo, long BytesLista, long BytesAlFinal,
    double MsRecorridoArreglo, double MsRecorridoLista,
    double MsAccesoArreglo, double MsAccesoLista);

/// <summary>Lógica del Programa 2, separada del formulario.</summary>
public static class PruebasMemoria
{
    /// <summary>Caso 1: un struct se copia; una clase comparte el mismo objeto.</summary>
    public static string ValorContraReferencia()
    {
        var texto = new StringBuilder();

        var punto1 = new PuntoValor { X = 1, Y = 1 };
        var punto2 = punto1;      // se copian los datos
        punto2.X = 50;
        texto.AppendLine("struct (tipo por valor): punto2 = punto1; punto2.X = 50;");
        texto.AppendLine($"   punto1.X = {punto1.X}, punto2.X = {punto2.X}  -> son copias independientes");
        texto.AppendLine();

        var alumno1 = new Alumno("Ana", 80);
        var alumno2 = alumno1;    // se copia solo la referencia
        alumno2.Calificacion = 100;
        texto.AppendLine("class (tipo por referencia): alumno2 = alumno1; alumno2.Calificacion = 100;");
        texto.AppendLine($"   alumno1 = {alumno1}, alumno2 = {alumno2}  -> ambas variables apuntan al mismo objeto");
        return texto.ToString();
    }

    /// <summary>Caso 2: varias referencias a un objeto y el momento en que deja de ser alcanzable.</summary>
    public static string ReferenciasCompartidas()
    {
        var texto = new StringBuilder();

        Alumno? original = new Alumno("Luis", 90);
        Alumno? otraReferencia = original;
        texto.AppendLine($"¿original y otraReferencia apuntan al mismo objeto? {ReferenceEquals(original, otraReferencia)}");

        Alumno copiaNueva = new Alumno(original.Nombre, original.Calificacion);
        texto.AppendLine($"¿Un objeto nuevo con los mismos datos es el mismo objeto? {ReferenceEquals(original, copiaNueva)}");

        original = null;   // se pierde una referencia, pero el objeto sigue vivo
        texto.AppendLine($"original = null, pero otraReferencia sigue mostrando: {otraReferencia}");
        texto.AppendLine("Cuando ninguna variable lo referencie, el recolector de basura podrá liberarlo.");
        return texto.ToString();
    }

    /// <summary>Casos 4 y 5: memoria y tiempo de un arreglo contra una lista enlazada.</summary>
    public static ResultadoMedicion Medir(int cantidad)
    {
        // ---------- Memoria ----------
        long inicio = GC.GetTotalMemory(forceFullCollection: true);
        int[]? arreglo = new int[cantidad];
        long conArreglo = GC.GetTotalMemory(true);

        ListaEnlazada<int>? lista = new ListaEnlazada<int>();
        for (int i = 0; i < cantidad; i++)
        {
            arreglo[i] = i;
            lista.InsertarAlFinal(i);
        }
        long conLista = GC.GetTotalMemory(true);

        // ---------- Rendimiento: recorrido completo (10 repeticiones) ----------
        var cronometro = Stopwatch.StartNew();
        long sumaArreglo = 0;
        for (int r = 0; r < 10; r++)
            for (int i = 0; i < arreglo.Length; i++) sumaArreglo += arreglo[i];
        double msRecorridoArreglo = cronometro.Elapsed.TotalMilliseconds;

        cronometro.Restart();
        long sumaLista = 0;
        for (int r = 0; r < 10; r++)
            foreach (int valor in lista.Recorrer()) sumaLista += valor;
        double msRecorridoLista = cronometro.Elapsed.TotalMilliseconds;

        // ---------- Rendimiento: acceso a la posición central ----------
        int mitad = cantidad / 2;
        cronometro.Restart();
        int valorArreglo = arreglo[mitad];                 // acceso directo por índice
        double msAccesoArreglo = cronometro.Elapsed.TotalMilliseconds;

        cronometro.Restart();
        int valorLista = lista.ObtenerEn(mitad);           // recorre nodo por nodo
        double msAccesoLista = cronometro.Elapsed.TotalMilliseconds;

        if (sumaArreglo != sumaLista || valorArreglo != valorLista)
            throw new InvalidOperationException("Las dos estructuras no contienen los mismos datos.");

        // ---------- Liberación ----------
        // Se sueltan las referencias. GC.Collect solo se llama aquí con fines demostrativos.
        arreglo = null;
        lista = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long alFinal = GC.GetTotalMemory(true);

        return new ResultadoMedicion(cantidad,
            conArreglo - inicio, conLista - conArreglo, alFinal - inicio,
            msRecorridoArreglo, msRecorridoLista, msAccesoArreglo, msAccesoLista);
    }
}