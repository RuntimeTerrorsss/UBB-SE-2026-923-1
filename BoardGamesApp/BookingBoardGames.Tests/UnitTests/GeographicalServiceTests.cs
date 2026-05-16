using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.Services;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class GeographicalServiceTests : IDisposable
    {
        private readonly string _testAssetsFolder;
        private readonly string _testFilePath;

        public GeographicalServiceTests()
        {
            _testAssetsFolder = Path.Combine(AppContext.BaseDirectory, "Assets");
            _testFilePath = Path.Combine(_testAssetsFolder, "RO.txt");
        }

        public void Dispose()
        {
            CleanUpTestFile();
        }

        private void SetupTestFile(string[] fileLines)
        {
            Directory.CreateDirectory(_testAssetsFolder);
            File.WriteAllLines(_testFilePath, fileLines);
        }

        private void CleanUpTestFile()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
            if (Directory.Exists(_testAssetsFolder))
            {
                Directory.Delete(_testAssetsFolder);
            }
        }

        #region LoadFromFileAsync & LoadCitiesFromFileAsync

        [Fact]
        public async Task LoadFromFileAsync_FileDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            CleanUpTestFile();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => GeographicalService.LoadFromFileAsync());
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_LineHasInsufficientColumns_LineIsSkipped()
        {
            // Arrange
            var lines = new[] { "Column1\tColumn2" };
            SetupTestFile(lines);

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            var details = service.GetCityDetails("Column2");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidFeatureClass_LineIsSkipped()
        {
            // Arrange
            // Column Index 6 is FeatureClass ('X' is invalid, expects 'P' or 'PPLC')
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tX\t\t\t\t\t\t\t\t100000";
            SetupTestFile(new[] { line });

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_PopulationBelowMinimum_LineIsSkipped()
        {
            // Arrange
            // Column Index 14 is Population (4999 is below 5000 threshold)
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t4999";
            SetupTestFile(new[] { line });

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidLatitude_LineIsSkipped()
        {
            // Arrange
            // Column Index 4 is Latitude ("INVALID")
            var line = "Id\tCluj\tCluj\t\tINVALID\t23.62\tP\t\t\t\t\t\t\t\t6000";
            SetupTestFile(new[] { line });

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidLongitude_LineIsSkipped()
        {
            // Arrange
            // Column Index 5 is Longitude ("INVALID")
            var line = "Id\tCluj\tCluj\t\t46.77\tINVALID\tP\t\t\t\t\t\t\t\t6000";
            SetupTestFile(new[] { line });

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_ValidBucuresti_AppendsSpecialBucharestAliases()
        {
            // Arrange
            // Feature class PPLC, Name: Bucuresti, Pop: 2000000, Alternates: "Buc,B-est"
            var line = "Id\tBucuresti\tBucuresti\tBuc,B-est\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            SetupTestFile(new[] { line });

            var service = new GeographicalService();

            // Act
            await service.LoadCitiesFromFileAsync();

            // Assert
            Assert.True(service.GetCityDetails("Bucuresti").IsFound);
            Assert.True(service.GetCityDetails("Bucharest").IsFound); // Special branch alias
            Assert.True(service.GetCityDetails("București").IsFound); // Special branch alias
            Assert.True(service.GetCityDetails("Buc").IsFound);       // Alternate split branch
            Assert.True(service.GetCityDetails("B est").IsFound);     // Normalized alternate check
        }

        #endregion

        #region GetCityDetails

        [Fact]
        public async Task GetCityDetails_CityNotFound_ReturnsFalseAndDefaultCoordinates()
        {
            // Arrange
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetCityDetails("NonExistentCity");

            // Assert
            Assert.False(result.IsFound);
            Assert.Equal("", result.CityName);
            Assert.Equal(0, result.Latitude);
            Assert.Equal(0, result.Longitude);
        }

        [Fact]
        public async Task GetCityDetails_CityFoundWithNormalization_ReturnsTrueAndCorrectDetails()
        {
            // Arrange
            var line = "Id\tSânnicolau-Mare\tSannicolau-Mare\t\t46.07\t20.62\tP\t\t\t\t\t\t\t\t12000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act & Assert
            // Testing normalization branch logic (Dashes to spaces, diacritics replacement)
            var result = service.GetCityDetails("sânnicolau mare");

            Assert.True(result.IsFound);
            Assert.Equal("Sânnicolau-Mare", result.CityName);
            Assert.Equal(46.07, result.Latitude);
            Assert.Equal(20.62, result.Longitude);
        }

        #endregion

        #region GetDistanceBetweenCities

        [Fact]
        public async Task GetDistanceBetweenCities_OriginNotFound_ReturnsNull()
        {
            // Arrange
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetDistanceBetweenCities("MissingCity", "Cluj");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDistanceBetweenCities_DestinationNotFound_ReturnsNull()
        {
            // Arrange
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetDistanceBetweenCities("Cluj", "MissingCity");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDistanceBetweenCities_BothCitiesExist_ReturnsCalculatedDistance()
        {
            // Arrange
            var line1 = "Id1\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            var line2 = "Id2\tOradea\tOradea\t\t47.04\t21.91\tP\t\t\t\t\t\t\t\t200000";
            SetupTestFile(new[] { line1, line2 });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetDistanceBetweenCities("Cluj", "Oradea");

            // Assert
            Assert.NotNull(result);
            Assert.True(result > 0);
        }

        #endregion

        #region GetCitySuggestions

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCitySuggestions_NullOrWhiteSpaceInput_ReturnsEmptyList(string input)
        {
            // Arrange
            var service = new GeographicalService();

            // Act
            var result = service.GetCitySuggestions(input);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCitySuggestions_MatchesFound_ReturnsNormalizedSuggestionsUpToMaximumLimit()
        {
            // Arrange
            // We input 12 distinct matched variations to verify the Take(10) capping branch works.
            var lines = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                lines.Add($"Id{i}\tTestCity{i}\tTestCity{i}\t\t46.0\t23.0\tP\t\t\t\t\t\t\t\t6000");
            }
            SetupTestFile(lines.ToArray());
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetCitySuggestions("Test");

            // Assert
            Assert.Equal(10, result.Count); // Capped by MaximumCitySuggestions = 10
            Assert.Contains("TestCity1", result);
        }

        [Fact]
        public async Task GetCitySuggestions_BucharestAliasInput_NormalizesSuggestionsToBucuresti()
        {
            // Arrange
            var line = "Id\tBucharest\tBucharest\t\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetCitySuggestions("Bucha");

            // Assert
            Assert.Single(result);
            Assert.Equal("București", result[0]); // Hits NormalizeSuggestion branch
        }

        [Fact]
        public async Task GetCitySuggestions_BucurestiNameInput_NormalizesSuggestionsToBucurestiWithDiacritics()
        {
            // Arrange
            var line = "Id\tBucuresti\tBucuresti\t\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            // Act
            var result = service.GetCitySuggestions("Bucur");

            // Assert
            Assert.Single(result);
            Assert.Equal("București", result[0]); // Hits string.Equals("Bucuresti") branch
        }

        #endregion

        #region Normalization and Aliasing

        [Fact]
        public async Task LoadCitiesFromFileAsync_AlternateNamesHasEmptyElements_TriggersAddCityAliasEarlyReturn()
        {
            // Arrange
            // Notice the trailing comma and spaces in the Alternate Names column (Index 3): "Cluj, , "
            // The Split(',') will produce an empty string element, forcing AddCityAlias to hit its early return branch.
            var line = "Id\tCluj\tCluj\tCluj, , \t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            SetupTestFile(new[] { line });
            var service = new GeographicalService();

            // Act
            var exception = await Record.ExceptionAsync(() => service.LoadCitiesFromFileAsync());

            // Assert
            Assert.Null(exception); // Confirms the early return safely bypassed processing without errors
            Assert.True(service.GetCityDetails("Cluj").IsFound);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCityDetails_NullOrWhiteSpaceInput_TriggersNormalizeCityNameEarlyReturn(string invalidCityName)
        {
            // Arrange
            var service = new GeographicalService();

            // Act
            // Passing a null or whitespace string directly into a public method that calls NormalizeCityName
            var result = service.GetCityDetails(invalidCityName);

            // Assert
            Assert.False(result.IsFound);
            Assert.Equal(string.Empty, result.CityName);
        }

        #endregion
    }
}