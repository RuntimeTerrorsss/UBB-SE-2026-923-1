using System;
using System.Collections.Generic;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal Balance { get; set; } = 0m;
    public bool IsSuspended { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? Street { get; set; }
    public string? StreetNumber { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public ICollection<Game> OwnedGames { get; set; } = new List<Game>();
    public ICollection<Rental> RentalsAsClient { get; set; } = new List<Rental>();
    public ICollection<Rental> RentalsAsOwner { get; set; } = new List<Rental>();
    public ICollection<ConversationParticipant> Conversations { get; set; } = new List<ConversationParticipant>();
}