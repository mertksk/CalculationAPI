using Microsoft.AspNetCore.Mvc;
using CalculationAPI.Database;
using CalculationAPI.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalculationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculationsController : ControllerBase
    {
        private readonly ExpressionEvaluator _expressionEvaluator;
        private readonly ILogger<CalculationDbContext> _logger;

        public CalculationsController(ExpressionEvaluator expressionEvaluator, ILogger<CalculationDbContext> logger)
        {
            _expressionEvaluator = expressionEvaluator;
            _logger = logger;
        }

        // POST /api/calculations
        [HttpPost]
        public async Task<IActionResult> CreateCalculation([FromBody] string expression)
        {
            try
            {
                // Replace references to previous calculations
                expression = ReplaceReferences(expression);

                var result = _expressionEvaluator.Evaluate(expression);
                var calculation = new Calculation
                {
                    Expression = expression,
                    Result = result.ToString()
                };

                // Store the calculation in SQLite
                using (var dbContext = new CalculationDbContext(_logger))
                {
                    await dbContext.InsertCalculationAsync(calculation.Expression, calculation.Result);
                    calculation.Id = dbContext.GetLastInsertId();
                }

                return CreatedAtAction(nameof(GetCalculation), new { id = calculation.Id }, calculation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/calculations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCalculation(int id)
        {
            using (var dbContext = new CalculationDbContext(_logger))
            {
                var calculation = await dbContext.GetCalculationByIdAsync(id);
                if (calculation == null)
                {
                    return NotFound();
                }
                return Ok(calculation);
            }
        }

        // PUT /api/calculations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalculation(int id, [FromBody] string newExpression)
        {
            using (var dbContext = new CalculationDbContext(_logger))
            {
                var existingCalculation = await dbContext.GetCalculationByIdAsync(id);
                if (existingCalculation == null)
                {
                    return NotFound();
                }

                var updatedResult = _expressionEvaluator.Evaluate(newExpression);
                existingCalculation.Expression = newExpression;
                existingCalculation.Result = updatedResult.ToString();

                await dbContext.UpdateCalculationAsync(id, existingCalculation.Expression, existingCalculation.Result);

                return Ok(existingCalculation);
            }
        }

        // DELETE /api/calculations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalculation(int id)
        {
            using (var dbContext = new CalculationDbContext(_logger))
            {
                var existingCalculation = await dbContext.GetCalculationByIdAsync(id);
                if (existingCalculation == null)
                {
                    return NotFound();
                }

                await dbContext.DeleteCalculationAsync(id);
                return NoContent();
            }
        }

        private string ReplaceReferences(string expression)
        {
            // Implementation for replacing {id} references in the expression
            return expression; // This would contain the logic to replace references with actual expressions
        }
    }
}
