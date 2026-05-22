using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Sharing.Services;

namespace BookingBoardGames.Src.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;

        public RegisterViewModel(IUserService userService)
        {
            this.userService = userService;
        }
    }
}
