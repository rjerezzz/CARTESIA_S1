void private_trip()
{
    var carTypes;

    Console.WriteLine("=== CÁLCULO DE VIAJE EN VEHÍCULO PRIVADO ===\n");

    for (int i = 0; i < carTypes.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {carTypes[i].Tipo}");
    }

    Console.WriteLine();
    Console.WriteLine("Seleccione la categoría de su vehículo:");

    bool valid = false;
    int op = 0;

    do
    {
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out op))
        {
            if (op >= 1 && op <= carTypes.Count)
            {
                valid = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("La opción seleccionada no existe. Intente nuevamente.");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Debe ingresar un número válido.");
            Console.ResetColor();
        }

    } while (!valid);

    var selectedCar = carTypes[op - 1];

    Console.WriteLine("\nSeleccione su lugar de origen:");

    var places = ReadPlacesFile();
    var (originLat, originLon) = SelectPlace(places);
    var (destLat, destLon) = SelectPlace(places);

}


Dictionary<string, Dictionary<string, double>> ReadPlacesFile()
{
    string[] placeLines = File.ReadAllLines("ubicaciones.txt");

    var places = new Dictionary<string, Dictionary<string, double>>();

    for (int i = 1; i < placeLines.Length; i++)
    {
        string[] separated = placeLines[i].Split(',');

        string name = separated[0];

        try
        {
            double lat = double.Parse(separated[1]);
            double lon = double.Parse(separated[2]);

            places[name] = new Dictionary<string, double>
            {
                { "lat", lat },
                { "lon", lon }
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"No fue posible procesar la línea {i}: {ex.Message}");
            Console.ResetColor();
        }
    }

    return places;
}

(double lat, double lon) SelectPlace(Dictionary<string, Dictionary<string, double>> places)
{
    var placeNames = places.Keys.ToList();

    for (int i = 0; i < placeNames.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {placeNames[i]}");
    }

    Console.WriteLine($"{placeNames.Count + 1}. Mi ubicación no aparece en la lista");

    bool valid = false;
    int op = 0;

    do
    {
        Console.Write("\nOpción: ");

        if (int.TryParse(Console.ReadLine(), out op))
        {
            if (op >= 1 && op <= placeNames.Count + 1)
            {
                valid = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("La opción seleccionada no existe. Intente nuevamente.");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Debe ingresar un número válido.");
            Console.ResetColor();
        }

    } while (!valid);

    double originLat = places[placeNames[op - 1]]["lat"];
    double originLon = places[placeNames[op - 1]]["lon"];

    return (originLat, originLon);
}
