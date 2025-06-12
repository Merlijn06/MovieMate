using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;

namespace MovieMate.DAL.Repositories
{
    public class ReviewRepository : BaseRepository, IReviewRepository
    {
        public ReviewRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT r.ReviewID, r.UserID, r.MovieID, r.RatingValue, r.Comment, r.CreatedAt, r.UpdatedAt, u.Username
                FROM Reviews r
                JOIN Users u ON r.UserID = u.UserID
                WHERE r.ReviewID = @ReviewId;";
            return await connection.QuerySingleOrDefaultAsync<Review>(sql, new { ReviewId = reviewId });
        }

        public async Task<IEnumerable<Review>> GetReviewsByMovieIdAsync(int movieId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT r.ReviewID, r.UserID, r.MovieID, r.RatingValue, r.Comment, r.CreatedAt, r.UpdatedAt, u.Username
                FROM Reviews r
                JOIN Users u ON r.UserID = u.UserID
                WHERE r.MovieID = @MovieId
                ORDER BY r.CreatedAt DESC;";
            return await connection.QueryAsync<Review>(sql, new { MovieId = movieId });
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT 
                    r.ReviewID, r.UserID, r.MovieID, r.RatingValue, r.Comment, r.CreatedAt, r.UpdatedAt, 
                    u.Username,
                    m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt
                FROM Reviews r
                JOIN Users u ON r.UserID = u.UserID
                JOIN Movies m ON r.MovieID = m.MovieID
                WHERE r.UserID = @UserId
                ORDER BY r.CreatedAt DESC;";
            return await connection.QueryAsync<Review, User, Movie, Review>(
                sql,
                (review, user, movie) =>
                {
                    review.Username = user.Username;
                    review.Movie = movie;
                    return review;
                },
                new { UserId = userId },
                splitOn: "Username,MovieID"
            );
        }

        public async Task<Review?> GetReviewByUserAndMovieAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT r.ReviewID, r.UserID, r.MovieID, r.RatingValue, r.Comment, r.CreatedAt, r.UpdatedAt, u.Username
                FROM Reviews r
                JOIN Users u ON r.UserID = u.UserID
                WHERE r.UserID = @UserId AND r.MovieID = @MovieId;";
            return await connection.QuerySingleOrDefaultAsync<Review>(sql, new { UserId = userId, MovieId = movieId });
        }

        public async Task<bool> UserHasReviewedMovieAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM Reviews WHERE UserID = @UserId AND MovieID = @MovieId;";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, MovieId = movieId });
            return count > 0;
        }

        public async Task<int> AddAsync(Review review)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Reviews (UserID, MovieID, RatingValue, Comment, CreatedAt, UpdatedAt)
                VALUES (@UserId, @MovieId, @RatingValue, @Comment, NOW(), NOW());
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, review);
        }

        public async Task<bool> UpdateAsync(Review review)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE Reviews SET
                    RatingValue = @RatingValue,
                    Comment = @Comment,
                    UpdatedAt = NOW()
                WHERE ReviewID = @ReviewID;";
            return await connection.ExecuteAsync(sql, review) > 0;
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM Reviews WHERE ReviewID = @ReviewId;";
            return await connection.ExecuteAsync(sql, new { ReviewId = reviewId }) > 0;
        }
    }
}
