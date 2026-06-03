using Microsoft.AspNetCore.Identity;

namespace DealEvaluator.Domain.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; }

    public int ApiCallCount { get; set; }
    public DateTime? ApiCallCountResetDate { get; set; }
}