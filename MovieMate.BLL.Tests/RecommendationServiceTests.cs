using Moq;
using MovieMate.BLL.Recommendations;
using MovieMate.BLL.Services;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace MovieMate.BLL.Tests
{
    public class RecommendationServiceTests
    {
        [Fact]
        public async Task GetRecommendations_UserWithPreferences_ReturnsGenreBasedMovies()
        {
            // Arrange
            var mockMovieRepo = new Mock<IMovieRepository>();
            var mockReviewRepo = new Mock<IReviewRepository>();
            var mockWatchlistRepo = new Mock<IWatchlistRepository>();
            var mockFeedbackRepo = new Mock<IRecommendationFeedbackRepository>();

            int testUserId = 1;
            var highRatedMovie = new Movie { MovieId = 1, Title = "High Rated Movie", Genre = "Action, Sci-Fi" };
            var watchlistMovie = new Movie { MovieId = 2, Title = "Watchlist Movie", Genre = "Sci-Fi, Drama" };

            var allMoviesForSearch = new List<Movie>
        {
            highRatedMovie,
            watchlistMovie,
            new Movie { MovieId = 3, Title = "Recommendation 1", Genre = "Action" },
            new Movie { MovieId = 4, Title = "Recommendation 2", Genre = "Drama" },
            new Movie { MovieId = 5, Title = "Wrong Genre Movie", Genre = "Comedy" }
        };

            var highRatingStrategy = new HighRatingStrategy(mockReviewRepo.Object);
            var watchlistStrategy = new WatchlistStrategy(mockWatchlistRepo.Object);

            mockReviewRepo.Setup(r => r.GetReviewsByUserIdAsync(testUserId))
                .ReturnsAsync(new List<Review> { new Review { MovieId = 1, RatingValue = 8.0m, Movie = highRatedMovie } });

            mockWatchlistRepo.Setup(w => w.GetWatchlistByUserIdAsync(testUserId))
                .ReturnsAsync(new List<WatchlistItem> { new WatchlistItem { MovieId = 2, Movie = watchlistMovie } });

            mockMovieRepo.Setup(m => m.GetAllAsync()).ReturnsAsync(allMoviesForSearch);
            mockFeedbackRepo.Setup(f => f.GetFeedbacksByUserAsync(testUserId)).ReturnsAsync(new List<RecommendationFeedback>());

            var recommendationService = new RecommendationService(
                mockMovieRepo.Object,
                new List<IPreferenceSourceStrategy> { highRatingStrategy, watchlistStrategy },
                new List<IMovieFinderStrategy> { new GenreBasedFinderStrategy() },
                mockReviewRepo.Object,
                mockWatchlistRepo.Object,
                mockFeedbackRepo.Object
            );

            // Act
            var result = await recommendationService.GetRecommendationsForUserAsync(testUserId, 2);
            var recommendations = result.Data!.ToList();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(recommendations);
            Assert.Equal(2, recommendations.Count);
            Assert.Contains(recommendations, m => m.MovieId == 3);
            Assert.Contains(recommendations, m => m.MovieId == 4);
            Assert.DoesNotContain(recommendations, m => m.MovieId == 5);
        }

        [Fact]
        public async Task GetRecommendations_RepositoryThrowsException_ReturnsUnsuccessfulResult()
        {
            // Arrange
            var mockMovieRepo = new Mock<IMovieRepository>();
            var mockReviewRepo = new Mock<IReviewRepository>();
            var mockWatchlistRepo = new Mock<IWatchlistRepository>();
            var mockFeedbackRepo = new Mock<IRecommendationFeedbackRepository>();
            var mockSourceStrategies = new Mock<IEnumerable<IPreferenceSourceStrategy>>();
            var mockFinderStrategies = new Mock<IEnumerable<IMovieFinderStrategy>>();

            mockMovieRepo
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Simulated database connection failed"));

            var recommendationService = new RecommendationService(
                mockMovieRepo.Object,
                mockSourceStrategies.Object,
                mockFinderStrategies.Object,
                mockReviewRepo.Object,
                mockWatchlistRepo.Object,
                mockFeedbackRepo.Object
            );

            int testUserId = 1;

            // Act
            var result = await recommendationService.GetRecommendationsForUserAsync(testUserId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success, "ServiceResult.Success should be false when an exception occurs.");
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("An unexpected error occurred while generating recommendations.", result.ErrorMessage);
        }
    }
}
