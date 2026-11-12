using System;
using MySql.Data.MySqlClient;

class Program
{
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
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("          HOLIDAY MAKER - MENY          ");
            Console.WriteLine("========================================");
            Console.WriteLine($"[Aktuell kund ID: {(currentCustomerId == -1 ? "Ingen vald" : currentCustomerId.ToString())}]");
            Console.WriteLine($"[Valt rum ID:     {(selectedRoomId == -1 ? "Inget valt" : selectedRoomId.ToString())}]");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("1. Välj eller registrera kund");
            Console.WriteLine("2. Sök lediga rum & välj rum");
            Console.WriteLine("3. Skapa bokning");
            Console.WriteLine("4. Ändra antal personer i en bokning");
            Console.WriteLine("5. Avboka bokning");
            Console.WriteLine("6. Avsluta");
            Console.Write("\nVälj ett alternativ (1-6): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ManageCustomer(); // Din metod för kundhantering
                    break;
                case "2":
                    SearchRooms();    // Sökmetoden vi tittade på tidigare
                    break;
                case "3":
                    CreateBooking();  // Din färdiga bokningsmetod
                    break;
                case "4":
                    UpdateBookingGuests();
                    break;
                case "5":
                    CancelBooking(); // Avbokar bokningen
                    break;
                case "6":
                    running = false;
                    Console.WriteLine("\nTack för att du använde Holiday Maker!");
                    break;
                default:
                    Console.WriteLine("\nOgiltigt val. Tryck på valfri tangent för att försöka igen.");
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
    Console.WriteLine("--- SÖK LEDIGA RUM & FILTRERA ---\n");

    Console.Write("Ange ort (lämna tomt för alla orter): ");
    string? city = Console.ReadLine();

    Console.Write("Ange incheckningsdatum (YYYY-MM-DD): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkIn))
    {
        Console.WriteLine("Ogiltigt datumformat. Återgår till meny.");
        Console.ReadKey();
        return;
    }

    Console.Write("Ange utcheckningsdatum (YYYY-MM-DD): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOut) || checkOut <= checkIn)
    {
        Console.WriteLine("Utcheckningsdatum måste ligga efter incheckningsdatum. Återgår till meny.");
        Console.ReadKey();
        return;
    }

    Console.Write("Antal personer: ");
    if (!int.TryParse(Console.ReadLine(), out int guests) || guests <= 0)
    {
        Console.WriteLine("Ogiltigt antal personer. Återgår till meny.");
        Console.ReadKey();
        return;
    }

    // Valfria filter
    Console.Write("Krävs pool? (j/n): ");
    bool needPool = Console.ReadLine()?.Trim().ToLower() == "j";

    Console.Write("Krävs restaurang? (j/n): ");
    bool needRestaurant = Console.ReadLine()?.Trim().ToLower() == "j";

    Console.Write("Max avstånd till strand (meter, lämna tomt för inget krav): ");
    string? strandInput = Console.ReadLine();
    int? maxBeach = int.TryParse(strandInput, out int bDist) ? bDist : null;

    Console.Write("Max avstånd till centrum (meter, lämna tomt för inget krav): ");
    string? centrumInput = Console.ReadLine();
    int? maxCenter = int.TryParse(centrumInput, out int cDist) ? cDist : null;

    // Sortering
    Console.WriteLine("\nSortera efter:");
    Console.WriteLine("1. Pris per natt (lägst först)");
    Console.WriteLine("2. Omdöme (högst först)");
    Console.Write("Välj alternativ (1-2): ");
    string? sortChoice = Console.ReadLine();

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Bygg SQL-frågan dynamiskt
            string query = @"
                SELECT r.id AS room_id, h.name AS hotel_name, h.city, r.room_number, r.capacity, r.price_per_night, h.rating, h.pool, h.restaurant, h.distance_to_beach, h.distance_to_center
                FROM Rooms r
                JOIN Hotels h ON r.hotels_id = h.id
                WHERE r.capacity >= @guests
                  AND r.id NOT IN (
                      SELECT room_id FROM Bookings
                      WHERE check_in < @checkOut AND check_out > @checkIn
                  )
            ";

            if (!string.IsNullOrWhiteSpace(city))
            {
                query += " AND h.city LIKE @city";
            }

            if (needPool)
            {
                query += " AND h.pool = 1";
            }
            if (needRestaurant)
            {
                query += " AND h.restaurant = 1";
            }
            if (maxBeach.HasValue)
            {
                query += " AND h.distance_to_beach <= @maxBeach";
            }
            if (maxCenter.HasValue)
            {
                query += " AND h.distance_to_center <= @maxCenter";
            }

            // Sortering
            if (sortChoice == "2")
            {
                query += " ORDER BY h.rating DESC, r.price_per_night ASC";
            }
            else
            {
                query += " ORDER BY r.price_per_night ASC";
            }

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@guests", guests);
                cmd.Parameters.AddWithValue("@checkIn", checkIn.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@checkOut", checkOut.ToString("yyyy-MM-dd"));

                if (!string.IsNullOrWhiteSpace(city))
                {
                    cmd.Parameters.AddWithValue("@city", "%" + city.Trim() + "%");
                }
                if (maxBeach.HasValue)
                {
                    cmd.Parameters.AddWithValue("@maxBeach", maxBeach.Value);
                }
                if (maxCenter.HasValue)
                {
                    cmd.Parameters.AddWithValue("@maxCenter", maxCenter.Value);
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine("--- SÖKRESULTAT: LEDIGA RUM ---\n");
                    Console.WriteLine($"{"ID",-5} | {"Hotell",-18} | {"Stad",-10} | {"Rum",-5} | {"Kapacitet",-9} | {"Pris/natt",-10} | {"Omdöme"}");
                    Console.WriteLine(new string('-', 85));

                    bool foundAny = false;

                    while (reader.Read())
                    {
                        foundAny = true;
                        Console.WriteLine($"{reader["room_id"],-5} | {reader["hotel_name"],-18} | {reader["city"],-10} | {reader["room_number"],-5} | {reader["capacity"],-9} | {reader["price_per_night"],-10} kr | {reader["rating"]} / 5");
                    }

                    if (!foundAny)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nInga rum matchade dina sökkriterier och datum.");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
                        Console.ReadKey();
                        return;
                    }

                    //  Låt användaren välja ett rum för bokning
                    Console.WriteLine("\n----------------------------------------");
                    Console.Write("Vill du boka ett av dessa rum? Ange rummets ID (eller tryck Enter för att hoppa över): ");
                    string? input = Console.ReadLine();

                    if (int.TryParse(input, out int chosenRoomId))
                    {
                        // Spara till din globala variabel
                        selectedRoomId = chosenRoomId;
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nRum med ID {selectedRoomId} har valts! Gå till menyvalet för att skapa bokning.");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod vid sökning: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
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
        Console.Clear();
    Console.WriteLine("--- KUNDHANTERING ---\n");

    Console.Write("Ange kundens e-postadress: ");
    string? email = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(email))
    {
        Console.WriteLine("E-post kan inte vara tom. Återgår till meny.");
        Console.ReadKey();
        return;
    }

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // 1. Kolla om kunden redan finns
            string checkQuery = "SELECT id, name FROM Customers WHERE email = @email";
            using (MySqlCommand cmd = new MySqlCommand(checkQuery, conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Kunden finns! Spara ID:t
                        currentCustomerId = reader.GetInt32("id");
                        string customerName = reader.GetString("name");

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nKunden hittades! Aktiv kund satt till: {customerName} (ID: {currentCustomerId})");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            // 2. Om kunden inte finns, fråga om vi ska registrera en ny
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nKunden hittades inte i databasen.");
            Console.ResetColor();
            Console.Write("Vill du registrera en ny kund med denna e-post? (j/n): ");
            
            if (Console.ReadLine()?.Trim().ToLower() == "j")
            {
                Console.Write("Ange kundens namn: ");
                string? name = Console.ReadLine()?.Trim();

                Console.Write("Ange telefonnummer: ");
                string? phone = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("Namn kan inte vara tomt. Avbryter registrering.");
                    Console.ReadKey();
                    return;
                }

                // Spara ny kund i databasen
                string insertQuery = "INSERT INTO Customers (name, email, phone) VALUES (@name, @email, @phone); SELECT LAST_INSERT_ID();";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@email", email);
                    insertCmd.Parameters.AddWithValue("@phone", phone ?? "");

                    // Hämta det genererade ID:t för den nya kunden direkt
                    long newId = Convert.ToInt64(insertCmd.ExecuteScalar());
                    currentCustomerId = (int)newId;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nNy kund skapades och sattes som aktiv! (ID: {currentCustomerId})");
                    Console.ResetColor();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod vid kundhantering: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
    Console.ReadKey();
    }

    static void UpdateBookingGuests()
    {
      Console.Clear();
    Console.WriteLine("--- ÄNDRA ANTAL PERSONER I BOKNING ---\n");

    if (currentCustomerId == -1)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Ingen kund är vald! Välj en kund först (Val 1 i menyn).");
        Console.ResetColor();
        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
        Console.ReadKey();
        return;
    }

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // 1. Hämta med amount_guests istället för guests
            string query = @"
                SELECT b.id AS booking_id, h.name AS hotel_name, r.room_number, b.amount_guests, r.capacity
                FROM Bookings b
                JOIN Rooms r ON b.room_id = r.id
                JOIN Hotels h ON r.hotels_id = h.id
                WHERE b.customer_id = @customerId
            ";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@customerId", currentCustomerId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"Bokningar för aktiv kund (ID: {currentCustomerId}):\n");
                    Console.WriteLine($"{"Boknings-ID",-12} | {"Hotell",-18} | {"Rum",-5} | {"Nuvarande personer",-18} | {"Rumskapacitet"}");
                    Console.WriteLine(new string('-', 75));

                    bool foundAny = false;
                    while (reader.Read())
                    {
                        foundAny = true;
                        Console.WriteLine($"{reader["booking_id"],-12} | {reader["hotel_name"],-18} | {reader["room_number"],-5} | {reader["amount_guests"],-18} | {reader["capacity"]} pers");
                    }

                    if (!foundAny)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nDu har inga bokningar att ändra.");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            // 2. Välj bokning och nytt antal personer
            Console.WriteLine("\n----------------------------------------");
            Console.Write("Ange ID för den bokning du vill ändra: ");
            if (!int.TryParse(Console.ReadLine(), out int bookingId)) return;

            Console.Write("Ange det nya antalet personer: ");
            if (!int.TryParse(Console.ReadLine(), out int newGuests) || newGuests <= 0)
            {
                Console.WriteLine("Ogiltigt antal personer.");
                Console.ReadKey();
                return;
            }

            // 3. Kontrollera kapacitet
            string checkCapacityQuery = @"
                SELECT r.capacity 
                FROM Bookings b 
                JOIN Rooms r ON b.room_id = r.id 
                WHERE b.id = @bookingId AND b.customer_id = @customerId
            ";

            int maxCapacity = 0;
            using (MySqlCommand capCmd = new MySqlCommand(checkCapacityQuery, conn))
            {
                capCmd.Parameters.AddWithValue("@bookingId", bookingId);
                capCmd.Parameters.AddWithValue("@customerId", currentCustomerId);
                object result = capCmd.ExecuteScalar();
                if (result != null) maxCapacity = Convert.ToInt32(result);
            }

            if (newGuests > maxCapacity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nDet valda rummet rymmer max {maxCapacity} personer! Kan inte uppdatera.");
                Console.ResetColor();
                Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                Console.ReadKey();
                return;
            }

            // 4. Uppdatera amount_guests i databasen
            string updateQuery = "UPDATE Bookings SET amount_guests = @newGuests WHERE id = @bookingId AND customer_id = @customerId";
            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
            {
                updateCmd.Parameters.AddWithValue("@newGuests", newGuests);
                updateCmd.Parameters.AddWithValue("@bookingId", bookingId);
                updateCmd.Parameters.AddWithValue("@customerId", currentCustomerId);

                int rows = updateCmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nBokning {bookingId} har uppdaterats till {newGuests} personer!");
                    Console.ResetColor();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att återgå.");
    Console.ReadKey();
    }

    static void CancelBooking()
    {
      Console.Clear();
    Console.WriteLine("--- AVBOKA BOKNING ---\n");

    // Kontrollera att en kund är vald
    if (currentCustomerId == -1)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Ingen kund är vald! Välj en kund först (Val 1 i menyn).");
        Console.ResetColor();
        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
        Console.ReadKey();
        return;
    }

    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // 1. Hämta kundens aktiva bokningar med hotell- och rumsinformation
            string query = @"
                SELECT b.id AS booking_id, h.name AS hotel_name, r.room_number, b.check_in, b.check_out, b.total_price
                FROM Bookings b
                JOIN Rooms r ON b.room_id = r.id
                JOIN Hotels h ON r.hotels_id = h.id
                WHERE b.customer_id = @customerId
            ";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@customerId", currentCustomerId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"Bokningar för aktiv kund (ID: {currentCustomerId}):\n");
                    Console.WriteLine($"{"Boknings-ID",-12} | {"Hotell",-18} | {"Rum",-5} | {"Incheckning",-12} | {"Utcheckning",-12} | {"Pris"}");
                    Console.WriteLine(new string('-', 85));

                    bool foundAny = false;
                    while (reader.Read())
                    {
                        foundAny = true;
                        DateTime checkIn = reader.GetDateTime("check_in");
                        DateTime checkOut = reader.GetDateTime("check_out");
                        decimal price = reader.GetDecimal("total_price");

                        Console.WriteLine($"{reader["booking_id"],-12} | {reader["hotel_name"],-18} | {reader["room_number"],-5} | {checkIn:yyyy-MM-dd}  | {checkOut:yyyy-MM-dd}  | {price} kr");
                    }

                    if (!foundAny)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nDu har inga aktiva bokningar att avboka.");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            // 2. Låt användaren skriva in ID för den bokning som ska tas bort
            Console.WriteLine("\n----------------------------------------");
            Console.Write("Ange ID för den bokning du vill avboka (eller tryck Enter för att avbryta): ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int bookingId))
            {
                // 3. Radera bokningen (säkerställ att den tillhör den aktuella kunden)
                string deleteQuery = "DELETE FROM Bookings WHERE id = @bookingId AND customer_id = @customerId";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@bookingId", bookingId);
                    deleteCmd.Parameters.AddWithValue("@customerId", currentCustomerId);

                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nBokning med ID {bookingId} har avbokats framgångsrikt!");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nKunde inte hitta en bokning med det ID:t som tillhör dig.");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nEtt fel uppstod vid avbokning: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
    Console.ReadKey();
    }
}