using Application.Dtos;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<Guid> RegisterUserAsync(UserRegisterDto userDto);

        Task<User> GetUserAsync(string email);

        Task<User> GetUserAsync(Guid userId);

        Task<bool> VerifyUserAsync(UserVerifyDto userDto);

        bool VerifyPassword(User user, string password);
    }
}
