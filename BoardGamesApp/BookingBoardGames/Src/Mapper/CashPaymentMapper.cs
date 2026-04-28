using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Models;
namespace BookingBoardGames.Src.Mapper
{
	public class CashPaymentMapper : ICashPaymentMapper
	{
        private const string CashPaymentMethod = "CASH";

		public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto)
		{
            return new Payment
            {
                TransactionIdentifier = paymentDto.Id,
                RequestId = paymentDto.RequestId,
                ClientId = paymentDto.ClientId,
                OwnerId = paymentDto.OwnerId,
                PaidAmount = paymentDto.PaidAmount,
                PaymentMethod = CashPaymentMethod
            };
        }

		public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment)
		{
			return new CashPaymentDataTransferObject(payment.TransactionIdentifier, payment.RequestId, payment.ClientId, payment.OwnerId, payment.PaidAmount);
		}
	}
}
