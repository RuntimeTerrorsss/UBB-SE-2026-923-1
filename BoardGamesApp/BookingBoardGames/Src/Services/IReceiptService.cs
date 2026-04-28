
namespace BookingBoardGames.Src.Services
{
	public interface IReceiptService
	{
		public string GenerateReceiptRelativePath(int rentalId);
		public string GetReceiptDocument(Payment payment);
	}
}
