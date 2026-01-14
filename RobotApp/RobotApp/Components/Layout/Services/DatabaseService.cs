using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RobotApp.Services
{
    public class DatabaseService
    {
        private static readonly string servernaam = "wall-e";
        private static readonly string databasename = "robot";
        private static readonly string Gebruikersnaam = "robot";
        private static readonly string wachtwoord = "robot-01";
        public static readonly string _connectionString = $"Server={servernaam};Database={databasename};UID={Gebruikersnaam};password={wachtwoord};TrustServerCertificate=true;";

        public void InsertUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO [User] (Name, Age, IsActive) VALUES (@Name, @Age, @IsActive)";
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Age", user.Age);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }


}
