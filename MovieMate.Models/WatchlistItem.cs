using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.Models
{
    public class WatchlistItem
    {
        public int WatchlistId { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime AddedAt { get; set; }

        public Movie? Movie { get; set; }
    }
}
