using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deal_Evaluator.Models;

public class Evaluation
{
    [Required] 
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("PropertyId")]
    public int PropertyId { get; set; }

    public int? Arv { get; set; }
    public int? RepairCost { get; set; }
    public int? PurchasePrice { get; set; }
    public int? RentalIncome { get; set; }
    public int? CapRate { get; set; }
    public int? CashOnCash { get; set; }
    
    [DataType(DataType.DateTime)] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Property Property { get; set; }
}