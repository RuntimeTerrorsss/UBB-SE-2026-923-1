using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Rental> Rentals { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ConversationId, cp.UserId });

            modelBuilder.Entity<Rental>().HasOne(r => r.Client).WithMany(u => u.RentalsAsClient).HasForeignKey(r => r.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rental>().HasOne(r => r.Owner).WithMany(u => u.RentalsAsOwner).HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>().HasOne(p => p.Client).WithMany().HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>().HasOne(p => p.Owner).WithMany().HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasDiscriminator<string>("MessageCategory")
                .HasValue<TextMessage>("Text")
                .HasValue<ImageMessage>("Image")
                .HasValue<SystemMessage>("System")
                .HasValue<RentalRequestMessage>("RentalRequest")
                .HasValue<CashAgreementMessage>("CashAgreement");

            modelBuilder.Entity<Payment>()
                .HasDiscriminator<string>("PaymentCategory")
                .HasValue<Payment>("Standard")
                .HasValue<HistoryPayment>("History");
        }
    }
}