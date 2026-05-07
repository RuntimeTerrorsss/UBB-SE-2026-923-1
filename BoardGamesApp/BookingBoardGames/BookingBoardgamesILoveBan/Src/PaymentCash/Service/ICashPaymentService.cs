namespace BookingBoardgamesILoveBan.Src.PaymentCash.Service
{
    public interface ICashPaymentService
    {
        int MakePayment(BookingBoardgamesILoveBan.Src.PaymentCash.Model.CashPaymentDataTransferObject dto);
    }
}
