using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class MyViewModel
    {
        public string MyRadioField { get; set; }
        [Required]
        public string incomeType { get; set; }

    }
}
