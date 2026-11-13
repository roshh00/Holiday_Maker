using System;
using MySql.Data.MySqlClient;

namespace Holiday_Maker
{
    public static class RoomManager
    {
        public static void SearchRooms()
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
                {
                    conn.Open();

                    // Bygg SQL-frågan dynamiskt
                    string query = @"
                        SELECT DISTINCT r.id AS room_id, h.name AS hotel_name, h.city, r.room_number, r.capacity, r.price_per_night, h.rating, h.pool, h.restaurant, h.distance_to_beach, h.distance_to_center
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

                            // Låt användaren välja ett rum för bokning
                            Console.WriteLine("\n----------------------------------------");
                            Console.Write("Vill du boka ett av dessa rum? Ange rummets ID (eller tryck Enter för att hoppa över): ");
                            string? input = Console.ReadLine();

                            if (int.TryParse(input, out int chosenRoomId))
                            {
                                // Spara till din globala variabel i AppData
                                AppData.SelectedRoomId = chosenRoomId;
                                
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"\nRum med ID {AppData.SelectedRoomId} har valts! Gå till menyvalet för att skapa bokning.");
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
    }
}