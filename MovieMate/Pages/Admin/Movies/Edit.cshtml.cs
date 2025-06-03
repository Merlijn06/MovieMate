using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System.Security.Claims;
using static MovieMate.WebApp.Pages.Admin.Movies.CreateModel;

namespace MovieMate.WebApp.Pages.Admin.Movies
{
    public class EditModel : PageModel
    {
        private readonly IMovieService _movieService;
        private readonly IAuditLogService _auditLogService;

        public EditModel(IMovieService movieService, IAuditLogService auditLogService)
        {
            _movieService = movieService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public MovieInputModel MovieInput { get; set; } = new MovieInputModel();

        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Id = id;

            var movie = await _movieService.GetMovieByIdAsync(id);

            if (movie == null)
            {
                TempData["ErrorMessage"] = "Movie not found.";
                return RedirectToPage("./Index");
            }

            MovieInput = new MovieInputModel
            {
                Title = movie.Title,
                Genre = movie.Genre,
                Description = movie.Description,
                ReleaseDate = movie.ReleaseDate,
                PosterURL = movie.PosterURL
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var updatedMovie = new Movie()
            {
                MovieId = id,
                Title = MovieInput.Title,
                Genre = MovieInput.Genre,
                Description = MovieInput.Description,
                ReleaseDate = MovieInput.ReleaseDate,
                PosterURL = MovieInput.PosterURL
            };

            var adminUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminUserIdString, out int adminUserId);

            var result = await _movieService.UpdateMovieAsync(updatedMovie, adminUserId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Movie '{updatedMovie.Title}' was successfully updated.";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while updating the movie.");
                return Page();
            }
        }
    }
}
