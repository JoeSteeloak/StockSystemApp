using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockSystemApp.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Amount cannot be less than zero.")]
    public int Stock { get; set; } // Lagersaldo

    [Required]
    public int CategoryId { get; set; } // Främmande nyckel
    public Category? Category { get; set; } // Navigation property

    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>(); 
}
