using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace SQLite.Library
{
    public class Database
    {
        /// <summary>
        /// The name of the database. This will be set when the <see cref="SQLiteDatabase"/> class is initialized.
        /// </summary>
        public static string DatabaseName { get; private set; }

        /// <summary>
        /// This is the directory for where databases will be stored.
        /// </summary>
        public static string DatabaseDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string AppName { get; } = "SQLiteDatabaseExample";

        public static string TableSchemaFile { get; private set; } = "./TableSchema.sqlite";

        public Database(string databaseName, string tableSchemaFile)
        {
            if (null == databaseName) throw new ArgumentNullException("A database name needs to be provided");
            if (string.IsNullOrWhiteSpace(tableSchemaFile) || !File.Exists(tableSchemaFile)) throw new ArgumentException("A valid table schema file must be provided.");
            TableSchemaFile = tableSchemaFile;
            DatabaseName = databaseName;
            Initialize(databaseName);
        }

        /// <summary>
        /// Creates a new database, using the given database name.
        /// </summary>
        /// <param name="databaseName">The name of the database to create.</param>
        /// <returns>Returns true if the database was successfully created.</returns>
        public static bool Create(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");

            if (Exists(databaseName))
                return false;   // Database already exists.

            var path = GetDatabasePath(databaseName);

            if (Directory.GetParent(path) is DirectoryInfo parent && !parent.Exists)
                Directory.CreateDirectory(parent.FullName);

            SQLiteConnection.CreateFile(path);

            return Exists(databaseName);
        }

        /// <summary>
        /// Deletes a given database.
        /// </summary>
        /// <param name="databaseName">The name of the database to delete.</param>
        /// <returns>Returns true if the database was successfully deleted.</returns>
        public static bool Delete(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");

            // Check the database exists.
            if (!Exists(databaseName)) return false;

            using (var connection = ConnectToDatabase(databaseName))
            {
                // if the database is currently open, close and dispose of it's connection.
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            // Forcing the Garbage Cleaner ensures database connection data has been completed cleared.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Delete the database file.
            File.Delete(GetDatabasePath(databaseName));

            return !Exists(databaseName);   // Checks whether the database still exists. If it does, this will return false.
        }

        public static bool Delete(Database database) => Delete(database.ToString());

        public static bool Initialize(string databaseName)
        {
            // Check whether the database already exists.
            if (Exists(databaseName))
            {
                // Database does exist, check it is sound.

                // Fetch all tables in DB.
                var tables = GetExistingTables(databaseName);

                // Check whether tables exist.
                return tables.Count != 0;
            }
            else
            {
                // Database doesn't exist, create it.
                if (!Create(databaseName) || !CreateTables(databaseName))
                {
                    // Couldn't create database, or couldn't create database tables.
                    throw new Exception("Failed to create new database.");
                }
            }

            // Database was sucessfully initialized.
            return true;
        }

        /// <summary>
        /// Creates tables within the provided database.
        /// <para>Table schema needs to be manually provided within the TableSchema.sqlite file, within the project root.</para>
        /// </summary>
        /// <param name="databaseName">The name of the database which the tables should be added to.</param>
        /// <returns>Returns true if tables were successfully added.</returns>
        private static bool CreateTables(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");

            if (!Exists(databaseName)) return false;    // Database doesn't exist.

            if (!File.Exists(TableSchemaFile)) return false;    // Table schema file doesn't exist.

            // Retrieve current tables, so that we know what is already there.
            var currentTables = GetExistingTables(databaseName);

            try
            {
                // Read in schema for table creation.
                var sql = File.ReadAllText(TableSchemaFile);
                using (var connection = ConnectToDatabase(databaseName))
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }   
            }
            catch (Exception ex)
            {
                // Log Error.
                Console.WriteLine(ex);
            }

            // Original table count should be greater than the new table count (given new tables will have been added).
            return currentTables.Count < GetExistingTables(databaseName).Count;
        }

        /// <summary>
        /// Creates a new instanced of <see cref="SQLiteConnection"/> and then opens the connection. This instance is then returned.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static SQLiteConnection ConnectToDatabase(string databaseName = "")
        {
            var dbName = string.IsNullOrWhiteSpace(databaseName) ? DatabaseName : databaseName; // Set database name to provided input if not empty, otherwise use existing name.

            if (Exists(dbName)) // Check provided name exists. This will ensure DatabaseName isn't empty, and exists.
            {
                var newConnection = new SQLiteConnection(GetConnectionString(dbName));
                newConnection.Open();
                return newConnection;
            }
            else
            {
                throw new ArgumentException("Unable to connect Database. Ensure it has been initialized.");
            }
        }

        /// <summary>
        /// Provides whether a given Database exists.
        /// </summary>
        /// <param name="databaseName">The database name for which to check whether exists.</param>
        /// <returns>Returns true if the database is found.</returns>
        public static bool Exists(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");

            return File.Exists(GetDatabasePath(databaseName));
        }

        public static bool Exists(Database database) => Exists(database.ToString());

        /// <summary>
        /// Provides the file path for where a database should be stored.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>Returns a path string of where the database should be located, regardless of whether the database exists.</returns>
        public static string GetDatabasePath(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");
            return Path.Combine(DatabaseDirectory, AppName, Path.ChangeExtension(databaseName, "db"));
        }

        public string GetDatabasePath()
        {
            return GetDatabasePath(DatabaseName);
        }

        /// <summary>
        /// Provides the connection string of a given database name.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>Returns a connection string for any database. This does not ensure that the given database actually exists.</returns>
        public static string GetConnectionString(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("A database name needs to be provided.");
            return $"Data Source={GetDatabasePath(databaseName)};Version=3";
        }

        /// <summary>
        /// Returns all the tables which are found in a given database,
        /// </summary>
        /// <param name="databaseName">The name of the database to retrieve the names from.</param>
        /// <returns>Returns a list of strings, of the names of the tables found within the database.</returns>
        public static List<string> GetExistingTables(string databaseName)
        {
            List<string> tables = null;

            using (var connection = ConnectToDatabase(databaseName))
            using (var command = new SQLiteCommand($@"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'", connection))
            {
                var reader = command.ExecuteReader();

                tables = new List<string>();

                while (reader.Read())
                {
                    var nextTable = reader.GetString(0);

                    if (!string.IsNullOrWhiteSpace(nextTable))
                    {
                        tables.Add(nextTable);
                    }
                }
            }

            return tables;
        }

        public override string ToString()
        {
            return DatabaseName;
        }
    }
}
