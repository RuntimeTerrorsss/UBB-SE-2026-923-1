using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Models;
namespace BookingBoardGames.Src.Mapper
{
	public interface ICashPaymentMapper
	{
		public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto);
		public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment);
	}
}
