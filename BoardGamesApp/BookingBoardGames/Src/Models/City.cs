using System.Collections.Generic;

namespace BookingBoardGames.Src.Models;

public class City
{
    public int CityId { get; set; }

    public required string MainName { get; set; }

    public required List<string> Names { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
