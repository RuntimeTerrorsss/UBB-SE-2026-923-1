using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.Services
{
    public interface IPaymentService
    {
        public void GenerateReceipt(int paymentId);

        public string GetReceipt(int paymentId);
    }
}
