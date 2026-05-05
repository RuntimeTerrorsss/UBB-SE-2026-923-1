namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
    public interface IPaymentRepository
    {
        BookingBoardgamesILoveBan.Src.PaymentCommon.Model.Payment? GetPaymentById(int id);
    }
}
