using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Application.Common.Interfaces;

/// <summary>
/// Abstraction for database operations on claims and covers.
/// </summary>
public interface IClaimsContext
{
    DbSet<Claim> Claims { get; }
    DbSet<Cover> Covers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
