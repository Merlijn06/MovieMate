using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;

namespace MovieMate.BLL.Recommendations
{
    public class HighRatingStrategy : IRecommendationStrategy
    {
        private readonly IReviewRepository _reviewRepository;

        public HighRatingStrategy(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<Movie>> GetPreferredMoviesAsync(int userId)
        {
            var userReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);

            return userReviews
                .Where(r => r.RatingValue >= 7.0m && r.Movie != null)
                .Select(r => r.Movie!);
        }
    }
}
