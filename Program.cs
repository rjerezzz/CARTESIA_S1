using System.Text.Json;
using System;

string rutaArchivo = "ubicaciones.txt";
string rutaHistorial = "historial.txt";
string rutaRutas = "timetable.json";

const int MAX_UBICACIONES = 100;
Ubicacion[] ubicaciones = new Ubicacion[MAX_UBICACIONES];
int totalUbicaciones = 0;

await Main();

async Task Main()
{
    cargarUbicaciones();
    await mostrarMenuPrincipal();
}

async Task mostrarMenuPrincipal()
{
    Console.Clear();
    int opcion;

    do
    {

        Console.WriteLine("\n=== MENU PRINCIPAL ===\n");
        Console.WriteLine("1. Administrar ubicaciones");
        Console.WriteLine("2. Calcular Viaje Privado");
        Console.WriteLine("3. Transporte Público");
        Console.WriteLine("4. Simulador Presupuesto");
        Console.WriteLine("5. Historial");
        Console.WriteLine("6. Salir");

        Console.Write("\nIngrese su opción: ");

        if (!int.TryParse(Console.ReadLine(), out opcion))
        {
            Console.WriteLine("Opción inválida.");
            Console.ReadKey();
            continue;
        }

        Console.WriteLine();

        switch (opcion)
        {
            case 1:
                await administrarUbicaciones();
                break;

            case 2:
                Console.WriteLine(private_trip());
                break;

            case 3:
                public_transport();
                break;

            case 4:
                budget_trip();
                break;

            case 5:
                mostrarHistorial();
                break;

            case 6:
                Console.WriteLine("Saliendo del programa...");
                Console.Clear();
                return;

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Opción no válida.");
                Console.ResetColor();
                break;
        }
    } while (opcion != 7);
}

async Task administrarUbicaciones()
{
    int opcion;

    do
    {
        opcion = mostrarSubmenu();

        switch (opcion)
        {
            case 1: // Ver Ubicaciones
                mostrarArregloUbicaciones();
                break;

            case 2: // Agregar Ubicación
                await agregarUbicacionMenu();
                break;

            case 3: // Volver
                Console.WriteLine("Volviendo al menú anterior...");
                break;
        }

    } while (opcion != 3);
}

int mostrarSubmenu()
{
    int opcion;
    bool valido = false;

    do
    {
        Console.WriteLine("\n--- Administrar Ubicaciones ---");
        Console.WriteLine("1. Ver Ubicaciones");
        Console.WriteLine("2. Agregar Ubicación");
        Console.WriteLine("3. Volver");
        Console.Write("Seleccione una opción: ");

        if (int.TryParse(Console.ReadLine(), out opcion) && opcion >= 1 && opcion <= 3)
        {
            valido = true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida. Intente nuevamente.");
            Console.ResetColor();
        }
    } while (!valido);

    return opcion;
}

void mostrarArregloUbicaciones()
{
    Console.WriteLine("\n--- Ubicaciones Registradas ---");

    if (totalUbicaciones == 0)
    {
        Console.WriteLine("No hay ubicaciones registradas.");
        return;
    }

    for (int i = 0; i < totalUbicaciones; i++)
    {
        Console.WriteLine($"{i + 1}. Nombre: {ubicaciones[i].nombre}");
    }
}

