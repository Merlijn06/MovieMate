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
    public class RecommendationService : IRecommendationService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IWatchlistRepository _watchlistRepository;
        private readonly IRecommendationFeedbackRepository _feedbackRepository;

        public RecommendationService(
            IMovieRepository movieRepository,
            IReviewRepository reviewRepository,
            IWatchlistRepository watchlistRepository,
            IRecommendationFeedbackRepository feedbackRepository)
        {
            _movieRepository = movieRepository;
            _reviewRepository = reviewRepository;
            _watchlistRepository = watchlistRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<Movie>> GetRecommendationsForUserAsync(int userId, int count)
        {
            if (userId <= 0)
            {
                return Enumerable.Empty<Movie>();
            }

            try
            {
                var userReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
                var userWatchlistItems = await _watchlistRepository.GetWatchlistByUserIdAsync(userId);
                var userFeedback = await _feedbackRepository.GetFeedbacksByUserAsync(userId);

                var highlyRatedMovieIds = userReviews
                    .Where(r => r.RatingValue >= 7.0m)
                    .Select(r => r.MovieId);

                var watchlistMovieIds = userWatchlistItems.Select(item => item.MovieId);

                var likedFeedbackMovieIds = userFeedback
                    .Where(f => f.Liked)
                    .Select(f => f.MovieId);

                var allPreferredMovieIds = new HashSet<int>(highlyRatedMovieIds);
                allPreferredMovieIds.UnionWith(watchlistMovieIds);
                allPreferredMovieIds.UnionWith(likedFeedbackMovieIds);

                IEnumerable<Movie> preferredMovies = new List<Movie>();
                if (allPreferredMovieIds.Any())
                {
                    preferredMovies = await _movieRepository.GetByIdsAsync(allPreferredMovieIds);
                }

                var preferredGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var movie in preferredMovies)
                {
                    if (!string.IsNullOrWhiteSpace(movie.Genre))
                    {
                        var genres = movie.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim());
                        foreach (var genre in genres)
                        {
                            preferredGenres.Add(genre);
                        }
                    }
                }

                var interactedMovieIds = new HashSet<int>();
                interactedMovieIds.UnionWith(userReviews.Select(r => r.MovieId));
                interactedMovieIds.UnionWith(userWatchlistItems.Select(item => item.MovieId));
                interactedMovieIds.UnionWith(userFeedback.Select(f => f.MovieId));

                var allMovies = await _movieRepository.GetAllAsync();

                IEnumerable<Movie> recommendations;

                if (preferredGenres.Any())
                {
                    recommendations = allMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId))
                        .Where(m => !string.IsNullOrWhiteSpace(m.Genre) &&
                                    m.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(g => g.Trim())
                                           .Any(g => preferredGenres.Contains(g)))
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count)
                        .ToList();
                }
                else
                {
                    recommendations = allMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId))
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count)
                        .ToList();
                }

                if (recommendations.Count() < count)
                {
                    var fallbackMovies = allMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId) && !recommendations.Any(rec => rec.MovieId == m.MovieId))
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count - recommendations.Count());
                    recommendations = recommendations.Concat(fallbackMovies).ToList();
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating recommendations for user {userId}: {ex.Message}");
                return Enumerable.Empty<Movie>();
            }
        }
    }
}
