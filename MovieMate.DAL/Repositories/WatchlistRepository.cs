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
    public class WatchlistRepository : BaseRepository, IWatchlistRepository
    {
        public WatchlistRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<WatchlistItem>> GetWatchlistByUserIdAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    WatchlistID, UserID, MovieID, AddedAt
                FROM Watchlist
                WHERE UserID = @UserId
                ORDER BY AddedAt DESC;";

            return await connection.QueryAsync<WatchlistItem>(sql, new { UserId = userId });
        }

        public async Task<bool> AddToWatchlistAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();

            var sqlInsert = @"
                INSERT INTO Watchlist (UserID, MovieID, AddedAt)
                VALUES (@UserId, @MovieId, NOW());";

            try
            {
                int rowsAffected = await connection.ExecuteAsync(sqlInsert, new { UserId = userId, MovieId = movieId });
                return rowsAffected > 0;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
            {
                return true; // Als de film al in de watchlist staat returnt die nu True omdat de film succesvol in de watchlist staat (ook al stond de film er al)
            }
        }

        public async Task<bool> RemoveFromWatchlistAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM Watchlist WHERE UserID = @UserId AND MovieID = @MovieId;";
            return await connection.ExecuteAsync(sql, new { UserId = userId, MovieId = movieId }) > 0;
        }

        public async Task<bool> IsMovieInWatchlistAsync(int userId, int movieId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM Watchlist WHERE UserID = @UserId AND MovieID = @MovieId;";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, MovieId = movieId });
            return count > 0;
        }
    }
}
