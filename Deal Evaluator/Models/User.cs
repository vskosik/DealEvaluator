using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Deal_Evaluator.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}