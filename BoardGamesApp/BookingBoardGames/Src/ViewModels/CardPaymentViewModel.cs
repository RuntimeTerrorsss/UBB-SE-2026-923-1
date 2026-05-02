// <copyright file="CardPaymentViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardGames.Src.Commands;
using BookingBoardGames.Src.Constants;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Src.ViewModels
{
    public class CardPaymentViewModel : INotifyPropertyChanged
    {
        private readonly ICardPaymentService cardPaymentService;
        private readonly IUserRepository userService;
        private readonly System.Timers.Timer inactivityTimer;
        private readonly System.Timers.Timer balanceRefreshTimer;
        private readonly SynchronizationContext? synchronizationContext;

        private decimal balanceAmount;
        private bool areTermsAccepted;
        private bool isCurrentlyLoading;
        private string currentStatusMessage = string.Empty;
        private bool isPaymentSuccessful;
        private string cardNumber = string.Empty;
        private string cardholderName = string.Empty;
        private string expiryDate = string.Empty;
        private string cardVerificationValue = string.Empty;
        private bool isPageCurrentlyActive;

        public CardPaymentViewModel(
            ICardPaymentService cardPaymentService,
            IUserRepository userService,
            int requestId,
            string deliveryAddress,
            int bookingMessageIdentifier,
            IConversationService conversationService)
        {
            this.cardPaymentService = cardPaymentService;
            this.userService = userService;

            this.RequestIdentifier = requestId;
            this.DeliveryAddress = deliveryAddress;
            this.BookingMessageIdentifier = bookingMessageIdentifier;
            RentalDataTransferObject requestDataTransferObject = this.cardPaymentService.GetRequestDataTransferObject(requestId);
            this.ConversationService = conversationService;

            this.ClientIdentifier = requestDataTransferObject.ClientId;
            this.OwnerIdentifier = requestDataTransferObject.OwnerId;
            this.GameName = requestDataTransferObject.GameName;
            this.OwnerName = requestDataTransferObject.OwnerName;
            this.ClientName = requestDataTransferObject.ClientName;
            this.RequestDates = requestDataTransferObject.StartDate.ToShortDateString() + " to " + requestDataTransferObject.EndDate.Date.Date.ToShortDateString();
            this.Price = requestDataTransferObject.Price;
            this.DeliveryDate = requestDataTransferObject.StartDate.ToShortDateString();

            this.FinishPaymentCommand = new RelayCommand(_ => { _ = this.FinishPaymentAsync(); }, () => this.IsPaymentButtonEnabled);
            this.ExitCommand = new RelayCommand(_ => this.NavigateBackwardsAction?.Invoke());
            this.ResetInactivityCommand = new RelayCommand(_ => this.ResetInactivityTimer());

            this.balanceRefreshTimer = new System.Timers.Timer(CardPaymentConstants.TimerForRefreshingBalance);
            this.balanceRefreshTimer.Elapsed += (timerSender, timerEventArguments) => this.RefreshBalance();
            this.balanceRefreshTimer.AutoReset = true;

            this.inactivityTimer = new System.Timers.Timer(CardPaymentConstants.TimerBeforeClosingPayment);
            this.inactivityTimer.Elapsed += this.OnSessionExpired;
            this.inactivityTimer.AutoReset = false;

            this.synchronizationContext = SynchronizationContext.Current;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int RequestIdentifier { get; init; }

        public int ClientIdentifier { get; init; }

        public int OwnerIdentifier { get; init; }

        public string GameName { get; init; }

        public string OwnerName { get; init; }

        public string ClientName { get; init; }

        public string DeliveryAddress { get; init; }

        public string DeliveryDate { get; init; }

        public string RequestDates { get; init; }

        public decimal Price { get; init; }

        public int BookingMessageIdentifier { get; init; }

        public IConversationService ConversationService { get; init; }

        public decimal BalanceAmount
        {
            get => this.balanceAmount;
            set
            {
                if (this.balanceAmount == value)
                {
                    return;
                }

                this.balanceAmount = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.OnPropertyChanged(nameof(this.IsWarningMessageVisible));
            }
        }

        public bool AreTermsAccepted
        {
            get => this.areTermsAccepted;
            set
            {
                if (this.areTermsAccepted == value)
                {
                    return;
                }

                this.areTermsAccepted = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsCurrentlyLoading
        {
            get => this.isCurrentlyLoading;
            set
            {
                if (this.isCurrentlyLoading == value)
                {
                    return;
                }

                this.isCurrentlyLoading = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
            }
        }

        public string CurrentStatusMessage
        {
            get => this.currentStatusMessage;
            set
            {
                this.currentStatusMessage = value ?? string.Empty;
                this.OnPropertyChanged();
            }
        }

        public bool IsPaymentSuccessful
        {
            get => this.isPaymentSuccessful;
            set
            {
                this.isPaymentSuccessful = value;
                this.OnPropertyChanged();
            }
        }

        public string CardNumber
        {
            get => this.cardNumber;
            set
            {
                if (this.cardNumber == value)
                {
                    return;
                }

                this.cardNumber = value ?? string.Empty;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string CardholderName
        {
            get => this.cardholderName;
            set
            {
                if (this.cardholderName == value)
                {
                    return;
                }

                this.cardholderName = value ?? string.Empty;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string ExpiryDate
        {
            get => this.expiryDate;
            set
            {
                if (this.expiryDate == value)
                {
                    return;
                }

                this.expiryDate = value ?? string.Empty;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public string CardVerificationValue
        {
            get => this.cardVerificationValue;
            set
            {
                if (this.cardVerificationValue == value)
                {
                    return;
                }

                this.cardVerificationValue = value ?? string.Empty;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsPaymentButtonEnabled));
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsPaymentButtonEnabled =>
            this.BalanceAmount >= this.Price &&
            this.AreTermsAccepted &&
            !this.IsCurrentlyLoading &&
            !string.IsNullOrWhiteSpace(this.CardNumber) &&
            !string.IsNullOrWhiteSpace(this.CardholderName) &&
            !string.IsNullOrWhiteSpace(this.ExpiryDate) &&
            !string.IsNullOrWhiteSpace(this.CardVerificationValue);

        public bool IsWarningMessageVisible => this.BalanceAmount < this.Price;

        public RelayCommand FinishPaymentCommand { get; }

        public RelayCommand ExitCommand { get; }

        public RelayCommand ResetInactivityCommand { get; }

        public Action? NavigateBackwardsAction { get; set; }

        public Action? NavigateToExitAction { get; set; }

        public void OnPageActivated()
        {
            this.isPageCurrentlyActive = true;
            this.RefreshBalance();
            this.balanceRefreshTimer.Start();
            this.inactivityTimer.Start();
        }

        public void OnPageDeactivated()
        {
            this.balanceRefreshTimer.Stop();
            this.balanceRefreshTimer?.Dispose();
            this.inactivityTimer.Stop();
            this.inactivityTimer?.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshBalance()
        {
            if (!this.isPageCurrentlyActive)
            {
                return;
            }

            decimal newBalance = this.userService.GetUserBalance(this.ClientIdentifier);
            this.synchronizationContext?.Post(
                threadState =>
            {
                this.BalanceAmount = newBalance;
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            },
                null);
        }

        private async Task FinishPaymentAsync()
        {
            this.IsCurrentlyLoading = true;
            this.CurrentStatusMessage = string.Empty;
            this.FinishPaymentCommand.NotifyCanExecuteChanged();

            await Task.Delay(CardPaymentConstants.LoadingTime);

            try
            {
                await Task.Run(() =>
                    this.cardPaymentService.AddCardPayment(this.RequestIdentifier, this.ClientIdentifier, this.OwnerIdentifier, this.Price));

                ((ConversationService)this.ConversationService).OnCardPaymentSelected(this.BookingMessageIdentifier);
                this.RefreshBalance();
                this.IsPaymentSuccessful = true;
                this.CurrentStatusMessage = "Payment successful!";
                this.balanceRefreshTimer.Stop();
                this.inactivityTimer.Stop();
            }
            catch (Exception paymentException)
            {
                this.CurrentStatusMessage = $"Payment failed: {paymentException.Message}";
            }
            finally
            {
                this.IsCurrentlyLoading = false;
                this.FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnSessionExpired(object? timerSender, System.Timers.ElapsedEventArgs elapsedEventArguments)
        {
            if (!this.isPageCurrentlyActive)
            {
                return;
            }

            this.balanceRefreshTimer.Stop();
            this.CurrentStatusMessage = "Session expired due to inactivity.";
            this.synchronizationContext?.Post(
                threadState =>
            {
                if (!this.isPageCurrentlyActive)
                {
                    return;
                }

                this.NavigateToExitAction?.Invoke();
                },
                null);
        }

        private void ResetInactivityTimer()
        {
            this.inactivityTimer.Stop();
            this.inactivityTimer.Start();
        }
    }
}
