using System.Collections.Generic;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PricePerDay { get; set; }
    public int MaximumPlayerNumber { get; set; }
    public int MinimumPlayerNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public byte[]? Image { get; set; }
    public bool IsActive { get; set; }

    public int OwnerId { get; set; }

    public User Owner { get; set; }
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}