using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using SQLite.Library;

namespace UnitTests
{
    [TestClass]
    public class DatabaseTests
    {
        private readonly Database _database;

        public DatabaseTests()
        {
            ClearDBDirectory();
            _database = new Database("Test_0", "./TableSchema.sqlite");
        }

        private static void ClearDBDirectory()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(Database.DatabaseDirectory, Database.AppName));

            // Clear out directory of any or all pre-existing database files.
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                Database.Delete(file.Name);
            }
        }

        [TestMethod]
        public void CheckDatabaseNameAfterInit()
        {
            Assert.AreEqual("Test_0", Database.DatabaseName);
        }

        [TestMethod]
        public void GetDatabasePath()
        {
            Assert.ThrowsException<ArgumentException>(() => Database.GetDatabasePath(null));
            Assert.ThrowsException<ArgumentException>(() => Database.GetDatabasePath(""));
            Assert.ThrowsException<ArgumentException>(() => Database.GetDatabasePath("    "));
            Assert.AreEqual(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Database.AppName, "Test_1.db"), Database.GetDatabasePath("Test_1"));
        }

        [TestMethod]
        public void Exists()
        {
            // SETUP
            Assert.IsTrue(Database.Create("Test_2"));

            Assert.IsTrue(Database.Exists("Test_2"));

            Assert.IsFalse(Database.Exists("NotADatabaseToTest"));
            Assert.ThrowsException<ArgumentException>(() => Database.Exists(""));
            Assert.ThrowsException<ArgumentException>(() => Database.Exists("     "));

            // TEAR DOWN
            Assert.IsTrue(Database.Delete("Test_2"));
        }

        [TestMethod]
        public void GetConnectionString()
        {
            Assert.ThrowsException<ArgumentException>(() => Database.GetConnectionString(null));
            Assert.ThrowsException<ArgumentException>(() => Database.GetConnectionString(""));
            Assert.ThrowsException<ArgumentException>(() => Database.GetConnectionString("     "));
            var path = Database.GetDatabasePath("Test_3");
            Assert.AreEqual($"Data Source={path};Version=3", Database.GetConnectionString("Test_3"));
        }

        [TestMethod]
        public void ConnectToDatabase()
        {
            // SETUP
            Assert.IsTrue(Database.Create("Test_4"));

            Assert.ThrowsException<ArgumentException>(() => Database.ConnectToDatabase("ADatabaseThatDoesNotExist"));

            using (var connection1 = Database.ConnectToDatabase("Test_4"))
            {
                Assert.IsNotNull(connection1);
                Assert.AreEqual(System.Data.ConnectionState.Open, connection1.State);
                Assert.AreEqual(Database.GetConnectionString("Test_4"), connection1.ConnectionString);
            }

            var connection2 = Database.ConnectToDatabase();       // Should connect to initialised DB (created in the CTOR).
            Assert.IsNotNull(connection2);
            Assert.AreEqual(System.Data.ConnectionState.Open, connection2.State);
            Assert.AreEqual(Database.GetConnectionString("Test_0"), connection2.ConnectionString);

            // TEAR DOWN
            Assert.IsTrue(Database.Delete("Test_4"));
        }

        /// <summary>
        /// This test is very breakable, wherein it will need to be updated every time a new table is added to the SQLite Schema file in project root.
        /// </summary>
        [TestMethod]
        public void GetExistingTables()
        {
            // SETUP
            var testDb5 = new Database("Test_5", "./TableSchema.sqlite");
            Assert.IsNotNull(testDb5);

            Assert.ThrowsException<ArgumentException>(() => Database.GetExistingTables("ADatabaseThatDoesNotExist"));

            var tables = Database.GetExistingTables("Test_5");
            Assert.IsNotNull(tables);
            Assert.AreEqual(2, tables.Count);
            Assert.IsTrue(tables.Contains("Profile"));
            Assert.IsTrue(tables.Contains("Image"));

            // TEAR DOWN
            Assert.IsTrue(Database.Delete("Test_5"));
        }

        [TestMethod]
        public void CreateDatabase()
        {
            Assert.ThrowsException<ArgumentException>(() => Database.Create(null));
            Assert.ThrowsException<ArgumentException>(() => Database.Create(""));
            Assert.ThrowsException<ArgumentException>(() => Database.Create("   "));

            Assert.IsTrue(Database.Create("Test_6"));
            Assert.IsFalse(Database.Create("Test_6"));    // Shouldn't be able to create an existing database.

            Assert.IsTrue(Database.Delete("Test_6"));
        }

        [TestMethod]
        public void ListContain_One()
        {
            var list = new List<string>
            {
                "Profile",
                "Image"
            };

            Assert.ThrowsException<ArgumentNullException>(() => Utilities.ListContains(null, "Profile"));
            Assert.ThrowsException<ArgumentException>(() => Utilities.ListContains(list, ""));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Utilities.ListContains(new List<string>(), "Profile"));

            Assert.IsTrue(Utilities.ListContains(list, "Profile"));
            Assert.IsTrue(Utilities.ListContains(list, "profile"));
            Assert.IsTrue(Utilities.ListContains(list, "Image"));
            Assert.IsFalse(Utilities.ListContains(list, "Other"));
        }

        [TestMethod]
        public void ListContains_Many()
        {
            var list = new List<string>
            {
                "Profile",
                "Image"
            };

            Assert.ThrowsException<ArgumentNullException>(() => Utilities.ListContains(null, "Profile", "Image"));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Utilities.ListContains(new List<string>(), "Profile"));

            Assert.IsTrue(Utilities.ListContains(list, "Profile", "Image"));
            Assert.IsTrue(Utilities.ListContains(list, "profile", "IMAGE"));
            Assert.IsTrue(Utilities.ListContains(list, "profile", null));   // Should ignore the null value.
            Assert.IsTrue(Utilities.ListContains(list, "profile", "")); // Should ignore the empty value.
            Assert.IsFalse(Utilities.ListContains(list, "Profile", "Other"));
        }

        [TestMethod]
        public void InitialiseDatabase()
        {
            Assert.IsTrue(Database.Initialize("Test_0"));  // Will initialise the database created in the CTOR.
            Assert.IsTrue(Database.Initialize("Test_8"));  // Will initialise a new database by creating it.
        }

        [TestMethod]
        public void DeleteDatabase()
        {
            // SETUP
            Assert.IsTrue(Database.Create("Test_9"));
            var database10 = new Database("Test_10", "./TableSchema.sqlite");
            Assert.IsNotNull(database10);

            Assert.ThrowsException<ArgumentException>(() => Database.Delete(""));
            Assert.ThrowsException<ArgumentException>(() => Database.Delete("    "));

            Assert.IsTrue(Database.Exists("Test_9"));

            Assert.IsTrue(Database.Delete("Test_9"));

            Assert.IsFalse(Database.Exists("Test_9"));


            Assert.IsTrue(Database.Exists(database10));

            Assert.IsTrue(Database.Delete(database10));

            Assert.IsFalse(Database.Exists(database10));
        }

        [ClassCleanup]
        public static void RemoveDatabaseInitialised()
        {
            Assert.IsTrue(Database.Delete("Test_0"));
            ClearDBDirectory();
        }
    }
}
