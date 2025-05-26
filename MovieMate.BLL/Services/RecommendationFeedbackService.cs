using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Services
{
    public class RecommendationFeedbackService : IRecommendationFeedbackService
    {
        private readonly IRecommendationFeedbackRepository _feedbackRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuditLogService _auditLogService;

        public RecommendationFeedbackService(
            IRecommendationFeedbackRepository feedbackRepository,
            IMovieRepository movieRepository,
            IAuditLogService auditLogService)
        {
            _feedbackRepository = feedbackRepository;
            _movieRepository = movieRepository;
            _auditLogService = auditLogService;
        }

        public async Task<ServiceResult> RecordFeedbackAsync(int userId, int movieId, bool liked)
        {
            if (userId <= 0 || movieId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid user or movie ID." };
            }

            var movieExists = await _movieRepository.GetByIdAsync(movieId);
            if (movieExists == null)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Movie not found." };
            }

            try
            {
                // The DAL's AddOrUpdateFeedbackAsync handles insert or update logic
                await _feedbackRepository.AddOrUpdateFeedbackAsync(userId, movieId, liked);
                string action = liked ? "Liked Recommendation" : "Disliked Recommendation";
                await _auditLogService.LogActionAsync(userId, action, $"MovieID: {movieId}");
                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording recommendation feedback: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred while recording feedback." };
            }
        }

        public async Task<ServiceResult> ClearFeedbackAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid user or movie ID." };
            }

            try
            {
                bool success = await _feedbackRepository.RemoveFeedbackAsync(userId, movieId);
                if (success)
                {
                    await _auditLogService.LogActionAsync(userId, "Cleared Recommendation Feedback", $"MovieID: {movieId}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = true, ErrorMessage = "No feedback found to clear." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing recommendation feedback: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred." };
            }
        }

        public async Task<RecommendationFeedback?> GetUserFeedbackForMovieAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0) return null;
            return await _feedbackRepository.GetFeedbackAsync(userId, movieId);
        }

        public async Task<IEnumerable<RecommendationFeedback>> GetAllUserFeedbackAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<RecommendationFeedback>();
            return await _feedbackRepository.GetFeedbacksByUserAsync(userId);
        }
    }
}
