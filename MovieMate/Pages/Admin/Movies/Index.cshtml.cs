using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;

namespace MovieMate.WebApp.Pages.Admin.Movies
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public IndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IEnumerable<Movie> Movies { get; private set; } = Enumerable.Empty<Movie>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Movies = await _movieService.SearchMoviesAsync(SearchTerm);
            }
            else
            {
                Movies = await _movieService.GetAllMoviesAsync();
            }
            Movies = Movies.OrderBy(m => m.Title).ToList();
        }
    }
}
