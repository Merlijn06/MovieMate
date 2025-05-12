using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;

namespace MovieMate.DAL.Repositories
{
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IConfiguration configuration) : base(configuration) { }

        public async Task AddLogAsync(AuditLog log)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO AuditLogs (UserID, Action, Details)
                VALUES (@UserId, @Action, @Details);";
            await connection.ExecuteAsync(sql, log);
        }

        public async Task<IEnumerable<AuditLog>> GetLogsAsync(int pageNumber, int pageSize)
        {
            using var connection = CreateConnection();
            var offset = (pageNumber - 1) * pageSize;
            var sql = @"
                SELECT LogID, UserID, Action, Details, Timestamp
                FROM AuditLogs
                ORDER BY Timestamp DESC
                LIMIT @PageSize OFFSET @Offset;";
            return await connection.QueryAsync<AuditLog>(sql, new { PageSize = pageSize, Offset = offset });
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(int userId, int pageNumber, int pageSize)
        {
            using var connection = CreateConnection();
            var offset = (pageNumber - 1) * pageSize;
            var sql = @"
                SELECT LogID, UserID, Action, Details, Timestamp
                FROM AuditLogs
                WHERE UserID = @UserId
                ORDER BY Timestamp DESC
                LIMIT @PageSize OFFSET @Offset;";
            return await connection.QueryAsync<AuditLog>(sql, new { UserId = userId, PageSize = pageSize, Offset = offset });
        }

        public async Task<int> GetTotalLogCountAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(*) FROM AuditLogs;";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> GetTotalLogCountByUserIdAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(*) FROM AuditLogs WHERE UserID = @UserId;";
            return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }
    }
}
