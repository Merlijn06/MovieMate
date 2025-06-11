using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.BLL.Interfaces;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IAuditLogService _auditLogService;

        public MovieService(IMovieRepository movieRepository, IAuditLogService auditLogService)
        {
            _movieRepository = movieRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return await GetAllMoviesAsync();
            }
            return await _movieRepository.SearchByTitleAsync(title.Trim());
        }

        public async Task<ServiceResult<int>> AddMovieAsync(Movie movie, int? performingUserId)
        {
            if (string.IsNullOrWhiteSpace(movie.Title))
            {
                return new ServiceResult<int> { Success = false, ErrorMessage = "Movie title cannot be empty." };
            }

            try
            {
                var existing = await _movieRepository.SearchByTitleAsync(movie.Title);
                if (existing.Any(m => m.Title.Equals(movie.Title, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ServiceResult<int> { Success = false, ErrorMessage = "A movie with this title already exists." };
                }

                int newMovieId = await _movieRepository.AddAsync(movie);
                if (newMovieId > 0)
                {
                    await _auditLogService.LogActionAsync(performingUserId, "Admin Added Movie", $"MovieID: {newMovieId}, Title: {movie.Title}");
                    return new ServiceResult<int> { Success = true, Data = newMovieId };
                }
                else
                {
                    return new ServiceResult<int> { Success = false, ErrorMessage = "Failed to add movie to the database." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding movie: {ex.Message}");
                return new ServiceResult<int> { Success = false, ErrorMessage = "An unexpected error occurred while adding the movie." };
            }
        }

        public async Task<ServiceResult> UpdateMovieAsync(Movie movie, int? performingUserId)
        {
            if (movie.MovieId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid Movie ID provided." };
            }
            if (string.IsNullOrWhiteSpace(movie.Title))
            {
                return new ServiceResult { Success = false, ErrorMessage = "Movie title cannot be empty." };
            }

            try
            {
                bool success = await _movieRepository.UpdateAsync(movie);
                if (success)
                {
                    await _auditLogService.LogActionAsync(performingUserId, "Admin Updated Movie", $"MovieID: {movie.MovieId}, Title: {movie.Title}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Movie not found or failed to update." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating movie: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred while updating the movie." };
            }
        }

        public async Task<ServiceResult> DeleteMovieAsync(int id, int? performingUserId)
        {
            if (id <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid Movie ID provided." };
            }

            try
            {
                var movieToDelete = await _movieRepository.GetByIdAsync(id);
                if (movieToDelete == null)
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Movie not found." };
                }

                bool success = await _movieRepository.DeleteAsync(id);
                if (success)
                {
                    await _auditLogService.LogActionAsync(performingUserId, "Admin Deleted Movie", $"MovieID: {id}, Title: {movieToDelete.Title}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Failed to delete movie." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting movie: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred while deleting the movie." };
            }
        }
    }
}
