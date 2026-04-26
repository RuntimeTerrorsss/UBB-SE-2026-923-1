namespace BookingBoardGames.Src.DTO
{
	public class CashPaymentDataTransferObject
	{
		public int Id { get; set; }
		public int RequestId { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal PaidAmount { get; set; }

		public CashPaymentDataTransferObject(int paymentId, int requestId, int clientId, int ownerId, decimal amount)
		{
            Id = paymentId;
            RequestId = requestId;
            ClientId = clientId;
            OwnerId = ownerId;
            PaidAmount = amount;
		}
	}
}
