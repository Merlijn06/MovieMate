using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.Models
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; } // Nullable if action can be by system or anonymous
        public string Action { get; set; } = string.Empty; // e.g., "Rated Movie", "Added to Watchlist", "Admin Deleted Movie"
        public string? Details { get; set; } // e.g., "MovieID: 5, Rating: 8" or "UserID: 10, MovieID: 20"
        public DateTime Timestamp { get; set; }
    }
}
