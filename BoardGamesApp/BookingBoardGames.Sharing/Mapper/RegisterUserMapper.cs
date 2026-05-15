using BookingBoardGames.Sharing.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Sharing.Mapper
{
    public class RegisterUserMapper
    {
        public User TurnDataTransferObjectIntoEntity(RegisterDTO registeringUserDTO)
        {
            return new User
            {
                Username = registeringUserDTO.Username,
                DisplayName = registeringUserDTO.DisplayName,
                Email = registeringUserDTO.Email,
                PasswordHash = registeringUserDTO.Password,
                City = registeringUserDTO.City,
                Country = registeringUserDTO.Country
            };
        }
    }
}
