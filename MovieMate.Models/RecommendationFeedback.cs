using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.Models
{
    public class RecommendationFeedback
    {
        public int RecommendationId { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public bool Liked { get; set; }
        public bool Disliked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
