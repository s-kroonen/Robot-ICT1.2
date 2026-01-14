using Microsoft.Data.SqlClient;
using RobotApp.Services;

public class User
{
    public required int Id { get; set;}
    public required string Name { get; set;}
    public required int Age { get; set;}
    public required bool IsActive { get; set;}

    public void InsertUser(User user)
    {
        using (var connection = new SqlConnection(DatabaseService._connectionString))
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