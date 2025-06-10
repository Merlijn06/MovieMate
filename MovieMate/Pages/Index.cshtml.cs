using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;

namespace MovieMate.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;
        private readonly IRecommendationService _recommendationService;
        private readonly IRecommendationFeedbackService _feedbackService;

        public IndexModel(IMovieService movieService, IRecommendationService recommendationService, IRecommendationFeedbackService feedbackService)
        {
            _movieService = movieService;
            _recommendationService = recommendationService;
            _feedbackService = feedbackService;
        }

        public IEnumerable<Movie> DisplayMovies { get; private set; } = Enumerable.Empty<Movie>();
        public IEnumerable<Movie> RecommendedMovies { get; private set; } = Enumerable.Empty<Movie>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                DisplayMovies = await _movieService.SearchMoviesAsync(SearchTerm);
            }
            else
            {
                var allMovies = await _movieService.GetAllMoviesAsync();

                DisplayMovies = allMovies
                                 .OrderByDescending(m => m.AverageRating)
                                 .ThenByDescending(m => m.TotalRatings)
                                 .Take(12);
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int.TryParse(userIdString, out int userId);
                RecommendedMovies = await _recommendationService.GetRecommendationsForUserAsync(userId, 6);
            }
        }

        public async Task<IActionResult> OnPostLikeRecommendationAsync(int movieId)
        {
            return await HandleFeedbackAsync(movieId, true);
        }

        public async Task<IActionResult> OnPostDislikeRecommendationAsync(int movieId)
        {
            return await HandleFeedbackAsync(movieId, false);
        }

        private async Task<IActionResult> HandleFeedbackAsync(int movieId, bool liked)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            await _feedbackService.RecordFeedbackAsync(userId, movieId, liked);

            return RedirectToPage();
        }
    }
}
