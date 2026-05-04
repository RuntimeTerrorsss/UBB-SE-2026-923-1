using Xunit;

namespace BookingBoardGames.Tests.Mocks.UserMock
{
    public class UserUnitTests
    {
        [Fact]
        public void User_ShortConstructor_SetsPropertiesCorrectly()
        {
            var user = new User("testuser", "testuser", "testuser@example.com", "hash", "Cluj", "Romania");

            var expectedUser = new { Id = 1, Username = "testuser", Country = "Romania", City = "Cluj", Street = "Street", StreetNumber = "5", DisplayName = "testuser", AvatarUrl = "", Balance = 0m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }

        [Fact]
        public void User_FullConstructor_SetsPropertiesCorrectly()
        {
            var user = new User("fulluser", "Display", "fulluser@example.com", "hash", "Chisinau", "Moldova");

            var expectedUser = new { Id = 2, Username = "fulluser", Country = "Moldova", City = "Chisinau", Street = "Main", StreetNumber = "10", DisplayName = "Display", AvatarUrl = "http://avatar", Balance = 50.5m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }

        [Fact]
        public void User_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var user = new User("old", "old", "old@example.com", "hash", "city", "ro");
            user.Id = 3;
            user.Username = "new";
            user.DisplayName = "newDisplay";
            user.Country = "newCountry";
            user.City = "newCity";
            user.Street = "newStreet";
            user.StreetNumber = "newNumber";
            user.AvatarUrl = "newUrl";
            user.Balance = 99m;

            var expectedUser = new { Id = 3, Username = "new", Country = "newCountry", City = "newCity", Street = "newStreet", StreetNumber = "newNumber", DisplayName = "newDisplay", AvatarUrl = "newUrl", Balance = 99m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }
    }
}