string private_trip()
{

    Console.WriteLine("=== CÁLCULO DE VIAJE EN VEHÍCULO PRIVADO ===\n");


    Console.WriteLine("\nSeleccione su lugar de origen:");

    var places = ReadPlacesFile();
    var (originLat, originLon, originName) = SelectPlace(places);
    if (originName == "") return "Volviendo al menú anterior...";
    var (destLat, destLon, destName) = SelectPlace(places);
    if (destName == "") return "Volviendo al menú anterior...";

    double distance = haversine(originLat, originLon, destLat, destLon);
    distance = road_factor(distance);

    (int selected_consumption, string selected_gas) = selectCarType();
    if (selected_consumption == 0) return "Volviendo al menú anterior...";


    double litros = distance / selected_consumption;
    double totalCost = get_trip_cost(litros, selected_gas.ToLower());
    Console.WriteLine();

    Console.WriteLine($"El costo aproximado de su viaje es de C$ {totalCost}.");


    bool save = false;
    Console.WriteLine("Desea guardar este viaje en el historial? (s/n)");
    string response = Console.ReadLine();
    if (response.ToLower() == "s")
    {
        save = true;
    }

    if (save)
    {
        Console.WriteLine("Guardando viaje en el historial...");
        guardarViajeEnHistorial(originName, destName, distance, totalCost);
        return "Viaje guardado en el historial.";
    }
    else
    {
        return "Viaje no guardado en el historial.";
    }

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
    var (Lat, Lon, place) = SelectPlace(places);
    if (place == "") return;
    (int selected_consumption, string selected_gas) = selectCarType();
    if (selected_consumption == 0) return;
    double gas_price = get_gas_price(selected_gas);

    double max_distance = budget * selected_consumption / gas_price;
    Console.WriteLine($"La distancia máxima que puede recorrer es: {Math.Round(max_distance, 2)} km");
    string[] valid_options = new string[ubicaciones.Length];
    string largest = "";
    double largest_d = 0;
    for (int i = 0; i < totalUbicaciones; i++)
    {
        double distance = haversine(Lat, Lon, ubicaciones[i].latitud, ubicaciones[i].longitud);
        distance = road_factor(distance);
        if (distance <= max_distance)
        {
            valid_options[i] = ubicaciones[i].nombre;

            if (distance > largest_d)
            {
                largest_d = distance;
                largest = ubicaciones[i].nombre;
            }
        }
    }
    Console.WriteLine("\nCon tu presupuesto, puedes visitar los siguientes lugares previamente guardados:\n");
    foreach (var option in valid_options)
    {
        if (!string.IsNullOrEmpty(option))
        {
            Console.WriteLine(option);
        }
    }
    Console.WriteLine($"\nDestino más lejano alcanzable:\n {largest} ({Math.Round(largest_d, 2)} km)\n\n");

}

bool validarCoordenadas(double lat, double lon)
{
    const double minLat = 11.80;
    const double maxLat = 12.35;
    const double minLon = -86.50;
    const double maxLon = -85.90;

    return lat >= minLat && lat <= maxLat && lon >= minLon && lon <= maxLon;
}

void guardarUbicacionEnArchivo(string nombre, double lat, double lon)
{
    using StreamWriter archivo = new StreamWriter(rutaArchivo, append: true);
    archivo.WriteLine($"{nombre};{lat};{lon}");
}

void cargarUbicaciones()
{
    if (!File.Exists(rutaArchivo))
    {
        return;
    }

    StreamReader lector = new StreamReader(rutaArchivo);
    string? linea;

    while ((linea = lector.ReadLine()) != null && totalUbicaciones < MAX_UBICACIONES)
    {
        var partes = linea.Split(';');
        if (partes.Length >= 3 &&
            double.TryParse(partes[1], out double lat) &&
            double.TryParse(partes[2], out double lon))
        {
            ubicaciones[totalUbicaciones].nombre = partes[0];
            ubicaciones[totalUbicaciones].latitud = lat;
            ubicaciones[totalUbicaciones].longitud = lon;
            totalUbicaciones++;
        }
    }

    lector.Close();
}

async Task agregarUbicacionMenu()
{
    int opcion = mostrarSubmenuAgregar();

    switch (opcion)
    {
        case 1: // Buscar ubicación
            await BuscarUbicacionPorApi();
            break;

        case 2: // Agregar manualmente
            agregarUbicacionManual();
            break;
    }
}

