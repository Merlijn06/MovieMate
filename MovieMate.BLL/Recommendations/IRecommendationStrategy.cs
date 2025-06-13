using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.BLL.Recommendations
{
    public interface IRecommendationStrategy
    {
        Task<IEnumerable<Movie>> GetPreferredMoviesAsync(int userId);
    }
}
