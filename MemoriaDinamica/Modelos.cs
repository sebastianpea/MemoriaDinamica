namespace MemoriaDinamica;

/// <summary>
/// Tipo por referencia: cada "new Alumno(...)" reserva un objeto en el heap administrado
/// y la variable solo guarda la referencia (la dirección) a ese objeto.
/// </summary>
public class Alumno
{
    public string Nombre { get; set; }
    public int Calificacion { get; set; }

    public Alumno(string nombre, int calificacion)
    {
        Nombre = nombre;
        Calificacion = calificacion;
    }

    public override string ToString() => $"{Nombre} ({Calificacion})";
}

/// <summary>Tipo por valor: al asignarlo a otra variable se copian sus datos.</summary>
public struct PuntoValor
{
    public int X;
    public int Y;
}

/// <summary>Nodo de una lista enlazada: guarda un dato y la referencia al siguiente nodo.</summary>
public class Nodo<T>
{
    public T Dato;
    public Nodo<T>? Siguiente;

    public Nodo(T dato)
    {
        Dato = dato;
    }
}

/// <summary>
/// Lista enlazada simple. No tiene capacidad fija: cada inserción crea un nodo nuevo
/// en el heap y cada eliminación deja un nodo sin referencias para el recolector de basura.
/// </summary>
public class ListaEnlazada<T>
{
    private Nodo<T>? cabeza;   // primer nodo
    private Nodo<T>? cola;     // último nodo (permite insertar al final en O(1))

    public int Cantidad { get; private set; }

    public void InsertarAlFinal(T dato)
    {
        var nuevo = new Nodo<T>(dato);
        if (cola is null)
        {
            cabeza = nuevo;
        }
        else
        {
            cola.Siguiente = nuevo;
        }
        cola = nuevo;
        Cantidad++;
    }

    /// <summary>Elimina el primer elemento que cumpla la condición. Devuelve false si no existe.</summary>
    public bool EliminarPrimero(Predicate<T> condicion)
    {
        Nodo<T>? anterior = null;
        Nodo<T>? actual = cabeza;

        while (actual is not null)
        {
            if (condicion(actual.Dato))
            {
                if (anterior is null) cabeza = actual.Siguiente;   // era el primero
                else anterior.Siguiente = actual.Siguiente;        // se "salta" el nodo

                if (actual == cola) cola = anterior;               // era el último
                Cantidad--;
                return true;
            }
            anterior = actual;
            actual = actual.Siguiente;
        }
        return false;
    }

    /// <summary>Devuelve el elemento de una posición recorriendo los nodos (no hay acceso directo).</summary>
    public T ObtenerEn(int posicion)
    {
        if (posicion < 0 || posicion >= Cantidad)
            throw new ArgumentOutOfRangeException(nameof(posicion));

        Nodo<T> actual = cabeza!;
        for (int i = 0; i < posicion; i++)
        {
            actual = actual.Siguiente!;
        }
        return actual.Dato;
    }

    /// <summary>Recorre la lista desde la cabeza hasta encontrar null.</summary>
    public IEnumerable<T> Recorrer()
    {
        for (Nodo<T>? actual = cabeza; actual is not null; actual = actual.Siguiente)
        {
            yield return actual.Dato;
        }
    }
}