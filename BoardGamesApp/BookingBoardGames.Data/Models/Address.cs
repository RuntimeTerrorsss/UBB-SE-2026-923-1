using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

[Owned]
public class Address
{
    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("street")]
    public string Street { get; set; } = string.Empty;

    [Column("street_number")]
    public string StreetNumber { get; set; } = string.Empty;
}
