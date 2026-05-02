// <copyright file="FilteredSearchView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.Shared;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Src.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilteredSearchView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredSearchView"/> class.
        /// </summary>
        public FilteredSearchView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="e">Event data that can be examined by overriding code.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var criteria = e.Parameter as FilterCriteria ?? new FilterCriteria();
            var viewModel = new FilteredSearchViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);
            viewModel.OnGameSelectedRequest += gameId =>
            {
                this.Frame.Navigate(typeof(GameDetailsView), gameId);
            };
            viewModel.OnGoBackRequest += () => this.Frame.Navigate(typeof(DiscoveryView));
            viewModel.Initialize(criteria);
            this.DataContext = viewModel;
        }
    }
}
