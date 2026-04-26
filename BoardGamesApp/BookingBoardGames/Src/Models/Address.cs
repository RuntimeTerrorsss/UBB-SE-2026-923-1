using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace BookingBoardGames.Src.Models;

[Owned]
public class Address
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
}
