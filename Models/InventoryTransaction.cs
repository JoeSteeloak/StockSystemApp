using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockSystemApp.Models;

public class InventoryTransaction
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public int Quantity { get; set; } // Antal in/ut

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

}

