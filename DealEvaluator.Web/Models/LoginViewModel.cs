using System.ComponentModel.DataAnnotations;

namespace DealEvaluator.Web.Models;

public class LoginViewModel
{
    [Required]
    public string Email { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    
    public bool RememberMe { get; set; }
}