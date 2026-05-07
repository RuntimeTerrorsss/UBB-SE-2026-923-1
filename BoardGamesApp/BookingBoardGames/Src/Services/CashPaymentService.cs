// <copyright file="CashPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Mapper;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Data.Services
{
    public class CashPaymentService : PaymentService, ICashPaymentService
    {
        private const string CashPaymentMethod = "CASH";
        private readonly ICashPaymentMapper cashPaymentMapper;

        public CashPaymentService(
            IPaymentRepository paymentRepository,
            ICashPaymentMapper cashPaymentMapper,
            IReceiptService receiptService)
            : base(paymentRepository, receiptService)
        {
            this.cashPaymentMapper = cashPaymentMapper;
        }

        public async Task<int> AddCashPaymentAsync(CashPaymentDataTransferObject cashPaymentDataTransferObject)
        {
            Payment paymentEntity = this.cashPaymentMapper.TurnDataTransferObjectIntoEntity(cashPaymentDataTransferObject);
            paymentEntity.PaymentMethod = CashPaymentMethod;
            paymentEntity.PaymentState = PaymentConstrants.StateCompleted;

            int paymentIdentifier = await this.paymentRepository.AddPaymentAsync(paymentEntity);

            return paymentIdentifier;
        }

        public async Task<CashPaymentDataTransferObject> GetCashPaymentAsync(int paymentIdentifier)
        {
            var payment = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            return this.cashPaymentMapper.TurnEntityIntoDataTransferObject(payment);
        }

        public async Task ConfirmDeliveryAsync(int paymentIdentifier)
        {
            Payment paymentToConfirm = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            paymentToConfirm.DateConfirmedBuyer = DateTime.Now;

            if (await this.IsAllConfirmedAsync(paymentIdentifier))
            {
                paymentToConfirm.ReceiptFilePath = this.receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            await this.paymentRepository.UpdatePaymentAsync(paymentToConfirm);
        }

        public async Task ConfirmPaymentAsync(int paymentIdentifier)
        {
            Payment paymentToConfirm = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            paymentToConfirm.DateConfirmedSeller = DateTime.Now;

            if (await this.IsAllConfirmedAsync(paymentIdentifier))
            {
                paymentToConfirm.ReceiptFilePath = this.receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            await this.paymentRepository.UpdatePaymentAsync(paymentToConfirm);
        }

        public async Task<bool> IsAllConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedSeller != null && paymentEntity.DateConfirmedBuyer != null)
            {
                paymentEntity.PaymentState = PaymentConstrants.StateConfirmed;

                return true;
            }

            return false;
        }

        public async Task<bool> IsDeliveryConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedBuyer != null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPaymentConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedSeller != null)
            {
                return true;
            }

            return false;
        }
    }
}
