using Moq;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;

namespace BookingBoardGames.Tests.SearchAndBook.ViewModels;

public class GameDetailsViewModelTests
{
    [Fact]
    public void Constructor_ServiceReturnsData_SetsPropertiesCorrectly()
    {
        var (SystemUnderTesting, _, messages) = CreateSystemUnderTesting();

        Assert.False(SystemUnderTesting.HasError);
        Assert.NotNull(SystemUnderTesting.GameAndUserDetails);
        Assert.NotNull(SystemUnderTesting.UnavailableTimeRanges);
        Assert.Empty(messages);
    }

    [Fact]
    public void Constructor_ServiceThrowsException_SetsErrorAndRaisesMessage()
    {
        var bookingService = new Mock<InterfaceBookingService>();
        bookingService.Setup(bookingService => bookingService.GetBookingInformationForSpecificGame(It.IsAny<int>()))
            .Throws(new Exception("boom"));

        var messages = new List<string>();
        var SystemUnderTesting = new GameDetailsViewModel(bookingService.Object, 1);
        SystemUnderTesting.OnMessageRequested += messages.Add;

        Assert.True(SystemUnderTesting.HasError);
        Assert.Empty(SystemUnderTesting.UnavailableTimeRanges);
    }

    [Fact]
    public void CheckGameAvailability_RangeIsNull_ReturnsFalse()
    {
        var (SystemUnderTesting, _, _) = CreateSystemUnderTesting();

        var result = SystemUnderTesting.CheckGameAvailability(null!);

        Assert.False(result);
    }

    [Fact]
    public void CheckGameAvailability_ServiceReturnsTrue_ReturnsTrue()
    {
        var (SystemUnderTesting, bookingService, _) = CreateSystemUnderTesting();

        var range = CreateRange();
        bookingService.Setup(bookingService => bookingService.CheckGameAvailability(It.IsAny<int>(), range)).Returns(true);

        var result = SystemUnderTesting.CheckGameAvailability(range);

        Assert.True(result);
    }

    [Fact]
    public void CheckGameAvailability_ServiceThrowsException_ReturnsFalseAndRaisesMessage()
    {
        var (SystemUnderTesting, bookingService, messages) = CreateSystemUnderTesting();

        var range = CreateRange();
        bookingService.Setup(bookingService => bookingService.CheckGameAvailability(It.IsAny<int>(), range))
            .Throws(new Exception("boom"));

        var result = SystemUnderTesting.CheckGameAvailability(range);

        Assert.False(result);
        Assert.Single(messages);
    }

    [Fact]
    public void CalculatePrice_ValidRange_ReturnsCorrectTotal()
    {
        var (SystemUnderTesting, bookingService, _) = CreateSystemUnderTesting();

        var range = CreateRange();
        bookingService.Setup(bookingService => bookingService.CalculateTotalPriceForRentingASpecificGame(It.IsAny<decimal>(), range))
            .Returns(100);

        var result = SystemUnderTesting.CalculatePrice(range);

        Assert.Equal(100, result);
        Assert.Equal(100, SystemUnderTesting.TotalPrice);
    }

    [Fact]
    public void CalculatePrice_RangeIsNull_ReturnsZeroAndRaisesMessage()
    {
        var (SystemUnderTesting, _, messages) = CreateSystemUnderTesting();

        var result = SystemUnderTesting.CalculatePrice(null!);

        Assert.Equal(0, result);
        Assert.Equal(0, SystemUnderTesting.TotalPrice);
        Assert.Single(messages);
    }

    [Fact]
    public void CalculatePrice_ServiceThrowsException_ReturnsZeroAndRaisesMessage()
    {
        var (SystemUnderTesting, bookingService, messages) = CreateSystemUnderTesting();

        var range = CreateRange();
        bookingService.Setup(bookingService => bookingService.CalculateTotalPriceForRentingASpecificGame(It.IsAny<decimal>(), range))
            .Throws(new Exception("boom"));

        var result = SystemUnderTesting.CalculatePrice(range);

        Assert.Equal(0, result);
        Assert.Equal(0, SystemUnderTesting.TotalPrice);
        Assert.Single(messages);
    }

