using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Castle.Components.DictionaryAdapter;

namespace Deal_Evaluator.Models;

public class ApiLog
{
    [Required]
    public int Id { get; set; }
    
    [ForeignKey("PropertyId")] 
    public int PropertyId { get; set; }
    [Required]
    [ForeignKey("UserId")]
    public int UserId { get; set; }
    
    [Required] 
    [MaxLength(255)]
    public string Endpoint { get; set; }

    public string RequestData { get; set; }
    public string ResponseData { get; set; }
    
    [Required] 
    public bool Success { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Property Property { get; set; }
    public virtual User User { get; set; }
}