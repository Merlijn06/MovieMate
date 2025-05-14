using MovieMate.BLL.Interfaces;
using MovieMate.DAL.Interfaces;
using MovieMate.DAL.Repositories;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        }

        public async Task LogActionAsync(int? userId, string action, string? details = null)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                Console.WriteLine("Attempted to log an action with empty action string.");
                return;
            }

            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow // Uses UTC time
            };

            try
            {
                await _auditLogRepository.AddLogAsync(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write audit log: {ex.Message} - Action: {action}, UserID: {userId}, Details: {details}");
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;
            return await _auditLogRepository.GetLogsAsync(pageNumber, pageSize);
        }

        public async Task<int> GetTotalLogCountAsync()
        {
            return await _auditLogRepository.GetTotalLogCountAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(int userId, int pageNumber = 1, int pageSize = 50)
        {
            if (userId <= 0) return Enumerable.Empty<AuditLog>();
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;
            return await _auditLogRepository.GetLogsByUserIdAsync(userId, pageNumber, pageSize);
        }

        public async Task<int> GetTotalLogCountByUserAsync(int userId)
        {
            if (userId <= 0) return 0;
            return await _auditLogRepository.GetTotalLogCountByUserIdAsync(userId);
        }
    }
}
