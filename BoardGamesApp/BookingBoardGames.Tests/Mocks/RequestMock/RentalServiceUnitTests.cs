using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.DTO;
using System;
using Moq;
using Xunit;

public class RentalServiceUnitTests
{
    private const int TestRequestId = 1;
    private const int TestGameId = 10;
    private const int TestClientId = 2;
    private const int TestOwnerId = 3;
    private const decimal TestPricePerDay = 25.50m;
    private const string TestGameName = "Chess";
    private static readonly DateTime TestStartDate = new DateTime(2023, 10, 01);
    private static readonly DateTime TestEndDate = new DateTime(2023, 10, 06);
    private const int TestDays = 5;
    private const decimal TestExpectedPrice = TestPricePerDay * TestDays;

    private readonly Mock<IRentalRepository> mockRentalRepository;
    private readonly Mock<InterfaceGamesRepository> mockGamesRepository;
    private readonly RentalService RentalService;

    public RentalServiceUnitTests()
    {
        mockRentalRepository = new Mock<IRentalRepository>();
        mockGamesRepository = new Mock<InterfaceGamesRepository>();
        RentalService = new RentalService(mockRentalRepository.Object, mockGamesRepository.Object);
    }

    [Fact]
    public void GetRentalById_RequestExists_ReturnsCorrectRequest()
    {
        var expectedRequest = new Rental(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(expectedRequest);

        var resultedRental = RentalService.GetRentalById(TestRequestId);

        Assert.NotNull(resultedRental);
        Assert.Equal(
            new { expectedRequest.RentalId, expectedRequest.GameId, expectedRequest.StartDate, expectedRequest.EndDate },
            new { resultedRental.RentalId, resultedRental.GameId, resultedRental.StartDate, resultedRental.EndDate });
    }

    [Fact]
    public void GetRentalPrice_RequestExists_ReturnsCorrectPrice()
    {
        var Rental = new Rental(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(Rental);
        mockGamesRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRental = RentalService.GetRentalPrice(TestRequestId);

        Assert.Equal(TestExpectedPrice, resultedRental);
    }

    [Fact]
    public void GetGameName_RequestAndGameExist_ReturnsCorrectName()
    {
        var Rental = new Rental(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        var game = new Game(TestGameName, TestPricePerDay, 2, 4, "Description", 1);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(Rental);
        mockGamesRepository.Setup(g => g.GetGameById(TestGameId)).Returns(game);

        var resultedRental = RentalService.GetGameName(TestRequestId);

        Assert.Equal(TestGameName, resultedRental);
    }

    [Fact]
    public void GetRentalPrice_RequestDoesNotExist_ReturnsZero()
    {
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns((Rental)null);

        var resultedRental = RentalService.GetRentalPrice(TestRequestId);

        Assert.Equal(0m, resultedRental);
    }

    [Fact]
    public void GetRentalPrice_ZeroDays_CalculatesAsOneDay()
    {
        var sameDay = new DateTime(2023, 10, 1);
        var Rental = new Rental(TestRequestId, TestGameId, TestClientId, TestOwnerId, sameDay, sameDay);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(Rental);
        mockGamesRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRental = RentalService.GetRentalPrice(TestRequestId);

        Assert.Equal(TestPricePerDay * 1, resultedRental);
    }

    [Fact]
    public void GetGameName_RequestDoesNotExist_ReturnsUnknownRequest()
    {
        mockRentalRepository.Setup(mockRepository => mockRepository.GetById(TestRequestId)).Returns((Rental)null);

        var resultedRental = RentalService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Rental", resultedRental);
    }

    [Fact]
    public void GetGameName_GameDoesNotExist_ReturnsUnknownGame()
    {
        var Rental = new Rental(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(Rental);
        mockGamesRepository.Setup(g => g.GetById(TestGameId)).Returns((Game)null);

        var resultedRental = RentalService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Game", resultedRental);
    }
}





