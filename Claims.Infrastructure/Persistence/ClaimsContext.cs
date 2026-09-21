using Claims.Application.Common.Interfaces;
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Infrastructure.Persistence;

public class ClaimsContext(DbContextOptions<ClaimsContext> options) : DbContext(options), IClaimsContext
{
    /// <summary>
    /// Gets or sets the DbSet for Claim entities.
    /// </summary>
    public DbSet<Claim> Claims { get; set; } = null!;
    /// <summary>
    /// Gets or sets the DbSet for Cover entities.
    /// </summary>
    public DbSet<Cover> Covers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Claim>().ToCollection("claims");
        modelBuilder.Entity<Cover>().ToCollection("covers");
    }
}
