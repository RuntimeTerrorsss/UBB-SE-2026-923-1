using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("games")]
public class Game
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]

    public string Name { get; set; } = string.Empty;

    [Column("price")]

    public decimal PricePerDay { get; set; }

    [Column("maximum_player_number")]

    public int MaximumPlayerNumber { get; set; }

    [Column("minimum_player_number")]

    public int MinimumPlayerNumber { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("image")]

    public byte[]? Image { get; set; }

    [Column("is_active")]

    public bool IsActive { get; set; }

    [Column("owner_id")]

    public int OwnerId { get; set; }

    [ForeignKey("OwnerId")]

    public User? Owner { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
