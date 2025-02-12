using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Deal_Evaluator.Models;

public class User : IdentityUser
{
    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; }

    public virtual ICollection<Property> Properties { get; set; }
    public virtual ICollection<ApiLog> ApiLogs { get; set; }
}