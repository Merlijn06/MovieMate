using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;

namespace MovieMate.BLL.Recommendations
{
    public class WatchlistStrategy : IRecommendationStrategy
    {
        private readonly IWatchlistRepository _watchlistRepository;
        public WatchlistStrategy(IWatchlistRepository watchlistRepository)
        {
            _watchlistRepository = watchlistRepository;
        }
        public async Task<IEnumerable<Movie>> GetPreferredMoviesAsync(int userId)
        {
            var userWatchlistItems = await _watchlistRepository.GetWatchlistByUserIdAsync(userId);

            return userWatchlistItems
                .Where(item => item.Movie != null)
                .Select(item => item.Movie!);
        }
    }
}
