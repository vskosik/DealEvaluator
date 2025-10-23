namespace DealEvaluator.Domain.Entities;

public class ApiLog
{
    public int Id { get; set; }
    public int? PropertyId { get; set; }
    public string? UserId { get; set; }
    public string Endpoint { get; set; }
    public string RequestData { get; set; }
    public string ResponseData { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}