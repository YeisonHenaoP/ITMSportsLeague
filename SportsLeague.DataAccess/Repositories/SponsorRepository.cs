using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
{
    public SponsorRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<Sponsor?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<Sponsor?> GetByIdWithTournamentsAsync(int id)
    {
        return await _dbSet
            .Where(s => s.Id == id)
            .Include(s => s.TournamentSponsors)
                .ThenInclude(ts => ts.Tournament)
            .FirstOrDefaultAsync();
    }
}
