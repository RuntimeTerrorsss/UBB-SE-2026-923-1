using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
                Console.WriteLine("Using database connection string from environment variable." + overrideConnection);
                return overrideConnection;
            }

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BookingBoardGamesWeb"))
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (!string.IsNullOrWhiteSpace(connectionString))
                { 
                    Console.WriteLine("Using database connection string from appsettings.json." + connectionString);
                    return connectionString;
                }
                    
            }
            catch
            {
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
                    Console.WriteLine("Using database connection string: " + candidate);
                    return candidate;
                }
            }

            Console.WriteLine("No valid database connection string found. Using default: " + candidates[0]);
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