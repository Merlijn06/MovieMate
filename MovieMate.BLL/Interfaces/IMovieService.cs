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

            Task<ServiceResult<int>> AddMovieAsync(Movie movie, int? performingUserId);
            Task<ServiceResult> UpdateMovieAsync(Movie movie, int? performingUserId);
            Task<ServiceResult> DeleteMovieAsync(int id, int? performingUserId);
        }
    }
}
