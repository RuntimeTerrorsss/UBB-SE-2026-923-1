using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;


namespace BookingBoardGames.Src.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Payment> GetAllPayments()
        {
            return _context.Payments.ToList();
        }

        public virtual Payment GetPaymentByIdentifier(int paymentId)
        {
            return _context.Payments.FirstOrDefault(p => p.TransactionIdentifier == paymentId);
        }

        public virtual int AddPayment(Payment payment)
        {
            if (payment.DateOfTransaction == default)
            {
                payment.DateOfTransaction = DateTime.Now;
            }

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return payment.TransactionIdentifier;
        }

        public bool DeletePayment(Payment payment)
        {
            var paymentToDelete = _context.Payments.Find(payment.TransactionIdentifier);
            if (paymentToDelete == null) return false;

            _context.Payments.Remove(paymentToDelete);
            return _context.SaveChanges() > 0;
        }

        public virtual Payment UpdatePayment(Payment payment)
        {
            var existingPayment = _context.Payments.Find(payment.TransactionIdentifier);

            if (existingPayment == null) return null;

            var previousPayment = new Payment
            {
                TransactionIdentifier = existingPayment.TransactionIdentifier,
                RequestId = existingPayment.RequestId,
                ClientId = existingPayment.ClientId,
                OwnerId = existingPayment.OwnerId,
                PaidAmount = existingPayment.PaidAmount,
                ReceiptFilePath = existingPayment.ReceiptFilePath,
                DateOfTransaction = existingPayment.DateOfTransaction,
                DateConfirmedBuyer = existingPayment.DateConfirmedBuyer,
                DateConfirmedSeller = existingPayment.DateConfirmedSeller,
                PaymentMethod = existingPayment.PaymentMethod,
                PaymentState = existingPayment.PaymentState
            };

            existingPayment.ReceiptFilePath = payment.ReceiptFilePath ?? string.Empty;
            existingPayment.DateOfTransaction = payment.DateOfTransaction ?? DateTime.Now;
            existingPayment.DateConfirmedBuyer = payment.DateConfirmedBuyer;
            existingPayment.DateConfirmedSeller = payment.DateConfirmedSeller;

            _context.SaveChanges();

            return previousPayment;
        }
    }
}
