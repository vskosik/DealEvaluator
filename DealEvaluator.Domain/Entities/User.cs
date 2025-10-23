using Microsoft.AspNetCore.Identity;

namespace DealEvaluator.Domain.Entities;

public class User : IdentityUser
{
    public string CompanyName { get; set; }
}