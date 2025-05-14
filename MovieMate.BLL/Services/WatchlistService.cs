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
    public class WatchlistService : IWatchlistService
    {
        private readonly IWatchlistRepository _watchlistRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuditLogService _auditLogService;

        public WatchlistService(IWatchlistRepository watchlistRepository, IMovieRepository movieRepository, IAuditLogService auditLogService)
        {
            _watchlistRepository = watchlistRepository ?? throw new ArgumentNullException(nameof(watchlistRepository));
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        }

        public async Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<WatchlistItem>();
            // The DAL method GetWatchlistByUserIdAsync now returns WatchlistItems with Movie object populated if you used Dapper multi-mapping
            // If not, then this method returns basic WatchlistItems, and UI/PageModel needs to fetch Movie details.
            // Let's assume DAL provides items with MovieId, and if we need full movie details for display,
            // we might have a different method or ViewModel. For now, this returns what the DAL gives.
            return await _watchlistRepository.GetWatchlistByUserIdAsync(userId);
        }


        public async Task<ServiceResult> AddMovieToWatchlistAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid user or movie ID." };
            }

            var movieExists = await _movieRepository.GetByIdAsync(movieId);
            if (movieExists == null)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Movie not found." };
            }

            bool alreadyInWatchlist = await _watchlistRepository.IsMovieInWatchlistAsync(userId, movieId);
            if (alreadyInWatchlist)
            {
                return new ServiceResult { Success = true, ErrorMessage = "Movie is already in your watchlist." }; // Still success
            }

            try
            {
                bool success = await _watchlistRepository.AddToWatchlistAsync(userId, movieId);
                if (success)
                {
                    await _auditLogService.LogActionAsync(userId, "Added Movie to Watchlist", $"MovieID: {movieId}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    // This branch might be hit if AddToWatchlistAsync's try-catch for duplicate returns false
                    return new ServiceResult { Success = false, ErrorMessage = "Failed to add movie to watchlist. It might already be there." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to watchlist: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred." };
            }
        }

        public async Task<ServiceResult> RemoveMovieFromWatchlistAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid user or movie ID." };
            }

            try
            {
                bool success = await _watchlistRepository.RemoveFromWatchlistAsync(userId, movieId);
                if (success)
                {
                    await _auditLogService.LogActionAsync(userId, "Removed Movie from Watchlist", $"MovieID: {movieId}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Movie not found in watchlist or failed to remove." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from watchlist: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred." };
            }
        }

        public async Task<bool> IsMovieInUserWatchlistAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0) return false;
            return await _watchlistRepository.IsMovieInWatchlistAsync(userId, movieId);
        }
    }
}
