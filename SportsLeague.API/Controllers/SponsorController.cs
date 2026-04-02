using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _sponsorService;
    private readonly IMapper _mapper;

    public SponsorController(ISponsorService sponsorService, IMapper mapper)
    {
        _sponsorService = sponsorService;
        _mapper = mapper;
    }

    // GET /api/Sponsor
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
    {
        var sponsors = await _sponsorService.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors));
    }

    // GET /api/Sponsor/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
    {
        var sponsor = await _sponsorService.GetByIdAsync(id);
        if (sponsor == null)
            return NotFound(new { message = $"Sponsor con ID {id} no encontrado" });
        return Ok(_mapper.Map<SponsorResponseDTO>(sponsor));
    }

    // POST /api/Sponsor
    [HttpPost]
    public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            var created = await _sponsorService.CreateAsync(sponsor);
            var responseDto = _mapper.Map<SponsorResponseDTO>(created);
            return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/Sponsor/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            await _sponsorService.UpdateAsync(id, sponsor);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // DELETE /api/Sponsor/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _sponsorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // POST /api/Sponsor/{id}/tournaments
    [HttpPost("{id}/tournaments")]
    public async Task<ActionResult> LinkToTournament(int id, TournamentSponsorRequestDTO dto)
    {
        try
        {
            await _sponsorService.LinkToTournamentAsync(id, dto.TournamentId, dto.ContractAmount);
            return Ok(new { message = "Sponsor vinculado al torneo exitosamente" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // GET /api/Sponsor/{id}/tournaments
    [HttpGet("{id}/tournaments")]
    public async Task<ActionResult<IEnumerable<TournamentResponseDTO>>> GetTournaments(int id)
    {
        try
        {
            var tournaments = await _sponsorService.GetTournamentsBySponsorAsync(id);
            return Ok(_mapper.Map<IEnumerable<TournamentResponseDTO>>(tournaments));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // DELETE /api/Sponsor/{id}/tournaments/{tid}
    [HttpDelete("{id}/tournaments/{tid}")]
    public async Task<ActionResult> UnlinkFromTournament(int id, int tid)
    {
        try
        {
            await _sponsorService.UnlinkFromTournamentAsync(id, tid);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}


