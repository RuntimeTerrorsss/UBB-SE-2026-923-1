using BookingBoardGames.Src.Repositories;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardGames.Src.Services
{
	public abstract class PaymentService : IPaymentService
	{
		protected readonly IPaymentRepository paymentRepository;
		protected readonly IReceiptService receiptService;

		protected PaymentService(IPaymentRepository paymentRepository, IReceiptService receiptService)
		{
			this.receiptService = receiptService;
			this.paymentRepository = paymentRepository;
		}

		/// <summary>
		/// Set the receipt file path of a payment (when everything is confirmed).
		/// </summary>
		/// <param name="paymentId">of payment to set file path to</param>
		public void GenerateReceipt(int paymentId)
		{
            Payment paymentToUpdate = paymentRepository.GetPaymentByIdentifier(paymentId);

			paymentToUpdate.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentToUpdate.RequestId);

            paymentRepository.UpdatePayment(paymentToUpdate);
		}

		/// <summary>
		/// Get the full path to the saved receipt pdf.
		/// </summary>
		/// <param name="paymentId">of payment to get pdf path</param>
		/// <returns>full path to pdf</returns>
		public string GetReceipt(int paymentId)
		{
            Payment paymentToRead = paymentRepository.GetPaymentByIdentifier(paymentId);

			if (string.IsNullOrEmpty(paymentToRead.ReceiptFilePath))
			{
                GenerateReceipt(paymentId);
                paymentToRead = paymentRepository.GetPaymentByIdentifier(paymentId);
            }

			return receiptService.GetReceiptDocument(paymentToRead);
		}
	}
}
