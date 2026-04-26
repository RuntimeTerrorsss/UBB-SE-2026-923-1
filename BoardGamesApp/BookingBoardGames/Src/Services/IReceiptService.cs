using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardGames.Src.Services
{
	public interface IReceiptService
	{
		public string GenerateReceiptRelativePath(int rentalId);
		public string GetReceiptDocument(PaymentCommon.Model.Payment payment);
	}
}
