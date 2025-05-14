using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<Movie>> GetRecommendationsForUserAsync(int userId, int count = 10);

        // Optional: Method to explicitly trigger an update/recalculation of recommendations if needed
        // Task RefreshUserRecommendationsAsync(int userId);
    }
}
