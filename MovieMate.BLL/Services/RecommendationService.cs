using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Recommendations;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IEnumerable<IPreferenceSourceStrategy> _sourceStrategies;
        private readonly IEnumerable<IMovieFinderStrategy> _finderStrategies;

        private readonly IReviewRepository _reviewRepository;
        private readonly IWatchlistRepository _watchlistRepository;
        private readonly IRecommendationFeedbackRepository _feedbackRepository;

        public RecommendationService(
           IMovieRepository movieRepository,
           IEnumerable<IPreferenceSourceStrategy> sourceStrategies,
           IEnumerable<IMovieFinderStrategy> finderStrategies,
           IReviewRepository reviewRepository,
           IWatchlistRepository watchlistRepository,
           IRecommendationFeedbackRepository feedbackRepository)
        {
            _movieRepository = movieRepository;
            _sourceStrategies = sourceStrategies;
            _finderStrategies = finderStrategies;
            _reviewRepository = reviewRepository;
            _watchlistRepository = watchlistRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<ServiceResult<IEnumerable<Movie>>> GetRecommendationsForUserAsync(int userId, int count)
        {
            if (userId <= 0)
            {
                return new ServiceResult<IEnumerable<Movie>> { Success = false, ErrorMessage = "Invalid user ID provided." };
            }

            try
            {
                var sourceMoviesTasks = _sourceStrategies.Select(s => s.GetSourceMoviesAsync(userId));
                var sourceMoviesResults = await Task.WhenAll(sourceMoviesTasks);

                var uniqueSourceMovies = sourceMoviesResults
                    .SelectMany(movies => movies)
                    .GroupBy(m => m.MovieId)
                    .Select(g => g.First());

                var allMoviesForFinding = await _movieRepository.GetAllAsync();
                var candidateMovies = new List<Movie>();

                if (uniqueSourceMovies.Any())
                {
                    foreach (var strategy in _finderStrategies)
                    {
                        var candidates = strategy.FindCandidateMovies(uniqueSourceMovies, allMoviesForFinding);
                        candidateMovies.AddRange(candidates);
                    }
                }

                var uniqueCandidateMovies = candidateMovies
                    .GroupBy(m => m.MovieId)
                    .Select(g => g.First());

                var userReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
                var userWatchlistItems = await _watchlistRepository.GetWatchlistByUserIdAsync(userId);
                var userFeedback = await _feedbackRepository.GetFeedbacksByUserAsync(userId);

                var interactedMovieIds = new HashSet<int>();
                interactedMovieIds.UnionWith(userReviews.Select(r => r.MovieId));
                interactedMovieIds.UnionWith(userWatchlistItems.Select(item => item.MovieId));
                interactedMovieIds.UnionWith(userFeedback.Select(f => f.MovieId));
                interactedMovieIds.UnionWith(uniqueSourceMovies.Select(m => m.MovieId));

                var recommendations = uniqueCandidateMovies
                    .Where(m => !interactedMovieIds.Contains(m.MovieId))
                    .OrderByDescending(m => m.AverageRating)
                    .ThenByDescending(m => m.TotalRatings)
                    .Take(count)
                    .ToList();

                if (recommendations.Count < count)
                {
                    var fallbackMovies = allMoviesForFinding
                       .Where(m => !interactedMovieIds.Contains(m.MovieId) && !recommendations.Any(rec => rec.MovieId == m.MovieId))
                       .OrderByDescending(m => m.AverageRating)
                       .ThenByDescending(m => m.TotalRatings)
                       .Take(count - recommendations.Count);

                    recommendations.AddRange(fallbackMovies);
                }

                return new ServiceResult<IEnumerable<Movie>> { Success = true, Data = recommendations };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecommendationsForUserAsync for userId {userId}: {ex.Message}");
                return new ServiceResult<IEnumerable<Movie>>
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred while generating recommendations."
                };
            }
        }
    }
}
