using System.Collections.Generic;

namespace BookingBoardGames.Src.Repositories
{
	public interface IPaymentRepository
	{
		public IReadOnlyList<Payment> GetAllPayments();
		public Payment GetPaymentByIdentifier(int paymentId);
		public int AddPayment(Payment payment);
		public bool DeletePayment(Payment payment);
		public Payment UpdatePayment(Payment payment);
	}
}
