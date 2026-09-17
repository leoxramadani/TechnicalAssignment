using Claims.Domain.Entities;
using Claims.Persistance;
using Claims.Services.AuditingServices;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services.ClaimsServices;

/// <summary>
/// Service implementing business workflows for insurance claims.
/// </summary>
public class ClaimsService : IClaimsService
{
    private readonly ClaimsContext _claimsContext;
    private readonly IAuditQueue _auditQueue;
    public ClaimsService(
            ClaimsContext claimsContext,
            IAuditQueue auditQueue)
    {
        _claimsContext = claimsContext;
        _auditQueue = auditQueue;
    }

    /// <summary>
    /// Retrieves all claims from the database.
    /// </summary>
    public async Task<IEnumerable<Claim>> GetClaimsAsync(CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Claims.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a claim by its unique identifier.
    /// </summary>
    public async Task<Claim?> GetClaimByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _claimsContext.Claims
            .FirstOrDefaultAsync(claim => claim.Id == id, cancellationToken);
    }

    /// <summary>
    /// Creates a new claim and records an audit event.
    /// </summary>
    public async Task<Claim> CreateClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        claim.Id = Guid.NewGuid().ToString();
        await _claimsContext.Claims.AddAsync(claim, cancellationToken);
        await _claimsContext.SaveChangesAsync(cancellationToken);

        await _auditQueue.EnqueueAsync(
            new AuditMessage(
                claim.Id,
                "Claim",
                "POST"),
                cancellationToken);

        return claim;
    }

    /// <summary>
    /// Deletes a claim by its unique identifier and records an audit event.
    /// </summary>
    public async Task<bool> DeleteClaimAsync(string id, CancellationToken cancellationToken = default)
    {
        var claim = await _claimsContext.Claims
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (claim is null)
        {
            return false;
        }

        _claimsContext.Claims.Remove(claim);
        await _claimsContext.SaveChangesAsync(cancellationToken);

        await _auditQueue.EnqueueAsync(
            new AuditMessage(
                claim.Id,
                "Claim",
                "DELETE"),
                cancellationToken);

        return true;
    }
}
