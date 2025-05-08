using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieMate.Models;
using MovieMate.DAL.Interfaces;

namespace MovieMate.DAL.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT UserID, Username, Email, PasswordHash, IsAdmin, CreatedAt FROM Users WHERE UserID = @UserId;";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = CreateConnection();
            var sql = "SELECT UserID, Username, Email, PasswordHash, IsAdmin, CreatedAt FROM Users WHERE Username = @Username;";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = CreateConnection();
            var sql = "SELECT UserID, Username, Email, PasswordHash, IsAdmin, CreatedAt FROM Users WHERE Email = @Email;";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<int> AddAsync(User user)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Users (Username, Email, PasswordHash, IsAdmin, CreatedAt)
                VALUES (@Username, @Email, @PasswordHash, @IsAdmin, NOW());
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE Users SET
                    Username = @Username,
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    IsAdmin = @IsAdmin
                WHERE UserID = @UserID;";
            return await connection.ExecuteAsync(sql, user) > 0;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM Users WHERE UserID = @UserId;";
            return await connection.ExecuteAsync(sql, new { UserId = userId }) > 0;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT UserID, Username, Email, IsAdmin, CreatedAt FROM Users ORDER BY Username;";
            return await connection.QueryAsync<User>(sql);
        }
    }
}
