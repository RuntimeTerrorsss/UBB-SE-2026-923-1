// <copyright file="DeliveryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardGames.Src.Navigation;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Validators;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class DeliveryView : Page
    {
        private DeliveryViewModel deliveryViewModel;

        private double pendingLatitude;
        private double pendingLongitude;

        private int currentUserId;
        private int requestId;
        private int incomingMessageId;
        private ConversationService conversationService;
        private Window currentWindow;

        public DeliveryView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);

            var arguments = ((int UserId, int RequestId, int MessageId, ConversationService ConversationService, Window ToWindow))navigationEvent.Parameter;
            this.currentUserId = arguments.UserId;
            this.requestId = arguments.RequestId;
            this.incomingMessageId = arguments.MessageId;
            this.conversationService = arguments.ConversationService;
            this.currentWindow = arguments.ToWindow;

            this.deliveryViewModel = new DeliveryViewModel(
                this.currentUserId,
                App.MapService,
                App.UserRepository,
                new AddressValidator());

            this.deliveryViewModel.OnNavigateToPayment = () =>
            {
                var bookingArguments = new BookingNavigationArguments
                {
                    RequestIdentifier = this.requestId,
                    DeliveryAddress = this.deliveryViewModel.CurrentAddress.ToString(),
                    BookingMessageIdentifier = this.incomingMessageId,
                    ConversationService = this.conversationService,
                    CurrentWindow = this.currentWindow,
                };

                // Debug.WriteLine(_conversationService.UserId);
                if (this.CashPaymentRadio.IsChecked == true)
                {
                    this.Frame.Navigate(typeof(CashPaymentPage), bookingArguments);
                }
                else
                {
                    this.Frame.Navigate(typeof(CardPaymentPage), bookingArguments);
                }
            };

            this.deliveryViewModel.StateChanged += this.RefreshUi;
            this.deliveryViewModel.Initialize(this.currentUserId);
            this.RefreshUi();
        }

        private void RefreshUi()
        {
            // Sync all text fields from CurrentAddress (also handles map auto-fill)
            this.CountryInput.Text = this.deliveryViewModel.CurrentAddress.Country;
            this.CityInput.Text = this.deliveryViewModel.CurrentAddress.City;
            this.StreetInput.Text = this.deliveryViewModel.CurrentAddress.Street;
            this.StreetNumberInput.Text = this.deliveryViewModel.CurrentAddress.StreetNumber;

            // Show/hide the map overlay
            this.MapOverlay.Visibility = this.deliveryViewModel.IsMapVisible
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Show or clear validation errors per field
            this.ShowFieldError(this.CountryInput, this.CountryError, "Country");
            this.ShowFieldError(this.CityInput, this.CityError, "City");
            this.ShowFieldError(this.StreetInput, this.StreetError, "Street");
            this.ShowFieldError(this.StreetNumberInput, this.StreetNumberError, "StreetNumber");
        }

        private void ShowFieldError(TextBox input, TextBlock errorBlock, string fieldName)
        {
            if (this.deliveryViewModel.ValidationErrors.TryGetValue(fieldName, out string? message))
            {
                errorBlock.Text = message;
                errorBlock.Visibility = Visibility.Visible;
                VisualStateManager.GoToState(input, "InvalidUnfocused", true);
            }
            else
            {
                errorBlock.Visibility = Visibility.Collapsed;
                VisualStateManager.GoToState(input, "Normal", true);
            }
        }

        private void OnFieldChanged(object sender, TextChangedEventArgs textEventArguments)
        {
            if (sender is TextBox tb && tb.Tag is string fieldName)
            {
                this.deliveryViewModel.OnFieldChange(fieldName, tb.Text);
            }
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs routedEventArguments)
            => this.deliveryViewModel.IsSaveAddress = true;

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs routedEventArguments)
            => this.deliveryViewModel.IsSaveAddress = false;

        private void OnOpenMapClicked(object sender, RoutedEventArgs routedEventArguments)
            => _ = this.InitializeMapAsync();

        private void OnCloseMapClicked(object sender, RoutedEventArgs routedEventArguments)
            => this.deliveryViewModel.CloseMap();

        private void OnSubmitClicked(object sender, RoutedEventArgs routedEventArguments)
            => this.deliveryViewModel.SubmitDelivery();

        private async void OnConfirmLocationClicked(object sender, RoutedEventArgs routedEventArguments)
            => await this.deliveryViewModel.ConfirmMapLocationAsync(this.pendingLatitude, this.pendingLongitude);

        private async Task InitializeMapAsync()
        {
            this.deliveryViewModel.OpenMap();
            await this.MapWebView.EnsureCoreWebView2Async();

            this.MapWebView.CoreWebView2.Settings.UserAgent = "BookingBoardgamesApp/1.0 (Contact: your.email@gmail.com)";
            this.MapWebView.CoreWebView2.WebMessageReceived -= this.OnMapMessageReceived;
            this.MapWebView.CoreWebView2.WebMessageReceived += this.OnMapMessageReceived;

            // VERY
            // VERY
            // IMPORTANT
            // GO to your device settings to Time and Language
            // Select Region
            // Select region format
            // Change to English US
            this.MapWebView.CoreWebView2.NavigateToString("""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="utf-8"/>
                  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
                  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                  <style>html, body, #map { height: 100%; margin: 0; padding: 0; }</style>
                </head>
                <body>
                  <div id="map"></div>
                  <script>
                    var map = L.map('map').setView([46.7712, 23.5897], 13);
                    var marker = null;
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '© OpenStreetMap contributors'
                    }).addTo(map);
                    map.on('click', function(e) {
                        if (marker) marker.setLatLng(e.latlng);
                        else marker = L.marker(e.latlng).addTo(map);
                        window.chrome.webview.postMessage(
                            JSON.stringify({ lat: e.latlng.lat, lng: e.latlng.lng }));
                    });
                  </script>
                </body>
                </html>
                """);
        }

        private void OnMapMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs eventArguments)
        {
            try
            {
                string rawMessage = eventArguments.TryGetWebMessageAsString();

                using JsonDocument jsonDocument = JsonDocument.Parse(rawMessage);

                this.pendingLatitude = jsonDocument.RootElement.GetProperty("lat").GetDouble();
                this.pendingLongitude = jsonDocument.RootElement.GetProperty("lng").GetDouble();

                Debug.WriteLine($"MAP CLICK REGISTERED -> Lat: {this.pendingLatitude}, Lon: {this.pendingLongitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON PARSE ERROR: {ex.Message}");
            }
        }
    }
}
