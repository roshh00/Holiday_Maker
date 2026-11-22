using System;
using MySql.Data.MySqlClient;

namespace Holiday_Maker
{
    public static class RoomManager
    {
        public static void SearchRooms()
        {
            Console.Clear();
            Console.WriteLine("--- SÖK LEDIGA HOTELL & RUM ---\n");

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
            Console.WriteLine("1. Lägsta pris per natt först");
            Console.WriteLine("2. Högst omdöme först");
            Console.Write("Välj alternativ (1-2): ");
            string? sortChoice = Console.ReadLine();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
                {
                    conn.Open();

                    // 1. Sök fram unika HOTELL som har minst ett ledigt rum som matchar
                    string query = @"
                        SELECT h.id AS hotel_id, h.name AS hotel_name, h.city, h.rating, h.pool, h.restaurant, 
                               MIN(r.price_per_night) AS min_price, 
                               COUNT(r.id) AS available_rooms_count
                        FROM Hotels h
                        JOIN Rooms r ON r.hotels_id = h.id
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

                    query += " GROUP BY h.id, h.name, h.city, h.rating, h.pool, h.restaurant";

                    // Sortering
                    if (sortChoice == "2")
                    {
                        query += " ORDER BY h.rating DESC, min_price ASC";
                    }
                    else
                    {
                        query += " ORDER BY min_price ASC";
                    }

                    bool foundAny = false;

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
                            Console.WriteLine("--- SÖKRESULTAT: TILLGÄNGLIGA HOTELL ---\n");
                            Console.WriteLine($"{"Hotell-ID",-10} | {"Hotellnamn",-20} | {"Stad",-12} | {"Lägsta pris",-12} | {"Lediga rum",-12} | {"Omdöme"}");
                            Console.WriteLine(new string('-', 85));

                            while (reader.Read())
                            {
                                foundAny = true;
                                Console.WriteLine($"{reader["hotel_id"],-10} | {reader["hotel_name"],-20} | {reader["city"],-12} | {reader["min_price"],-10} kr | {reader["available_rooms_count"],-12} st | {reader["rating"]} / 5");
                            }
                        } // <-- reader stängs här innan vi läser inmatning eller kör nästa fråga
                    }

                    if (!foundAny)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nInga hotell matchade dina sökkriterier och datum.");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }

                    // 2. Låt användaren välja ett hotell för att se dess rum
                    Console.WriteLine("\n----------------------------------------");
                    Console.Write("Ange ID för det hotell du vill titta på rum för (eller tryck Enter för att avbryta): ");
                    string? hotelInput = Console.ReadLine();

                    if (int.TryParse(hotelInput, out int chosenHotelId))
                    {
                        ShowRoomsForHotel(conn, chosenHotelId, checkIn, checkOut, guests);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nEtt fel uppstod vid sökning: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nTryck på valfri tangent för att fortsätta.");
                Console.ReadKey();
            }
        }

        // Hjälpmetod för att visa specifika lediga rum för det valda hotellet
        private static void ShowRoomsForHotel(MySqlConnection conn, int hotelId, DateTime checkIn, DateTime checkOut, int guests)
        {
            string roomQuery = @"
                SELECT r.id AS room_id, r.room_number, r.capacity, r.price_per_night
                FROM Rooms r
                WHERE r.hotels_id = @hotelId
                  AND r.capacity >= @guests
                  AND r.id NOT IN (
                      SELECT room_id FROM Bookings
                      WHERE check_in < @checkOut AND check_out > @checkIn
                  )
            ";

            using (MySqlCommand roomCmd = new MySqlCommand(roomQuery, conn))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                roomCmd.Parameters.AddWithValue("@hotelId", hotelId);
                roomCmd.Parameters.AddWithValue("@guests", guests);
                roomCmd.Parameters.AddWithValue("@checkIn", checkIn.ToString("yyyy-MM-dd"));
                roomCmd.Parameters.AddWithValue("@checkOut", checkOut.ToString("yyyy-MM-dd"));

                using (MySqlDataReader roomReader = roomCmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine($"--- LEDIGA RUM FÖR HOTELL ID {hotelId} ---\n");
                    Console.WriteLine($"{"Rum-ID",-8} | {"Rumsknummer",-12} | {"Kapacitet",-10} | {"Pris per natt"}");
                    Console.WriteLine(new string('-', 55));

                    bool foundRooms = false;
                    while (roomReader.Read())
                    {
                        foundRooms = true;
                        Console.WriteLine($"{roomReader["room_id"],-8} | {roomReader["room_number"],-12} | {roomReader["capacity"],-10} pers | {roomReader["price_per_night"]} kr");
                    }

                    if (!foundRooms)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nDet finns inga lediga rum på detta hotell som matchar antalet gäster och datum.");
                        Console.ResetColor();
                        Console.WriteLine("\nTryck på valfri tangent för att återgå.");
                        Console.ReadKey();
                        return;
                    }

                    Console.WriteLine("\n----------------------------------------");
                    Console.Write("Ange ID på det rum du vill boka: ");
                    string? roomInput = Console.ReadLine();

                    if (int.TryParse(roomInput, out int chosenRoomId))
                    {
                        AppData.SelectedRoomId = chosenRoomId;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nRum med ID {AppData.SelectedRoomId} har valts! Gå till menyval 3 för att skapa bokning.");
                        Console.ResetColor();
                    }
                }
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
            Console.ReadKey();
        }
    }
}