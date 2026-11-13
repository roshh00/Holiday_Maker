using System;
using MySql.Data.MySqlClient;

namespace Holiday_Maker
{
    public static class CustomerManager
    {
        public static void ManageCustomer()
        {
            // Hanterar kundval och registrering 

            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("            HANTERA KUND                ");
                Console.WriteLine("========================================");
                
                if (AppData.CurrentCustomerId != -1)
                {
                    Console.WriteLine($"Aktiv kund: {AppData.CurrentCustomerName} (ID: {AppData.CurrentCustomerId})");
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

        public static void SelectExistingCustomer()
        {
            Console.Clear();
            Console.WriteLine("--- VÄLJ BEFINTLIG KUND ---\n");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
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
                    using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
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
                                    AppData.CurrentCustomerId = reader.GetInt32("id");
                                    AppData.CurrentCustomerName = reader.GetString("name");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\nKunden '{AppData.CurrentCustomerName}' har valts!");
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

        public static void RegisterNewCustomer()
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
                using (MySqlConnection conn = new MySqlConnection(AppData.ConnectionString))
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

                        AppData.CurrentCustomerId = (int)newId;
                        AppData.CurrentCustomerName = name;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nKund registrerad framgångsrikt och har valts automatiskt! (ID: {AppData.CurrentCustomerId})");
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
    }
}