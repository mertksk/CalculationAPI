using CalculationAPI.Controllers;
using CalculationAPI.Models;
using CalculationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using CalculationAPI.Controllers;
using CalculationAPI.Models;
using CalculationAPI.Services;
using System.Diagnostics.Metrics;

namespace YourNamespace.Tests
{
    public class ControllerTest

        private readonly Mock<ExpressionEvaluator> tester = new();
        private readonly CalculationsController _controller;

        public CalculationsControllerTests()
    {
            _controller = new CalculationsController(tester.Object);
        }

         public void DoesItSaveTheCalculation()
        {
            var expression = "3 + 2";
        tester.Setup(e => e.Evaluate(expression)).Returns(5);

            var result = _controller.CreateCalculation(expression) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        
         public void DeletedContentCheck()
        {
            var id = 1;

            var result = _controller.DeleteCalculation(id) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        public void GetCalculationWorks()
        {
            var id = 1;
            var calculation = new Calculation { Id = id, Expression = "3 + 2", Result = 5 };

            // Simulate finding the calculation
            var testDatabase = new Mock<IList<Calculation>>();
            testDatabase.Setup(db => db.Find(id)).Returns(calculation);

            var result = _controller.GetCalculation(id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(calculation, result.Value);
        }
}
}

