using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email); // Useful for checking if email is already registered
        Task<int> AddAsync(User user); // Returns the ID of the newly created user
        Task<bool> UpdateAsync(User user); // Returns true if update was successful
        Task<bool> DeleteAsync(int userId); // Returns true if delete was successful
        Task<IEnumerable<User>> GetAllAsync(); // For admin purpose
    }
}
