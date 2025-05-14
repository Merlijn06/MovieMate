using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<ServiceResult> UpdateUserAsync(User userToUpdate, int performingAdminUserId);
        Task<ServiceResult> DeleteUserAsync(int userIdToDelete, int performingAdminUserId);
    }
}
