using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;

namespace MovieMate.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public IEnumerable<Movie> AllMovies { get; private set; } = Enumerable.Empty<Movie>();

        public IndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task OnGetAsync()
        {
            AllMovies = await _movieService.GetAllMoviesAsync();
        }
    }
}
