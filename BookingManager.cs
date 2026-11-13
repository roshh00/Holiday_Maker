using System;
using MySql.Data.MySqlClient;

namespace Holiday_Maker
{
    public static class BookingManager
    {
        public static void CreateBooking()
        {
            Console.Clear();
            Console.WriteLine("--- SKAPA BOKNING ---\n");

            // 1. Kontrollera att en kund är vald
            if (AppData.CurrentCustomerId == -1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Ingen kund är vald! Välj eller registrera en kund först (Val 1 i menyn).");
                Console.ResetColor();
                Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                Console.ReadKey();
                return;
            }

            // 2. Kontrollera att ett rum är valt
            if (AppData.SelectedRoomId == -1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Inget rum är valt! Gå till rumsöversikten (Val 2) och välj ett rum först.");
                Console.ResetColor();
                Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Aktiv kund ID: {AppData.CurrentCustomerId}");
            Console.WriteLine($"Valt rum ID: {AppData.SelectedRoomId}");
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
                {
                    conn.Open();

                    // 3. Hämta pris per natt och kapacitet för det valda rummet
                    string roomQuery = "SELECT price_per_night, capacity FROM Rooms WHERE id = @roomId";
                    decimal pricePerNight = 0;
                    int capacity = 0;

                    using (MySqlCommand roomCmd = new MySqlCommand(roomQuery, conn))
                    {
                        roomCmd.Parameters.AddWithValue("@roomId", AppData.SelectedRoomId);
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
                        insertCmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);
                        insertCmd.Parameters.AddWithValue("@roomId", AppData.SelectedRoomId);
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
                        AppData.SelectedRoomId = -1;
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

        public static void UpdateBookingGuests()
        {
            Console.Clear();
            Console.WriteLine("--- ÄNDRA ANTAL PERSONER I BOKNING ---\n");

            if (AppData.CurrentCustomerId == -1)
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT b.id AS booking_id, h.name AS hotel_name, r.room_number, b.amount_guests, r.capacity
                        FROM Bookings b
                        JOIN Rooms r ON b.room_id = r.id
                        JOIN Hotels h ON r.hotels_id = h.id
                        WHERE b.customer_id = @customerId
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"Bokningar för aktiv kund (ID: {AppData.CurrentCustomerId}):\n");
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
                        capCmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);
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

                    string updateQuery = "UPDATE Bookings SET amount_guests = @newGuests WHERE id = @bookingId AND customer_id = @customerId";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@newGuests", newGuests);
                        updateCmd.Parameters.AddWithValue("@bookingId", bookingId);
                        updateCmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);

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

        public static void CancelBooking()
        {
            Console.Clear();
            Console.WriteLine("--- AVBOKA BOKNING ---\n");

            if (AppData.CurrentCustomerId == -1)
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT b.id AS booking_id, h.name AS hotel_name, r.room_number, b.check_in, b.check_out, b.total_price
                        FROM Bookings b
                        JOIN Rooms r ON b.room_id = r.id
                        JOIN Hotels h ON r.hotels_id = h.id
                        WHERE b.customer_id = @customerId
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"Bokningar för aktiv kund (ID: {AppData.CurrentCustomerId}):\n");
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

                    Console.WriteLine("\n----------------------------------------");
                    Console.Write("Ange ID för den bokning du vill avboka (eller tryck Enter för att avbryta): ");
                    string? input = Console.ReadLine();

                    if (int.TryParse(input, out int bookingId))
                    {
                        string deleteQuery = "DELETE FROM Bookings WHERE id = @bookingId AND customer_id = @customerId";
                        using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@bookingId", bookingId);
                            deleteCmd.Parameters.AddWithValue("@customerId", AppData.CurrentCustomerId);

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
}