using System;
using BookingBoardGames.Src.Mapper;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Constants;

namespace BookingBoardGames.Src.Services
{
	public class CashPaymentService : PaymentService, ICashPaymentService
	{
        private const string CashPaymentMethod = "CASH";
        private readonly ICashPaymentMapper cashPaymentMapper;

		public CashPaymentService(
            IPaymentRepository paymentRepository,
            ICashPaymentMapper cashPaymentMapper,
            IReceiptService receiptService) : base(paymentRepository, receiptService)
		{
			this.cashPaymentMapper = cashPaymentMapper;
		}

		public int AddCashPayment(CashPaymentDataTransferObject cashPaymentDataTransferObject)
		{
            Payment paymentEntity = cashPaymentMapper.TurnDataTransferObjectIntoEntity(cashPaymentDataTransferObject);
            paymentEntity.PaymentMethod = CashPaymentMethod;
			paymentEntity.PaymentState = PaymentConstrants.StateCompleted;

			int paymentIdentifier = paymentRepository.AddPayment(paymentEntity);

			return paymentIdentifier;
		}

		public CashPaymentDataTransferObject GetCashPayment(int paymentIdentifier)
		{
			return cashPaymentMapper.TurnEntityIntoDataTransferObject(paymentRepository.GetPaymentByIdentifier(paymentIdentifier));
		}

		public void ConfirmDelivery(int paymentIdentifier)
		{
            Payment paymentToConfirm = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);
			paymentToConfirm.DateConfirmedBuyer = DateTime.Now;

			if (IsAllConfirmed(paymentIdentifier))
			{
                paymentToConfirm.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            paymentRepository.UpdatePayment(paymentToConfirm);
		}

		public void ConfirmPayment(int paymentIdentifier)
		{
            Payment paymentToConfirm = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);
			paymentToConfirm.DateConfirmedSeller = DateTime.Now;

			if (IsAllConfirmed(paymentIdentifier))
			{
                paymentToConfirm.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            paymentRepository.UpdatePayment(paymentToConfirm);
		}

		public bool IsAllConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedSeller != null && paymentEntity.DateConfirmedBuyer != null)
			{
				paymentEntity.PaymentState = PaymentConstrants.StateConfirmed;

				return true;
			}

			return false;
		}

		public bool IsDeliveryConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedBuyer != null)
			{
				return true;
			}

			return false;
		}

		public bool IsPaymentConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedSeller != null)
			{
				return true;
			}

			return false;
		}
	}
}
