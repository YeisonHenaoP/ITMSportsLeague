using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        // Validar nombre único
        var existing = await _sponsorRepository.GetByNameAsync(sponsor.Name);
        if (existing != null)
        {
            _logger.LogWarning("Sponsor with name '{Name}' already exists", sponsor.Name);
            throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
        }

        // Validar formato de email
        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del ContactEmail no es válido");

        _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        // Validar nombre único si cambió
        if (!existing.Name.Equals(sponsor.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameConflict = await _sponsorRepository.GetByNameAsync(sponsor.Name);
            if (nameConflict != null)
                throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
        }

        // Validar formato de email
        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El formato del ContactEmail no es válido");

        existing.Name = sponsor.Name;
        existing.ContactEmail = sponsor.ContactEmail;
        existing.Phone = sponsor.Phone;
        existing.WebsiteUrl = sponsor.WebsiteUrl;
        existing.Category = sponsor.Category;

        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        // Validar que el sponsor exista
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        // Validar que el torneo exista
        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

        // Validar que no esté duplicada la vinculación
        var existing = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existing != null)
            throw new InvalidOperationException("Este sponsor ya está vinculado a este torneo");

        // Validar ContractAmount > 0
        if (contractAmount <= 0)
            throw new InvalidOperationException("El ContractAmount debe ser mayor a 0");

        var link = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Linking sponsor {SponsorId} to tournament {TournamentId}",
            sponsorId, tournamentId);
        await _tournamentSponsorRepository.CreateAsync(link);
    }

    public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
    {
        var link = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (link == null)
            throw new KeyNotFoundException(
                $"No se encontró la vinculación entre el sponsor {sponsorId} y el torneo {tournamentId}");

        _logger.LogInformation(
            "Unlinking sponsor {SponsorId} from tournament {TournamentId}",
            sponsorId, tournamentId);
        await _tournamentSponsorRepository.DeleteAsync(link.Id);
    }

    public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var exists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        var links = await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        return links.Select(ts => ts.Tournament);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}