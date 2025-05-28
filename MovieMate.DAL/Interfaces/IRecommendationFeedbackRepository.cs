using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.DAL.Interfaces
{
    public interface IRecommendationFeedbackRepository
    {
        Task<RecommendationFeedback?> GetFeedbackAsync(int userId, int movieId);
        Task AddOrUpdateFeedbackAsync(int userId, int movieId, bool liked);
        Task<bool> RemoveFeedbackAsync(int userId, int movieId);
        Task<IEnumerable<RecommendationFeedback>> GetFeedbacksByUserAsync(int userId);
    }
}
