using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;

        public AdminService(IUserRepository userRepository, IAuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0) return null;
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<ServiceResult> UpdateUserAsync(User userToUpdate, int performingAdminUserId)
        {
            if (userToUpdate == null || userToUpdate.UserId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid user data provided." };
            }

            var existingUser = await _userRepository.GetByIdAsync(userToUpdate.UserId);
            if (existingUser == null)
            {
                return new ServiceResult { Success = false, ErrorMessage = "User not found." };
            }

            existingUser.IsAdmin = userToUpdate.IsAdmin;

            try
            {
                bool success = await _userRepository.UpdateAsync(existingUser);
                if (success)
                {
                    await _auditLogService.LogActionAsync(performingAdminUserId, "Admin Updated User Profile", $"TargetUserID: {existingUser.UserId}, IsAdmin: {existingUser.IsAdmin}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Failed to update user profile." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user profile by admin: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred while updating the user profile." };
            }
        }

        public async Task<ServiceResult> DeleteUserAsync(int userIdToDelete, int performingAdminUserId)
        {
            if (userIdToDelete <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid User ID." };
            }
            if (userIdToDelete == performingAdminUserId)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Admin cannot delete their own account." };
            }

            var user = await _userRepository.GetByIdAsync(userIdToDelete);
            if (user == null)
            {
                return new ServiceResult { Success = false, ErrorMessage = "User not found." };
            }

            try
            {
                bool success = await _userRepository.DeleteAsync(userIdToDelete);
                if (success)
                {
                    await _auditLogService.LogActionAsync(performingAdminUserId, "Admin Deleted User", $"DeletedUserID: {userIdToDelete}, Username: {user.Username}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Failed to delete user." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user by admin: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred while deleting the user." };
            }
        }
    }
}