using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MovieMate.WebApp.Pages.Admin.Movies
{
    public class CreateModel : PageModel
    {
        private readonly IMovieService _movieService;
        private readonly IAuditLogService _auditLogService;

        public CreateModel(IMovieService movieService, IAuditLogService auditLogService)
        {
            _movieService = movieService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public MovieInputModel MovieInput { get; set; } = new MovieInputModel();

        public class MovieInputModel
        {
            [Required(ErrorMessage = "Title is required.")]
            [StringLength(255, MinimumLength = 1, ErrorMessage = "Title must be betwee 1 and 255 characters.")]
            public string Title { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "Genre can be up to 100 characters.")]
            public string? Genre { get; set; }

            [DataType(DataType.MultilineText)]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Release date is required.")]
            [DataType(DataType.Date)]
            [Display(Name = "Release Date")]
            public DateTime ReleaseDate { get; set; } = DateTime.Today;

            [Display(Name = "Poster URL")]
            [Url(ErrorMessage = "Please enter a valid URL for the poster.")]
            [StringLength(255, ErrorMessage = "Poster URL can be up to 255 characters.")]
            public string? PosterURL { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newMovie = new Movie()
            {
                Title = MovieInput.Title,
                Genre = MovieInput.Genre,
                Description = MovieInput.Description,
                ReleaseDate = MovieInput.ReleaseDate,
                PosterURL = MovieInput.PosterURL
            };

            var adminUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminUserIdString, out int adminUserId);

            var result = await _movieService.AddMovieAsync(newMovie, adminUserId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Movie '{newMovie.Title}' was successfully created.";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while creating the movie.");
                return Page();
            }
        }
    }
}
