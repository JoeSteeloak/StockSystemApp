using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Description { get; set; }

    [Required]
    public int Stock { get; set; } // Lagersaldo

    [Required]
    public int CategoryId { get; set; } // Främmande nyckel
    public required Category Category { get; set; } // Navigation property

    public ICollection<InventoryTransaction>? Transactions { get; set; }
}
