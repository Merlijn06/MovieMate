using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MovieMate.WebApp.Pages.Movies
{
    public class DetailsModel : PageModel
    {
        private readonly IMovieService _movieService;
        private readonly IReviewService _reviewService;
        private readonly IWatchlistService _watchlistService;

        public DetailsModel(
           IMovieService movieService,
           IReviewService reviewService,
           IWatchlistService watchlistService)
        {
            _movieService = movieService;
            _reviewService = reviewService;
            _watchlistService = watchlistService;
        }

        public Movie? Movie { get; private set; }
        public IEnumerable<Review> Reviews { get; private set; } = Enumerable.Empty<Review>();
        public bool IsInWatchlist { get; private set; } = false;
        public bool CanUserReview { get; private set; } = false;

        [BindProperty]
        public NewReviewInputModel? ReviewInput { get; set; }

        public class NewReviewInputModel
        {
            [Required]
            [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10.")]
            public decimal RatingValue { get; set; }

            [StringLength(10000, ErrorMessage = "Comment cannot be longer than 10000 characters.")]
            public string? Comment { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Movie = await _movieService.GetMovieByIdAsync(id);

            if (Movie == null)
            {
                return NotFound();
            }

            Reviews = await _reviewService.GetReviewsForMovieAsync(id);

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out int userId))
                {
                    IsInWatchlist = await _watchlistService.IsMovieInUserWatchlistAsync(userId, id);
                    CanUserReview = await _reviewService.CanUserReviewMovieAsync(userId, id);
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddToWatchlistAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdString, out int userId);

            var result = await _watchlistService.AddMovieToWatchlistAsync(userId, id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Movie added to your watchlist!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to add movie to watchlist.";
            }

            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostRemoveFromWatchlistAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            var result = await _watchlistService.RemoveMovieFromWatchlistAsync(userId, id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Movie removed from your watchlist.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to remove movie from watchlist.";
            }

            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int id)
        {
            Movie = await _movieService.GetMovieByIdAsync(id);
            if (Movie == null)
            {
                return NotFound();
            }

            Reviews = await _reviewService.GetReviewsForMovieAsync(id);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId;
            int.TryParse(userIdString, out userId);

            IsInWatchlist = await _watchlistService.IsMovieInUserWatchlistAsync(userId, id);
            CanUserReview = await _reviewService.CanUserReviewMovieAsync(userId, id);

            if (!ModelState.IsValid || ReviewInput == null)
            {
                return Page();
            
            }

            var newReview = new Review
            {
                MovieId = id,
                UserId = userId,
                RatingValue = ReviewInput.RatingValue,
                Comment = ReviewInput.Comment
            };

            var result = await _reviewService.AddOrUpdateReviewAsync(newReview, userId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Your review has been submitted successfully!";
                return RedirectToPage(new { id = id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to submit your review. Please try again.");
                return Page();
            }
        }
    }
}
