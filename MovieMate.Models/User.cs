using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        // public DateTime UpdatedAt { get; set; } // must be added in ERD

        //public List<Rating> Ratings { get; set; } = new List<Rating>();
        //public List<Review> Reviews { get; set; } = new List<Review>();
        //public List<WatchlistItem> Watchlist { get; set; } = new List<WatchlistItem>();
    }
}
