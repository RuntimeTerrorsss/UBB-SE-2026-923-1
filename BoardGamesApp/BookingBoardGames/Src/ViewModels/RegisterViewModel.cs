using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Src.Commands;

namespace BookingBoardGames.Src.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;

        private string username = string.Empty;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        private string city = string.Empty;
        private string country = string.Empty;
        private bool isLoading;

        private string usernameError = string.Empty;
        private string displayNameError = string.Empty;
        private string emailError = string.Empty;
        private string passwordError = string.Empty;
        private string confirmPasswordError = string.Empty;
        private string cityError = string.Empty;
        private string countryError = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action? NavigateToLogin;

        public ICommand RegisterCommand { get; }

        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(IUserService userService)
        {
            this.userService = userService;
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
            GoToLoginCommand = new RelayCommand(_ => NavigateToLogin?.Invoke());
        }

        private async Task RegisterAsync()
        {

        }
    }
}
