using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MovieMate.DAL.Interfaces;

namespace MovieMate.DAL.Repositories
{
    public class WatchlistRepository : BaseRepository, IWatchlistRepository
    {
        public WatchlistRepository(IConfiguration configuration) : base(configuration) { }


    }
}
