// <copyright file="PaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using BookingBoardGames.Src.Repositories;

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
            Payment paymentToUpdate = this.paymentRepository.GetPaymentByIdentifier(paymentId);

            paymentToUpdate.ReceiptFilePath = this.receiptService.GenerateReceiptRelativePath(paymentToUpdate.RequestId);

            this.paymentRepository.UpdatePayment(paymentToUpdate);
        }

        /// <summary>
        /// Get the full path to the saved receipt pdf.
        /// </summary>
        /// <param name="paymentId">of payment to get pdf path</param>
        /// <returns>full path to pdf</returns>
        public async Task<string> GetReceipt(int paymentId)
        {
            Payment paymentToRead = this.paymentRepository.GetPaymentByIdentifier(paymentId);

            if (string.IsNullOrEmpty(paymentToRead.ReceiptFilePath))
            {
                this.GenerateReceipt(paymentId);
                paymentToRead = this.paymentRepository.GetPaymentByIdentifier(paymentId);
            }

            return await this.receiptService.GetReceiptDocument(paymentToRead);
        }
    }
}
