using Moq;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;

namespace BookingBoardGames.Tests.SearchAndBook.ViewModels;

public class FilteredSearchViewModelTests
{
    [Fact]
    public void LoadSearchResults_WithValidFilter_ReturnsResultsFromSearchService()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        var results = new[]
        {
            CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2),
            CreateGameDto(2, "Azul", 15m, "Iasi", 4, 2),
        };

        searchService.Setup(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>())).Returns(results);

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Empty(errors);
        Assert.Equal(results, SystemUnderTesting.BaseResults);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        Assert.Equal(2, SystemUnderTesting.VisibleGames.Count);
        Assert.False(SystemUnderTesting.HasNoResults);
    }

    [Fact]
    public void LoadSearchResults_WhenServiceThrowsException_RaisesErrorAndClearsResults()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        searchService.Setup(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>())).Throws(new Exception("boom"));

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Single(errors);
        Assert.Contains("Could not load search results.", errors[0]);
        Assert.Empty(SystemUnderTesting.BaseResults);
        Assert.Empty(SystemUnderTesting.DisplayedResults);
        Assert.Empty(SystemUnderTesting.VisibleGames);
        Assert.True(SystemUnderTesting.HasNoResults);
    }

    [Fact]
    public void LoadDiscoveryResults_WithValidResults_ReturnsProvidedResults()
    {
        var (SystemUnderTesting, _, _, errors) = CreateSystemUnderTesting();
        var results = new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) };

        SystemUnderTesting.LoadDiscoveryResults(results);

        Assert.Empty(errors);
        Assert.Equal(results, SystemUnderTesting.BaseResults);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        Assert.Single(SystemUnderTesting.VisibleGames);
        Assert.False(SystemUnderTesting.HasNoResults);
    }

    [Fact]
    public void ApplyFilters_WithValidDateRange_DelegatesToSearchService()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        var results = new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) };
        SystemUnderTesting.LoadDiscoveryResults(new[] { CreateGameDto(2, "Azul", 15m, "Iasi", 4, 2) });
        SystemUnderTesting.CurrentFilter.AvailabilityRange = new TimeRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        searchService.Setup(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>())).Returns(results);

        SystemUnderTesting.ApplyFilters();

        Assert.Empty(errors);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        Assert.Single(SystemUnderTesting.VisibleGames);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithInvalidDateRange_RaisesErrorAndSkipsSearch()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.LoadDiscoveryResults(new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) });
        SystemUnderTesting.CurrentFilter.AvailabilityRange = new TimeRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        searchService.Setup(service => service.IsValidDateRange(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Returns(false);

        SystemUnderTesting.ApplyFilters();

        Assert.Single(errors);
        Assert.Contains("Please select a valid date range.", errors[0]);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Never);
    }

    [Fact]
    public void ApplySelectedUiFilters_WithValidValues_UpdatesFilterAndAppliesResults()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        var results = new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) };
        searchService.Setup(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>())).Returns(results);

        SystemUnderTesting.SelectedMaximumPrice = 12.5;
        SystemUnderTesting.SelectedMinimumPlayers = 3;
        SystemUnderTesting.SelectedStartDate = new DateTimeOffset(new DateTime(2026, 1, 1));
        SystemUnderTesting.SelectedEndDate = new DateTimeOffset(new DateTime(2026, 1, 2));

        SystemUnderTesting.ApplySelectedUiFilters();

        Assert.Empty(errors);
        Assert.Equal(12.5m, SystemUnderTesting.CurrentFilter.MaximumPrice);
        Assert.Equal(3, SystemUnderTesting.CurrentFilter.PlayerCount);
        Assert.NotNull(SystemUnderTesting.CurrentFilter.AvailabilityRange);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        searchService.Verify(service => service.UpdateFilterFromUI(It.IsAny<FilterCriteria>(), 12.5, 3, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void ApplySelectedUiFilters_WithInvalidPlayers_RaisesValidationError()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        searchService.Setup(service => service.IsValidPlayersCount(It.IsAny<int?>())).Returns(false);

        SystemUnderTesting.SelectedMinimumPlayers = -1;
        SystemUnderTesting.ApplySelectedUiFilters();

        Assert.Single(errors);
        Assert.Contains("Please enter valid filter values.", errors[0]);
        searchService.Verify(service => service.UpdateFilterFromUI(It.IsAny<FilterCriteria>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Never);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Never);
    }

    [Fact]
    public void RemoveNameFilter_WhenCalled_ClearsNameAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.Name = "Catan";

        SystemUnderTesting.RemoveNameFilter();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.Name);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void RemoveCityFilter_WhenCalled_ClearsCityAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.City = "Cluj";

        SystemUnderTesting.RemoveCityFilter();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.City);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void RemovePriceFilter_WhenCalled_ClearsPriceAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.MaximumPrice = 12m;

        SystemUnderTesting.RemovePriceFilter();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.MaximumPrice);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void RemovePlayersFilter_WhenCalled_ClearsPlayerCountAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.PlayerCount = 3;

        SystemUnderTesting.RemovePlayersFilter();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.PlayerCount);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void RemoveDateFilter_WhenCalled_ClearsAvailabilityRangeAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.AvailabilityRange = new TimeRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));

        SystemUnderTesting.RemoveDateFilter();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.AvailabilityRange);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void SetPriceAscendingSort_WhenCalled_SetsSortOptionAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        SystemUnderTesting.SetPriceAscendingSort();

        Assert.Empty(errors);
        Assert.Equal(SortOption.PriceAscending, SystemUnderTesting.CurrentFilter.SortOption);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void SetPriceDescendingSort_WhenCalled_SetsSortOptionAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        SystemUnderTesting.SetPriceDescendingSort();

        Assert.Empty(errors);
        Assert.Equal(SortOption.PriceDescending, SystemUnderTesting.CurrentFilter.SortOption);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void ClearSorting_WhenCalled_ResetsSortOptionAndReappliesFilters()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.CurrentFilter.SortOption = SortOption.PriceDescending;

        SystemUnderTesting.ClearSorting();

        Assert.Empty(errors);
        Assert.Equal(SortOption.None, SystemUnderTesting.CurrentFilter.SortOption);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void ClearAllFilters_WhenCalled_ResetsSelectionsAndRestoresBaseResults()
    {
        var (SystemUnderTesting, _, _, errors) = CreateSystemUnderTesting();
        var baseResults = new[]
        {
            CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2),
            CreateGameDto(2, "Azul", 15m, "Iasi", 4, 2),
        };
        SystemUnderTesting.LoadDiscoveryResults(baseResults);
        SystemUnderTesting.CurrentFilter.Name = "Catan";
        SystemUnderTesting.CurrentFilter.City = "Cluj";
        SystemUnderTesting.CurrentFilter.AvailabilityRange = new TimeRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        SystemUnderTesting.CurrentFilter.MaximumPrice = 12m;
        SystemUnderTesting.CurrentFilter.PlayerCount = 3;
        SystemUnderTesting.CurrentFilter.SortOption = SortOption.PriceAscending;
        SystemUnderTesting.SelectedMaximumPrice = 12;
        SystemUnderTesting.SelectedMinimumPlayers = 3;
        SystemUnderTesting.SelectedStartDate = new DateTimeOffset(new DateTime(2026, 1, 1));
        SystemUnderTesting.SelectedEndDate = new DateTimeOffset(new DateTime(2026, 1, 2));
        SystemUnderTesting.CitySearchText = "Cluj";
        SystemUnderTesting.LocationError = "error";

        SystemUnderTesting.ClearAllFilters();

        Assert.Empty(errors);
        Assert.Null(SystemUnderTesting.CurrentFilter.Name);
        Assert.Equal(string.Empty, SystemUnderTesting.CurrentFilter.City);
        Assert.Null(SystemUnderTesting.CurrentFilter.AvailabilityRange);
        Assert.Null(SystemUnderTesting.CurrentFilter.MaximumPrice);
        Assert.Null(SystemUnderTesting.CurrentFilter.PlayerCount);
        Assert.Equal(SortOption.None, SystemUnderTesting.CurrentFilter.SortOption);
        Assert.Equal(0, SystemUnderTesting.SelectedMaximumPrice);
        Assert.Equal(0, SystemUnderTesting.SelectedMinimumPlayers);
        Assert.Null(SystemUnderTesting.SelectedStartDate);
        Assert.Null(SystemUnderTesting.SelectedEndDate);
        Assert.Equal(string.Empty, SystemUnderTesting.CitySearchText);
        Assert.Equal(string.Empty, SystemUnderTesting.LocationError);
        Assert.Equal(baseResults, SystemUnderTesting.DisplayedResults);
        Assert.Equal(baseResults, SystemUnderTesting.VisibleGames);
    }

    [Fact]
    public void ApplySortOnly_WithClosestToMeWithoutCity_ShowsLocationError()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        SystemUnderTesting.SelectedSortOption = "Closest to me";

        Assert.Empty(errors);
        Assert.Equal("Please enter a city to measure from.", SystemUnderTesting.LocationError);
        Assert.Null(SystemUnderTesting.SelectedSortOption);
        searchService.Verify(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()), Times.Never);
        searchService.Verify(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>()), Times.Never);
    }

    [Fact]
    public void SearchGamesByFilter_WithValidSelectedDates_LoadsResults()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        var results = new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) };
        searchService.Setup(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>())).Returns(results);
        SystemUnderTesting.SelectedStartDate = new DateTimeOffset(new DateTime(2026, 1, 1));
        SystemUnderTesting.SelectedEndDate = new DateTimeOffset(new DateTime(2026, 1, 2));

        SystemUnderTesting.SearchGamesByFilter(new FilterCriteria());

        Assert.Empty(errors);
        Assert.Equal(results, SystemUnderTesting.BaseResults);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        searchService.Verify(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>()), Times.Once);
    }

    [Fact]
    public void SearchGamesByFilter_WithInvalidSelectedDates_RaisesError()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();
        SystemUnderTesting.SelectedStartDate = new DateTimeOffset(new DateTime(2026, 1, 2));
        SystemUnderTesting.SelectedEndDate = new DateTimeOffset(new DateTime(2026, 1, 1));
        searchService.Setup(service => service.IsValidDateRange(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Returns(false);

        SystemUnderTesting.SearchGamesByFilter(new FilterCriteria());

        Assert.Single(errors);
        Assert.Contains("Please select a valid date range.", errors[0]);
        searchService.Verify(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>()), Times.Never);
    }

    [Fact]
    public void Initialize_WithExistingFilter_CopiesStateAndSearches()
    {
        var (SystemUnderTesting, searchService, geoService, errors) = CreateSystemUnderTesting();
        var results = new[] { CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2) };
        searchService.Setup(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>())).Returns(results);
        geoService.Setup(service => service.GetCitySuggestions("Cl")).Returns(new List<string> { "Cluj", "Cluj-Napoca" });
        var filter = new FilterCriteria
        {
            City = "Cl",
            AvailabilityRange = new TimeRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
        };

        SystemUnderTesting.Initialize(filter);

        Assert.Empty(errors);
        Assert.Equal("Cl", SystemUnderTesting.CitySearchText);
        Assert.NotNull(SystemUnderTesting.SelectedStartDate);
        Assert.NotNull(SystemUnderTesting.SelectedEndDate);
        Assert.Equal(results, SystemUnderTesting.DisplayedResults);
        searchService.Verify(service => service.SearchGamesByFilter(filter), Times.Once);
    }

    [Fact]
    public void CitySearchText_WithTwoCharacters_LoadsSuggestions()
    {
        var (SystemUnderTesting, _, geoService, errors) = CreateSystemUnderTesting();
        geoService.Setup(service => service.GetCitySuggestions("Cl")).Returns(new List<string> { "Cluj", "Cluj-Napoca" });

        SystemUnderTesting.CitySearchText = "Cl";

        Assert.Empty(errors);
        Assert.Equal("Cl", SystemUnderTesting.CurrentFilter.City);
        Assert.Equal(new[] { "Cluj", "Cluj-Napoca" }, SystemUnderTesting.CitySuggestions);
        geoService.Verify(service => service.GetCitySuggestions("Cl"), Times.Once);
    }

    private static (FilteredSearchViewModel SystemUnderTesting, Mock<InterfaceSearchAndFilterService> SearchService, Mock<InterfaceGeographicalService> GeoService, List<string> Errors) CreateSystemUnderTesting()
    {
        var searchService = new Mock<InterfaceSearchAndFilterService>(MockBehavior.Loose);
        searchService.Setup(service => service.IsValidDateRange(It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Returns(true);
        searchService.Setup(service => service.IsValidPlayersCount(It.IsAny<int?>())).Returns(true);
        searchService.Setup(service => service.SearchGamesByFilter(It.IsAny<FilterCriteria>())).Returns(Array.Empty<GameDTO>());
        searchService.Setup(service => service.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>())).Returns(Array.Empty<GameDTO>());
        searchService.Setup(service => service.UpdateFilterFromUI(
                It.IsAny<FilterCriteria>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .Callback<FilterCriteria, double, double, DateTime?, DateTime?>((filter, maxPrice, players, start, end) =>
            {
                filter.MaximumPrice = maxPrice > 0 ? (decimal?)maxPrice : null;
                filter.PlayerCount = players > 0 ? (int?)players : null;
                filter.AvailabilityRange = start.HasValue && end.HasValue ? new TimeRange(start.Value, end.Value) : null;
            });

        var geoService = new Mock<InterfaceGeographicalService>(MockBehavior.Loose);
        geoService.Setup(service => service.GetCitySuggestions(It.IsAny<string>())).Returns(new List<string>());
        geoService.Setup(service => service.GetCityDetails(It.IsAny<string>())).Returns((false, string.Empty, 0, 0));

        var SystemUnderTesting = new FilteredSearchViewModel(searchService.Object, geoService.Object);
        var errors = new List<string>();
        SystemUnderTesting.OnErrorOccurred += errors.Add;

        return (SystemUnderTesting, searchService, geoService, errors);
    }

    private static GameDTO CreateGameDto(int id, string name, decimal price, string city, int maximumPlayers, int minimumPlayers)
    {
        return new GameDTO
        {
            GameId = id,
            Name = name,
            Price = price,
            City = city,
            MaximumPlayerNumber = maximumPlayers,
            MinimumPlayerNumber = minimumPlayers,
        };
    }

    [Fact]
    public void ApplyFilters_WithAvailableGames_ShowsOnlyAvailableGames()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        var baseGames = new[]
        {
        CreateGameDto(1, "Game1", 10m, "Cluj", 4, 2),
        CreateGameDto(2, "Game2", 10m, "Cluj", 4, 2),
    };

        var filtered = new[]
        {
        baseGames[0]
    };

        SystemUnderTesting.LoadDiscoveryResults(baseGames);

        searchService.Setup(searchService => searchService.ApplyFilters(It.IsAny<GameDTO[]>(), It.IsAny<FilterCriteria>()))
            .Returns(filtered);

        SystemUnderTesting.ApplyFilters();

        Assert.Empty(errors);
        Assert.Single(SystemUnderTesting.VisibleGames);
        Assert.Equal(1, SystemUnderTesting.VisibleGames.First().GameId);
    }

    [Fact]
    public void LoadSearchResults_WithDuplicateGameListings_AllowsMultipleListings()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        var results = new[]
        {
        CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2),
        CreateGameDto(2, "Catan", 25m, "Cluj", 4, 2),
    };

        searchService.Setup(searchService => searchService.SearchGamesByFilter(It.IsAny<FilterCriteria>()))
            .Returns(results);

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Empty(errors);
        Assert.Equal(2, SystemUnderTesting.VisibleGames.Count);
    }

    [Fact]
    public void LoadSearchResults_WithManyResults_ShowsSubset()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        var results = Enumerable.Range(1, 50)
            .Select(i => CreateGameDto(i, "Game", 10m, "Cluj", 4, 2))
            .ToArray();

        searchService.Setup(searchService => searchService.SearchGamesByFilter(It.IsAny<FilterCriteria>()))
            .Returns(results);

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Empty(errors);
        Assert.True(SystemUnderTesting.VisibleGames.Count <= results.Length);
    }

    [Fact]
    public void GamesShown_WhenNavigating_ContainsValidGameIds()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        var results = new[]
        {
        CreateGameDto(10, "Catan", 20m, "Cluj", 4, 2),
    };

        searchService.Setup(searchService => searchService.SearchGamesByFilter(It.IsAny<FilterCriteria>()))
            .Returns(results);

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Empty(errors);
        Assert.True(SystemUnderTesting.VisibleGames.First().GameId > 0);
    }

    [Fact]
    public void LoadSearchResults_WhenCalled_DoesNotRequireUserAuthentication()
    {
        var (SystemUnderTesting, searchService, _, errors) = CreateSystemUnderTesting();

        searchService.Setup(searchService => searchService.SearchGamesByFilter(It.IsAny<FilterCriteria>()))
            .Returns(Array.Empty<GameDTO>());

        SystemUnderTesting.LoadSearchResults(new FilterCriteria());

        Assert.Empty(errors);
        Assert.NotNull(SystemUnderTesting.VisibleGames);
    }

    [Fact]
    public void SelectGame_WhenCalled_RaisesNavigationEvent()
    {
        var (SystemUnderTesting, _, _, errors) = CreateSystemUnderTesting();

        int? selectedId = null;
        SystemUnderTesting.OnGameSelectedRequest += id => selectedId = id;

        var game = CreateGameDto(1, "Catan", 20m, "Cluj", 4, 2);

        SystemUnderTesting.VisibleGames.Add(game);

        SystemUnderTesting.SelectGame(game.GameId);

        Assert.Empty(errors);
        Assert.Equal(1, selectedId);
    }

    [Fact]
    public void NoResultsMessage_WhenNoGames_ShowsMessage()
    {
        var (SystemUnderTesting, _, _, _) = CreateSystemUnderTesting();

        SystemUnderTesting.LoadDiscoveryResults(Array.Empty<GameDTO>());

        Assert.True(SystemUnderTesting.HasNoResults);
        Assert.NotEmpty(SystemUnderTesting.NoResultsMessage);
    }
}
