using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deal_Evaluator.Models;

public class MarketData
{
    [Required]
    public int Id { get; set; }

    [ForeignKey("PropertyId")]
    public int PropertyId { get; set; }
    
    [Required] 
    [MaxLength(255)]
    public string Source { get; set; }
    [Required] 
    public string DataJson { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime LastUpdated { get; set; }

    public virtual Property Property { get; set; }
}