using SQLite.Library;
using System;

namespace SQLiteDatabaseExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var databaseName = $"MyDatabase-{DateTime.Now:yyyyMMddhhmmss}";

            var database = CreateDatabase(databaseName);

            CreateTables(databaseName);

            ExitOperation(database);
        }

        private static void ExitOperation(Database database)
        {
            while (true)
            {
                Console.WriteLine("Would you like to delete the database before exit? (Y/N)");
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Y)
                {
                    if (Database.Delete(database))
                        Console.WriteLine($"Successfully deleted Database: {database}.");
                    else
                        Console.WriteLine($"Failed to delete Database: {database}.");
                    break;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    Console.WriteLine($"Database located at: {database.GetDatabasePath()}");
                    break;
                }
                else
                    Console.WriteLine("Invalid input. Please enter either Y or N.");
            }
            Console.ReadLine();
        }

        private static void CreateTables(string databaseName)
        {
            var tables = Database.GetExistingTables(databaseName);
            Console.WriteLine($"{databaseName} has {tables.Count} tables{(tables.Count > 0 ? ":" : string.Empty)}");

            if (tables.Count > 0)
                foreach (var table in tables)
                    Console.WriteLine($"  - {table}");
        }

        private static Database CreateDatabase(string databaseName)
        {
            var database = new Database(databaseName, "./TableSchema.sqlite");

            if (Database.Exists(database))
                Console.WriteLine($"Created Database: {database}.");
            else
                Console.WriteLine($"Failed to create Database: {database}.");

            return database;
        }
    }
}
