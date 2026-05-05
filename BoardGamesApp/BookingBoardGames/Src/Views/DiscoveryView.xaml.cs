// <copyright file="DiscoveryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Src.Views
{
    /// <summary>
    /// Provides the main discovery interface for browsing and filtering available games.
    /// </summary>
    public sealed partial class DiscoveryView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryView"/> class.
        /// </summary>
        public DiscoveryView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with the discovery logic.
        /// </summary>
        public DiscoveryViewModel ViewModel { get; private set; } = null!;

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="e">Event data that can be examined by overriding code.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel = new DiscoveryViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);

            this.ViewModel.OnSearchRequest += this.HandleSearchRequest;
            this.ViewModel.OnGameSelectedRequest += gameId =>
            {
                this.Frame.Navigate(typeof(GameDetailsView), gameId);
            };

            this.ViewModel.OnPageChanged += () =>
            {
                this.MainScrollViewer.ScrollToVerticalOffset(0);
            };

            this.DataContext = this.ViewModel;
            this.StartDatePicker.Date = null;
            this.EndDatePicker.Date = null;
        }

        private void HandleSearchRequest(FilterCriteria filter)
        {
            this.Frame.Navigate(typeof(FilteredSearchView), filter);
        }

        private void Game_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GameDTO game)
            {
                this.Frame.Navigate(typeof(GameDetailsView), game.GameId);
            }
        }

        private void EndDatePicker_DayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            if (this.ViewModel?.SelectedStartDate.HasValue == true)
            {
                var date = args.Item.Date.Date;
                var selectedStartDate = this.ViewModel.SelectedStartDate.Value.Date;

                if (date < selectedStartDate)
                {
                    args.Item.IsBlackout = true;
                }
            }
        }
        private void DashboardButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DashboardView));
        }

        private void ChatButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            int currentUserId = ((App)Microsoft.UI.Xaml.Application.Current).DashboardUser;
            this.Frame.Navigate(typeof(ChatViews.ChatPageView), currentUserId);
        }
    }
}
