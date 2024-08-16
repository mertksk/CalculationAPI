using Xunit;
using CalculationAPI.Controllers;
using CalculationAPI.Database;
using CalculationAPI.Services;
using Microsoft.AspNetCore.Mvc;

public class ControllerTest
{
    [Fact]
    public void CreateCalculation_Should_SaveToDatabase()
    {
        // Arrange
        var evaluator = new ExpressionEvaluator(); // Assuming your evaluator is already implemented
        var controller = new CalculationsController(evaluator);
        string expression = "5 + 3";

        // Act
        var result = controller.CreateCalculation(expression) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result); // Check that we got a successful response
        Assert.IsType<Calculation>(result.Value); // Ensure a Calculation object was returned
        var calculation = result.Value as Calculation;
        Assert.Equal("5 + 3", calculation.Expression);
        Assert.Equal("8", calculation.Result); // Assuming the evaluator returns a double and it's converted to string

        // Check if it's saved in the database
        using (var dbContext = new CalculationDbContext())
        {
            var savedCalculation = dbContext.GetCalculationById(calculation.Id);
            Assert.NotNull(savedCalculation);
            Assert.Equal("5 + 3", savedCalculation.Expression);
            Assert.Equal("8", savedCalculation.Result);
        }
    }
}
