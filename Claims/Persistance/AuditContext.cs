using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Persistance
{
    public class AuditContext : DbContext
    {
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        /// <summary>
        /// Gets or sets the DbSet for ClaimAudit entities.
        /// </summary>
        public DbSet<ClaimAudit> ClaimAudits { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for CoverAudit entities.
        /// </summary>
        public DbSet<CoverAudit> CoverAudits { get; set; }
    }
}
