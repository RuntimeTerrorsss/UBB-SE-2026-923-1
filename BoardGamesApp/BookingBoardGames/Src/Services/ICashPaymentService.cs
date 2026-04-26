using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Services
{
	public interface ICashPaymentService
	{
		public int AddCashPayment(CashPaymentDataTransferObject paymentDto);
		public CashPaymentDataTransferObject GetCashPayment(int paymentId);
		public void ConfirmDelivery(int paymentId);
		public void ConfirmPayment(int paymentId);
		public bool IsAllConfirmed(int paymentId);
		public bool IsDeliveryConfirmed(int paymentId);
		public bool IsPaymentConfirmed(int paymentId);
	}
}
