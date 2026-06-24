//Falta guardar el viaje en el historial, eso será en develop si

string private_trip()
{


    Console.WriteLine("=== CÁLCULO DE VIAJE EN VEHÍCULO PRIVADO ===\n");


    Console.WriteLine("\nSeleccione su lugar de origen:");

    var places = ReadPlacesFile();
    var (originLat, originLon) = SelectPlace(places);
    var (destLat, destLon) = SelectPlace(places);

    double distance = haversine(originLat, originLon, destLat, destLon);
    distance = road_factor(distance);

    (int selected_consumption, string selected_gas) = selectCarType();
    double totalConsumption = distance * selected_consumption;
    double totalCost = get_trip_cost(totalConsumption, selected_gas.ToLower());

    return $"El costo aproximado de su viaje es de C$ {totalCost}.";


}

void budget_trip()
{

    double ask_budget()
    {
        bool valido = false;
        double budget = 0.0;
        do
        {
            Console.Write("Ingrese su presupuesto en cordobas: ");
            if (double.TryParse(Console.ReadLine(), out budget))
            {
                valido = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Entrada invalida. Intente de nuevo");
                Console.ResetColor();
            }
        } while (!valido);
        return budget;
    }

    double budget = ask_budget();
    var places = ReadPlacesFile();
    var (Lat, Lon) = SelectPlace(places);
    (int selected_consumption, string selected_gas) = selectCarType();
    double gas_price = get_gas_price(selected_gas);

    double max_distance = budget * selected_consumption / gas_price;

    //Continuará sienddo desarrollada en develop dado que necesita historial
    Console.WriteLine($"La distancia máxima que puede recorrer es: {max_distance} km");


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

(int, string) selectCarType()
{

    var carTypes = new List<(string Tipo, string Combustible, int Consumo)>
    {
        ("Sedan", "Gasoline", 45),
        ("Camioneta", "Diesel", 36),
        ("Microbus", "Diesel", 31),
        ("SUV", "Gasoline", 35),
        ("Camion", "Diesel", 18),
        ("Moto", "Gasoline", 120),
        ("Mototaxi", "Gasoline", 70)
    };

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

    return (carTypes[op - 1].Consumo, carTypes[op - 1].Combustible);

}

double haversine(double lat1, double lon1, double lat2, double lon2)
{
    double to_radians(double data)
    {
        return data * Math.PI / 180.0;
    }

    const double EARTH_RADIUS = 6371.0;
    lat1 = to_radians(lat1);
    lon1 = to_radians(lon1);
    lat2 = to_radians(lat2);
    lon2 = to_radians(lon2);

    double dlat = lat2 - lat1;
    double dlon = lon2 - lon1;

    double a =
        Math.Pow(Math.Sin(dlat / 2), 2) +
        Math.Cos(lat1) *
        Math.Cos(lat2) *
        Math.Pow(Math.Sin(dlon / 2), 2);

    double c = 2 * Math.Asin(Math.Sqrt(a));
    return EARTH_RADIUS * c;

}

double road_factor(double d)
{
    if (d < 5)
    {
        return 1.3;
    }
    else if (d < 20)
    {
        return 1.2;
    }
    else
    {
        return 1.15;
    }
}

double get_trip_cost(double consumption, string gas)
{

    double price = get_gas_price(gas);
    return consumption * price;
}

double get_gas_price(string gas)
{
    const double DIESEL = 43.21;
    const double GASOLINE = 48.98;

    if (gas == "diesel")
    {
        return DIESEL;
    }
    else if (gas == "gasoline")
    {
        return GASOLINE;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Problema al obtener precio de gasolina.");
        Console.ResetColor();
        return 0;
    }
}
