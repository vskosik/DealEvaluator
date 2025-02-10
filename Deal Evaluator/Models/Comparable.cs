using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deal_Evaluator.Models;

public class Comparable
{
    [Required] 
    public int Id { get; set; }
    
    [ForeignKey("PropertyId")]
    public int PropertyId { get; set; }
    
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

    [DataType(DataType.DateTime)]
    public DateTime SaleDate { get; set; }
    
    [Required] 
    public ListingStatuses ListingStatus { get; set; }
    [Required] 
    [MaxLength(255)]
    public string Source { get; set; }
    
    public virtual Property Property { get; set; }
}