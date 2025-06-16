using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieMate.Models;

namespace MovieMate.BLL.Recommendations
{
    public class GenreBasedFinderStrategy : IMovieFinderStrategy
    {
        public IEnumerable<Movie> FindCandidateMovies(IEnumerable<Movie> sourceMovies, IEnumerable<Movie> allMovies)
        {
            if (!sourceMovies.Any())
            {
                return Enumerable.Empty<Movie>();
            }

            var preferredGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var movie in sourceMovies)
            {
                if (!string.IsNullOrWhiteSpace(movie.Genre))
                {
                    var genres = movie.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim());
                    foreach (var genre in genres)
                    {
                        preferredGenres.Add(genre);
                    }
                }
            }

            if (!preferredGenres.Any())
            {
                return Enumerable.Empty<Movie>();
            }

            return allMovies
                .Where(m => !string.IsNullOrWhiteSpace(m.Genre) &&
                            m.Genre.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(g => g.Trim())
                                   .Any(g => preferredGenres.Contains(g)));
        }
    }
}
