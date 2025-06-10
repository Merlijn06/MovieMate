using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.DAL.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieMate.Models;

namespace MovieMate.DAL.Repositories
{
    public class MovieRepository : BaseRepository, IMovieRepository
    {
        public MovieRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Movie?> GetByIdAsync(int movieId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt,
                    COALESCE(AVG(r.RatingValue), 0) AS AverageRating,
                    COUNT(DISTINCT r.ReviewID) AS TotalRatings
                FROM Movies m
                LEFT JOIN Reviews r ON m.MovieID = r.MovieID
                WHERE m.MovieID = @MovieId
                GROUP BY m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt;
            ";
            return await connection.QuerySingleOrDefaultAsync<Movie>(sql, new { MovieId = movieId });
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt,
                    COALESCE(AVG(r.RatingValue), 0) AS AverageRating,
                    COUNT(DISTINCT r.ReviewID) AS TotalRatings
                FROM Movies m
                LEFT JOIN Reviews r ON m.MovieID = r.MovieID
                GROUP BY m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt
                ORDER BY m.Title;
            ";
            return await connection.QueryAsync<Movie>(sql);
        }

        public async Task<IEnumerable<Movie>> SearchByTitleAsync(string titleQuery)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt,
                    COALESCE(AVG(r.RatingValue), 0) AS AverageRating,
                    COUNT(DISTINCT r.ReviewID) AS TotalRatings
                FROM Movies m
                LEFT JOIN Reviews r ON m.MovieID = r.MovieID
                WHERE m.Title LIKE @Query                    -- Using LIKE for partial matching
                GROUP BY m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt
                ORDER BY m.Title;
            ";
            return await connection.QueryAsync<Movie>(sql, new { Query = $"%{titleQuery}%" });
        }

        public async Task<int> AddAsync(Movie movie)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Movies (Title, Genre, Description, ReleaseDate, PosterURL, CreatedAt, UpdatedAt)
                VALUES (@Title, @Genre, @Description, @ReleaseDate, @PosterURL, NOW(), NOW());
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, movie);
        }

        public async Task<bool> UpdateAsync(Movie movie)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE Movies SET
                    Title = @Title,
                    Genre = @Genre,
                    Description = @Description,
                    ReleaseDate = @ReleaseDate,
                    PosterURL = @PosterURL,
                    UpdatedAt = NOW()
                WHERE MovieID = @MovieID;";
            return await connection.ExecuteAsync(sql, movie) > 0;
        }

        public async Task<bool> DeleteAsync(int movieId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM Movies WHERE MovieID = @MovieId;";
            return await connection.ExecuteAsync(sql, new { MovieId = movieId }) > 0;
        }

        public async Task<IEnumerable<Movie>> GetByIdsAsync(IEnumerable<int> movieIds)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt,
                    COALESCE(AVG(r.RatingValue), 0) AS AverageRating,
                    COUNT(DISTINCT r.ReviewID) AS TotalRatings
                FROM Movies m
                LEFT JOIN Reviews r ON m.MovieID = r.MovieID
                WHERE m.MovieID IN @MovieIds
                GROUP BY m.MovieID, m.Title, m.Genre, m.Description, m.ReleaseDate, m.PosterURL, m.CreatedAt, m.UpdatedAt;
            ";
            return await connection.QueryAsync<Movie>(sql, new { MovieIds = movieIds });
        }
    }
}
