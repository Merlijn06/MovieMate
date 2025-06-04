using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.BLL.Services;
using MovieMate.Models;
using System.Security.Claims;

namespace MovieMate.WebApp.Pages.Admin.Movies
{
    public class DeleteModel : PageModel
    {
        public readonly IMovieService _movieService;
        public readonly IAuditLogService _auditLogService;

        public DeleteModel (IMovieService movieService, IAuditLogService auditLogService)
        {
            _movieService = movieService;
            _auditLogService = auditLogService;
        }

        public Movie? MovieToDelete { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            MovieToDelete = await _movieService.GetMovieByIdAsync(id);

            if (MovieToDelete == null)
            {
                TempData["ErrorMessage"] = "Movie not found. It might have already been deleted.";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var movieBeingDeleted = await _movieService.GetMovieByIdAsync(id);
            string movieTitleForLog = movieBeingDeleted?.Title ?? "Unknown Movie";

            var adminUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminUserIdString, out int adminUserId);

            var result = await _movieService.DeleteMovieAsync(id, adminUserId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Movie '{movieTitleForLog}' (ID: {id}) was successfully deleted.";
                return RedirectToPage("./Index");
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? $"An error occurred while deleting movie ID: {id}.";
                return RedirectToPage("./Index");
            }
        }
    }
}
