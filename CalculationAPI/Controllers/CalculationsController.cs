using Microsoft.AspNetCore.Mvc;
using CalculationAPI.Models;
using CalculationAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculationsController : ControllerBase
    {
        private static readonly Dictionary<int, Calculation> _calculations = new();
        private static int _nextId = 1;
        private readonly ExpressionEvaluator _expressionEvaluator;

        public CalculationsController(ExpressionEvaluator expressionEvaluator)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        // POST /calculations
        [HttpPost]
        public IActionResult CreateCalculation([FromBody] string expression)
        {
            try
            {
                // Replace references in the expression like {1}, {2}
                expression = ReplaceReferences(expression);

                var result = _expressionEvaluator.Evaluate(expression);
                var calculation = new Calculation { Id = _nextId++, Expression = expression, Result = result };
                _calculations[calculation.Id] = calculation;

                return CreatedAtAction(nameof(GetCalculation), new { id = calculation.Id }, calculation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /calculations/{id}
        [HttpGet("{id}")]
        public IActionResult GetCalculation(int id)
        {
            if (_calculations.TryGetValue(id, out var calculation))
            {
                return Ok(calculation);
            }

            return NotFound();
        }

        // PUT /calculations/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCalculation(int id, [FromBody] string expression)
        {
            if (_calculations.TryGetValue(id, out var calculation))
            {
                expression = ReplaceReferences(expression);
                calculation.Expression = expression;
                calculation.Result = _expressionEvaluator.Evaluate(expression);
                _calculations[id] = calculation;

                return Ok(calculation);
            }

            return NotFound();
        }

        // DELETE /calculations/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCalculation(int id)
        {
            if (_calculations.Remove(id))
            {
                return NoContent();
            }

            return NotFound();
        }

        // Method to replace references in the expression like {1}, {2}
        private string ReplaceReferences(string expression)
        {
            var matches = Regex.Matches(expression, @"\{(\d+)\}");
            foreach (Match match in matches)
            {
                int id = int.Parse(match.Groups[1].Value);
                if (_calculations.TryGetValue(id, out var referencedCalculation))
                {
                    expression = expression.Replace(match.Value, referencedCalculation.Result.ToString());
                }
                else
                {
                    throw new InvalidOperationException($"Referenced expression with ID {id} not found.");
                }
            }

            return expression;
        }
    }
}