async Task BuscarUbicacionPorApi()
{
    if (totalUbicaciones >= MAX_UBICACIONES)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se pueden agregar más ubicaciones, el arreglo está lleno.");
        Console.ResetColor();
    }

    bool valido = false;
    string lugar = "";
    do
    {
        Console.Write("Ingresa el nombre del lugar a buscar: ");
        lugar = Console.ReadLine();
        if (!string.IsNullOrEmpty(lugar))
        {
            valido = true;
        }
    } while (!valido);

    var parametros = new Dictionary<string, string>
    {
        ["type"] = "search",
        ["hl"] = "es",
        ["engine"] = "google_maps",
        ["api_key"] = "0f623482556b342af3e7117b162e8b72e6dbfbb1c2b50881a6e542bf3897f3d5",
        ["gl"] = "ni",
        ["q"] = lugar,
        ["ll"] = "@12.045,-86.19,11z"
    };

    using HttpClient client = new();

    string url = "https://serpapi.com/search.json?" +
                 string.Join("&", parametros.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

    string respuesta;
    try
    {
        respuesta = await client.GetStringAsync(url);
    }
    catch (HttpRequestException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se pudo conectar con el servicio de búsqueda. Intente más tarde.");
        Console.ResetColor();
        return;
    }

    using JsonDocument doc = JsonDocument.Parse(respuesta);
    var raiz = doc.RootElement;

    if (!raiz.TryGetProperty("local_results", out var resultados))
    {
        Console.WriteLine("No se encontraron resultados para esta búsqueda en la zona.");
    }

    var lugaresEncontrados = new List<(string nombre, double lat, double lng)>();

    foreach (var negocio in resultados.EnumerateArray().Take(5))
    {
        string nombre = negocio.GetProperty("title").GetString();

        if (negocio.TryGetProperty("gps_coordinates", out var gps))
        {
            double lat = gps.GetProperty("latitude").GetDouble();
            double lng = gps.GetProperty("longitude").GetDouble();
            lugaresEncontrados.Add((nombre, lat, lng));
        }
    }

    lugaresEncontrados.RemoveAll(l => !validarCoordenadas(l.lat, l.lng));

    if (lugaresEncontrados.Count == 0)
    {
        Console.WriteLine("No se encontraron resultados dentro del área permitida (Managua y Masaya).");
    }

    Console.WriteLine("\n--- Resultados obtenidos ---");
    for (int i = 0; i < lugaresEncontrados.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {lugaresEncontrados[i].nombre}");
    }

    int seleccion;
    bool seleccionValida = false;
    do
    {
        Console.Write("Seleccione el lugar: ");
        if (int.TryParse(Console.ReadLine(), out seleccion) && seleccion >= 1 && seleccion <= lugaresEncontrados.Count)
        {
            seleccionValida = true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida. Intente nuevamente.");
            Console.ResetColor();
        }
    } while (!seleccionValida);

    var elegido = lugaresEncontrados[seleccion - 1];

    ubicaciones[totalUbicaciones].nombre = elegido.nombre;
    ubicaciones[totalUbicaciones].latitud = elegido.lat;
    ubicaciones[totalUbicaciones].longitud = elegido.lng;
    totalUbicaciones++;

    guardarUbicacionEnArchivo(elegido.nombre, elegido.lat, elegido.lng);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Ubicación agregada correctamente desde la búsqueda.");
    Console.ResetColor();

}

int mostrarSubmenuAgregar()
{
    int opcion;
    bool valido = false;

    do
    {
        Console.WriteLine("\n--- Agregar Ubicación ---");
        Console.WriteLine("1. Buscar ubicación");
        Console.WriteLine("2. Agregar manualmente");
        Console.WriteLine("3. Volver");
        Console.Write("Seleccione una opción: ");

        if (int.TryParse(Console.ReadLine(), out opcion) && opcion >= 1 && opcion <= 3)
        {
            valido = true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida. Intente nuevamente.");
            Console.ResetColor();
        }
    } while (!valido);

    return opcion;
}

void agregarUbicacionManual()
{
    if (totalUbicaciones >= MAX_UBICACIONES)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se pueden agregar más ubicaciones, el arreglo está lleno.");
        Console.ResetColor();
        return;
    }

    string nombre = ingresarNombreUbicacion();
    var (lat, lon) = ingresarLongitudLatitud();

    ubicaciones[totalUbicaciones].nombre = nombre;
    ubicaciones[totalUbicaciones].latitud = lat;
    ubicaciones[totalUbicaciones].longitud = lon;
    totalUbicaciones++;

    guardarUbicacionEnArchivo(nombre, lat, lon);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Ubicación agregada correctamente.");
    Console.ResetColor();
}

