using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CalculationAPI.Controllers;
using CalculationAPI.Database;
using CalculationAPI.Services;
using Microsoft.Data.Sqlite;

namespace CalculationAPI.Tests
{
    public class CalculationsControllerTests
    {
        private readonly Mock<ExpressionEvaluator> _mockEvaluator;
        private readonly Mock<ILogger<CalculationDbContext>> _mockLogger;
        private readonly CalculationsController _controller;
        private readonly string _connectionString = "Data Source=TestCalculations.db";

        public CalculationsControllerTests()
        {
            _mockEvaluator = new Mock<ExpressionEvaluator>();
            _mockLogger = new Mock<ILogger<CalculationDbContext>>();
            _controller = new CalculationsController(_mockEvaluator.Object, _mockLogger.Object);

            // Ensure the test database is created cleanly for each test run
            if (File.Exists("TestCalculations.db"))
            {
                File.Delete("TestCalculations.db");
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Calculations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Expression TEXT NOT NULL,
                        Result TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    )";
                tableCmd.ExecuteNonQuery();
            }
        }

        [Fact]
        public async Task CreateCalculation_SavesToDatabase()
        {
            // Arrange
            var expression = "2 + 3";
            var result = 5.0;
            _mockEvaluator.Setup(e => e.Evaluate(It.IsAny<string>())).Returns(result);

            // Act
            await _controller.CreateCalculation(expression);

            // Assert
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT COUNT(*) FROM Calculations WHERE Expression = $expression AND Result = $result";
                selectCmd.Parameters.AddWithValue("$expression", expression);
                selectCmd.Parameters.AddWithValue("$result", result.ToString());
                var count = (long)selectCmd.ExecuteScalar();

                Assert.Equal(1, count); // Ensure one record is saved
            }
        }

        [Fact]
        public async Task UpdateCalculation_UpdatesDatabaseRecord()
        {
            // Arrange
            var expression = "2 + 3";
            var result = 5.0;
            var newExpression = "3 + 4";
            var newResult = 7.0;

            _mockEvaluator.Setup(e => e.Evaluate(expression)).Returns(result);
            _mockEvaluator.Setup(e => e.Evaluate(newExpression)).Returns(newResult);

            // Act - Create initial calculation
            var createdResult = await _controller.CreateCalculation(expression) as CreatedAtActionResult;
            var calculation = createdResult.Value as Calculation;

            // Act - Update the calculation
            await _controller.UpdateCalculation(calculation.Id, newExpression);

            // Assert
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT Result FROM Calculations WHERE Id = $id";
                selectCmd.Parameters.AddWithValue("$id", calculation.Id);
                var updatedResult = (string)selectCmd.ExecuteScalar();

                Assert.Equal(newResult.ToString(), updatedResult); // Ensure the record is updated
            }
        }

        [Fact]
        public async Task DeleteCalculation_RemovesFromDatabase()
        {
            // Arrange
            var expression = "2 + 3";
            var result = 5.0;
            _mockEvaluator.Setup(e => e.Evaluate(It.IsAny<string>())).Returns(result);

            // Act - Create initial calculation
            var createdResult = await _controller.CreateCalculation(expression) as CreatedAtActionResult;
            var calculation = createdResult.Value as Calculation;

            // Act - Delete the calculation
            await _controller.DeleteCalculation(calculation.Id);

            // Assert
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT COUNT(*) FROM Calculations WHERE Id = $id";
                selectCmd.Parameters.AddWithValue("$id", calculation.Id);
                var count = (long)selectCmd.ExecuteScalar();

                Assert.Equal(0, count); // Ensure the record is deleted
            }
        }
    }
}
