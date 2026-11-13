using System;
using MySql.Data.MySqlClient;

namespace Holiday_Maker
{
    class Program
    {
        static void Main(string[] args)
        {
            TestConnection();

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           HOLIDAY MAKER - MENY         ");
                Console.WriteLine("========================================");
                Console.WriteLine($"[Aktuell kund ID: {(AppData.CurrentCustomerId == -1 ? "Ingen vald" : AppData.CurrentCustomerId.ToString())}]");
                Console.WriteLine($"[Valt rum ID:     {(AppData.SelectedRoomId == -1 ? "Inget valt" : AppData.SelectedRoomId.ToString())}]");
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
                        CustomerManager.ManageCustomer();
                        break;
                    case "2":
                        RoomManager.SearchRooms();
                        break;
                    case "3":
                        BookingManager.CreateBooking();
                        break;
                    case "4":
                        BookingManager.UpdateBookingGuests();
                        break;
                    case "5":
                        BookingManager.CancelBooking();
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
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
    }
}