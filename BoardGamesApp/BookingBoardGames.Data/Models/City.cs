using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("cities")]
public class City
{
    [Key]
    [Column("id")]
    public int CityId { get; set; }

    [Column("main_name")]
    required public string MainName { get; set; }

    [Column("names")]
    required public List<string> Names { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }
}