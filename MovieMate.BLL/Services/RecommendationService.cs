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
        private readonly IReviewRepository _reviewRepository;
        private readonly IWatchlistRepository _watchlistRepository;
        private readonly IRecommendationFeedbackRepository _feedbackRepository;
        private readonly IEnumerable<IRecommendationStrategy> _strategies;

        public RecommendationService(
            IMovieRepository movieRepository,
            IReviewRepository reviewRepository,
            IWatchlistRepository watchlistRepository,
            IRecommendationFeedbackRepository feedbackRepository,
            IEnumerable<IRecommendationStrategy> strategies)
        {
            _movieRepository = movieRepository;
            _reviewRepository = reviewRepository;
            _watchlistRepository = watchlistRepository;
            _feedbackRepository = feedbackRepository;
            _strategies = strategies;
        }

        public async Task<IEnumerable<Movie>> GetRecommendationsForUserAsync(int userId, int count)
        {
            if (userId <= 0)
            {
                return Enumerable.Empty<Movie>();
            }

            try
            {
                var allPreferredMovies = new List<Movie>();
                foreach (var strategy in _strategies)
                {
                    var preferredFromStrategy = await strategy.GetPreferredMoviesAsync(userId);
                    allPreferredMovies.AddRange(preferredFromStrategy);
                }

                var uniquePreferredMovies = allPreferredMovies
                    .GroupBy(m => m.MovieId)
                    .Select(g => g.First());

                var preferredGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var movie in uniquePreferredMovies)
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

                var userReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
                var userWatchlistItems = await _watchlistRepository.GetWatchlistByUserIdAsync(userId);
                var userFeedback = await _feedbackRepository.GetFeedbacksByUserAsync(userId);

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
                        .Take(count);
                }
                else
                {
                    recommendations = allMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId))
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count);
                }

                var finalList = recommendations.ToList();

                if (finalList.Count < count)
                {
                    var fallbackMovies = allMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId))
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count - finalList.Count);

                    finalList.AddRange(fallbackMovies);
                }

                return finalList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating recommendations for user {userId}: {ex.Message}");
                return Enumerable.Empty<Movie>();
            }
        }
    }
}
