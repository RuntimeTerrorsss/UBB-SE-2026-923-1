using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Data
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
                .HasKey(conversationParticipant => new { conversationParticipant.ConversationId, conversationParticipant.UserId });

            modelBuilder.Entity<Rental>().HasOne(rental => rental.Client).WithMany(user => user.RentalsAsClient).HasForeignKey(rental => rental.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rental>().HasOne(rental => rental.Owner).WithMany(user => user.RentalsAsOwner).HasForeignKey(rental => rental.OwnerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>().HasOne(payment => payment.Client).WithMany().HasForeignKey(payment => payment.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>().HasOne(payment => payment.Owner).WithMany().HasForeignKey(payment => payment.OwnerId).OnDelete(DeleteBehavior.Restrict);

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