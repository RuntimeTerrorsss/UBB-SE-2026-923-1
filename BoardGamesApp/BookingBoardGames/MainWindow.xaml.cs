// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BookingBoardgamesILoveBan.Src.Chat.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BookingBoardGames
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.RootFrame.Navigate(typeof(Src.Views.DiscoveryView));

            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            frame1.Navigate(typeof(ChatPageView), 1); // user id 1
            window1.Title = "Alice";
            window1.Activate();

            var window2 = new Window();
            var frame2 = new Frame();
            window2.Content = frame2;
            frame2.Navigate(typeof(ChatPageView), 2); // user id 2
            window2.Title = "Bob";
            window2.Activate();

            this.Closed += this.MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