string ingresarNombreUbicacion()
{
    string nombre;
    bool valido = false;

    do
    {
        Console.Write("Ingrese el nombre de la ubicación: ");
        nombre = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            valido = true;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("El nombre no puede estar vacío. Intente nuevamente.");
            Console.ResetColor();
        }
    } while (!valido);

    return nombre;
}

(double lat, double lon) ingresarLongitudLatitud()
{
    double lat, lon;
    bool valido = false;
    Console.WriteLine("Puede revisar las coordenadas de su ubicación en el siguiente enlace: https://www.latlong.net/");

    do
    {
        Console.Write("Ingrese la latitud: ");
        bool latValida = double.TryParse(Console.ReadLine(), out lat);

        Console.Write("Ingrese la longitud: ");
        bool lonValida = double.TryParse(Console.ReadLine(), out lon);

        if (!latValida || !lonValida)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Debe ingresar valores numéricos válidos.");
            Console.ResetColor();
            continue;
        }

        if (!validarCoordenadas(lat, lon))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Las coordenadas están fuera del área permitida (Managua y Masaya, Nicaragua). Intente nuevamente.");
            Console.ResetColor();
            continue;
        }

        valido = true;

    } while (!valido);

    return (lat, lon);
}



void guardarViajeEnHistorial(string origen, string destino, double distancia, double costo)
{
    using (StreamWriter sw = new StreamWriter(rutaHistorial, true))
    {
        sw.WriteLine($"{origen};{destino};{distancia:F2};{costo:F2}");
    }
}

(int, string) selectCarType()
{

    Console.Clear();
    var carTypes = new List<(string Tipo, string Combustible, int Consumo)>
    {
        ("Sedán",      "Gasoline", 14),
        ("Camioneta",  "Diesel",   10),
        ("Microbús",   "Diesel",    8),
        ("SUV",        "Gasoline", 11),
        ("Camión",     "Diesel",    4),
        ("Moto",       "Gasoline", 35),
        ("Mototaxi",   "Gasoline", 28)
    };

    for (int i = 0; i < carTypes.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {carTypes[i].Tipo}");
    }
    Console.WriteLine("0. Volver");

    Console.WriteLine();
    Console.WriteLine("Seleccione la categoría de su vehículo:");

    bool valid = false;
    int op = 0;

    do
    {
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out op))
        {
            if (op == 0) return (0, "");
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
        return d * 1.3;
    }
    else if (d < 20)
    {
        return d * 1.2;
    }
    else
    {
        return d * 1.15;
    }
}

double get_trip_cost(double consumption, string gas)
{

    double price = get_gas_price(gas);
    return Math.Round(consumption * price, 2);
}

