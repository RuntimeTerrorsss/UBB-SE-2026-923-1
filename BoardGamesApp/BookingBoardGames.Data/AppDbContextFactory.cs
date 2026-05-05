using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;
using BookingBoardGames.Data;

namespace BookingBoardGames.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(ResolveConnectionString());

            return new AppDbContext(optionsBuilder.Options);
        }

        private static string ResolveConnectionString()
        {
            string? overrideConnection = Environment.GetEnvironmentVariable("BOOKINGBOARDGAMES_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(overrideConnection))
            {
                return overrideConnection;
            }

            const string databaseName = "MergedBoardGamesDb";
            string[] candidates =
            {
                $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;",
                $"Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;",
            };

            foreach (string candidate in candidates)
            {
                if (CanConnect(candidate))
                {
                    return candidate;
                }
            }

            return candidates[0];
        }

        private static bool CanConnect(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}