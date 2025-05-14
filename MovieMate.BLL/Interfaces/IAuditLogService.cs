using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(int? userId, string action, string? details = null);

        // Methods for retrieving logs (used by Admin)
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int pageNumber = 1, int pageSize = 50);
        Task<int> GetTotalLogCountAsync();
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(int userId, int pageNumber = 1, int pageSize = 50);
        Task<int> GetTotalLogCountByUserAsync(int userId);
    }
}
