using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;

namespace MovieMate.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public IEnumerable<Movie> DisplayMovies { get; private set; } = Enumerable.Empty<Movie>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

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
        }
    }
}
