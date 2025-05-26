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
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuditLogService _auditLogService;

        public ReviewService(IReviewRepository reviewRepository, IMovieRepository movieRepository, IAuditLogService auditLogService)
        {
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            if (reviewId <= 0) return null;
            return await _reviewRepository.GetByIdAsync(reviewId);
        }

        public async Task<IEnumerable<Review>> GetReviewsForMovieAsync(int movieId)
        {
            if (movieId <= 0) return Enumerable.Empty<Review>();
            return await _reviewRepository.GetReviewsByMovieIdAsync(movieId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<Review>();
            return await _reviewRepository.GetReviewsByUserIdAsync(userId);
        }

        public async Task<bool> CanUserReviewMovieAsync(int userId, int movieId)
        {
            if (userId <= 0 || movieId <= 0) return false;
            return !(await _reviewRepository.UserHasReviewedMovieAsync(userId, movieId));
        }

        public async Task<ServiceResult<Review>> AddOrUpdateReviewAsync(Review review, int performingUserId)
        {
            if (review == null || review.MovieId <= 0 || review.UserId <= 0)
            {
                return new ServiceResult<Review> { Success = false, ErrorMessage = "Invalid review data." };
            }
            if (review.UserId != performingUserId)
            {
                return new ServiceResult<Review> { Success = false, ErrorMessage = "User mismatch. Cannot submit review for another user." };
            }
            if (review.RatingValue < 0 || review.RatingValue > 10)
            {
                return new ServiceResult<Review> { Success = false, ErrorMessage = "Rating must be between 0 and 10." };
            }

            var movieExists = await _movieRepository.GetByIdAsync(review.MovieId);
            if (movieExists == null)
            {
                return new ServiceResult<Review> { Success = false, ErrorMessage = "Movie not found." };
            }

            try
            {
                var existingReview = await _reviewRepository.GetReviewByUserAndMovieAsync(performingUserId, review.MovieId);

                if (existingReview != null) // Update existing review
                {
                    existingReview.RatingValue = review.RatingValue;
                    existingReview.Comment = review.Comment;
                    existingReview.UpdatedAt = DateTime.UtcNow;

                    bool updated = await _reviewRepository.UpdateAsync(existingReview);
                    if (updated)
                    {
                        await _auditLogService.LogActionAsync(performingUserId, "User Updated Review", $"MovieID: {review.MovieId}, Rating: {review.RatingValue}");
                        // Since MovieRepository's Get queries now calculate average rating, no need to trigger a separate update.
                        return new ServiceResult<Review> { Success = true, Data = existingReview };
                    }
                    else
                    {
                        return new ServiceResult<Review> { Success = false, ErrorMessage = "Failed to update review." };
                    }
                }
                else // Add new review
                {
                    review.CreatedAt = DateTime.UtcNow;
                    review.UpdatedAt = DateTime.UtcNow;
                    int newReviewId = await _reviewRepository.AddAsync(review);
                    if (newReviewId > 0)
                    {
                        review.ReviewId = newReviewId;
                        await _auditLogService.LogActionAsync(performingUserId, "User Added Review", $"MovieID: {review.MovieId}, Rating: {review.RatingValue}");
                        return new ServiceResult<Review> { Success = true, Data = review };
                    }
                    else
                    {
                        return new ServiceResult<Review> { Success = false, ErrorMessage = "Failed to add review." };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding/updating review: {ex.Message}");
                return new ServiceResult<Review> { Success = false, ErrorMessage = "An unexpected error occurred." };
            }
        }

        public async Task<ServiceResult> DeleteReviewAsync(int reviewId, int performingUserId)
        {
            if (reviewId <= 0)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Invalid review ID." };
            }

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                return new ServiceResult { Success = false, ErrorMessage = "Review not found." };
            }

            // Check if the performing user is the owner of the review OR an admin
            var performingUser = await GetPerformingUserSomehow(performingUserId); // You'd need a way to get this user's details if not passed
            if (review.UserId != performingUserId && (performingUser == null || !performingUser.IsAdmin))
            {
                return new ServiceResult { Success = false, ErrorMessage = "You do not have permission to delete this review." };
            }

            try
            {
                bool success = await _reviewRepository.DeleteAsync(reviewId);
                if (success)
                {
                    await _auditLogService.LogActionAsync(performingUserId, "User Deleted Review", $"ReviewID: {reviewId}, MovieID: {review.MovieId}");
                    return new ServiceResult { Success = true };
                }
                else
                {
                    return new ServiceResult { Success = false, ErrorMessage = "Failed to delete review." };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting review: {ex.Message}");
                return new ServiceResult { Success = false, ErrorMessage = "An unexpected error occurred." };
            }
        }
        // Placeholder - in a real app, you'd get this from your auth context or IUserService
        private Task<User?> GetPerformingUserSomehow(int userId) => Task.FromResult<User?>(new User { UserId = userId, IsAdmin = true }); // MOCK
    }
}
