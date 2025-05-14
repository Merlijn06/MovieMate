using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IWatchlistService
    {
        Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(int userId);

        Task<ServiceResult> AddMovieToWatchlistAsync(int userId, int movieId);
        Task<ServiceResult> RemoveMovieFromWatchlistAsync(int userId, int movieId);
        Task<bool> IsMovieInUserWatchlistAsync(int userId, int movieId);
    }
}