double get_gas_price(string gas)
{
    const double DIESEL = 43.21;
    const double GASOLINE = 48.98;
    gas = gas.ToLower();

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

List<(string nombre, double latitud, double longitud)> ReadPlacesFile()
{
    var places = new List<(string nombre, double latitud, double longitud)>();

    for (int i = 0; i < totalUbicaciones; i++)
    {
        places.Add((ubicaciones[i].nombre, ubicaciones[i].latitud, ubicaciones[i].longitud));
    }

    return places;
}

(double latitud, double longitud, string nombre) SelectPlace(List<(string nombre, double latitud, double longitud)> places)
{
    Console.Clear();
    int opcion;
    bool valido = false;

    do
    {
        for (int i = 0; i < places.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {places[i].nombre}");
        }
        Console.WriteLine("0. Volver");

        Console.Write("\nSeleccione una opción: ");

        if (int.TryParse(Console.ReadLine(), out opcion))
        {
            if (opcion == 0)
            {
                valido = true;
                return (0, 0, "");
            }
            else if (opcion >= 1 && opcion <= places.Count + 1)
            {
                valido = true;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida. Intente nuevamente.");
            Console.ResetColor();
        }
    } while (!valido);

    return (places[opcion - 1].latitud, places[opcion - 1].longitud, places[opcion - 1].nombre);
}

// Módulo "Mostrar historial"

// Inicio -> Abrir historial.txt -> Hay registros?
//   Si -> Mostrar registros -> FIN
//   No -> Mostrar mensaje -> FIN
void mostrarHistorial()
{
    // Abrir historial.txt
    if (!File.Exists(rutaHistorial))
    {
        // No hay registros (el archivo ni siquiera existe todavía)
        mostrarMensajeSinHistorial();
        return; // FIN
    }

    List<string> registros = leerRegistrosHistorial();

    // Hay registros?
    if (registros.Count > 0)
    {
        // Sí -> Mostrar registros
        mostrarRegistrosHistorial(registros);
    }
    else
    {
        // No -> Mostrar mensaje
        mostrarMensajeSinHistorial();
    }
    // FIN
}

// Lee historial.txt con StreamReader y devuelve cada línea no vacía
// como un registro.
List<string> leerRegistrosHistorial()
{
    var registros = new List<string>();

    StreamReader lector = new StreamReader(rutaHistorial);
    string? linea;

    while ((linea = lector.ReadLine()) != null)
    {
        if (!string.IsNullOrWhiteSpace(linea))
        {
            registros.Add(linea);
        }
    }

    lector.Close();

    return registros;
}

// Rama "Sí": Mostrar registros
void mostrarRegistrosHistorial(List<string> registros)
{
    Console.WriteLine("\n===== HISTORIAL DE VIAJES =====");

    for (int i = 0; i < registros.Count; i++)
    {
        string[] datos = registros[i].Split(';');

        if (datos.Length >= 4)
        {
            Console.WriteLine($"\nViaje #{i + 1}");
            Console.WriteLine($"Origen    : {datos[0]}");
            Console.WriteLine($"Destino   : {datos[1]}");
            Console.WriteLine($"Distancia : {datos[2]} km");
            Console.WriteLine($"Costo     : C$ {datos[3]}\n");
        }
    }
}

// Rama "No": Mostrar mensaje
void mostrarMensajeSinHistorial()
{
    Console.WriteLine("No hay registros en el historial.\n");
}

void public_transport()
{
    Console.WriteLine("=== MOTOR DE BUSQUEDA DE RUTAS ===\n");
    string json = File.ReadAllText(rutaRutas);
    using JsonDocument doc = JsonDocument.Parse(json);
    var raiz = doc.RootElement;


    if (!raiz.TryGetProperty("lines", out var resultados))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se encontraron resultados para esta búsqueda.");
        Console.ResetColor();
        return;
    }

    List<string> paradas = get_paradas(resultados);

    Console.WriteLine("Seleccione su lugar de origen:\n");
    string origin = ask_parada(paradas);
    if (origin == "") return;
    Console.WriteLine("\nSeleccione su lugar de destino:\n");
    string destination = ask_parada(paradas);
    if (destination == "") return;

    Console.WriteLine();
    if (search_ruta_directa(resultados, origin, destination))
    {
        return;
    }
    else
    {
        bool transbordo_exist = search_transbordo(resultados, origin, destination);
        if (!transbordo_exist)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No existe transbordo.\n\n");
            Console.ResetColor();
        }
    }


    bool search_transbordo(JsonElement resultados, string origin, string destination)
    {
        Console.WriteLine($"Buscando transbordo entre {origin} y {destination}...\n");
        foreach (JsonProperty ruta1 in resultados.EnumerateObject())
        {
            foreach (JsonElement recorrido1 in ruta1.Value.EnumerateArray())
            {
                var listaParadas1 = new List<string>();
                foreach (JsonElement parada in recorrido1.GetProperty("stations").EnumerateArray())
                {
                    listaParadas1.Add(parada.GetString());
                }

                int indexInicio = -1;
                for (int i = 0; i < listaParadas1.Count; i++)
                {
                    if (listaParadas1[i] == origin)
                    {
                        indexInicio = i;
                        break;
                    }
                }

                if (indexInicio == -1)
                {
                    continue;
                }

                for (int i = indexInicio + 1; i < listaParadas1.Count; i++)
                {
                    string puntoTransbordo = listaParadas1[i];

                    if (puntoTransbordo == destination)
                    {
                        continue;
                    }

                    foreach (JsonProperty ruta2 in resultados.EnumerateObject())
                    {
                        if (ruta1.Name == ruta2.Name)
                        {
                            continue;
                        }

                        foreach (JsonElement recorrido2 in ruta2.Value.EnumerateArray())
                        {
                            var listaParadas2 = new List<string>();
                            foreach (JsonElement parada2 in recorrido2.GetProperty("stations").EnumerateArray())
                            {
                                listaParadas2.Add(parada2.GetString());
                            }

                            int indexTransbordo = -1;
                            int indexDestino = -1;

                            for (int j = 0; j < listaParadas2.Count; j++)
                            {
                                if (listaParadas2[j] == puntoTransbordo)
                                {
                                    indexTransbordo = j;
                                }
                                if (listaParadas2[j] == destination)
                                {
                                    indexDestino = j;
                                }
                            }

                            if (indexTransbordo != -1 && indexDestino != -1 && indexTransbordo < indexDestino)
                            {
                                Console.WriteLine("========================================");
                                Console.WriteLine("¡Ruta con transbordo encontrada:\n");
                                Console.WriteLine($"1. Sube en: {origin} (Ruta {ruta1.Name})");
                                Console.WriteLine($"2. Cámbiate en: {puntoTransbordo}");
                                Console.WriteLine($"3. Toma la Ruta {ruta2.Name} hacia: {destination}");
                                Console.WriteLine("========================================\n");
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }


    bool search_ruta_directa(JsonElement resultados, string origin, string destination)
    {
        foreach (JsonProperty ruta in resultados.EnumerateObject())
        {
            foreach (JsonElement recorrido in ruta.Value.EnumerateArray())
            {
                bool encontramosOrigen = false;
                foreach (JsonElement parada in recorrido.GetProperty("stations").EnumerateArray())
                {
                    string nombre = parada.GetString();

                    if (nombre == origin)
                    {
                        encontramosOrigen = true;
                    }

                    if (encontramosOrigen && nombre == destination)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Ruta encontrada");
                        Console.ResetColor();

                        Console.WriteLine($"Ruta: {ruta.Name}");
                        Console.WriteLine($"Recorrido: {origin} -> {destination}\n");

                        return true;
                    }
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No existe una ruta directa.\n");
        Console.ResetColor();

        return false;
    }


    string ask_parada(List<string> paradas)
    {
        for (int i = 0; i < paradas.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {paradas[i]}");
        }
        Console.WriteLine("0. Volver");

        bool valid = false;
        int index = 0;

        do
        {
            Console.Write("Ingrese el número de la parada: ");
            if (int.TryParse(Console.ReadLine(), out index))
            {
                if (index == 0) return "";
                if (index >= 1 && index <= paradas.Count)
                {
                    valid = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Número de parada inválido.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Número de parada inválido.");
                Console.ResetColor();
            }

        } while (!valid);

        return paradas[index - 1];
    }

    List<string> get_paradas(JsonElement resultados)
    {
        List<string> paradas = new();

        foreach (JsonProperty ruta in resultados.EnumerateObject())
        {
            foreach (JsonElement recorrido in ruta.Value.EnumerateArray())
            {
                foreach (JsonElement parada in recorrido.GetProperty("stations").EnumerateArray())
                {
                    string nombreParada = parada.GetString();

                    if (!paradas.Contains(nombreParada))
                    {
                        paradas.Add(nombreParada);
                    }
                }
            }
        }

        return paradas;
    }




}

struct Ubicacion
{
    public string nombre;
    public double latitud;
    public double longitud;
}
