using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using MovieMate.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IRecommendationFeedbackService
    {
        Task<ServiceResult> RecordFeedbackAsync(int userId, int movieId, bool liked);
        Task<ServiceResult> ClearFeedbackAsync(int userId, int movieId);
        Task<RecommendationFeedback?> GetUserFeedbackForMovieAsync(int userId, int movieId);
        Task<IEnumerable<RecommendationFeedback>> GetAllUserFeedbackAsync(int userId);
    }
}
