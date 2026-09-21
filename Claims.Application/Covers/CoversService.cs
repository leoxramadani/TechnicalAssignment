using Claims.Application.Auditing;
using Claims.Application.Common.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Claims.Application.Covers;

/// <summary>
/// Service implementing business workflows for insurance covers.
/// </summary>
public class CoversService(
    IClaimsContext claimsContext,
    IPremiumComputationService premiumComputationService,
    IAuditQueue auditQueue) : ICoversService
{
    private readonly IClaimsContext _claimsContext = claimsContext;
    private readonly IPremiumComputationService _premiumComputationService = premiumComputationService;
    private readonly IAuditQueue _auditQueue = auditQueue;

    /// <summary>
    /// Retrieves all insurance covers from the database.
    /// </summary>
    public async Task<IEnumerable<Cover>> GetCoversAsync(CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Covers.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific insurance cover by its unique identifier.
    /// </summary>
    public async Task<Cover?> GetCoverByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Covers
            .FirstOrDefaultAsync(cover => cover.Id == id, cancellationToken);
    }

    /// <summary>
    /// Creates a new insurance cover, computes its premium, saves it to the database, and enqueues an audit message.
    /// </summary>
    public async Task<Cover> CreateCoverAsync(Cover cover, CancellationToken cancellationToken = default)
    {
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = _premiumComputationService.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);

        await _claimsContext.Covers.AddAsync(cover, cancellationToken);
        await _claimsContext.SaveChangesAsync(cancellationToken);

        await _auditQueue.EnqueueAsync(
            new AuditMessage(
                cover.Id,
                "Cover",
                "POST"),
                cancellationToken);

        return cover;
    }

    /// <summary>
    /// Deletes an existing insurance cover by its unique identifier, removes it from the database, and enqueues an audit message.
    /// </summary>
    public async Task<bool> DeleteCoverAsync(string id, CancellationToken cancellationToken = default)
    {
        var cover = await _claimsContext.Covers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (cover is null)
        {
            return false;
        }

        _claimsContext.Covers.Remove(cover);
        await _claimsContext.SaveChangesAsync(cancellationToken);

        await _auditQueue.EnqueueAsync(
            new AuditMessage(
                cover.Id,
                "Cover",
                "DELETE"),
                cancellationToken);
        return true;
    }

    /// <summary>
    /// Computes the premium for a given insurance cover based on its start date, end date, and cover type.
    /// </summary>
    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType)
    {
        return _premiumComputationService.ComputePremium(startDate, endDate, coverType);
    }
}
