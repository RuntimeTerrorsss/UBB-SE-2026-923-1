// <copyright file="CashPaymentPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.Navigation;
using BookingBoardGames.Src.ViewModels;
using BookingBoardGames.Src.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class CashPaymentPage : Page
    {
        public CashPaymentViewModel PaymentViewModel { get; set; }

        private Window currentApplicationWindow;

        public CashPaymentPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is BookingNavigationArguments booking)
            {
                this.PaymentViewModel = new CashPaymentViewModel(
                    App.CashPaymentService,
                    App.UserRepository,
                    App.RentalService,
                    App.GameRepository,
                    booking.RequestIdentifier,
                    booking.DeliveryAddress,
                    booking.BookingMessageIdentifier,
                    booking.ConversationService);

                this.DataContext = this.PaymentViewModel;
                this.currentApplicationWindow = booking.CurrentWindow;
            }
        }

        private void NavigateToChatButton_Click(object sender, RoutedEventArgs e)
        {
            this.currentApplicationWindow.Close();
            /*
            if (Frame.CanGoBack)
            {
                Frame.Navigate(typeof(ChatPageView), App.CURRENT_USER_WILL_DELETE);
            }*/
        }
    }
}
