using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.DAL.Interfaces
{
    public interface IWatchlistRepository
    {
        Task<IEnumerable<WatchlistItem>> GetWatchlistByUserIdAsync(int userId);
        Task<bool> AddToWatchlistAsync(int userId, int movieId);
        Task<bool> RemoveFromWatchlistAsync(int userId, int movieId);
        Task<bool> IsMovieInWatchlistAsync(int userId, int movieId);
    }
}
