using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.DAL.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddLogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLogsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(int userId, int pageNumber, int pageSize);
        Task<int> GetTotalLogCountAsync();
        Task<int> GetTotalLogCountByUserIdAsync(int userId);
    }
}
