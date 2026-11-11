using System;
using MySql.Data.MySqlClient;

class Program
{
    // Byt ut "DITT_LÖSENORD" mot ditt faktiska lösenord till MySQL i DataGrip
    static string connectionString = "Server=localhost;Database=Holiday_Maker;Uid=holiday_user;Pwd=mittlosenord123;";
    static int selectedRoomId = -1;
    static int currentCustomerId = -1;
    static string currentCustomerName = "";
    static void Main(string[] args)
    {
        // Testa anslutningen direkt när programmet startar
        TestConnection();

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("       HOLIDAY MAKER - BOKNINGSYSTEM     ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Välj eller registrera kund");
            Console.WriteLine("2. Sök efter lediga rum");
            Console.WriteLine("3. Skapa en bokning");
            Console.WriteLine("4. Visa kundens bokningar");
            Console.WriteLine("5. Ändra antal personer i en bokning");
            Console.WriteLine("6. Avboka");
            Console.WriteLine("0. Avsluta programmet");
            Console.WriteLine("========================================");
            Console.Write("Välj ett alternativ (0-6): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ManageCustomer();
                    break;
                case "2":
                    SearchRooms();
                    break;
                case "3":
                    CreateBooking();
                    break;
                case "4":
                    ShowCustomerBookings();
                    break;
                case "5":
                    UpdateBookingGuests();
                    break;
                case "6":
                    CancelBooking();
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("Tack för idag! Programmet avslutas.");
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Tryck på valfri tangent för att försöka igen.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static void TestConnection()
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Anslutning till databasen 'holiday_maker' lyckades!");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fel vid anslutning till databasen: {ex.Message}");
            Console.ResetColor();
        }
    }

    static void ManageCustomer()
    {
        // Hanterar kundval och registrering 

        bool back = false;
    while (!back)
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("            HANTERA KUND                ");
        Console.WriteLine("========================================");
        
        if (currentCustomerId != -1)
        {
            Console.WriteLine($"Aktiv kund: {currentCustomerName} (ID: {currentCustomerId})");
        }
        else
        {
            Console.WriteLine("Aktiv kund: Ingen kund vald");
        }
        
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("1. Visa alla kunder och välj befintlig");
        Console.WriteLine("2. Registrera ny kund");
        Console.WriteLine("0. Tillbaka till huvudmenyn");
        Console.WriteLine("========================================");
        Console.Write("Välj ett alternativ: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                SelectExistingCustomer();
                break;
            case "2":
                RegisterNewCustomer();
                break;
            case "0":
                back = true;
                break;
            default:
                Console.WriteLine("Ogiltigt val. Tryck på valfri tangent.");
                Console.ReadKey();
                break;
        }
    }
}

static void SelectExistingCustomer()
{
    Console.Clear();
    Console.WriteLine("--- VÄLJ BEFINTLIG KUND ---\n");

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = "SELECT id, name, email, phone_number FROM Customers";
            
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"{"ID",-5} | {"Namn",-20} | {"E-post",-25} | {"Telefon"}");
                    Console.WriteLine(new string('-', 70));

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["id"],-5} | {reader["name"],-20} | {reader["email"],-25} | {reader["phone_number"]}");
                    }
                }
            }
        }

        Console.Write("\nAnge ID på den kund du vill välja: ");
        string? customerIdInput = Console.ReadLine();
        if (int.TryParse(customerIdInput, out int customerId))
        {
            // Kontrollera att kunden finns
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, name FROM Customers WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentCustomerId = reader.GetInt32("id");
                            currentCustomerName = reader.GetString("name");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\nKunden '{currentCustomerName}' har valts!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nIngen kund hittades med det ID:t.");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Ogiltigt ID-format.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ett fel uppstod: {ex.Message}");
    }

    Console.WriteLine("\nTryck på valfri tangent för att fortsätta.");
    Console.ReadKey();
}

