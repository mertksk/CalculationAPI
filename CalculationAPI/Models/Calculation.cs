using System.ComponentModel.DataAnnotations;

namespace CalculationAPI.Models
{
    public class Calculation
    {
        [Key]
        public int Id { get; set; }
        public string Expression { get; set; }
        public double Result { get; set; }
    }
}