using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CalculationAPI.Database
{
    public class CalculationDbContext : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ILogger<CalculationDbContext> _logger; // Logs the data to track issues may occur

        public CalculationDbContext(ILogger<CalculationDbContext> logger)
        {
            _logger = logger;
            _connection = new SqliteConnection("Data Source=calculations.db");
            _connection.Open();

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the table.");
                throw;
            }
        }

        public async Task InsertCalculationAsync(string expression, string result)
        {
            try
            {
                var insertCmd = _connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Calculations (Expression, Result, CreatedAt)
                    VALUES ($expression, $result, $createdAt)";
                insertCmd.Parameters.AddWithValue("$expression", expression);
                insertCmd.Parameters.AddWithValue("$result", result);
                insertCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));

                await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the calculation.");
                throw;
            }
        }

        public async Task<Calculation> GetCalculationByIdAsync(int id)
        {
            try
            {
                var selectCmd = _connection.CreateCommand();
                selectCmd.CommandText = "SELECT Id, Expression, Result, CreatedAt FROM Calculations WHERE Id = $id";
                selectCmd.Parameters.AddWithValue("$id", id);

                using (var reader = await selectCmd.ExecuteReaderAsync())
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
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the calculation.");
                throw;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
