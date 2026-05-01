// <copyright file="ReadReceiptDTO.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.DTO
{
    public record ReadReceiptDTO(
        int conversationId,
        int readerId,
        int receiverId,
        DateTime receiptTimeStamp);
}
