# Holiday Maker — Hotell- och Bokningssystem


Ett konsolbaserat bokningssystem i C# kopplat till en MySQL-databas. 
Applikationen låter användaren söka och filtrera lediga hotell/rum baserat på datum, ort och bekvämligheter, samt hantera kunder och bokningar.

För att kunna börja med det behöver du sätta upp databasen först. Då du ska öppna ditt databasverktyg, anslut till din MYSQL-server och kör följade SQL-skript för att skapa databasen och tabellerna: 

CREATE DATABASE IF NOT EXISTS holiday_maker;
USE holiday_maker;

-- 1. Tabell för Hotell
CREATE TABLE IF NOT EXISTS Hotels (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    city VARCHAR(50) NOT NULL,
    rating DECIMAL(2,1) DEFAULT 0.0,
    pool TINYINT(1) DEFAULT 0,
    restaurant TINYINT(1) DEFAULT 0,
    distance_to_beach INT DEFAULT NULL,
    distance_to_center INT DEFAULT NULL
);

-- 2. Tabell för Rum
CREATE TABLE IF NOT EXISTS Rooms (
    id INT AUTO_INCREMENT PRIMARY KEY,
    hotels_id INT NOT NULL,
    room_number VARCHAR(10) NOT NULL,
    capacity INT NOT NULL,
    price_per_night DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (hotels_id) REFERENCES Hotels(id) ON DELETE CASCADE
);

-- 3. Tabell för Kunder
CREATE TABLE IF NOT EXISTS Customers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone_number VARCHAR(20) DEFAULT NULL
);

-- 4. Tabell för Bokningar
CREATE TABLE IF NOT EXISTS Bookings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    room_id INT NOT NULL,
    check_in DATE NOT NULL,
    check_out DATE NOT NULL,
    number_of_guests INT NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES Customers(id) ON DELETE CASCADE,
    FOREIGN KEY (room_id) REFERENCES Rooms(id) ON DELETE CASCADE
);

Sedan ska du konfigurera det i C# med visual studio code, så se till att din anslutningssträng i Appdata.cs pekar på din lokala MYSQL-databas med rätt användarnamn och lösenord: 

namespace Holiday_Maker
{
    public static class AppData
    {
        // Ändra server, user och password så att det matchar din lokala MySQL-uppsättning
        public static string ConnectionString = "server=localhost;database=holiday_maker;user=root;password=DITT_LÖSENORD;";
        
        public static int CurrentCustomerId { get; set; } = -1;
        public static int SelectedRoomId { get; set; } = -1;
    }
}


Nu kan du börja köra applikationen med att öppna terminalen i projektets rotmapp och sedan återställ beroenden och ladda ner nuget-paket (MySql.data) med ''dotnet restore''. Sedan startar du applikationen med ''dotnet run''. 



När programmet har startats möts du av huvudmenyn som består av följade: 

1. Välj eller registrera kund – Sök befintlig kund via e-post eller skapa en ny kund.

2. Sök lediga rum & välj rum – Sök tillgängliga hotell utifrån datum, ort, antal gäster samt bekvämligheter (pool, restaurang, avstånd). Välj sedan ett hotell och ett specifikt rum.

3. Skapa bokning – Slutför bokningen för den valda kunden och det valda rummet.

4. Ändra antal personer i en bokning – Uppdatera gästantal på en befintlig bokning.

5. Avboka bokning – Radera en sparad bokning från databasen.

6. Avsluta – Stänger applikationen.

Nu har du en komplett guide på hur du startar upp Holiday Maker. 
Lycka till! 


