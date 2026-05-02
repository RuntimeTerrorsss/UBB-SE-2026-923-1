// <copyright file="DeliveryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Validators;

namespace BookingBoardGames.Src.ViewModels
{
    public class DeliveryViewModel
    {
        private const int DefaultUserId = 1;

        public DeliveryViewModel(
            int currentUserId,
            IMapService mapService,
            IUserRepository userRepository,
            IValidator<Dictionary<string, string>, Address> validator)
        {
            this.CurrentId = currentUserId;
            this.MapService = mapService;
            this.UserRepository = userRepository;
            this.Validator = validator;
            this.CurrentUser = this.UserRepository.GetById(currentUserId);
            this.CurrentAddress = this.CurrentUser != null
                ? new Address(this.CurrentUser.Country, this.CurrentUser.City, this.CurrentUser.Street, this.CurrentUser.StreetNumber)
                : new Address();
        }

        public event Action StateChanged;

        public Address CurrentAddress { get; set; }

        public bool IsMapVisible { get; set; } = false;

        public bool IsSaveAddress { get; set; } = false;

        public Dictionary<string, string> ValidationErrors { get; set; } = new Dictionary<string, string>();

        public User CurrentUser { get; set; }

        public int CurrentId { get; set; } = DefaultUserId;

        public Action OnNavigateToPayment { get; set; }

        private IMapService MapService { get; set; }

        private IUserRepository UserRepository { get; set; }

        private IValidator<Dictionary<string, string>, Address> Validator { get; set; }

        public void Initialize(int userId)
        {
            this.CurrentId = userId;
            this.CurrentUser = this.UserRepository.GetById(userId);

            if (this.CurrentUser != null)
            {
                this.CurrentAddress = new Address(
                    this.CurrentUser.Country,
                    this.CurrentUser.City,
                    this.CurrentUser.Street,
                    this.CurrentUser.StreetNumber);
            }
        }

        public void OnFieldChange(string fieldName, string newValue)
        {
            typeof(Address).GetProperty(fieldName)?.SetValue(this.CurrentAddress, newValue);

            if (this.ValidationErrors.Remove(fieldName))
            {
                this.StateChanged?.Invoke();
            }
        }

        public void OpenMap()
        {
            this.IsMapVisible = true;
            this.StateChanged?.Invoke();
        }

        public void CloseMap()
        {
            this.IsMapVisible = false;
            this.StateChanged?.Invoke();
        }

        public async Task ConfirmMapLocationAsync(double latitude, double longitude)
        {
            Debug.WriteLine($"--- CONFIRM LOCATION CLICKED --- Lat: {latitude}, Lon: {longitude}");
            Address resolved = await this.MapService.GetAddressFromMapAsync(latitude, longitude);

            if (resolved != null)
            {
                this.CurrentAddress = resolved;
                this.IsMapVisible = false;
                this.StateChanged?.Invoke();
            }
            else
            {
                Debug.WriteLine($"Address not valid, received: Lat={latitude}, Lon={longitude}");
            }
        }

        public void SubmitDelivery()
        {
            this.ValidationErrors = this.Validator.Validate(this.CurrentAddress);
            this.StateChanged?.Invoke();

            if (this.ValidationErrors.Count == 0)
            {
                if (this.IsSaveAddress && this.CurrentUser is not null)
                {
                    this.UserRepository.SaveAddress(this.CurrentUser.Id, this.CurrentAddress);
                }

                this.OnNavigateToPayment?.Invoke();
            }
        }
    }
}
