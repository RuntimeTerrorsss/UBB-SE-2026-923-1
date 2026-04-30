using System;
using System.Collections.Generic;

public class Rental
{
    public int RentalId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal? TotalPrice { get; set; }

    public int GameId { get; set; }
    public int ClientId { get; set; }
    public int OwnerId { get; set; }


    public Game Game { get; set; }

    public User Client { get; set; }
    public User Owner { get; set; }

    public Payment Payment { get; set; }

    public ICollection<RentalRequestMessage> Messages { get; set; } = new List<RentalRequestMessage>();
}