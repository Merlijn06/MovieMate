using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.DAL.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetReviewsByMovieIdAsync(int movieId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
        Task<Review?> GetReviewByUserAndMovieAsync(int userId, int movieId);
        Task<int> AddAsync(Review review);
        Task<bool> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int reviewId);
        Task<bool> UserHasReviewedMovieAsync(int userId, int movieId); // More direct check than GetReviewByUserAndMovieAsync if only existence is needed
    }
}
