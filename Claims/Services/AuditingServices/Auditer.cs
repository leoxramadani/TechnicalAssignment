using Claims.Domain;
using Claims.Persistance;
using Claims.Domain.Entities;

namespace Claims.Services.AuditingServices
{
    public class Auditer(AuditContext auditContext) : IAuditService
    {
        private readonly AuditContext _auditContext = auditContext;

        public async Task AuditAsync(
            AuditMessage message,
            CancellationToken cancellationToken = default)
        {
            switch (message.EntityType)
            {
                case "Claim":
                    _auditContext.ClaimAudits.Add(new ClaimAudit
                    {
                        Created = DateTime.UtcNow,
                        HttpRequestType = message.HttpRequestType,
                        ClaimId = message.EntityId
                    });
                    break;

                case "Cover":
                    _auditContext.CoverAudits.Add(new CoverAudit
                    {
                        Created = DateTime.UtcNow,
                        HttpRequestType = message.HttpRequestType,
                        CoverId = message.EntityId
                    });
                    break;

                default:
                    throw new ArgumentException(
                        $"Unsupported entity type: {message.EntityType}");
            }

            await _auditContext.SaveChangesAsync(cancellationToken);
        }
    }
}
