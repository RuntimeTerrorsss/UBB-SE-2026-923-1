using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices
{
    public interface IMapService
    {
        Task<BookingBoardgamesILoveBan.Src.Delivery.Model.Address> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