    [Fact]
    public void StartBooking_UserNotLoggedIn_RaisesMessage()
    {
        var (SystemUnderTesting, _, messages) = CreateSystemUnderTesting();

        SessionContext.GetInstance().Clear();

        SystemUnderTesting.StartBooking(CreateRange());

        Assert.Single(messages);
        Assert.Contains("User not logged in", messages[0]);
    }

    [Fact]
    public void StartBooking_RangeIsNull_RaisesMessage()
    {
        var (SystemUnderTesting, _, messages) = CreateSystemUnderTesting();

        SessionContext.GetInstance().Populate(CreateUser());

        SystemUnderTesting.StartBooking(null!);

        Assert.Single(messages);
    }

    [Fact]
    public void StartBooking_ValidRange_RaisesStartBookingEvent()
    {
        var (SystemUnderTesting, _, _) = CreateSystemUnderTesting();

        SessionContext.GetInstance().Populate(CreateUser());

        BookingDTO? dto = null;
        TimeRange? rangeResult = null;

        SystemUnderTesting.OnStartBookingRequested += (BookingDTO, TimeRange) =>
        {
            dto = BookingDTO;
            rangeResult = TimeRange;
        };

        var range = CreateRange();
        SystemUnderTesting.StartBooking(range);

        Assert.NotNull(dto);
        Assert.Equal(range, rangeResult);
    }

    [Fact]
    public void GoBack_Invoked_RaisesEvent()
    {
        var (SystemUnderTesting, _, _) = CreateSystemUnderTesting();

        bool called = false;
        SystemUnderTesting.OnGoBackRequested += () => called = true;

        SystemUnderTesting.GoBack();

        Assert.True(called);
    }

    [Fact]
    public void BookCommand_InvalidParameter_RaisesMessage()
    {
        var (SystemUnderTesting, _, messages) = CreateSystemUnderTesting();

        SystemUnderTesting.BookCommand.Execute("invalid");

        Assert.Single(messages);
        Assert.Contains("Invalid booking interval", messages[0]);
    }

    [Fact]
    public void BookCommand_ValidParameter_StartsBooking()
    {
        var (SystemUnderTesting, _, _) = CreateSystemUnderTesting();

        SessionContext.GetInstance().Populate(CreateUser());

        bool called = false;
        SystemUnderTesting.OnStartBookingRequested += (_, _) => called = true;

        var range = CreateRange();
        SystemUnderTesting.BookCommand.Execute(range);

        Assert.True(called);
    }


    private static (GameDetailsViewModel SystemUnderTesting, Mock<InterfaceBookingService> BookingService, List<string> Messages)
        CreateSystemUnderTesting()
    {
        var bookingService = new Mock<InterfaceBookingService>(MockBehavior.Loose);

        bookingService.Setup(s => s.GetBookingInformationForSpecificGame(It.IsAny<int>()))
            .Returns(CreateDto());

        bookingService.Setup(s => s.GetUnavailableTimeRanges(It.IsAny<int>()))
            .Returns(Array.Empty<TimeRange>());

        var SystemUnderTesting = new GameDetailsViewModel(bookingService.Object, 1);

        var messages = new List<string>();
        SystemUnderTesting.OnMessageRequested += messages.Add;

        return (SystemUnderTesting, bookingService, messages);
    }

    private static BookingDTO CreateDto()
    {
        return new BookingDTO
        {
            GameId = 1,
            Name = "Catan",
            Price = 10,
            City = "Cluj",
            MinimumNrPlayers = 2,
            MaximumNumberPlayers = 4,
            Description = "desc",
            UserId = 1,
            DisplayName = "Owner"
        };
    }

    private static TimeRange CreateRange()
    {
        return new TimeRange(DateTime.Now, DateTime.Now.AddDays(1));
    }

    private static User CreateUser()
    {
        return new User
        {
            UserId = 1,
            Username = "user",
            DisplayName = "User",
            Email = "test@test.com",
            PasswordHash = "hash",
            City = "Cluj",
            Country = "RO"
        };
    }
}