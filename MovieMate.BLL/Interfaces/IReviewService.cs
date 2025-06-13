using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IReviewService
    {
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetReviewsForMovieAsync(int movieId);
        Task<IEnumerable<Review>> GetReviewsByUserAsync(int userId);

        Task<ServiceResult<Review>> AddOrUpdateReviewAsync(Review review, int performingUserId);
        Task<bool> CanUserReviewMovieAsync(int userId, int movieId);
    }
}
