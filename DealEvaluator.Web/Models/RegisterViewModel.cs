using System.ComponentModel.DataAnnotations;

namespace DealEvaluator.Web.Models;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }
    
    public string CompanyName { get; set; }
}