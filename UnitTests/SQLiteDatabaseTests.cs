﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteDatabaseExample;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class SQLiteDatabaseTests
    {
        private readonly SQLiteDatabase _database;

        public SQLiteDatabaseTests()
        {
            _database = new SQLiteDatabase("Test_0");
        }

        [TestMethod]
        public void CheckDatabaseNameAfterInit()
        {
            Assert.AreEqual("Test_0", SQLiteDatabase.DatabaseName);
        }

        [TestMethod]
        public void GetDatabasePath()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SQLiteDatabase.GetDatabasePath(null));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.GetDatabasePath(""));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.GetDatabasePath("    "));
            Assert.AreEqual(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Test_1.sqlite"), SQLiteDatabase.GetDatabasePath("Test_1"));
        }

        [TestMethod]
        public void Exists()
        {
            // SETUP
            Assert.IsTrue(SQLiteDatabase.Create("Test_2"));

            Assert.ThrowsException<ArgumentNullException>(() => SQLiteDatabase.Exists(null));

            Assert.IsTrue(SQLiteDatabase.Exists("Test_2"));

            Assert.IsFalse(SQLiteDatabase.Exists("NotADatabaseToTest"));
            Assert.IsFalse(SQLiteDatabase.Exists(""));
            Assert.IsFalse(SQLiteDatabase.Exists("     "));

            // TEAR DOWN
            Assert.IsTrue(SQLiteDatabase.Delete("Test_2"));
        }

        [TestMethod]
        public void GetConnectionString()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SQLiteDatabase.GetConnectionString(null));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.GetConnectionString(""));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.GetConnectionString("     "));
            var path = SQLiteDatabase.GetDatabasePath("Test_3");
            Assert.AreEqual($"Data Source={path};Version=3", SQLiteDatabase.GetConnectionString("Test_3"));
        }

        [TestMethod]
        public void ConnectToDatabase()
        {
            // SETUP
            Assert.IsTrue(SQLiteDatabase.Create("Test_4"));

            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.ConnectToDatabase("ADatabaseThatDoesNotExist"));

            var connection1 = SQLiteDatabase.ConnectToDatabase("Test_4");
            Assert.IsNotNull(connection1);
            Assert.AreEqual(System.Data.ConnectionState.Open, connection1.State);
            Assert.AreEqual(SQLiteDatabase.GetConnectionString("Test_4"), connection1.ConnectionString);

            var connection2 = SQLiteDatabase.ConnectToDatabase();       // Should connect to initialised DB (created in the CTOR).
            Assert.IsNotNull(connection2);
            Assert.AreEqual(System.Data.ConnectionState.Open, connection2.State);
            Assert.AreEqual(SQLiteDatabase.GetConnectionString("Test_0"), connection2.ConnectionString);

            // TEAR DOWN
            Assert.IsTrue(SQLiteDatabase.Delete("Test_4"));
        }

        /// <summary>
        /// This test is very breakable, wherein it will need to be updated every time a new table is added to the SQLite Schema file in project root.
        /// </summary>
        [TestMethod]
        public void GetExistingTables()
        {
            // SETUP
            Assert.IsTrue(SQLiteDatabase.Create("Test_5"));
            Assert.IsTrue(_database.CreateTables("Test_5"));

            Assert.ThrowsException<ArgumentException>(() => _database.GetExistingTables("ADatabaseThatDoesNotExist"));

            var tables = _database.GetExistingTables("Test_5");
            Assert.IsNotNull(tables);
            Assert.AreEqual(2, tables.Count);
            Assert.IsTrue(tables.Contains("Profile"));
            Assert.IsTrue(tables.Contains("Image"));

            // TEAR DOWN
            Assert.IsTrue(SQLiteDatabase.Delete("Test_5"));
        }

        [TestMethod]
        public void CreateDatabase()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SQLiteDatabase.Create(null));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.Create(""));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.Create("   "));

            Assert.IsTrue(SQLiteDatabase.Create("Test_6"));
            Assert.IsFalse(SQLiteDatabase.Create("Test_6"));    // Shouldn't be able to create an existing database.

            Assert.IsTrue(SQLiteDatabase.Delete("Test_6"));
        }

        [TestMethod]
        public void CreateTables()
        {
            // SETUP
            Assert.IsTrue(SQLiteDatabase.Create("Test_7"));

            Assert.ThrowsException<ArgumentNullException>(() => _database.CreateTables(null));
            Assert.ThrowsException<ArgumentException>(() => _database.CreateTables(""));
            Assert.ThrowsException<ArgumentException>(() => _database.CreateTables("   "));

            Assert.IsFalse(_database.CreateTables("ADatabaseThatDoesNotExist"));
            Assert.IsTrue(_database.CreateTables("Test_7"));

            // TEAR DOWN
            Assert.IsTrue(SQLiteDatabase.Delete("Test_7"));
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
            Assert.ThrowsException<ArgumentNullException>(() => Utilities.ListContains(list, "", ""));
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
            Assert.IsTrue(_database.Initialize("Test_0"));  // Will initialise the database created in the CTOR.
            Assert.IsTrue(_database.Initialize("Test_8"));  // Will initialise a new database by creating it.
        }

        [TestMethod]
        public void DeleteDatabase()
        {
            // SETUP
            Assert.IsTrue(SQLiteDatabase.Create("Test_9"));

            Assert.ThrowsException<ArgumentNullException>(() => SQLiteDatabase.Delete(null));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.Delete(""));
            Assert.ThrowsException<ArgumentException>(() => SQLiteDatabase.Delete("    "));

            Assert.IsTrue(SQLiteDatabase.Exists("Test_9"));

            Assert.IsTrue(SQLiteDatabase.Delete("Test_9"));

            Assert.IsFalse(SQLiteDatabase.Exists("Test_9"));

            Assert.IsFalse(SQLiteDatabase.Exists("Test_10"));
        }

        [ClassCleanup]
        public static void RemoveDatabaseInitialised()
        {
            Assert.IsTrue(SQLiteDatabase.Delete("Test_0"));
        }
    }
}
