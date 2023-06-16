using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task RegisterUserAsync(UserDto userDto);

        Task<bool> VerifyUserAsync(string login, string password);
    }
}
