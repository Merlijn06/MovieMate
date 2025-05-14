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
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;

        public UserService(IUserRepository userRepository, IAuditLogService auditLogService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        }

        public async Task<ServiceResult<User>> RegisterUserAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new ServiceResult<User> { Success = false, ErrorMessage = "Username, email, and password are required." };
            }
            if (!email.Contains('@') || !email.Contains('.'))
            {
                return new ServiceResult<User> { Success = false, ErrorMessage = "Invalid email format." };
            }
            if (password.Length < 6)
            {
                return new ServiceResult<User> { Success = false, ErrorMessage = "Password must be at least 6 characters long." };
            }

            try
            {
                var existingByUsername = await _userRepository.GetByUsernameAsync(username);
                if (existingByUsername != null)
                {
                    return new ServiceResult<User> { Success = false, ErrorMessage = "Username already exists." };
                }
                var existingByEmail = await _userRepository.GetByEmailAsync(email);
                if (existingByEmail != null)
                {
                    return new ServiceResult<User> { Success = false, ErrorMessage = "Email address is already registered." };
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    Username = username.Trim(),
                    Email = email.Trim().ToLower(),
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow // Use UTC time
                };

                int newUserId = await _userRepository.AddAsync(newUser);

                if (newUserId > 0)
                {
                    newUser.UserId = newUserId; // Assign the returned ID to the object
                    await _auditLogService.LogActionAsync(newUserId, "User Registered", $"Username: {newUser.Username}");
                    return new ServiceResult<User> { Success = true, Data = newUser };
                }
                else
                {
                    return new ServiceResult<User> { Success = false, ErrorMessage = "Failed to register user." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return new ServiceResult<User> { Success = false, ErrorMessage = "An unexpected error occurred during registration." };
            }
        }

        public async Task<ServiceResult<User>> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new ServiceResult<User> { Success = false, ErrorMessage = "Username and password are required." };
            }

            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    await _auditLogService.LogActionAsync(user.UserId, "User Logged In", $"Username: {user.Username}");
                    return new ServiceResult<User> { Success = true, Data = user };
                }
                else
                {
                    await _auditLogService.LogActionAsync(null, "Failed Login Attempt", $"Username: {username}");
                    return new ServiceResult<User> { Success = false, ErrorMessage = "Invalid username or password." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error authenticating user: {ex.Message}");
                return new ServiceResult<User> { Success = false, ErrorMessage = "An unexpected error occurred during login." };
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0) return null;
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Typically an admin-only operation
            return await _userRepository.GetAllAsync();
        }
    }
}
