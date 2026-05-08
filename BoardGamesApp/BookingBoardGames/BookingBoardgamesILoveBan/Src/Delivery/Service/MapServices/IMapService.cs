// <copyright file="IMapService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices
{
    public interface IMapService
    {
        Task<BookingBoardgamesILoveBan.Src.Delivery.Model.Address> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
