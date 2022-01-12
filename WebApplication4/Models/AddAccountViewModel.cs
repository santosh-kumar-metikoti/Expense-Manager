using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class AddAccountViewModel
    {
        public string MyRadioField { get; set; }
        [Required]
        public string incomeType { get; set; }

    }
}
