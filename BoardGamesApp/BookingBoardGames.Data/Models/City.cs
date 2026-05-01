using System.Collections.Generic;

public class City
{
    public int CityId { get; set; }
    required public string MainName { get; set; }
    required public List<string> Names { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}