using System;
using System.Data.SQLite;

namespace BankWebService.Data
{
    public class DBManager
    {
        public static string connectionString = "Data Source=mydatabase.db;Version=3;";

        public static void DBInitialize()
        {
            DropTables(); // Drop tables before creating new ones
            CreateTable();
            SeedData(); // Call your data seeding method
        }

        public static void DropTables()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Drop Users, Accounts, and Transactions tables if they exist
                string[] tables = { "Users", "Accounts", "Transactions" };

                foreach (var table in tables)
                {
                    string dropQuery = $"DROP TABLE IF EXISTS {table};";
                    using (SQLiteCommand command = new SQLiteCommand(dropQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Users, Accounts, and Transactions tables dropped.");
            }
        }

        public static bool CreateTable()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string createAccountsTable = @"
                    CREATE TABLE IF NOT EXISTS Accounts (
                        AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountNumber TEXT NOT NULL UNIQUE,
                        Balance REAL NOT NULL
                    );";

                    string createTransactionsTable = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountId INTEGER,
                        TransactionType TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        TransactionDate TEXT NOT NULL,
                        FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId)
                    );";

                    string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountId INTEGER,
                        Name TEXT NOT NULL,
                        Username TEXT NOT NULL UNIQUE,
                        Email TEXT NOT NULL UNIQUE,
                        Address TEXT,
                        Phone TEXT,
                        Picture TEXT,
                        Password TEXT NOT NULL,
                        Role TEXT NOT NULL, -- New Role field to store Admin or User
                        FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId)
                    );";

                    using (SQLiteCommand command = new SQLiteCommand(createAccountsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(createTransactionsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(createUsersTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error creating tables: {ex.Message}");
                return false;
            }
        }

        public static void SeedData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Seeding Users (2 Admins and 3 Regular Users)
                var users = new[]
                {
                    new { Name = "Uvindu", Username = "admin", Email = "uvindu@gmail.com", Address = "123 Elm St", Phone = "123-456-7890", Picture = "pic1.jpg", Password = "admin", Role = "Admin" },
                    new { Name = "Jane Smith", Username = "jane_smith", Email = "jane@example.com", Address = "456 Oak St", Phone = "987-654-3210", Picture = "pic2.jpg", Password = "password2", Role = "User" },
                    new { Name = "Alice Wong", Username = "alice_wong", Email = "alice@example.com", Address = "789 Pine St", Phone = "456-789-1234", Picture = "pic3.jpg", Password = "password3", Role = "User" },
                    new { Name = "Bob Johnson", Username = "bob_johnson", Email = "bob@example.com", Address = "101 Maple St", Phone = "555-123-4567", Picture = "pic4.jpg", Password = "password4", Role = "User" },
                    new { Name = "Charlie Brown", Username = "charlie_brown", Email = "charlie@example.com", Address = "202 Cedar St", Phone = "555-987-6543", Picture = "pic5.jpg", Password = "password5", Role = "User" }
                };

                foreach (var user in users)
                {
                    // Seeding Accounts for each user
                    string accountNumber = "ACCT" + new Random().Next(100000, 999999).ToString();
                    decimal balance = new Random().Next(1000, 50000);

                    // Insert an account for the user
                    string insertAccountQuery = "INSERT INTO Accounts (AccountNumber, Balance) VALUES (@AccountNumber, @Balance)";
                    long accountId;

                    using (SQLiteCommand command = new SQLiteCommand(insertAccountQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        command.Parameters.AddWithValue("@Balance", balance);
                        command.ExecuteNonQuery();
                    }

                    accountId = connection.LastInsertRowId; // Get the newly created AccountId

                    // Insert user
                    string insertUserQuery = "INSERT INTO Users (AccountId, Name, Username, Email, Address, Phone, Picture, Password, Role) " +
                                             "VALUES (@AccountId, @Name, @Username, @Email, @Address, @Phone, @Picture, @Password, @Role)";
                    using (SQLiteCommand command = new SQLiteCommand(insertUserQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId); // AccountId as foreign key
                        command.Parameters.AddWithValue("@Name", user.Name);
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Address", user.Address);
                        command.Parameters.AddWithValue("@Phone", user.Phone);
                        command.Parameters.AddWithValue("@Picture", user.Picture);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@Role", user.Role); // Assigning role as Admin or User
                        command.ExecuteNonQuery();
                    }

                    long userId = connection.LastInsertRowId;

                    // Seeding Transactions for each account
                    var transactions = new[]
                    {
                        new { TransactionType = "Credited", Amount = new Random().Next(100, 1000), TransactionDate = DateTime.UtcNow.ToString("o") },
                        new { TransactionType = "Debited", Amount = new Random().Next(100, 1000), TransactionDate = DateTime.UtcNow.AddDays(-5).ToString("o") }
                    };

                    foreach (var transaction in transactions)
                    {
                        string insertTransactionQuery = "INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate) " +
                                                        "VALUES (@AccountId, @TransactionType, @Amount, @TransactionDate)";
                        using (SQLiteCommand command = new SQLiteCommand(insertTransactionQuery, connection))
                        {
                            command.Parameters.AddWithValue("@AccountId", accountId);
                            command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                            command.Parameters.AddWithValue("@Amount", transaction.Amount);
                            command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("Data seeding completed!");
            }
        }
    }
}
