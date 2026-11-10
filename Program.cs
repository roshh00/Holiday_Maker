using System;
using MySql.Data.MySqlClient;

class Program
{
    // Byt ut "DITT_LÖSENORD" mot ditt faktiska lösenord till MySQL i DataGrip
    static string connectionString = "Server=localhost;Database=holiday_maker;Uid=root;Pwd=DITT_LÖSENORD;";

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

            string choice = Console.ReadLine();

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
        Console.WriteLine("\n--- Hantera Kund ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
        Console.ReadKey();
    }

    static void SearchRooms()
    {
        Console.WriteLine("\n--- Sök Lediga Rum ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
        Console.ReadKey();
    }

    static void CreateBooking()
    {
        Console.WriteLine("\n--- Skapa Bokning ---");
        Console.WriteLine("Tryck på valfri tangent för att återgå.");
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