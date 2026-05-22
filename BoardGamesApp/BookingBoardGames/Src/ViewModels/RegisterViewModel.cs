using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        public string DisplayName
        {
            get => displayName;
            set { displayName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set { confirmPassword = value; OnPropertyChanged(); }
        }

        public string City
        {
            get => city;
            set { city = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get => country;
            set { country = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommandNoParam)?.RaiseCanExecuteChanged();
            }
        }

        public RegisterViewModel(IUserService userService)
        {
            this.userService = userService;
            RegisterCommand = new RelayCommandNoParam(async () => await RegisterAsync(), () => !IsLoading);
            GoToLoginCommand = new RelayCommandNoParam(() => NavigateToLogin?.Invoke());
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task RegisterAsync()
        {

        }
    }
}
