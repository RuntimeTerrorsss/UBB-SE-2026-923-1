using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookingBoardGames.Api
{
    public static class DatabaseSeeder
    {
        public static void Seed(AppDbContext context)
        {
            try
            {
                // Note: We skip Migrate() here because EnsureCreated() in Program.cs handles the schema for now

                if (!context.Users.Any(u => u.Username == "henry_08"))
                {
                    Console.WriteLine("Database is missing Henry. Re-seeding...");

                    context.Database.ExecuteSqlRaw("DELETE FROM messages; DELETE FROM conversation_participants; DELETE FROM conversations; DELETE FROM payments; DELETE FROM rentals; DELETE FROM games; DELETE FROM users;");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('users', RESEED, 0);");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('games', RESEED, 0);");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('rentals', RESEED, 0);");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('payments', RESEED, 0);");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('conversations', RESEED, 0);");
                    context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('messages', RESEED, 0);");

                    var systemUser = new User { Username = "System", DisplayName = "System", Email = "sys@sys.com", CreatedAt = DateTime.UtcNow };
                    var alice = new User { Username = "alice99", DisplayName = "Alice", Email = "alice@example.com", PasswordHash = "hash1", PhoneNumber = "0711111111", AvatarUrl = "https://i.pravatar.cc/150?u=alice", Balance = 150.00m, Street = "Aleea Godeanu", StreetNumber = "23-25", City = "Cluj-Napoca", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var bob = new User { Username = "bobby_b", DisplayName = "Bob", Email = "bob@example.com", PasswordHash = "hash2", PhoneNumber = "0722222222", AvatarUrl = "hamster.jpg", Balance = 75.50m, Street = "Dorobantilor", StreetNumber = "27", City = "Oradea", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var carol = new User { Username = "carol_xo", DisplayName = "Carol", Email = "carol@example.com", PasswordHash = "hash3", PhoneNumber = "0733333333", AvatarUrl = "carol.jpg", Balance = 200.00m, Street = "Nicolae Titulescu", StreetNumber = "26", City = "Bucuresti", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var david = new User { Username = "dan_the_m", DisplayName = "Dan", Email = "david@example.com", PasswordHash = "hash4", PhoneNumber = "0744444444", AvatarUrl = "https://i.pravatar.cc/150?u=dan", Balance = 50.00m, Street = "Bulevardul Eroilor", StreetNumber = "4", City = "Timisoara", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var emma = new User { Username = "eva_plays", DisplayName = "Eva", Email = "emma@example.com", PasswordHash = "hash5", PhoneNumber = "0755555555", AvatarUrl = "https://i.pravatar.cc/150?u=eva", Balance = 320.00m, Street = "Strada Pacurari", StreetNumber = "88", City = "Iasi", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var frank = new User { Username = "frank_06", DisplayName = "Frank", Email = "frank@example.com", PasswordHash = "hash6", PhoneNumber = "0766666666", AvatarUrl = "https://i.pravatar.cc/150?u=frank", Balance = 10.00m, Street = "Sunset Blvd", StreetNumber = "45", City = "Timisoara", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var grace = new User { Username = "grace_07", DisplayName = "Grace", Email = "grace@example.com", PasswordHash = "hash7", PhoneNumber = "0777777777", AvatarUrl = "https://i.pravatar.cc/150?u=grace", Balance = 500.00m, Street = "Hill Road", StreetNumber = "3", City = "Iasi", Country = "Romania", CreatedAt = DateTime.UtcNow };
                    var henry = new User { Username = "henry_08", DisplayName = "Henry", Email = "henry@example.com", PasswordHash = "hash8", PhoneNumber = "0788888888", AvatarUrl = "https://i.pravatar.cc/150?u=henry", Balance = 0.00m, Street = "Lake Street", StreetNumber = "19", City = "Brasov", Country = "Romania", CreatedAt = DateTime.UtcNow };

                    context.Users.AddRange(systemUser, alice, bob, carol, david, emma, frank, grace, henry);
                    context.SaveChanges();

                    var catan = new Game { Name = "Catan", PricePerDay = 1.99m, MinimumPlayerNumber = 3, MaximumPlayerNumber = 4, Description = "Trade and build on the island of Catan.", IsActive = true, Owner = alice };
                    var monopoly = new Game { Name = "Monopoly", PricePerDay = 1.50m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 6, Description = "Classic property trading game.", IsActive = true, Owner = bob };
                    var carcassonne = new Game { Name = "Carcassonne", PricePerDay = 1.20m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Tile placement game.", IsActive = true, Owner = alice };
                    var terraformingMars = new Game { Name = "Terraforming Mars", PricePerDay = 2.50m, MinimumPlayerNumber = 1, MaximumPlayerNumber = 5, Description = "Strategy game about developing Mars.", IsActive = false, Owner = carol };
                    var activity = new Game { Name = "Activity", PricePerDay = 0.50m, MinimumPlayerNumber = 3, MaximumPlayerNumber = 12, Description = "Party game.", IsActive = true, Owner = carol };
                    var chess = new Game { Name = "Chess", PricePerDay = 0.86m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 2, Description = "Classic strategy.", IsActive = true, Owner = carol };
                    var sevenWonders = new Game { Name = "7 Wonders", PricePerDay = 16.00m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 7, Description = "Build a civilization.", IsActive = true, Owner = carol };

                    context.Games.AddRange(catan, monopoly, carcassonne, terraformingMars, activity, chess, sevenWonders);
                    context.SaveChanges();

                    var rental1 = new Rental { Game = catan, Client = bob, Owner = alice, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 3, 7), TotalPrice = 11.94m };
                    var rental2 = new Rental { Game = activity, Client = bob, Owner = carol, StartDate = new DateTime(2026, 3, 10), EndDate = new DateTime(2026, 3, 15), TotalPrice = 2.50m };
                    var rental3 = new Rental { Game = chess, Client = david, Owner = carol, StartDate = new DateTime(2026, 3, 20), EndDate = new DateTime(2026, 3, 25), TotalPrice = 5.16m };
                    var rental4 = new Rental { Game = sevenWonders, Client = david, Owner = carol, StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 10), TotalPrice = 160.00m };
                    var rental5 = new Rental { Game = sevenWonders, Client = frank, Owner = carol, StartDate = new DateTime(2026, 6, 15), EndDate = new DateTime(2026, 6, 18), TotalPrice = 48.00m };

                    context.Rentals.AddRange(rental1, rental2, rental3, rental4, rental5);
                    context.SaveChanges();

                    var payment1 = new Payment { Request = rental1, Client = bob, Owner = alice, PaidAmount = 11.94m, PaymentMethod = "CARD", PaymentState = 1, DateOfTransaction = new DateTime(2026, 3, 1, 10, 0, 0), DateConfirmedBuyer = new DateTime(2026, 3, 1, 10, 0, 0) };
                    var payment2 = new Payment { Request = rental2, Client = bob, Owner = carol, PaidAmount = 2.50m, PaymentMethod = "CASH", PaymentState = 1, DateOfTransaction = new DateTime(2026, 3, 11, 14, 30, 0) };
                    var payment3 = new Payment { Request = rental3, Client = david, Owner = carol, PaidAmount = 5.16m, PaymentMethod = "CARD", PaymentState = 0, DateOfTransaction = new DateTime(2026, 3, 20, 9, 0, 0), DateConfirmedBuyer = new DateTime(2026, 3, 20, 9, 0, 0) };

                    context.Payments.AddRange(payment1, payment2, payment3);

                    var conversation1 = new Conversation
                    {
                        Participants = new List<ConversationParticipant>
                        {
                            new ConversationParticipant { User = alice, LastMessageReadTime = new DateTime(2026, 3, 5, 12, 0, 0) },
                            new ConversationParticipant { User = bob, LastMessageReadTime = new DateTime(2026, 3, 5, 11, 45, 0) },
                        },
                    };

                    var conversation2 = new Conversation
                    {
                        Participants = new List<ConversationParticipant>
                        {
                            new ConversationParticipant { User = bob, LastMessageReadTime = new DateTime(2026, 3, 12, 9, 0, 0) },
                            new ConversationParticipant { User = carol, LastMessageReadTime = new DateTime(2026, 3, 12, 8, 50, 0) },
                        },
                    };

                    context.Conversations.AddRange(conversation1, conversation2);
                    context.SaveChanges();

                    var msgs = new List<Message>
                    {
                        new SystemMessage { Conversation = conversation1, Sender = systemUser, Receiver = systemUser, MessageSentTime = new DateTime(2026, 3, 1, 8, 55, 0), MessageContent = "New conversation", MessageContentAsString = "New conversation" },
                        new RentalRequestMessage { Conversation = conversation1, Sender = bob, Receiver = alice, MessageSentTime = new DateTime(2026, 3, 1, 9, 0, 0), RequestContent = "Hey, is Catan available March 1–7?", IsRequestResolved = false, IsRequestAccepted = false, RentalRequest = rental1, MessageContentAsString = "Rental Request" },
                        new TextMessage { Conversation = conversation1, Sender = alice, Receiver = bob, MessageSentTime = new DateTime(2026, 3, 1, 9, 5, 0), TextMessageContent = "Yes, it's free — it's all yours!", MessageContentAsString = "Yes, it's free — it's all yours!" },
                        new ImageMessage { Conversation = conversation1, Sender = bob, Receiver = alice, MessageSentTime = new DateTime(2026, 3, 1, 9, 8, 0), MessageImageUrl = "hamster.jpg", MessageContentAsString = "[Image]" },
                        new SystemMessage { Conversation = conversation2, Sender = systemUser, Receiver = systemUser, MessageSentTime = new DateTime(2026, 3, 10, 9, 55, 0), MessageContent = "New conversation", MessageContentAsString = "New conversation" },
                        new RentalRequestMessage { Conversation = conversation2, Sender = bob, Receiver = carol, MessageSentTime = new DateTime(2026, 3, 10, 10, 0, 0), RequestContent = "Can I borrow Activity March 10–15?", IsRequestResolved = false, IsRequestAccepted = false, RentalRequest = rental2, MessageContentAsString = "Rental Request" },
                        new TextMessage { Conversation = conversation2, Sender = carol, Receiver = bob, MessageSentTime = new DateTime(2026, 3, 10, 10, 10, 0), TextMessageContent = "Perfect, thanks a lot!", MessageContentAsString = "Perfect, thanks a lot!" },
                    };

                    context.Messages.AddRange(msgs);
                    context.SaveChanges();

                    Console.WriteLine("Mock Data successfully injected via EF Core!");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Database initialization failed: {exception.Message}");
            }
        }
    }
}