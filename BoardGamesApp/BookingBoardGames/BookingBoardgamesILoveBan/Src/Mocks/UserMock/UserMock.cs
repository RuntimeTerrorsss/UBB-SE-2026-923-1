namespace BookingBoardgamesILoveBan.Src.Mocks.UserMock
{
    public static class UserMock
    {
        public static BookingBoardgamesILoveBan.Src.Mocks.UserMock.User CreateDefaultUser() => new User { Id = 1, DisplayName = "Test User" };
    }

    public class User
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
