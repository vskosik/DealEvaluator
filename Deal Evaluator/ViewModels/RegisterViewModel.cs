using System.ComponentModel.DataAnnotations;

namespace Deal_Evaluator.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string CompanyName { get; set; }
}