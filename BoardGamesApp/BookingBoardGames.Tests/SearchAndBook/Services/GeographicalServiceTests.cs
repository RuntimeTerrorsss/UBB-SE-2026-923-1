using SearchAndBook.Domain;
using SearchAndBook.Services;
using System.Reflection;

namespace BookingBoardGames.Tests.SearchAndBook.Services;

public class GeographicalServiceTests
{

    [Fact]
    public void GetCityDetails_CityExists_ReturnsTrueWithCorrectCoordinates()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetCityDetails("Cluj-Napoca");

        Assert.True(result.isFound);
        Assert.Equal("Cluj-Napoca", result.cityName);
        Assert.Equal(46.7712, result.latitude);
        Assert.Equal(23.6236, result.longitude);
    }

    [Fact]
    public void GetCityDetails_CityDoesNotExist_ReturnsFalseWithEmptyValues()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities();

        var result = SystemUnderTesting.GetCityDetails("NonExistentCity");

        Assert.False(result.isFound);
        Assert.Equal("", result.cityName);
        Assert.Equal(0, result.latitude);
        Assert.Equal(0, result.longitude);
    }

    [Fact]
    public void GetCityDetails_CityNameWithDiacritics_NormalizesAndFindsCity()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Timișoara", 45.7489, 21.2087, normalizedKey: "timisoara"));

        var result = SystemUnderTesting.GetCityDetails("Timișoara");

        Assert.True(result.isFound);
        Assert.Equal("Timișoara", result.cityName);
    }

    [Fact]
    public void GetCityDetails_CityNameWithHyphen_NormalizesHyphenToSpaceAndFindsCity()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetCityDetails("Cluj-Napoca");

        Assert.True(result.isFound);
        Assert.Equal("Cluj-Napoca", result.cityName);
    }

    [Fact]
    public void GetCityDetails_CityNameWithMixedCase_NormalizesAndFindsCity()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Brașov", 45.6427, 25.5887, normalizedKey: "brasov"));

        var result = SystemUnderTesting.GetCityDetails("BRASOV");

        Assert.True(result.isFound);
        Assert.Equal("Brașov", result.cityName);
    }

    [Fact]
    public void GetCityDetails_CityNameWithLeadingAndTrailingWhitespace_NormalizesAndFindsCity()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Iași", 47.1585, 27.6014, normalizedKey: "iasi"));

        var result = SystemUnderTesting.GetCityDetails("  Iași  ");

        Assert.True(result.isFound);
        Assert.Equal("Iași", result.cityName);
    }

    [Fact]
    public void GetDistanceBetweenCities_BothCitiesExist_ReturnsPositiveDistance()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"),
            CreateCity("București", 44.4268, 26.1025, normalizedKey: "bucuresti"));

        var result = SystemUnderTesting.GetDistanceBetweenCities("Cluj-Napoca", "București");

        Assert.NotNull(result);
        Assert.True(result > 0);
    }

    [Fact]
    public void GetDistanceBetweenCities_SameCity_ReturnsZeroOrNearZero()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetDistanceBetweenCities("Cluj-Napoca", "Cluj-Napoca");

        Assert.NotNull(result);
        Assert.Equal(0, result!.Value, precision: 5);
    }

    [Fact]
    public void GetDistanceBetweenCities_OriginCityNotFound_ReturnsNull()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("București", 44.4268, 26.1025, normalizedKey: "bucuresti"));

        var result = SystemUnderTesting.GetDistanceBetweenCities("UnknownCity", "București");

        Assert.Null(result);
    }

    [Fact]
    public void GetDistanceBetweenCities_DestinationCityNotFound_ReturnsNull()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetDistanceBetweenCities("Cluj-Napoca", "UnknownCity");

        Assert.Null(result);
    }

    [Fact]
    public void GetDistanceBetweenCities_BothCitiesNotFound_ReturnsNull()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities();

        var result = SystemUnderTesting.GetDistanceBetweenCities("UnknownA", "UnknownB");

        Assert.Null(result);
    }


    [Fact]
    public void GetCitySuggestions_EmptyString_ReturnsEmptyList()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetCitySuggestions("");

        Assert.Empty(result);
    }

    [Fact]
    public void GetCitySuggestions_WhitespaceOnly_ReturnsEmptyList()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetCitySuggestions("   ");

        Assert.Empty(result);
    }

    [Fact]
    public void GetCitySuggestions_MatchingPartialName_ReturnsMatchingCityNames()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"),
            CreateCity("Cluj-Mănăștur", 46.7600, 23.5700, normalizedKey: "cluj manastur"));

        var result = SystemUnderTesting.GetCitySuggestions("cluj");

        Assert.Equal(2, result.Count);
        Assert.Contains("Cluj-Napoca", result);
        Assert.Contains("Cluj-Mănăștur", result);
    }

    [Fact]
    public void GetCitySuggestions_NoMatchingCities_ReturnsEmptyList()
    {
        var SystemUnderTesting = CreateSystemUnderTestingWithCities(
            CreateCity("Cluj-Napoca", 46.7712, 23.6236, normalizedKey: "cluj napoca"));

        var result = SystemUnderTesting.GetCitySuggestions("xyz");

        Assert.Empty(result);
    }

    [Fact]
    public void GetCitySuggestions_MoreThanTenMatches_ReturnsOnlyTen()
    {
        var cities = Enumerable.Range(1, 15)
            .Select(i => CreateCity($"City{i}", 0, 0, normalizedKey: $"city{i}"))
            .ToArray();

        var SystemUnderTesting = CreateSystemUnderTestingWithCities(cities);

        var result = SystemUnderTesting.GetCitySuggestions("city");

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public void GetCitySuggestions_MultipleLookupKeysPointToSameCity_ReturnsCityOnlyOnce()
    {
        var city = CreateCity("Cluj-Napoca", 46.7712, 23.6236);
        var SystemUnderTesting = CreateSystemUnderTestingWithCities();

        InjectCities(SystemUnderTesting, new Dictionary<string, City>
        {
            ["cluj napoca"] = city,
            ["klausenburg"]  = city
        });

        var result = SystemUnderTesting.GetCitySuggestions("u"); 

        Assert.Single(result);
        Assert.Equal("Cluj-Napoca", result[0]);
    }
    private static GeographicalService CreateSystemUnderTestingWithCities(params City[] cities)
    {
        var SystemUnderTesting = new GeographicalService();

        var lookup = new Dictionary<string, City>();
        foreach (var city in cities)
        {
            var key = city.Names.FirstOrDefault() ?? city.MainName.ToLower();
            lookup[key] = city;
        }

        InjectCities(SystemUnderTesting, lookup);
        return SystemUnderTesting;
    }

    private static void InjectCities(GeographicalService service, Dictionary<string, City> lookup)
    {
        var field = typeof(GeographicalService)
            .GetField("_cityLookupByNormalizedName", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var existing = (Dictionary<string, City>)field.GetValue(service)!;
        foreach (var kvp in lookup)
            existing[kvp.Key] = kvp.Value;
    }

    private static City CreateCity(
        string mainName,
        double latitude,
        double longitude,
        string? normalizedKey = null)
    {
        var key = normalizedKey ?? mainName.ToLower();
        return new City
        {
            MainName = mainName,
            Latitude = latitude,
            Longitude = longitude,
            Names = new List<string> { key }
        };
    }
}
