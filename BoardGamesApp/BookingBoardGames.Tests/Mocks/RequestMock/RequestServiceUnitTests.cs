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
    private readonly Mock<InterfaceGamesRepository> mockGameRepository;
    private readonly RentalService RentalService;

    public RentalServiceUnitTests()
    {
        mockRentalRepository = new Mock<IRentalRepository>();
        mockGameRepository = new Mock<InterfaceGamesRepository>();
        RentalService = new RentalService(mockRentalRepository.Object, mockGameRepository.Object);
    }

    [Fact]
    public void GetRequestById_RequestExists_ReturnsCorrectRequest()
    {
        var expectedRequest = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(expectedRequest);

        var resultedRequest = RentalService.GetRequestById(TestRequestId);

        Assert.NotNull(resultedRequest);
        Assert.Equal(
            new { expectedRequest.Id, expectedRequest.GameId, expectedRequest.StartDate, expectedRequest.EndDate },
            new { resultedRequest.Id, resultedRequest.GameId, resultedRequest.StartDate, resultedRequest.EndDate });
    }

    [Fact]
    public void GetRequestPrice_RequestExists_ReturnsCorrectPrice()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRequest = RentalService.GetRequestPrice(TestRequestId);

        Assert.Equal(TestExpectedPrice, resultedRequest);
    }

    [Fact]
    public void GetGameName_RequestAndGameExist_ReturnsCorrectName()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        var game = new Game(TestGameId, TestGameName, TestPricePerDay);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns(game);

        var resultedRequest = RentalService.GetGameName(TestRequestId);

        Assert.Equal(TestGameName, resultedRequest);
    }

    [Fact]
    public void GetRequestPrice_RequestDoesNotExist_ReturnsZero()
    {
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns((Request)null);

        var resultedRequest = RentalService.GetRequestPrice(TestRequestId);

        Assert.Equal(0m, resultedRequest);
    }

    [Fact]
    public void GetRequestPrice_ZeroDays_CalculatesAsOneDay()
    {
        var sameDay = new DateTime(2023, 10, 1);
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, sameDay, sameDay);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRequest = RentalService.GetRequestPrice(TestRequestId);

        Assert.Equal(TestPricePerDay * 1, resultedRequest);
    }

    [Fact]
    public void GetGameName_RequestDoesNotExist_ReturnsUnknownRequest()
    {
        mockRentalRepository.Setup(mockRepository => mockRepository.GetById(TestRequestId)).Returns((Request)null);

        var resultedRequest = RentalService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Request", resultedRequest);
    }

    [Fact]
    public void GetGameName_GameDoesNotExist_ReturnsUnknownGame()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRentalRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns((Game)null);

        var resultedRequest = RentalService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Game", resultedRequest);
    }
}