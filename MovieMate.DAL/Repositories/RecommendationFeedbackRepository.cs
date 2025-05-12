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
    public class RecommendationFeedbackRepository : BaseRepository, IRecommendationFeedbackRepository
    {
        public RecommendationFeedbackRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<RecommendationFeedback?> GetFeedbackAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM RecommendationFeedbacks WHERE UserID = @UserId AND MovieID = @MovieId;";
            return await connection.QuerySingleOrDefaultAsync<RecommendationFeedback>(sql, new { UserId = userId, MovieId = movieId });
        }

        public async Task AddOrUpdateFeedbackAsync(int userId, int movieId, bool liked)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO RecommendationFeedbacks (UserID, MovieID, Liked, CreatedAt)
                VALUES (@UserId, @MovieId, @LikedParam, NOW())
                ON DUPLICATE KEY UPDATE Liked = @LikedParam";

            await connection.ExecuteAsync(sql, new { UserId = userId, MovieId = movieId, LikedParam = liked });
        }

        public async Task<bool> RemoveFeedbackAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM RecommendationFeedbacks WHERE UserID = @UserId AND MovieID = @MovieId;";
            return await connection.ExecuteAsync(sql, new { UserId = userId, MovieId = movieId }) > 0;
        }

        public async Task<IEnumerable<RecommendationFeedback>> GetFeedbacksByUserAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM RecommendationFeedbacks WHERE UserID = @UserId;";
            return await connection.QueryAsync<RecommendationFeedback>(sql, new { UserId = userId });
        }
    }
}
