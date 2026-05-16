using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;


namespace BookingBoardGames.Tests.Services
{
    public class RentalServiceTests
    {
        private readonly Mock<IRentalRepository> _mockRentalRepository;
        private readonly Mock<InterfaceGamesRepository> _mockGameRepository;
        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            _mockRentalRepository = new Mock<IRentalRepository>();
            _mockGameRepository = new Mock<InterfaceGamesRepository>();

            _rentalService = new RentalService(
                _mockRentalRepository.Object,
                _mockGameRepository.Object);
        }

        #region GetRentalById

        [Fact]
        public async Task GetRentalById_ValidId_ReturnsRentalFromRepository()
        {
            // Arrange
            int rentalId = 1;
            var expectedRental = new Rental { GameId = 2 };
            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync(expectedRental);

            // Act
            var result = await _rentalService.GetRentalById(rentalId);

            // Assert
            Assert.Equal(expectedRental, result);
            _mockRentalRepository.Verify(r => r.GetById(rentalId), Times.Once);
        }

        #endregion

        #region GetRentalPrice

        [Fact]
        public async Task GetRentalPrice_RentalNotFound_ReturnsZero()
        {
            // Arrange
            int rentalId = 1;
            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync((Rental)null);

            // Act
            var result = await _rentalService.GetRentalPrice(rentalId);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetRentalPrice_RentalFound_CalculatesAndReturnsCorrectPrice()
        {
            // Arrange
            int rentalId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2); // 2 days diff + 1 minimum = 3 days total
            var rental = new Rental { GameId = 5, StartDate = startDate, EndDate = endDate };
            decimal pricePerDay = 15m;
            decimal expectedTotalPrice = 45m; // 3 days * 15m

            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync(rental);
            _mockGameRepository.Setup(g => g.GetPriceGameById(rental.GameId)).ReturnsAsync(pricePerDay);

            // Act
            var result = await _rentalService.GetRentalPrice(rentalId);

            // Assert
            Assert.Equal(expectedTotalPrice, result);
        }

        #endregion

        #region GetGameName

        [Fact]
        public async Task GetGameName_RentalNotFound_ReturnsUnknownRental()
        {
            // Arrange
            int rentalId = 1;
            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync((Rental)null);

            // Act
            var result = await _rentalService.GetGameName(rentalId);

            // Assert
            Assert.Equal("Unknown Rental", result);
        }

        [Fact]
        public async Task GetGameName_GameNotFound_ReturnsUnknownGame()
        {
            // Arrange
            int rentalId = 1;
            var rental = new Rental { GameId = 5 };

            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync(rental);
            _mockGameRepository.Setup(g => g.GetGameById(rental.GameId)).ReturnsAsync((Game)null);

            // Act
            var result = await _rentalService.GetGameName(rentalId);

            // Assert
            Assert.Equal("Unknown Game", result);
        }

        [Fact]
        public async Task GetGameName_ValidRentalAndGame_ReturnsGameName()
        {
            // Arrange
            int rentalId = 1;
            var rental = new Rental { GameId = 5 };
            var game = new Game { Name = "Catan" };

            _mockRentalRepository.Setup(r => r.GetById(rentalId)).ReturnsAsync(rental);
            _mockGameRepository.Setup(g => g.GetGameById(rental.GameId)).ReturnsAsync(game);

            // Act
            var result = await _rentalService.GetGameName(rentalId);

            // Assert
            Assert.Equal("Catan", result);
        }

        #endregion

        #region GetUnavailableTimeRanges

        [Fact]
        public async Task GetUnavailableTimeRanges_ValidGameId_ReturnsRangesFromRepository()
        {
            // Arrange
            int gameId = 1;
            var expectedRanges = new List<TimeRange>
            {
                new TimeRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(1))
            };

            _mockRentalRepository.Setup(r => r.GetUnavailableTimeRanges(gameId)).ReturnsAsync(expectedRanges);

            // Act
            var result = await _rentalService.GetUnavailableTimeRanges(gameId);

