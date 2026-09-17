using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Persistance;

public class ClaimsContext : DbContext
{
    /// <summary>
    /// Gets or sets the DbSet for Claim entities.
    /// </summary>
    public DbSet<Claim> Claims { get; set; } = null!;
    /// <summary>
    /// Gets or sets the DbSet for Cover entities.
    /// </summary>
    public DbSet<Cover> Covers { get; set; } = null!;

    public ClaimsContext(DbContextOptions<ClaimsContext> options) : base(options)
        {
        }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Claim>().ToCollection("claims");
        modelBuilder.Entity<Cover>().ToCollection("covers");
    }
}
