using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Data.Services;
using BookingBoardGames.Data.DTO;
using System;
using Xunit;

namespace BookingBoardGames.Tests.Mocks.RequestMock
{
    public class RequestUnitTests
    {
        [Fact]
        public void Request_ValidParameters_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var Rental = new Rental(1, 2, 3, 4, start, end);

            var expectedRequest = new { RentalId = 1, GameId = 2, ClientId = 3, OwnerId = 4, StartDate = start, EndDate = end };
            var actualRequest = new { Rental.RentalId, Rental.GameId, Rental.ClientId, Rental.OwnerId, Rental.StartDate, Rental.EndDate };

            Assert.Equal(expectedRequest, actualRequest);
        }

        [Fact]
        public void Request_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var Rental = new Rental(0, 0, 0, 0, DateTime.MinValue, DateTime.MaxValue);
            
            Rental.RentalId = 1;
            Rental.GameId = 2;
            Rental.ClientId = 3;
            Rental.OwnerId = 4;
            Rental.StartDate = start;
            Rental.EndDate = end;

            var expectedRequest = new { RentalId = 1, GameId = 2, ClientId = 3, OwnerId = 4, StartDate = start, EndDate = end };
            var actualRequest = new { Rental.RentalId, Rental.GameId, Rental.ClientId, Rental.OwnerId, Rental.StartDate, Rental.EndDate };

            Assert.Equal(expectedRequest, actualRequest);
        }
    }
}





