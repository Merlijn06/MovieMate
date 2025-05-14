using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    namespace MovieMate.BLL.Interfaces
    {
        public interface IMovieService
        {
            Task<Movie?> GetMovieByIdAsync(int id);
            Task<IEnumerable<Movie>> GetAllMoviesAsync();
            Task<IEnumerable<Movie>> SearchMoviesAsync(string? title);

            // Admin operations
            Task<ServiceResult<int>> AddMovieAsync(Movie movie, int? performingUserId);
            Task<ServiceResult> UpdateMovieAsync(Movie movie, int? performingUserId);
            Task<ServiceResult> DeleteMovieAsync(int id, int? performingUserId);
        }

        // Helper classes for returning results with success status and messages/data
        public class ServiceResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class ServiceResult<T> : ServiceResult
        {
            public T? Data { get; set; }
        }
    }
}
