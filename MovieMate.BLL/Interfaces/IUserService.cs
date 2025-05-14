using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<User>> RegisterUserAsync(string username, string email, string password);
        Task<ServiceResult<User>> AuthenticateUserAsync(string username, string password);

        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        // Task<ServiceResult> UpdateUserProfileAsync(User user, int performingUserId); // Example: If users can update their own profile (email, etc.)
    }
}
