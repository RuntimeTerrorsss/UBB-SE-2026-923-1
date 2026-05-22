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
