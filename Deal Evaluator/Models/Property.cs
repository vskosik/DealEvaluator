using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Deal_Evaluator.Models;

public class Property
{
    [Required] 
    public int Id { get; set; }
    
    [ForeignKey("UserId")]
    public int UserId { get; set; }
    
    [Required] 
    [MaxLength(255)]
    public string Address { get; set; }
    [Required] 
    [MaxLength(100)]
    public string City { get; set; }
    [Required] 
    [MaxLength(50)]
    public string State { get; set; }
    [Required] 
    [MaxLength(5)]
    public string ZipCode { get; set; }

    public int? Price { get; set; }
    public int? Sqft { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? LotSizeSqft { get; set; }
    public int? YearBuilt { get; set; }

    [Required] 
    public PropertyTypes PropertyType { get; set; }
    [Required] 
    public PropertyConditions PropertyConditions { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; }
}