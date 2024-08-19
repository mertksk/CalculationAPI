using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CalculationAPI.Database
{
    public class CalculationDbContext : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ILogger<CalculationDbContext> _logger;

        public CalculationDbContext(ILogger<CalculationDbContext> logger = null)
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
                _logger?.LogError(ex, "An error occurred while creating the table.");
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
                _logger?.LogError(ex, "An error occurred while inserting the calculation.");
                throw;
            }
        }

        public async Task UpdateCalculationAsync(int id, string expression, string result)
        {
            try
            {
                var updateCmd = _connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE Calculations
                    SET Expression = $expression, Result = $result, CreatedAt = $createdAt
                    WHERE Id = $id";
                updateCmd.Parameters.AddWithValue("$expression", expression);
                updateCmd.Parameters.AddWithValue("$result", result);
                updateCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
                updateCmd.Parameters.AddWithValue("$id", id);

                await updateCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while updating the calculation.");
                throw;
            }
        }

        public async Task DeleteCalculationAsync(int id)
        {
            try
            {
                var deleteCmd = _connection.CreateCommand();
                deleteCmd.CommandText = @"
                    DELETE FROM Calculations WHERE Id = $id";
                deleteCmd.Parameters.AddWithValue("$id", id);

                await deleteCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while deleting the calculation.");
                throw;
            }
        }

        public async Task<Calculation> GetCalculationByIdAsync(int id)
        {
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT Id, Expression, Result, CreatedAt
                FROM Calculations
                WHERE Id = $id";
            selectCmd.Parameters.AddWithValue("$id", id);

            using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
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

        public int GetLastInsertId()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT last_insert_rowid()";
            return (int)(long)cmd.ExecuteScalar();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
