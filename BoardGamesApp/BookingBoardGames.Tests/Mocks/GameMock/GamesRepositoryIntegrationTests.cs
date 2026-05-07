using BookingBoardGames;
using BookingBoardGames;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data;
using System;
using Microsoft.Data.SqlClient;
using Xunit;

public class GamesRepositoryIntegrationTests
{
    private readonly string connectionString;

    public GamesRepositoryIntegrationTests()
    {
        DatabaseBootstrap.Initialize();
        connectionString = DatabaseBootstrap.GetAppConnection();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string is null!");
        }
    }

    [Fact]
    public void GetById_GameExists_ReturnsGame()
    {
        int testId = 12344;

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var deleteOld = new SqlCommand("DELETE FROM Games WHERE Id = @Id", connection);
                deleteOld.Parameters.AddWithValue("@Id", testId);
                deleteOld.ExecuteNonQuery();

                new SqlCommand("SET IDENTITY_INSERT Games ON", connection).ExecuteNonQuery();
                var insert = new SqlCommand(
                    "INSERT INTO games (id, name, price, minimum_player_number, maximum_player_number, description, is_active, owner_id) VALUES (@Id, 'TestGame', 15, 1, 4, 'Test Description', 1, 1)", connection);
                insert.Parameters.AddWithValue("@Id", testId);
                insert.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT Games OFF", connection).ExecuteNonQuery();
            }

            var GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            var game = GamesRepository.GetGameById(testId);

            Assert.NotNull(game);
            Assert.Equal(
                new { Id = testId, Name = "TestGame", PricePerDay = (decimal)15 },
                new { game.Id, game.Name, game.PricePerDay });
        }
        finally
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var delete = new SqlCommand("DELETE FROM Games WHERE Id = @Id", connection);
                delete.Parameters.AddWithValue("@Id", testId);
                delete.ExecuteNonQuery();
            }
        }
    }

    [Fact]
    public void GetById_GameDoesNotExist_ReturnsNull()
    {
        var GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
        var game = GamesRepository.GetGameById(-999);

        Assert.Null(game);
    }

    [Fact]
    public void GetPriceGameById_GameExists_ReturnsCorrectPrice()
    {
        int testId = 12345;
        decimal expectedPrice = 25.50m;

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var deleteOld = new SqlCommand("DELETE FROM Games WHERE Id = @Id", connection);
                deleteOld.Parameters.AddWithValue("@Id", testId);
                deleteOld.ExecuteNonQuery();

                new SqlCommand("SET IDENTITY_INSERT Games ON", connection).ExecuteNonQuery();
                var insert = new SqlCommand(
                    "INSERT INTO games (id, name, price, minimum_player_number, maximum_player_number, description, is_active, owner_id) VALUES (@Id, 'PriceTestGame', @price, 1, 4, 'Test Description', 1, 1)", connection);
                insert.Parameters.AddWithValue("@Id", testId);
                insert.Parameters.AddWithValue("@price", expectedPrice);
                insert.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT Games OFF", connection).ExecuteNonQuery();
            }

            var GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));

            decimal actualPrice = GamesRepository.GetPriceGameById(testId);

            Assert.Equal(expectedPrice, actualPrice);
        }
        finally
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var delete = new SqlCommand("DELETE FROM Games WHERE Id = @Id", connection);
                delete.Parameters.AddWithValue("@Id", testId);
                delete.ExecuteNonQuery();
            }
        }
    }

    [Fact]
    public void GetPriceGameById_GameDoesNotExist_ReturnsZero()
    {
        var GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
        var price = GamesRepository.GetPriceGameById(-999);

        Assert.Equal(0m, price);
    }
}





