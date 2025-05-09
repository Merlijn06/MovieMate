using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.DAL.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie?> GetByIdAsync(int movieId);
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<IEnumerable<Movie>> SearchByTitleAsync(string titleQuery);
        Task<int> AddAsync(Movie movie);
        Task<bool> UpdateAsync(Movie movie);
        Task<bool> DeleteAsync(int movieId);
        // Task<void> UpdateAverageRatingAsync(int movieId, decimal averageRating, int totalRatings); // Movie model will hold this, calculation in BLL/DAL
    }
}
