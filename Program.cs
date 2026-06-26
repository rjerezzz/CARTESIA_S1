class Program
{
    static void Main()
    {
        MostrarEstadisticas();
    }

    static void MostrarEstadisticas()
    {
string archivo = "historial.txt";

// Abrir historial.txt
if (!File.Exists(archivo) || File.ReadAllLines(archivo).Length == 0)
{
Console.WriteLine("\nNo hay registros en el historial.");
return;
}

string[] registros = File.ReadAllLines(archivo);

int totalViajes = registros.Length;

double costoTotal = 0;
double viajeMasCaro = double.MinValue;
double viajeMasBarato = double.MaxValue;

string destinoMasFrecuente = "";
int mayorFrecuencia = 0;

var destinos = new Dictionary<string, int>();

foreach (string registro in registros)
{
// Formato esperado:
// Destino;Costo
string[] datos = registro.Split(';');

string destino = datos[0];
double costo = Convert.ToDouble(datos[1]);

costoTotal += costo;

if (costo > viajeMasCaro)
viajeMasCaro = costo;

if (costo < viajeMasBarato)
viajeMasBarato = costo;

if (destinos.ContainsKey(destino))
destinos[destino]++;
else
destinos[destino] = 1;
}

foreach (var destino in destinos)
{
if (destino.Value > mayorFrecuencia)
{
mayorFrecuencia = destino.Value;
destinoMasFrecuente = destino.Key;
}
}

double costoPromedio = costoTotal / totalViajes;

// Mostrar estadísticas
Console.WriteLine("\n=== ESTADÍSTICAS ===");
Console.WriteLine($"Total de viajes: {totalViajes}");
Console.WriteLine($"Costo promedio: {costoPromedio:C}");
Console.WriteLine($"Viaje más caro: {viajeMasCaro:C}");
Console.WriteLine($"Viaje más barato: {viajeMasBarato:C}");
Console.WriteLine($"Destino más frecuente: {destinoMasFrecuente}");
}
