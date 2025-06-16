using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.BLL.Recommendations
{
    public interface IMovieFinderStrategy
    {
        IEnumerable<Movie> FindCandidateMovies(IEnumerable<Movie> sourceMovies, IEnumerable<Movie> allMovies);
    }
}
