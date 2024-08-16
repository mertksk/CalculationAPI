using Microsoft.AspNetCore.Mvc;
using CalculationAPI.Database;
using CalculationAPI.Services;
using System.Text.RegularExpressions;

namespace CalculationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculationsController : ControllerBase
    {
        private readonly ExpressionEvaluator _expressionEvaluator;

        public CalculationsController(ExpressionEvaluator expressionEvaluator)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        // POST /api/calculations
        [HttpPost]
        public IActionResult CreateCalculation([FromBody] string expression)
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

                // storing the calculation in SQLite
                using (var dbContext = new CalculationDbContext())
                {
                    dbContext.InsertCalculation(calculation.Expression, calculation.Result);
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
        public IActionResult GetCalculation(int id)
        {
            using (var dbContext = new CalculationDbContext())
            {
                var calculation = dbContext.GetCalculationById(id);
                if (calculation == null)
                {
                    return NotFound();
                }

                return Ok(calculation);
            }
        }

        // PUT /api/calculations/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCalculation(int id, [FromBody] string expression)
        {
            using (var dbContext = new CalculationDbContext())
            {
                var calculation = dbContext.GetCalculationById(id);
                if (calculation == null)
                {
                    return NotFound();
                }

                expression = ReplaceReferences(expression);
                calculation.Expression = expression;
                calculation.Result = _expressionEvaluator.Evaluate(expression).ToString(); // Convert double to string here

                dbContext.UpdateCalculation(calculation.Id, calculation.Expression, calculation.Result);
                return Ok(calculation);
            }
        }

        // DELETE /api/calculations/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCalculation(int id)
        {
            using (var dbContext = new CalculationDbContext())
            {
                var deleted = dbContext.DeleteCalculation(id);
                if (deleted)
                {
                    return NoContent();
                }

                return NotFound();
            }
        }

        // To call previous calculations using IDs {1}
        private string ReplaceReferences(string expression)
        {
            var matches = Regex.Matches(expression, @"\{(\d+)\}");
            using (var dbContext = new CalculationDbContext())
            {
                foreach (Match match in matches)
                {
                    int id = int.Parse(match.Groups[1].Value);
                    var referencedCalculation = dbContext.GetCalculationById(id);
                    if (referencedCalculation != null)
                    {
                        expression = expression.Replace(match.Value, referencedCalculation.Result);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Referenced expression with ID {id} not found.");
                    }
                }
            }

            return expression;
        }
    }
}
