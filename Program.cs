using System;
using MySql.Data.MySqlClient;

class Program
{
    // Byt ut "DITT_LÖSENORD" mot ditt faktiska lösenord till MySQL i DataGrip
    static string connectionString = "Server=localhost;Database=holiday_maker;Uid=holiday_user;Pwd=mittlosenord123;";
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

        string choice = Console.ReadLine();

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
        if (int.TryParse(Console.ReadLine(), out int customerId))
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
    string name = Console.ReadLine();

    Console.Write("Ange e-post: ");
    string email = Console.ReadLine();

    Console.Write("Ange telefonnummer: ");
    string phone = Console.ReadLine();

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