            // Assert
            Assert.Equal(expectedRanges, result);
        }

        #endregion

        #region CheckGameAvailability

        [Fact]
        public async Task CheckGameAvailability_EndDateBeforeStartDate_ReturnsFalse()
        {
            // Arrange
            int gameId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-1); // End date before start date

            // Act
            var result = await _rentalService.CheckGameAvailability(gameId, startDate, endDate);

            // Assert
            Assert.False(result);
            _mockRentalRepository.Verify(r => r.CheckGameAvailability(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckGameAvailability_ValidDates_ReturnsRepositoryResult(bool repositoryResult)
        {
            // Arrange
            int gameId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(1);

            _mockRentalRepository.Setup(r => r.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(repositoryResult);

            // Act
            var result = await _rentalService.CheckGameAvailability(gameId, startDate, endDate);

            // Assert
            Assert.Equal(repositoryResult, result);
        }

        #endregion

        #region CalculateTotalPriceForRentingASpecificGame

        [Fact]
        public async Task CalculateTotalPriceForRentingASpecificGame_ValidInput_ReturnsCorrectTotal()
        {
            // Arrange
            decimal price = 20m;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2); // 3 days total
            var timeRange = new TimeRange(startDate, endDate);

            decimal expectedTotal = 60m; // 3 * 20

            // Act
            var result = await _rentalService.CalculateTotalPriceForRentingASpecificGame(price, timeRange);

            // Assert
            Assert.Equal(expectedTotal, result);
        }

        #endregion

        #region CalculateNumberOfDaysInAGivenTimeRange

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_PositiveDifference_ReturnsActualDaysPlusOne()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(4); // Diff is 4
            var timeRange = new TimeRange(startDate, endDate);

            // Act
            var result = await _rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(5, result); // 4 + 1 MinimumValidDayCount
        }

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_ZeroDifference_ReturnsMinimumValidDayCount()
        {
            // Arrange
            var sameDate = DateTime.UtcNow;
            var timeRange = new TimeRange(sameDate, sameDate); // Diff is 0

            // Act
            var result = await _rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(1, result); // 0 + 1 MinimumValidDayCount
        }

        [Fact]
        public async Task CalculateNumberOfDaysInAGivenTimeRange_NegativeDifference_ReturnsMinimumValidDayCount()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-2); // Diff is -2. (-2) + 1 = -1
            var timeRange = new TimeRange(startDate, endDate);

            // Act
            var result = await _rentalService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            // Assert
            Assert.Equal(1, result); // Fallbacks to MinimumValidDayCount (1)
        }

        #endregion

        #region CreateRental

        [Fact]
        public async Task CreateRental_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            // Arrange
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate));

            Assert.Equal("End date must be after start date.", exception.Message);
        }

        [Fact]
        public async Task CreateRental_GameUnavailable_ThrowsInvalidOperationException()
        {
            // Arrange
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(1);

            _mockRentalRepository.Setup(r => r.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate));

            Assert.Equal("The game is not available for the selected period.", exception.Message);
        }

        [Fact]
        public async Task CreateRental_ValidRequest_CreatesCalculatesPriceAndReturnsRental()
        {
            // Arrange
            int gameId = 1, clientId = 2, ownerId = 3;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(2); // 3 days total
            decimal pricePerDay = 10m;
            decimal expectedTotalPrice = 30m;

            _mockRentalRepository.Setup(r => r.CheckGameAvailability(startDate, endDate, gameId)).ReturnsAsync(true);
            _mockGameRepository.Setup(g => g.GetPriceGameById(gameId)).ReturnsAsync(pricePerDay);

            _mockRentalRepository.Setup(r => r.AddRental(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CreateRental(gameId, clientId, ownerId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(gameId, result.GameId);
            Assert.Equal(clientId, result.ClientId);
            Assert.Equal(ownerId, result.OwnerId);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(endDate, result.EndDate);
            Assert.Equal(expectedTotalPrice, result.TotalPrice);

            _mockRentalRepository.Verify(r => r.AddRental(It.Is<Rental>(ren => ren.TotalPrice == expectedTotalPrice)), Times.Once);
        }

        #endregion
    }
}