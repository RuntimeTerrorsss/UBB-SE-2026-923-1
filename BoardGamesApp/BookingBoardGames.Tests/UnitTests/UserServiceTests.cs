using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookingBoardGames.Sharing.Services.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User();
            _userRepositoryMock.Setup(repo => repo.GetById(1)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.Equal(expectedUser, result);
            _userRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenCalled_ReturnsUserList()
        {
            // Arrange
            var expectedUsers = new List<User> { new User(), new User() };
            _userRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.Equal(expectedUsers, result);
            _userRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserIsSuspended_ReturnsNull()
        {
            // Arrange
            var suspendedUser = new User { IsSuspended = true };
            _userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
                .ReturnsAsync(suspendedUser);

            // Act
            var result = await _userService.LoginAsync("testuser", "password123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentialsAndNotSuspended_ReturnsUser()
        {
            // Arrange
            var validUser = new User { IsSuspended = false };
            _userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
                .ReturnsAsync(validUser);

            // Act
            var result = await _userService.LoginAsync("testuser", "password123");

            // Assert
            Assert.Equal(validUser, result);
        }

        [Theory]
        [InlineData("", "Display", "Email", "Hash", "City", "Country")]
        [InlineData("User", "", "Email", "Hash", "City", "Country")]
        [InlineData("User", "Display", "", "Hash", "City", "Country")]
        [InlineData("User", "Display", "Email", "", "City", "Country")]
        [InlineData("User", "Display", "Email", "Hash", "", "Country")]
        [InlineData("User", "Display", "Email", "Hash", "City", "")]
        [InlineData(null, "Display", "Email", "Hash", "City", "Country")]
        public async Task RegisterUserAsync_MissingRequiredFields_ReturnsFalse(
            string username, string displayName, string email, string passwordHash, string city, string country)
        {
            // Arrange
            var invalidUser = new User
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                PasswordHash = passwordHash,
                City = city,
                Country = country
            };

            // Act
            var result = await _userService.RegisterUserAsync(invalidUser);

            // Assert
            Assert.False(result);
            _userRepositoryMock.Verify(repo => repo.Register(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidUser_ReturnsRepositoryResult()
        {
            // Arrange
            var validUser = new User
            {
                Username = "User",
                DisplayName = "Display",
                Email = "Email@test.com",
                PasswordHash = "Hash",
                City = "City",
                Country = "Country"
            };

            _userRepositoryMock.Setup(repo => repo.Register(validUser)).ReturnsAsync(true);

            // Act
            var result = await _userService.RegisterUserAsync(validUser);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.Register(validUser), Times.Once);
        }

        [Fact]
        public async Task GetBalanceAsync_ValidUserId_ReturnsBalance()
        {
            // Arrange
            decimal expectedBalance = 150.50m;
            _userRepositoryMock.Setup(repo => repo.GetUserBalance(1)).ReturnsAsync(expectedBalance);

            // Act
            var result = await _userService.GetBalanceAsync(1);

            // Assert
            Assert.Equal(expectedBalance, result);
            _userRepositoryMock.Verify(repo => repo.GetUserBalance(1), Times.Once);
        }

        [Fact]
        public async Task UpdateBalanceAsync_ValidInputs_CallsRepository()
        {
            // Arrange
            int userId = 1;
            decimal amount = 50.00m;
            _userRepositoryMock.Setup(repo => repo.UpdateBalance(userId, amount)).Returns(Task.CompletedTask);

            // Act
            await _userService.UpdateBalanceAsync(userId, amount);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateBalance(userId, amount), Times.Once);
        }
    }
}