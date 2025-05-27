using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System.Security.Claims;

namespace MovieMate.WebApp.Pages.Watchlist
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IWatchlistService _watchlistService;
        private readonly IMovieService _movieService;

        public IndexModel(IWatchlistService watchlistService, IMovieService movieService)
        {
            _watchlistService = watchlistService;
            _movieService = movieService;
        }

        public class WatchlistDisplayItem
        {
            public int WatchlistId { get; set; }
            public DateTime AddedAt { get; set; }
            public Movie MovieDetails { get; set; } = null!;
        }

        public IEnumerable<WatchlistDisplayItem> DisplayableWatchlistItems { get; private set; } = Enumerable.Empty<WatchlistDisplayItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            var basicWatchlistItems = await _watchlistService.GetUserWatchlistAsync(userId);

            var hydratedItems = new List<WatchlistDisplayItem>();

            foreach (var basicItem in basicWatchlistItems)
            {
                var movie = await _movieService.GetMovieByIdAsync(basicItem.MovieId);
                if (movie != null)
                {
                    hydratedItems.Add(new WatchlistDisplayItem
                    {
                        WatchlistId = basicItem.WatchlistId,
                        AddedAt = basicItem.AddedAt,
                        MovieDetails = movie
                    });
                }
            }

            DisplayableWatchlistItems = hydratedItems.OrderByDescending(i => i.AddedAt).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveFromWatchlistOnPageAsync(int movieIdToRemove)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            var result = await _watchlistService.RemoveMovieFromWatchlistAsync(userId, movieIdToRemove);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Movie removed from your watchlist.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to remove movie from watchlist.";
            }

            return RedirectToPage();
        }
    }
}
