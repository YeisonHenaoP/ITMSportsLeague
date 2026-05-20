using SportsLeague.Domain.Enums;

namespace SportsLeague.API.DTOs.Request;

public class UpdateTournametStatusDTO
{
    public TournamentStatus Status { get; set; }
}