static void RegisterNewCustomer()
{
    Console.Clear();
    Console.WriteLine("--- REGISTRERA NY KUND ---\n");

    Console.Write("Ange namn: ");
    string? name = Console.ReadLine();

    Console.Write("Ange e-post: ");
    string? email = Console.ReadLine();

    Console.Write("Ange telefonnummer: ");
    string? phone = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Namn kan inte vara tomt! Avbryter registrering.");
        Console.ReadKey();
        return;
    }

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = "INSERT INTO Customers (name, email, phone_number) VALUES (@name, @email, @phone); SELECT LAST_INSERT_ID();";
            
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@phone", phone);

                // Hämtar det nyskapade ID:t direkt från databasen
                long newId = Convert.ToInt64(cmd.ExecuteScalar());

                currentCustomerId = (int)newId;
                currentCustomerName = name;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nKund registrerad framgångsrikt och har valts automatiskt! (ID: {currentCustomerId})");
                Console.ResetColor();
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Kunde inte registrera kund: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att fortsätta.");
    Console.ReadKey();
    }

    static void SearchRooms()
    {
    Console.Clear();
    Console.WriteLine("--- RUMSÖVERBLICK OCH TILLGÄNGLIGHET ---\n");

    Console.Write("Ange stad att titta på (t.ex. Stockholm, lämna tomt för alla): ");
    string? city = Console.ReadLine();

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // 1. Hämta alla rum (filtrerat på stad om användaren skrev en)
            string query = @"
                SELECT r.id, h.name AS hotel_name, r.room_number, r.capacity, r.price_per_night, h.city
                FROM Rooms r
                JOIN Hotels h ON r.hotels_id = h.id
            ";

            if (!string.IsNullOrWhiteSpace(city))
            {
                query += " WHERE h.city LIKE @city";
            }

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                if (!string.IsNullOrWhiteSpace(city))
                {
                    cmd.Parameters.AddWithValue("@city", "%" + city + "%");
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine("--- RUM OCH BEFINTLIGA BOKNINGAR ---\n");
                    Console.WriteLine($"{"ID",-5} | {"Hotell",-18} | {"Stad",-12} | {"Rum",-6} | {"Kapacitet",-9} | {"Pris/natt"}");
                    Console.WriteLine(new string('-', 75));

                    bool foundAny = false;
                    List<int> roomIds = new List<int>();

                    while (reader.Read())
                    {
                        foundAny = true;
                        int roomId = reader.GetInt32("id");
                        roomIds.Add(roomId);

                        Console.WriteLine($"{roomId,-5} | {reader["hotel_name"],-18} | {reader["city"],-12} | {reader["room_number"],-6} | {reader["capacity"],-9} | {reader["price_per_night"]} kr");
                    }

                    if (!foundAny)
                    {
                        Console.WriteLine("\nInga rum hittades.");
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }

                    // 2. Visa vilka datum som är upptagna för rummen
                    Console.WriteLine("\n\n--- BOKADE DATUM FÖR RUMMEN ---");
                    bool hasBookings = false;

                    string bookingQuery = @"
                        SELECT room_id, check_in, check_out 
                        FROM Bookings 
                        WHERE check_out >= CURDATE()
                        ORDER BY check_in ASC;
                    ";

                    // Stäng läsaren för rumsfrågan innan vi gör en ny fråga på samma koppling
                    reader.Close();

                    using (MySqlCommand bookingCmd = new MySqlCommand(bookingQuery, conn))
                    {
                        using (MySqlDataReader bookingReader = bookingCmd.ExecuteReader())
                        {
                            while (bookingReader.Read())
                            {
                                hasBookings = true;
                                Console.WriteLine($"Rum ID {bookingReader["room_id"]}: Upptaget {(((DateTime)bookingReader["check_in"]).ToString("yyyy-MM-dd"))} till {(((DateTime)bookingReader["check_out"]).ToString("yyyy-MM-dd"))}");
                            }
                        }
                    }

                    if (!hasBookings)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Fantastiskt nyhet: Det finns inga aktiva framtida bokningar på några rum alls. Alla är helt lediga!");
                        Console.ResetColor();
                    }
                }
            }
        }

        // Låt användaren välja ett rum att gå vidare med
        Console.Write("\nAnge ID på det rum du vill boka (eller 0 för att gå tillbaka): ");
        if (int.TryParse(Console.ReadLine(), out int chosenId) && chosenId > 0)
        {
            selectedRoomId = chosenId;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nRum med ID {selectedRoomId} har valts! Du kan nu gå vidare till att skapa bokning (Val 3).");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att fortsätta.");
    Console.ReadKey();
    }

    // SKAPAR BOKNING 
    static void CreateBooking()

    {
    Console.Clear();
    Console.WriteLine("--- SKAPA BOKNING ---\n");

    // 1. Kontrollera att en kund är vald
    if (currentCustomerId == -1)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Ingen kund är vald! Välj eller registrera en kund först (Val 1 i menyn).");
        Console.ResetColor();
        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
        Console.ReadKey();
        return;
    }

    // 2. Kontrollera att ett rum är valt
    if (selectedRoomId == -1)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Inget rum är valt! Gå till rumsöversikten (Val 2) och välj ett rum först.");
        Console.ResetColor();
        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"Aktiv kund ID: {currentCustomerId}");
    Console.WriteLine($"Valt rum ID: {selectedRoomId}");
    Console.WriteLine("----------------------------------------");

    Console.Write("Ange incheckningsdatum (ÅÅÅÅ-MM-DD): ");
    string? checkIn = Console.ReadLine();

    Console.Write("Ange utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
    string? checkOut = Console.ReadLine();

    Console.Write("Ange antal gäster: ");
    if (!int.TryParse(Console.ReadLine(), out int guests) || guests <= 0)
    {
        Console.WriteLine("Ogiltigt antal gäster. Avbryter.");
        Console.ReadKey();
        return;
    }

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // 3. Hämta pris per natt och kapacitet för det valda rummet
            string roomQuery = "SELECT price_per_night, capacity FROM Rooms WHERE id = @roomId";
            decimal pricePerNight = 0;
            int capacity = 0;

            using (MySqlCommand roomCmd = new MySqlCommand(roomQuery, conn))
            {
                roomCmd.Parameters.AddWithValue("@roomId", selectedRoomId);
                using (MySqlDataReader reader = roomCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pricePerNight = reader.GetDecimal("price_per_night");
                        capacity = reader.GetInt32("capacity");
                    }
                    else
                    {
                        Console.WriteLine("Det valda rummet kunde inte hittas i databasen.");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            // Kontrollera att rummets kapacitet räcker
            if (guests > capacity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nVarning: Rummet rymmer endast {capacity} gäster (du angav {guests}). Bokningen kan inte genomföras.");
                Console.ResetColor();
                Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                Console.ReadKey();
                return;
            }

            // 4. Beräkna antal nätter och totalpris
           if (!DateTime.TryParseExact(checkIn, "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None, out DateTime inDate) ||
        !DateTime.TryParseExact(checkOut, "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None, out DateTime outDate))
    {
        Console.WriteLine("Ange datumen i formatet ÅÅÅÅ-MM-DD.");
        Console.ReadKey();
        return;
    }
            int nights = (int)(outDate - inDate).TotalDays;

            if (nights <= 0)
            {
                Console.WriteLine("Utcheckningsdatum måste vara efter incheckningsdatum.");
                Console.ReadKey();
                return;
            }

            decimal totalPrice = nights * pricePerNight;

            // 5. Spara bokningen i databasen
            string insertQuery = @"
                INSERT INTO Bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price)
                VALUES (@customerId, @roomId, @checkIn, @checkOut, @guests, @totalPrice);
            ";

            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
            {
                insertCmd.Parameters.AddWithValue("@customerId", currentCustomerId);
                insertCmd.Parameters.AddWithValue("@roomId", selectedRoomId);
                insertCmd.Parameters.AddWithValue("@checkIn", inDate);
                insertCmd.Parameters.AddWithValue("@checkOut", outDate);
                insertCmd.Parameters.AddWithValue("@guests", guests);
                insertCmd.Parameters.AddWithValue("@totalPrice", totalPrice);

                insertCmd.ExecuteNonQuery();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nBokningen skapades framgångsrikt!");
                Console.WriteLine($"Totalt pris: {totalPrice} kr ({nights} nätter).");
                Console.ResetColor();
                
                // Nollställ det valda rummet så man inte bokar samma av misstag igen
                selectedRoomId = -1;
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod vid bokning: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att fortsätta.");
    Console.ReadKey();
    }

    static void ShowCustomerBookings()
    {
        Console.WriteLine("\n--- Kundens Bokningar ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
        Console.ReadKey();
    }

    static void UpdateBookingGuests()
    {
        Console.WriteLine("\n--- Ändra Antal Personer ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
        Console.ReadKey();
    }

    static void CancelBooking()
    {
        Console.WriteLine("\n--- Avboka ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
        Console.ReadKey();
    }
}