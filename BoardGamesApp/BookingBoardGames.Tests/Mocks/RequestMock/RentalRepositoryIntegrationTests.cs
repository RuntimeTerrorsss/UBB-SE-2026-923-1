using BookingBoardGames.Data.Interfaces;
using BookingBoardGames;
using BookingBoardGames;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Data.Services;
using BookingBoardGames.Data.DTO;
using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BookingBoardGames.Tests.Mocks.RequestMock
{
    public class RentalRepositoryIntegrationTests
    {
        private readonly string connectionString;

        public RentalRepositoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        [Fact]
        public void GetById_RequestExists_ReturnsRequest()
        {
            int gid = 1334;
            int rid = 1335;
            var start = new DateTime(2023, 10, 01);
            var end = new DateTime(2023, 10, 06);

            try
            {
                // Setup Game
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM rentals WHERE game_id = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM games WHERE id = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT games ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand("INSERT INTO games (id, name, price, minimum_player_number, maximum_player_number, description, is_active, owner_id) VALUES (@Id, 'Test Game', @price, 1, 4, 'Test Description', 1, 1)", connection);
                    sqlCommand.Parameters.AddWithValue("@Id", gid);
                    sqlCommand.Parameters.AddWithValue("@price", 25.50m);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT games OFF", connection).ExecuteNonQuery();
                }

                // Setup Rental
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM rentals WHERE id = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT rentals ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand(
                        @"INSERT INTO rentals (id, game_id, client_id, owner_id, start_date, end_date) 
                        VALUES (@rid, @gid, 1, 1, @start, @end)",
                        connection);
                    sqlCommand.Parameters.AddWithValue("@rid", rid);
                    sqlCommand.Parameters.AddWithValue("@gid", gid);
                    sqlCommand.Parameters.AddWithValue("@start", start);
                    sqlCommand.Parameters.AddWithValue("@end", end);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT rentals OFF", connection).ExecuteNonQuery();
                }

                var RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>())).GetById(rid);

                Assert.NotNull(RentalRepository);
                Assert.Equal(
                    new { RentalId = rid, GameId = gid, StartDate = start, EndDate = end },
                    new { RentalRepository.RentalId, RentalRepository.GameId, RentalRepository.StartDate, RentalRepository.EndDate });
            }
            finally
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM rentals WHERE id = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM games WHERE id = {gid}", connection).ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public void GetById_RequestDoesNotExist_ReturnsNull()
        {
            var RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            var Rental = RentalRepository.GetById(-999);

            Assert.Null(Rental);
        }
    }
}





