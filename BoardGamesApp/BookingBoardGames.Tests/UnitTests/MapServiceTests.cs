using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardGames.Sharing.Services;
using Moq;
using Moq.Protected;
using Xunit;


namespace BookingBoardGames.Tests.Services
{
    public class MapServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public MapServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        #region Constructor Logic

        [Fact]
        public void MapService_ParameterlessConstructor_InitializesCorrectly()
        {
            // Act
            // This executes the parameterless constructor, which delegates to the 
            // parameterized constructor using: : this(new HttpClient())
            var service = new MapService();

            // Assert
            // The constructor should execute and instantiate without throwing any exceptions
            Assert.NotNull(service);
        }

        [Fact]
        public void MapService_ConstructorWithClient_SetsUserAgentHeader()
        {
            // Arrange & Act
            var service = new MapService(_httpClient);

            // Assert
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("User-Agent"));
            var userAgent = _httpClient.DefaultRequestHeaders.UserAgent.ToString();
            Assert.Equal("BookingBoardgames/1.0", userAgent);
        }

        #endregion

        #region GetAddressFromMapAsync - Edge Cases & Failures

        [Fact]
        public async Task GetAddressFromMapAsync_DefaultCoordinates_ReturnsNull()
        {
            // Arrange
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(0.0, 0.0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_HttpErrorResponse_ReturnsNull()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.77, 23.62);

            // Assert
            Assert.Null(result); // The try-catch block returns null on exception
        }

        [Fact]
        public async Task GetAddressFromMapAsync_InvalidJsonResponse_ReturnsNull()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ invalid json }")
                });

            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.77, 23.62);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAddressFromMapAsync - Address Type Selection Hierarchy

        [Fact]
        public async Task GetAddressFromMapAsync_HasCity_SelectsCity()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""city"": ""Cluj-Napoca"",
                    ""town"": ""ClujTown"",
                    ""village"": ""ClujVillage"",
                    ""road"": ""Strada Universitatii"",
                    ""house_number"": ""7""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.77, 23.62);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Romania", result.Country);
            Assert.Equal("Cluj-Napoca", result.City); // Hits first hierarchy level
            Assert.Equal("Strada Universitatii", result.Street);
            Assert.Equal("7", result.StreetNumber);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityHasTown_SelectsTown()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""town"": ""Floresti"",
                    ""village"": ""FlorestiVillage"",
                    ""road"": ""Strada Avram Iancu""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.72, 23.52);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Floresti", result.City); // Skips city -> hits town hierarchy level
            Assert.Equal(string.Empty, result.StreetNumber); // Verifies house_number fallback
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityNoTownHasVillage_SelectsVillage()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""village"": ""Chinteni""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.85, 23.53);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Chinteni", result.City); // Skips city and town -> hits village hierarchy level
            Assert.Equal(string.Empty, result.Street);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityNoTownNoVillage_ReturnsEmptyStringForCity()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.00, 23.00);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.City); // Skips all fallback loops -> falls back to string.Empty
        }

        [Fact]
        public async Task GetAddressFromMapAsync_PropertiesAreNullInJson_ReturnsEmptyStrings()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": null,
                    ""city"": null,
                    ""road"": null,
                    ""house_number"": null
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);

            // Act
            var result = await service.GetAddressFromMapAsync(46.00, 23.00);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Country);      // Verifies GetString() ?? string.Empty block
            Assert.Equal(string.Empty, result.City);         // Verifies GetString() ?? string.Empty block for City
            Assert.Equal(string.Empty, result.Street);       // Verifies GetString() ?? string.Empty block for Street
            Assert.Equal(string.Empty, result.StreetNumber); // Verifies GetString() ?? string.Empty block for House Number
        }

        #endregion

        #region Helper Methods

        private void SetupMockHttpMessageHandler(string jsonResponse)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });
        }

        #endregion
    }
}