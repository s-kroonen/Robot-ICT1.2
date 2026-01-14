using Microsoft.Data.SqlClient;
using RobotApp.Services;

public class Robot
{
    public required int Id { get; set;}
    public required string Name { get; set;}
    public required int Temp { get; set;}
    public required int Hum { get; set;}
    public required bool IsActive { get; set;}

    public void InsertRobot(Robot robot)
    {
        using (var connection = new SqlConnection(DatabaseService._connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO [User] (Name, Temp, Hum, IsActive) VALUES (@Name, @Temp, @Hum, @IsActive)"; 
                command.Parameters.AddWithValue("@Name", robot.Name);
                command.Parameters.AddWithValue("@Temp", robot.Temp);
                command.Parameters.AddWithValue("@Hum", robot.Hum);
                command.Parameters.AddWithValue("@IsActive", robot.IsActive);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}