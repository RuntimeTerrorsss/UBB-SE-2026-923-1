using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<InterfaceGamesRepository> _mockGamesRepository;
        private readonly Mock<IRentalRepository> _mockRentalsRepository;
        private readonly Mock<IUserRepository> _mockUsersRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockGamesRepository = new Mock<InterfaceGamesRepository>();
            _mockRentalsRepository = new Mock<IRentalRepository>();
            _mockUsersRepository = new Mock<IUserRepository>();

            _bookingService = new BookingService(
                _mockGamesRepository.Object,
                _mockRentalsRepository.Object,
                _mockUsersRepository.Object);
        }

        #region GetBookingInformationForSpecificGame

        [Fact]
        public async Task GetBookingInformationForSpecificGame_GameIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            _mockGamesRepository.Setup(r => r.GetGameById(gameId)).ReturnsAsync((Game)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal($"Game with id {gameId} was not found.", exception.Message);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_OwnerIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var ownerId = 2;
            var mockGame = new Game { Id = gameId, Name = "Test Game", OwnerId = ownerId };

            _mockGamesRepository.Setup(r => r.GetGameById(gameId)).ReturnsAsync(mockGame);
            _mockUsersRepository.Setup(r => r.GetGameById(ownerId)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal($"Owner for game id {gameId} was not found.", exception.Message);
        }

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_NegativeDays_HitsMinimumDayCountBranch()
        {
            // Arrange
            var pricePerDay = 15m;
            var startTime = DateTime.UtcNow;

            // Fix: Subtract at least 1 full day (e.g., 2 days) to force TimeSpan.Days to be negative.
            // (startTime.AddDays(-2) - startTime).Days evaluates to -2. 
            // -2 + 1 = -1. 
            // -1 < 1 is TRUE (Hits the if statement)
            var endTime = startTime.AddDays(-2);

            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            // The if statement will override the -1 back to 1
            var expectedPrice = 1 * pricePerDay;

            // Act
            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_ValidData_ReturnsBookingDTO()
        {
            // Arrange
            var gameId = 1;
            var ownerId = 2;
            var mockGame = new Game
            {
                Id = gameId,
                Name = "Test",
                Image = Array.Empty<byte>(),
                PricePerDay = 10m,
                OwnerId = ownerId,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "Desc"
            };
            var mockOwner = new User
            {
                Id = ownerId,
                DisplayName = "John Doe",
                City = "New York",
                IsSuspended = false,
                AvatarUrl = "avatar.png",
                CreatedAt = DateTime.UtcNow
            };

            _mockGamesRepository.Setup(r => r.GetGameById(gameId)).ReturnsAsync(mockGame);
            _mockUsersRepository.Setup(r => r.GetGameById(ownerId)).ReturnsAsync(mockOwner);

            // Act
            var result = await _bookingService.GetBookingInformationForSpecificGame(gameId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGame.Id, result.GameId);
            Assert.Equal(mockGame.Name, result.Name);
            Assert.Equal(mockOwner.DisplayName, result.DisplayName);
            Assert.Equal(mockOwner.City, result.City);
        }

        [Fact]
        public async Task GetBookingInformationForSpecificGame_RepositoryThrows_RethrowsException()
        {
            // Arrange
            var gameId = 1;
            var expectedException = new Exception("Database failure");
            _mockGamesRepository.Setup(r => r.GetGameById(gameId)).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _bookingService.GetBookingInformationForSpecificGame(gameId));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        #endregion

        #region GetUnavailableTimeRanges

        [Fact]
        public async Task GetUnavailableTimeRanges_ValidRequest_ReturnsTimeRangeArray()
        {
            // Arrange
            var gameId = 1;
            var mockRanges = new List<TimeRange>
            {
                new TimeRange { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1) }
            };

            _mockRentalsRepository.Setup(r => r.GetUnavailableTimeRanges(gameId))
                                  .ReturnsAsync(mockRanges);

            // Act
            var result = await _bookingService.GetUnavailableTimeRanges(gameId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetUnavailableTimeRanges_RepositoryThrows_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            _mockRentalsRepository.Setup(r => r.GetUnavailableTimeRanges(gameId))
                                  .ThrowsAsync(new Exception("DB Error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.GetUnavailableTimeRanges(gameId));

            Assert.StartsWith($"Failed to retrieve unavailable time ranges for game {gameId}.", exception.Message);
        }

        #endregion

        #region CheckGameAvailability

        [Fact]
        public async Task CheckGameAvailability_AvailableGame_ReturnsTrue()
        {
            // Arrange
            var gameId = 1;
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddDays(1);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(r => r.CheckGameAvailability(startTime, endTime, gameId))
                                  .ReturnsAsync(true);

            // Act
            var result = await _bookingService.CheckGameAvailability(gameId, timeRange);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckGameAvailability_RepositoryThrows_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddDays(1);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(r => r.CheckGameAvailability(startTime, endTime, gameId))
                                  .ThrowsAsync(new Exception("DB Error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.CheckGameAvailability(gameId, timeRange));

            Assert.StartsWith($"Failed to check availability for game {gameId}.", exception.Message);
        }

        #endregion

        #region CalculateTotalPriceForRentingASpecificGame

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_MultipleDays_ReturnsCorrectPrice()
        {
            // Arrange
            var pricePerDay = 10m;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(3); // 3 days difference
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            // Expected days: (3) + 1 = 4 days
            var expectedPrice = 4 * pricePerDay;

            // Act
            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public void CalculateTotalPriceForRentingASpecificGame_NegativeOrZeroDays_AppliesMinimumDayCount()
        {
            // Arrange
            var pricePerDay = 15m;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddHours(-5); // End time before start time
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            // Expected days should be fallback to MinimumValidDayCount (1)
            var expectedPrice = 1 * pricePerDay;

            // Act
            var result = _bookingService.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        #endregion

        #region CalculateNumberOfDaysInAGivenTimeRange

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_ValidDifference_ReturnsCalculatedDays()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            // Act
            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(3, result); // 2 days diff + 1 MinimumValidDayCount
        }

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_ZeroDifference_ReturnsMinimumValidDayCount()
        {
            // Arrange
            var time = DateTime.UtcNow;
            var timeRange = new TimeRange { StartTime = time, EndTime = time }; // 0 days diff

            // Act
            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(1, result); // 0 + 1 = 1, >= 1
        }

        [Fact]
        public void CalculateNumberOfDaysInAGivenTimeRange_NegativeDifference_ReturnsMinimumValidDayCount()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(-2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            // Act
            var result = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(1, result); // -2 + 1 = -1, which is < 1, so falls back to 1
        }

        #endregion

        #region AddBooking

        [Fact]
        public async Task AddBooking_ClientIdIsZeroOrLess_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var clientId = 0;
            var timeRange = new TimeRange { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.AddBooking(gameId, clientId, timeRange));

            Assert.Equal("A valid logged-in renter account is required to complete a booking.", exception.Message);
        }

        [Fact]
        public async Task AddBooking_ValidRequest_CallsBookGameWithRentalRequest()
        {
            // Arrange
            var gameId = 1;
            var clientId = 2;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(r => r.BookGameWithRentalRequest(clientId, gameId, startTime, endTime))
                                  .Returns(Task.CompletedTask);

            // Act
            await _bookingService.AddBooking(gameId, clientId, timeRange);

            // Assert
            _mockRentalsRepository.Verify(r => r.BookGameWithRentalRequest(clientId, gameId, startTime, endTime), Times.Once);
        }

        [Fact]
        public async Task AddBooking_RepositoryThrows_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var clientId = 2;
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);
            var timeRange = new TimeRange { StartTime = startTime, EndTime = endTime };

            _mockRentalsRepository.Setup(r => r.BookGameWithRentalRequest(clientId, gameId, startTime, endTime))
                                  .ThrowsAsync(new Exception("DB Connection Failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.AddBooking(gameId, clientId, timeRange));

            Assert.StartsWith($"Failed to add booking for game {gameId}.", exception.Message);
        }

        #endregion
    }
}