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

        public async Task<IEnumerable<Movie>> GetRecommendationsForUserAsync(int userId, int count = 10)
        {
            if (userId <= 0) return Enumerable.Empty<Movie>();

            try
            {
                // 1. Get movies user has interacted with positively (high ratings, liked recs, watchlist)
                var userReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
                var userWatchlistItems = await _watchlistRepository.GetWatchlistByUserIdAsync(userId); // DAL returns with Movie object
                var userFeedback = await _feedbackRepository.GetFeedbacksByUserAsync(userId);

                var preferredMovieIds = new HashSet<int>();
                var interactedMovieIds = new HashSet<int>(); // All movies user has touched
                var dislikedMovieIds = new HashSet<int>();

                foreach (var review in userReviews)
                {
                    interactedMovieIds.Add(review.MovieId);
                    if (review.RatingValue >= 7) // Define "high rating"
                    {
                        preferredMovieIds.Add(review.MovieId);
                    }
                }
                foreach (var item in userWatchlistItems)
                {
                    interactedMovieIds.Add(item.MovieId);
                    preferredMovieIds.Add(item.MovieId); // Adding to watchlist is a positive signal
                }
                foreach (var feedback in userFeedback)
                {
                    interactedMovieIds.Add(feedback.MovieId);
                    if (feedback.Liked)
                    {
                        preferredMovieIds.Add(feedback.MovieId);
                    }
                    else
                    {
                        dislikedMovieIds.Add(feedback.MovieId);
                    }
                }

                // 2. Get details of these preferred movies to find common genres
                var preferredGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (preferredMovieIds.Any())
                {
                    // In a real app, you'd have a _movieRepository.GetByIdsAsync(preferredMovieIds)
                    // For now, let's fetch them one by one (not ideal for performance but simple for example)
                    foreach (var movieId in preferredMovieIds)
                    {
                        var movie = await _movieRepository.GetByIdAsync(movieId);
                        if (movie != null && !string.IsNullOrWhiteSpace(movie.Genre))
                        {
                            // Split genre string if it contains multiple genres (e.g., "Action, Adventure")
                            var genres = movie.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(g => g.Trim());
                            foreach (var g in genres) preferredGenres.Add(g);
                        }
                    }
                }

                // If no preferred genres found (new user, or no strong signals), maybe recommend popular movies or a diverse set
                if (!preferredGenres.Any())
                {
                    // Fallback: get some highly-rated movies not interacted with by user
                    var allMovies = await _movieRepository.GetAllAsync(); // This already has average ratings
                    return allMovies.OrderByDescending(m => m.AverageRating)
                                    .ThenByDescending(m => m.TotalRatings)
                                    .Where(m => !interactedMovieIds.Contains(m.MovieId) && !dislikedMovieIds.Contains(m.MovieId))
                                    .Take(count);
                }

                // 3. Get all movies and filter
                var candidateMovies = await _movieRepository.GetAllAsync();

                var recommendations = candidateMovies
                    .Where(m => !interactedMovieIds.Contains(m.MovieId) &&      // Not interacted with
                                !dislikedMovieIds.Contains(m.MovieId) &&       // Not disliked
                                !string.IsNullOrWhiteSpace(m.Genre) &&
                                m.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(g => g.Trim())
                                       .Any(g => preferredGenres.Contains(g, StringComparer.OrdinalIgnoreCase))) // Matches preferred genres
                    .OrderByDescending(m => m.AverageRating) // Prioritize by rating within preferred genres
                    .ThenByDescending(m => m.TotalRatings)
                    .Take(count)
                    .ToList();

                // If not enough recommendations from preferred genres, fill with popular ones
                if (recommendations.Count < count)
                {
                    var popularFallback = candidateMovies
                        .Where(m => !interactedMovieIds.Contains(m.MovieId) &&
                                    !dislikedMovieIds.Contains(m.MovieId) &&
                                    !recommendations.Any(r => r.MovieId == m.MovieId)) // Exclude already selected
                        .OrderByDescending(m => m.AverageRating)
                        .ThenByDescending(m => m.TotalRatings)
                        .Take(count - recommendations.Count);
                    recommendations.AddRange(popularFallback);
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating recommendations: {ex.Message}");
                return Enumerable.Empty<Movie>(); // Return empty on error
            }
        }
    }
}
