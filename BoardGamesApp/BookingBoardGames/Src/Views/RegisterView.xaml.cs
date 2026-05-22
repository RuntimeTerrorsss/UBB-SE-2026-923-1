using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardGames.Src.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterView : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterView()
        {
            InitializeComponent();
            ViewModel = new RegisterViewModel(App.UserService);
            //ViewModel.NavigateToLogin += () => Frame.Navigate(typeof(LoginPage));
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.IsLoading))
                {
                    RegisterButton.IsEnabled = !ViewModel.IsLoading;
                    RegisterButton.Content = ViewModel.IsLoading ? "Creating account…" : "Create account";
                }
            };
            DataContext = ViewModel;
        }
    }
}
