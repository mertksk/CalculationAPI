using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
 
namespace CalculationAPI.Database
{
    public class CalculationDbContext : IDisposable
    {
        private readonly SqliteConnection _connection;

        public CalculationDbContext()
        {
            _connection = new SqliteConnection("Data Source=calculations.db");
            _connection.Open();

            var tableCmd = _connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Calculations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Expression TEXT NOT NULL,
                    Result TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";
            tableCmd.ExecuteNonQuery();
        }

        public void InsertCalculation(string expression, string result)
        {
            var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Calculations (Expression, Result, CreatedAt)
                VALUES ($expression, $result, $createdAt)";
            insertCmd.Parameters.AddWithValue("$expression", expression);
            insertCmd.Parameters.AddWithValue("$result", result);
            insertCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }

        public Calculation GetCalculationById(int id)
        {
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, Expression, Result, CreatedAt FROM Calculations WHERE Id = $id";
            selectCmd.Parameters.AddWithValue("$id", id);

            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Calculation
                    {
                        Id = reader.GetInt32(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        CreatedAt = DateTime.Parse(reader.GetString(3))
                    };
                }
            }

            return null;
        }

        public void UpdateCalculation(int id, string expression, string result)
        {
            var updateCmd = _connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Calculations
                SET Expression = $expression, Result = $result
                WHERE Id = $id";
            updateCmd.Parameters.AddWithValue("$expression", expression);
            updateCmd.Parameters.AddWithValue("$result", result);
            updateCmd.Parameters.AddWithValue("$id", id);
            updateCmd.ExecuteNonQuery();
        }

        public bool DeleteCalculation(int id)
        {
            var deleteCmd = _connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Calculations WHERE Id = $id";
            deleteCmd.Parameters.AddWithValue("$id", id);
            return deleteCmd.ExecuteNonQuery() > 0;
        }

        public int GetLastInsertId()
        {
            var lastIdCmd = _connection.CreateCommand();
            lastIdCmd.CommandText = "SELECT last_insert_rowid()";
            return (int)(long)lastIdCmd.ExecuteScalar();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
