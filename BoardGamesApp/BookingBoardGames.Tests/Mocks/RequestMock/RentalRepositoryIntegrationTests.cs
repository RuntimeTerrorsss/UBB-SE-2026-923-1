using BookingBoardGames.Src.Repositories;
using BookingBoardGames;
using BookingBoardGames;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.DTO;
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
                    new SqlCommand($"DELETE FROM Rental WHERE GameId = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Game ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand("INSERT INTO Game (gid, Name, PricePerDay) VALUES (@Id, 'Test Game', @price)", connection);
                    sqlCommand.Parameters.AddWithValue("@Id", gid);
                    sqlCommand.Parameters.AddWithValue("@price", 25.50m);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Game OFF", connection).ExecuteNonQuery();
                }

                // Setup Rental
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM Rental WHERE rid = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Rental ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand(
                        @"INSERT INTO Rental (rid, GameId, ClientId, OwnerId, StartDate, EndDate) 
                        VALUES (@rid, @gid, 0, 0, @start, @end)",
                        connection);
                    sqlCommand.Parameters.AddWithValue("@rid", rid);
                    sqlCommand.Parameters.AddWithValue("@gid", gid);
                    sqlCommand.Parameters.AddWithValue("@start", start);
                    sqlCommand.Parameters.AddWithValue("@end", end);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Rental OFF", connection).ExecuteNonQuery();
                }

                var RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>())).GetById(rid);

                Assert.NotNull(RentalRepository);
                Assert.Equal(
                    new { Id = rid, GameId = gid, StartDate = start, EndDate = end },
                    new { RentalRepository.RequestId, RentalRepository.GameId, RentalRepository.StartDate, RentalRepository.EndDate });
            }
            finally
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM Rental WHERE rid = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", connection).ExecuteNonQuery();
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





