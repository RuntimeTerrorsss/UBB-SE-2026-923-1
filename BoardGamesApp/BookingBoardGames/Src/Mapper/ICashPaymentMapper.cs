// <copyright file="ICashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Mapper
{
    public interface ICashPaymentMapper
    {
        public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto);

        public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment);
    }
}
