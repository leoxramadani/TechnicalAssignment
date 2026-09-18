using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Helpers.PremiumComputation;
using Claims.Persistance;
using Claims.Services.AuditingServices;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services.CoversServices;
/// <summary>
/// Service implementing business workflows for insurance covers.
/// </summary>
public class CoversService(ClaimsContext claimsContext, IPremiumComputationService premiumComputationService, IAuditQueue auditQueue) : ICoversService
{
    private readonly ClaimsContext _claimsContext = claimsContext;
    private readonly IPremiumComputationService _premiumComputationService = premiumComputationService;
    private readonly IAuditQueue _auditQueue = auditQueue;

    /// <summary>
    /// Retrieves all insurance covers from the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Cover>> GetCoversAsync(CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Covers.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific insurance cover by its unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Cover?> GetCoverByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Covers
            .FirstOrDefaultAsync(cover => cover.Id == id, cancellationToken);
    }

    /// <summary>
    /// Creates a new insurance cover, computes its premium, saves it to the database, and enqueues an audit message.
    /// </summary>
    /// <param name="cover"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    
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
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

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
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="coverType"></param>
    /// <returns></returns>
    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType)
    {
        return _premiumComputationService.ComputePremium(startDate, endDate, coverType);
    }

}
